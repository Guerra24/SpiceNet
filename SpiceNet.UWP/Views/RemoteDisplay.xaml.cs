using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Composition;
using Microsoft.Graphics.Canvas.UI.Xaml;
using SpiceNet.Protocol;
using SpiceNet.UWP.Extensions;
using SpiceNet.UWP.Models;
using SpiceNet.UWP.ViewModels;
using System.Collections.Concurrent;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Input;
using Windows.Graphics.DirectX;
using Windows.Media;
using Windows.Media.Audio;
using Windows.Media.MediaProperties;
using Windows.Media.Render;
using Windows.UI.Composition;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Hosting;

namespace SpiceNet.UWP.Views;

public sealed partial class RemoteDisplay : Page
{
    private DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

    private RemoteDisplayViewModel Data;

    private ConcurrentDictionary<uint, CanvasRenderTarget> surfaces = new();

    private Dictionary<ulong, CanvasBitmap> bitmaps = new();

    private Dictionary<ulong, (CanvasBitmap, Vector2)> cursors = new();

    private ApplicationView applicationView;
    private CoreWindow coreWindow;

    private MouseDevice mouseDevice;
    private SpriteVisual cursorVisual;
    private CompositionDrawingSurface cursorSurface = null!;
    private bool mouseLocked;
    private ushort currentMode;

    private TaskCompletionSource canvasReady = new();
    private CanvasDevice canvasDevice = null!;

    private AudioGraph graph = null!;
    private AudioFrameInputNode? frameInputNode;
    private AudioDeviceOutputNode deviceOutputNode = null!;
    private SpiceAudioDataMode audioDataMode = SpiceAudioDataMode.SPICE_AUDIO_DATA_MODE_RAW;

    public RemoteDisplay(string address, int port)
    {
        InitializeComponent();
        Data = new(address, port);
        Data.FitModeChanged += (sender, e) => FitCanvas();

        applicationView = ApplicationView.GetForCurrentView();
        applicationView.Consolidated += ApplicationView_Consolidated;
        coreWindow = CoreWindow.GetForCurrentThread();

        mouseDevice = MouseDevice.GetForCurrentView();

        var compositor = ElementCompositionPreview.GetElementVisual(RootCanvas).Compositor;

        cursorVisual = compositor.CreateSpriteVisual();

        ElementCompositionPreview.SetElementChildVisual(RootCanvas, cursorVisual);

        Window.Current.SetTitleBar(Titlebar);
    }

    private void Dispatcher_AcceleratorKeyActivated(CoreDispatcher sender, AcceleratorKeyEventArgs args)
    {
        if (args.VirtualKey == VirtualKey.Menu)
        {
            if (Data.Channel?.Inputs?.Ready != true)
                return;

            switch (args.EventType)
            {
                case CoreAcceleratorKeyEventType.KeyDown:
                case CoreAcceleratorKeyEventType.SystemKeyDown:
                    Data.Channel.Inputs.KeyDown(GetScancode(args.VirtualKey, args.KeyStatus));
                    args.Handled = true;
                    break;
                case CoreAcceleratorKeyEventType.KeyUp:
                case CoreAcceleratorKeyEventType.SystemKeyUp:
                    Data.Channel.Inputs.KeyUp(GetScancode(args.VirtualKey, args.KeyStatus));
                    args.Handled = true;
                    break;
            }
        }
    }

    private void RootCanvas_GotFocus(object sender, RoutedEventArgs e)
    {
        coreWindow.Dispatcher.AcceleratorKeyActivated += Dispatcher_AcceleratorKeyActivated;
    }

    private void RootCanvas_LostFocus(object sender, RoutedEventArgs e)
    {
        coreWindow.Dispatcher.AcceleratorKeyActivated -= Dispatcher_AcceleratorKeyActivated;
        // TODO: Reset held keys
    }

