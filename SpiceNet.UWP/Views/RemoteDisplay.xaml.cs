using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI.Xaml;
using SpiceNet.Protocol;
using SpiceNet.UWP.Extensions;
using SpiceNet.UWP.Models;
using SpiceNet.UWP.ViewModels;
using Windows.Graphics.DirectX;
using Windows.UI.ViewManagement;

namespace SpiceNet.UWP.Views;


public sealed partial class RemoteDisplay : Page
{
    private DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

    private RemoteDisplayViewModel Data;

    private Dictionary<uint, CanvasRenderTarget> surfaces = new();

    private Dictionary<ulong, CanvasBitmap> bitmaps = new();

    private ApplicationView applicationView;

    public RemoteDisplay(string address, int port)
    {
        InitializeComponent();
        Data = new(address, port);
        Data.FitModeChanged += (sender, e) => FitCanvas();

        applicationView = ApplicationView.GetForCurrentView();
        applicationView.Consolidated += ApplicationView_Consolidated;

        Window.Current.SetTitleBar(Titlebar);

        Data.Channel.DisplayInit += DisplayInit;
    }

    private async void ApplicationView_Consolidated(ApplicationView sender, ApplicationViewConsolidatedEventArgs args)
    {

        // Fix crash when vm shuts off
        // Keyboard modifiers are inverted in the vm, needs to sync that at start
        // Keyboard modifiers hang pressed when switch from gaphical to console
        // Console serial view is missing one line at the bottom

        Data.Channel.Dispose();
        await RootCanvas.RunOnGameLoopThreadAsync(() =>
        {
            foreach (var surface in surfaces)
                surface.Value.Dispose();
            foreach (var bitmap in bitmaps)
                bitmap.Value.Dispose();
        });
        surfaces.Clear();
        bitmaps.Clear();
        RootCanvas.RemoveFromVisualTree();
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized, false, false);
        Window.Current.Close();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        Data.Channel.Init();
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

        _ = RootCanvas.RunOnGameLoopThreadAsync(() =>
        {
            var surface = new CanvasRenderTarget(RootCanvas, e.width, e.height, 96);

            using (var ds = surface.CreateDrawingSession())
                ds.Clear(Colors.Black);

            surfaces.Add(e.surface_id, surface);
        });
    }

    private void Display_SurfaceDestroy(object? sender, uint e)
    {
        _ = RootCanvas.RunOnGameLoopThreadAsync(() =>
        {
            if (surfaces.TryGetValue(e, out var surface))
            {
                surfaces.Remove(e);
                surface.Dispose();
            }
        });
    }

    private void Display_SurfaceDrawCopy(object? sender, SurfaceDrawCopyArgs e)
    {
        _ = RootCanvas.RunOnGameLoopThreadAsync(() =>
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

                    bitmap = CanvasBitmap.CreateFromBytes(surface, e.Image, (int)e.ImageDescriptor.width, (int)e.ImageDescriptor.height, DirectXPixelFormat.B8G8R8A8UIntNormalized, 96);

                    if ((e.ImageDescriptor.flags & Spice.SPICE_IMAGE_FLAGS_CACHE_ME) == 1)
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

                        using (var layer = ds.CreateLayer(1.0f, group))
                        {
                            ds.DrawImage(bitmap, rect, e.Copy.src_area.ToRect());
                        }
                    }
                    else
                    {
                        ds.DrawImage(bitmap, rect, e.Copy.src_area.ToRect());
                    }
                }
            }
        });
        /*var bitmap = CanvasBitmap.CreateFromBytes(sender, surfaceArgs.Image, (int)surfaceArgs.ImageDescriptor.width, (int)surfaceArgs.ImageDescriptor.height, DirectXPixelFormat.R8G8B8A8UIntNormalized);
        args.DrawingSession.DrawImage(bitmap);*/
    }

    private void Display_SurfaceCopyBits(object? sender, SurfaceCopyBitsArgs e)
    {
        _ = RootCanvas.RunOnGameLoopThreadAsync(() =>
        {
            if (surfaces.TryGetValue(e.Display.surface_id, out var surface))
            {
                var dest = e.Display.box.ToRect();

                using var target = new CanvasRenderTarget(surface, (float)dest.Width, (float)dest.Height);

                /*using (var ds = target.CreateDrawingSession())
                {
                    ds.Clear(Colors.Transparent);

                    ds.DrawImage(surface, new Rect(0, 0, dest.Width, dest.Height), new Rect(e.Point.x, e.Point.y, dest.Width, dest.Height));
                }*/

                target.CopyPixelsFromBitmap(surface, 0, 0, e.Point.x, e.Point.y, (int)dest.Width, (int)dest.Height);

                using (var ds = surface.CreateDrawingSession())
                {
                    ds.DrawImage(target, dest, target.Bounds);
                    /*foreach (var clip in e.ClipRects)
                    {
                        ds.DrawRectangle(clip.ToRect(), Colors.Red);
                    }*/
                }
            }
        });
    }

    private void Display_SurfaceDrawFill(object? sender, SurfaceDrawFill e)
    {
        _ = RootCanvas.RunOnGameLoopThreadAsync(() =>
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
        });
    }

    private void Display_SurfaceInvalidateList(object? sender, List<ulong> e)
    {
        foreach (var id in e)
            if (bitmaps.Remove(id, out var bitmap))
                _ = RootCanvas.RunOnGameLoopThreadAsync(() =>
                {
                    bitmap.Dispose();
                });

        GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized, false, false);
    }

    private void RootCanvas_Draw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
    {
        foreach (var surface in surfaces)
            args.DrawingSession.DrawImage(surface.Value);
    }

    private void RootCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (Data.Channel.Inputs == null)
            return;
        var point = e.GetCurrentPoint(RootCanvas);

        Data.Channel.Inputs.MouseMove((uint)point.Position.X, (uint)point.Position.Y, 0);

        e.Handled = true;
    }
    private void RootCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (Data.Channel.Inputs == null)
            return;

        RootCanvas.Focus(FocusState.Programmatic);

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
        if (Data.Channel.Inputs == null)
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
        if (Data.Channel.Inputs == null)
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
        if (Data.Channel.Inputs == null)
            return;

        Data.Channel.Inputs.KeyDown(GetScancode(e.Key, e.KeyStatus.ScanCode));
        e.Handled = true;
    }

    private void RootCanvas_KeyUp(object sender, KeyRoutedEventArgs e)
    {
        if (Data.Channel.Inputs == null)
            return;

        Data.Channel.Inputs.KeyUp(GetScancode(e.Key, e.KeyStatus.ScanCode));
        e.Handled = true;
    }

    private uint GetScancode(VirtualKey key, uint scancode)
    {
        switch (key)
        {
            case VirtualKey.Tab:
                scancode = 15u;
                break;
        }

        return scancode;
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

    // ui events
    private void ResizeToGuest()
    {
        var height = RootCanvas.ActualHeight + Titlebar.ActualHeight + StatusBar.ActualHeight + 1;
        applicationView.TryResizeView(new Size(RootCanvas.ActualWidth, height));
    }
}