    private void ApplicationView_Consolidated(ApplicationView sender, ApplicationViewConsolidatedEventArgs args)
    {
        Data.Channel?.Dispose();
        foreach (var surface in surfaces)
            surface.Value.Dispose();
        foreach (var bitmap in bitmaps)
            bitmap.Value.Dispose();
        foreach (var bitmap in cursors)
            bitmap.Value.Item1.Dispose();
        surfaces.Clear();
        bitmaps.Clear();
        cursors.Clear();
        frameInputNode?.Stop();
        frameInputNode?.Dispose();
        graph.Stop();
        graph.Dispose();
        RootCanvas.RemoveFromVisualTree();
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized, true, false);
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        var settings = new AudioGraphSettings(AudioRenderCategory.Media);
        var graphResult = await AudioGraph.CreateAsync(settings);

        graph = graphResult.Graph;

        var deviceOutputNodeResult = await graph.CreateDeviceOutputNodeAsync();

        deviceOutputNode = deviceOutputNodeResult.DeviceOutputNode;

        graph.Start();

        await canvasReady.Task;

        Data.Channel?.DisplayInit += DisplayInit;
        Data.Channel?.InputsInit += InputsInit;
        Data.Channel?.CursorInit += CursorInit;
        Data.Channel?.PlaybackInit += PlaybackInit;
        Data.Channel?.MouseModeChanged += MouseModeChanged;
        Data.Channel?.OnDisconnected += OnDisconnected;
        Data.Channel?.Start();
    }

    private void DisplayInit(object? sender, DisplayChannel channel)
    {
        channel.SurfaceCreate += Display_SurfaceCreate;
        channel.SurfaceDestroy += Display_SurfaceDestroy;
        channel.SurfaceDrawCopy += Display_SurfaceDrawCopy;
        channel.SurfaceCopyBits += Display_SurfaceCopyBits;
        channel.SurfaceDrawFill += Display_SurfaceDrawFill;
        channel.SurfaceInvalidateList += Display_SurfaceInvalidateList;
    }

    private void InputsInit(object? sender, InputsChannel channel)
    {
        channel.Init += SyncKeyModifiers;
        channel.KeyModifiersChanged += SyncKeyModifiers;
    }

    private void CursorInit(object? sender, CursorChannel channel)
    {
        channel.Set += Cursor_Set;
        channel.Move += Cursor_Move;
        channel.InvalidateOne += Cursor_InvalidateOne;
        channel.InvalidateAll += Cursor_InvalidateAll;
        channel.Hide += Cursor_Hide;
    }

    private void PlaybackInit(object? sender, PlaybackChannel channel)
    {
        channel.Mode += Playback_Mode;
        channel.StartPlayback += Playback_Start;
        channel.Data += Playback_Data;
        channel.StopPlayback += Playback_Stop;
    }

    private void MouseModeChanged(object? sender, ushort e)
    {
        dispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, () =>
        {
            switch (e)
            {
                case Spice.SPICE_MOUSE_MODE_CLIENT:
                    if (currentMode == Spice.SPICE_MOUSE_MODE_CLIENT)
                        return;
                    mouseDevice.MouseMoved -= MouseDevice_MouseMoved;
                    mouseLocked = false;
                    break;
                case Spice.SPICE_MOUSE_MODE_SERVER:
                    if (currentMode == Spice.SPICE_MOUSE_MODE_SERVER)
                        return;
                    mouseDevice.MouseMoved += MouseDevice_MouseMoved;
                    mouseLocked = true;
                    break;
            }
            currentMode = e;
        });
    }

    private void OnDisconnected(object? sender, EventArgs e)
    {
        dispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, () =>
        {
            mouseDevice.MouseMoved -= MouseDevice_MouseMoved;
            mouseLocked = false;
            coreWindow.PointerCursor = new(CoreCursorType.Arrow, 0);
        });
    }

    #region Display

    private void Display_SurfaceCreate(object? sender, SpiceSurface e)
    {
        dispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, () =>
        {
            if ((e.flags & Spice.SPICE_SURFACE_FLAGS_PRIMARY) == 1)
            {
                RootCanvas.Width = e.width;
                RootCanvas.Height = e.height;
            }
        });

        var surface = new CanvasRenderTarget(canvasDevice, e.width, e.height, 96);

        using (var ds = surface.CreateDrawingSession())
            ds.Clear(Colors.Black);

        surfaces.TryAdd(e.surface_id, surface);
    }

    private void Display_SurfaceDestroy(object? sender, uint e)
    {
        if (surfaces.TryRemove(e, out var surface))
            surface.Dispose();
    }

    private void Display_SurfaceDrawCopy(object? sender, SurfaceDrawCopyArgs e)
    {
        if (surfaces.TryGetValue(e.Display.surface_id, out var surface))
        {
            if (bitmaps.TryGetValue(e.ImageDescriptor.id, out var bitmap))
            {
                if (e.Image.Length > 0)
                    bitmap.SetPixelBytes(e.Image);
            }
            else
            {
                if (e.Image.Length == 0)
                    return;

                bitmap = CanvasBitmap.CreateFromBytes(surface, e.Image, (int)e.ImageDescriptor.width, (int)e.ImageDescriptor.height, DirectXPixelFormat.B8G8R8A8UIntNormalized);

                if (e.ImageDescriptor.flags.HasFlag(SpiceImageFlags.SPICE_IMAGE_FLAGS_CACHE_ME))
                    bitmaps.Add(e.ImageDescriptor.id, bitmap);
            }

            var rect = e.Display.box.ToRect();

            using (var ds = surface.CreateDrawingSession())
            {
                if (e.ClipRects.Count > 0)
                {
                    var layers = new CanvasGeometry[e.ClipRects.Count];

                    for (int i = 0; i < e.ClipRects.Count; i++)
                    {
                        var clipRect = e.ClipRects[i];
                        layers[i] = CanvasGeometry.CreateRectangle(surface, clipRect.ToRect());
                    }

                    using var group = CanvasGeometry.CreateGroup(surface, layers);
                    using var layer = ds.CreateLayer(1.0f, group);

                    ds.DrawImage(bitmap, rect, e.Copy.src_area.ToRect());
                }
                else
                {
                    ds.DrawImage(bitmap, rect, e.Copy.src_area.ToRect());
                }
            }
        }
    }

    private void Display_SurfaceCopyBits(object? sender, SurfaceCopyBitsArgs e)
    {
        if (surfaces.TryGetValue(e.Display.surface_id, out var surface))
        {
            var rect = e.Display.box.ToRect();

            using var bitmap = new CanvasRenderTarget(surface, (float)rect.Width, (float)rect.Height);

            bitmap.CopyPixelsFromBitmap(surface, 0, 0, e.Point.x, e.Point.y, (int)rect.Width, (int)rect.Height);

            using (var ds = surface.CreateDrawingSession())
            {
                if (e.ClipRects.Count > 0)
                {
                    var layers = new CanvasGeometry[e.ClipRects.Count];

                    for (int i = 0; i < e.ClipRects.Count; i++)
                    {
                        var clipRect = e.ClipRects[i];
                        layers[i] = CanvasGeometry.CreateRectangle(surface, clipRect.ToRect());
                    }

                    using var group = CanvasGeometry.CreateGroup(surface, layers);
                    using var layer = ds.CreateLayer(1.0f, group);

                    ds.DrawImage(bitmap, rect, bitmap.Bounds);
                }
                else
                {
                    ds.DrawImage(bitmap, rect, bitmap.Bounds);
                }
            }
        }
    }

    private void Display_SurfaceDrawFill(object? sender, SurfaceDrawFill e)
    {
        if (surfaces.TryGetValue(e.Display.surface_id, out var surface))
        {
            using (var ds = surface.CreateDrawingSession())
            {
                var rawColor = e.Color & 0xffffff;
                var color = Color.FromArgb(0xff, (byte)(rawColor >> 16), (byte)((rawColor >> 8) & 0xff), (byte)(rawColor & 0xff));
                if (e.ClipRects.Count > 0)
                {
                    var layers = new CanvasGeometry[e.ClipRects.Count];

                    for (int i = 0; i < e.ClipRects.Count; i++)
                    {
                        var clipRect = e.ClipRects[i];
                        layers[i] = CanvasGeometry.CreateRectangle(surface, clipRect.ToRect());
                    }

                    using var group = CanvasGeometry.CreateGroup(surface, layers);

                    ds.FillGeometry(group, color);
                }
                else
                {
                    ds.FillRectangle(e.Display.box.ToRect(), color);
                }

            }
        }
    }

    private void Display_SurfaceInvalidateList(object? sender, List<ulong> e)
    {
        foreach (var id in e)
            if (bitmaps.Remove(id, out var bitmap))
                bitmap.Dispose();

        GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized, false, false);
    }

    private void RootCanvas_Draw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
    {
        foreach (var surface in surfaces)
            args.DrawingSession.DrawImage(surface.Value);
    }

    private void FitCanvas()
    {
        if (RootCanvas.ActualWidth == 0 || RootCanvas.ActualHeight == 0)
            return;
        float zoomFactor = 1;

        switch (Data.FitMode)
        {
            case FitMode.OneToOne:
                zoomFactor = 1;
                break;
            case FitMode.FitToSize:
                zoomFactor = (float)Math.Min(ScrollViewer.ViewportWidth / RootCanvas.ActualWidth, ScrollViewer.ViewportHeight / RootCanvas.ActualHeight);
                break;
        }

        ScrollViewer.ChangeView(null, null, zoomFactor, true);
    }

    private void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e) => FitCanvas();

    private void RootCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (Data.AutoResizeViewer)
            ResizeToGuest();
        FitCanvas();
    }

    private void ResizeToGuest()
    {
        var height = RootCanvas.ActualHeight + ScrollViewer.BorderThickness.Top + ScrollViewer.BorderThickness.Bottom + Titlebar.ActualHeight + StatusBar.ActualHeight + 1 * XamlRoot.RasterizationScale;
        applicationView.TryResizeView(new Size(RootCanvas.Width, height));
    }

    #endregion

    #region Inputs

    private async void SyncKeyModifiers(object? sender, InputKeyModifiers e)
    {
        var states = await dispatcherQueue.EnqueueAsync(() =>
        {
            var scrollLock = coreWindow.GetKeyState(VirtualKey.Scroll).HasFlag(CoreVirtualKeyStates.Locked);
            var numLock = coreWindow.GetKeyState(VirtualKey.NumberKeyLock).HasFlag(CoreVirtualKeyStates.Locked);
            var capsLock = coreWindow.GetKeyState(VirtualKey.CapitalLock).HasFlag(CoreVirtualKeyStates.Locked);

            return (scrollLock, numLock, capsLock);
        }, DispatcherQueuePriority.High);

        if (states.scrollLock != e.ScrollLock)
        {
            Data.Channel!.Inputs!.KeyDown(0x46);
            Data.Channel.Inputs.KeyUp(0x46);
        }

        if (states.numLock != e.NumLock)
        {
            Data.Channel!.Inputs!.KeyDown(0x45);
            Data.Channel.Inputs.KeyUp(0x45);
        }

        if (states.capsLock != e.CapsLock)
        {
            Data.Channel!.Inputs!.KeyDown(0x3a);
            Data.Channel.Inputs.KeyUp(0x3a);
        }
    }

    private void RootCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (Data.Channel?.Inputs?.Ready != true || Data!.Channel!.CurrentMouseMode == Spice.SPICE_MOUSE_MODE_SERVER)
            return;
        var point = e.GetCurrentPoint(RootCanvas);

        cursorVisual.Offset = new Vector3((float)point.Position.X, (float)point.Position.Y, 0);

        Data.Channel.Inputs.MouseMove((uint)point.Position.X, (uint)point.Position.Y, 0);

        e.Handled = true;
    }

    private void MouseDevice_MouseMoved(MouseDevice sender, MouseEventArgs args)
    {
        if (Data.Channel?.Inputs?.Ready != true || Data!.Channel!.CurrentMouseMode == Spice.SPICE_MOUSE_MODE_CLIENT || !mouseLocked)
            return;

        var delta = args.MouseDelta;
        Data.Channel.Inputs.MouseMove(delta.X, delta.Y);
    }

    private void RootCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (Data.Channel?.Inputs?.Ready != true)
            return;

        RootCanvas.Focus(FocusState.Programmatic);

        if (!mouseLocked && Data.Channel.CurrentMouseMode == Spice.SPICE_MOUSE_MODE_SERVER)
        {
            mouseLocked = true;
            coreWindow.PointerCursor = null;
        }

        var point = e.GetCurrentPoint(RootCanvas);
        byte button = 1;
        ushort buttonState = 1 << 0;

        switch (point.Properties.PointerUpdateKind)
        {
            case PointerUpdateKind.LeftButtonPressed:
                break;
            case PointerUpdateKind.RightButtonPressed:
                button = 3;
                buttonState = 1 << 2;
                break;
            case PointerUpdateKind.MiddleButtonPressed:
                button = 2;
                buttonState = 1 << 1;
                break;
            case PointerUpdateKind.XButton1Pressed:
                break;
            case PointerUpdateKind.XButton2Pressed:
                break;
        }

        Data.Channel.Inputs.MouseDown(button, buttonState);

        e.Handled = true;
    }

    private void RootCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (Data.Channel?.Inputs?.Ready != true)
            return;

        var point = e.GetCurrentPoint(RootCanvas);
        byte button = 1;
        ushort buttonState = 0;

        switch (point.Properties.PointerUpdateKind)
        {
            case PointerUpdateKind.LeftButtonReleased:
                break;
            case PointerUpdateKind.RightButtonReleased:
                button = 3;
                break;
            case PointerUpdateKind.MiddleButtonReleased:
                button = 2;
                break;
            case PointerUpdateKind.XButton1Released:
                break;
            case PointerUpdateKind.XButton2Released:
                break;
        }

        Data.Channel.Inputs.MouseUp(button, buttonState);

        e.Handled = true;
    }

    private void RootCanvas_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
        if (Data.Channel?.Inputs?.Ready != true)
            return;

        var point = e.GetCurrentPoint(RootCanvas);

        var delta = point.Properties.MouseWheelDelta;

        byte button = (byte)(delta < 0 ? SpiceMouseButton.SPICE_MOUSE_BUTTON_DOWN : SpiceMouseButton.SPICE_MOUSE_BUTTON_UP);
        ushort buttonState = 0;

        Data.Channel.Inputs.MouseDown(button, buttonState);

        Data.Channel.Inputs.MouseUp(button, buttonState);

        e.Handled = true;
    }

    private void RootCanvas_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (Data.Channel?.Inputs?.Ready != true)
            return;

        if (e.Key == VirtualKey.U && coreWindow.GetKeyState(VirtualKey.LeftMenu).HasFlag(CoreVirtualKeyStates.Down) && coreWindow.GetKeyState(VirtualKey.LeftControl).HasFlag(CoreVirtualKeyStates.Down))
        {
            mouseLocked = false;
            coreWindow.PointerCursor = new(CoreCursorType.Arrow, 0);
        }

        Data.Channel.Inputs.KeyDown(GetScancode(e.Key, e.KeyStatus));
        e.Handled = true;
    }

    private void RootCanvas_KeyUp(object sender, KeyRoutedEventArgs e)
    {
        if (Data.Channel?.Inputs?.Ready != true)
            return;

        Data.Channel.Inputs.KeyUp(GetScancode(e.Key, e.KeyStatus));
        e.Handled = true;
    }

    private uint GetScancode(VirtualKey key, CorePhysicalKeyStatus status)
    {
        var scancode = status.ScanCode;

        if (status.IsExtendedKey)
            scancode = 0xE0 | (scancode << 8);

        switch (key)
        {
            case VirtualKey.Tab:
                scancode = 15u;
                break;
        }

        return scancode;
    }

    #endregion

    #region Cursor

    private void Cursor_Move(object? sender, SpicePoint16 e)
    {
        if (Data!.Channel!.CurrentMouseMode != Spice.SPICE_MOUSE_MODE_SERVER)
            return;

        cursorVisual.Offset = new Vector3(e.x, e.y, 0);
    }

    private void Cursor_Set(object? sender, CursorSet e)
    {
        cursorVisual.IsVisible = e.Visible;

        if (!e.Visible)
            return;

        using var ds = CanvasComposition.CreateDrawingSession(cursorSurface);
        CanvasBitmap bitmap;

        if (cursors.TryGetValue(e.Header.unique, out var set))
        {
            if (e.Image.Length > 0)
                set.Item1.SetPixelBytes(e.Image);

            cursorVisual.AnchorPoint = set.Item2;

            bitmap = set.Item1;
        }
        else
        {
            if (e.Image.Length == 0)
                return;

            bitmap = CanvasBitmap.CreateFromBytes(ds, e.Image, e.Header.width, e.Header.height, DirectXPixelFormat.B8G8R8A8UIntNormalized);

            var anchorPoint = new Vector2(e.Header.hot_spot_x / cursorVisual.Size.X, e.Header.hot_spot_y / cursorVisual.Size.Y);

            cursorVisual.AnchorPoint = anchorPoint;

            cursors.Add(e.Header.unique, (bitmap, anchorPoint));
        }

        ds.Clear(Colors.Transparent);
        ds.DrawImage(bitmap);
    }

    private void Cursor_InvalidateOne(object? sender, ulong e)
    {
        if (cursors.Remove(e, out var bitmap))
            bitmap.Item1.Dispose();
    }

    private void Cursor_InvalidateAll(object? sender, EventArgs e)
    {
        var tmp = cursors.ToList();
        cursors.Clear();
        if (tmp.Count > 0)
            foreach (var bitmap in tmp)
                bitmap.Value.Item1.Dispose();
    }

    private void Cursor_Hide(object? sender, EventArgs e)
    {
        cursorVisual.IsVisible = false;
    }

    private void RootCanvas_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (!(mouseLocked || Data?.Channel?.CurrentMouseMode != Spice.SPICE_MOUSE_MODE_SERVER))
            return;
        coreWindow.PointerCursor = null;
    }

    private void RootCanvas_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        coreWindow.PointerCursor = new(CoreCursorType.Arrow, 0);
    }

    private void RootCanvas_CreateResources(CanvasAnimatedControl sender, CanvasCreateResourcesEventArgs args)
    {
        canvasDevice = RootCanvas.Device;

        var compositor = ElementCompositionPreview.GetElementVisual(RootCanvas).Compositor;

        var canvasComposition = CanvasComposition.CreateCompositionGraphicsDevice(compositor, RootCanvas.Device);

        cursorSurface = canvasComposition.CreateDrawingSurface(new Size(128, 128), DirectXPixelFormat.R8G8B8A8UIntNormalized, DirectXAlphaMode.Premultiplied);

        using (var ds = CanvasComposition.CreateDrawingSession(cursorSurface))
        {
            ds.Clear(Colors.Transparent);
        }

        var brush = compositor.CreateSurfaceBrush(cursorSurface);
        cursorVisual.Brush = brush;
        cursorVisual.Size = new Vector2(128, 128);

        canvasReady.SetResult();
    }

    #endregion

    #region Playback

    private void Playback_Mode(object? sender, SpiceAudioDataMode e) => audioDataMode = e;

    private void Playback_Start(object? sender, SpiceMsgRecordStart e)
    {
        var properties = AudioEncodingProperties.CreatePcm(e.frequency, e.channels, 16);
        //if (audioDataMode == SpiceAudioDataMode.SPICE_AUDIO_DATA_MODE_OPUS)
        //properties.Subtype = "OPUS";

        frameInputNode = graph.CreateFrameInputNode(properties);
        frameInputNode.AddOutgoingConnection(deviceOutputNode);
    }

    private void Playback_Data(object? sender, PlaybackData e)
    {
        var frame = new AudioFrame((uint)e.Data.Length * sizeof(short));

        using (var buffer = frame.LockBuffer(AudioBufferAccessMode.Write))
        using (var reference = buffer.CreateReference())
        {
            WindowsRuntimeMarshal.TryGetDataUnsafe(reference, out var dataInBytes, out var capacityInBytes);
            Marshal.Copy(e.Data, 0, dataInBytes, e.Data.Length);
        }

        frame.RelativeTime = TimeSpan.FromMilliseconds(e.Time);

        try
        {
            frameInputNode?.AddFrame(frame);
        }
        catch
        {
            frameInputNode?.DiscardQueuedFrames();
        }
    }

    private void Playback_Stop(object? sender, EventArgs e)
    {
        frameInputNode?.Stop();
        frameInputNode?.Dispose();
        frameInputNode = null;
    }

    #endregion

}
