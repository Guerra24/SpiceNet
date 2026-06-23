using SpiceNet.UWP.Models;

namespace SpiceNet.UWP.Views.Dialogs;

public sealed partial class ServerProfileDialog : ContentDialog
{

    public ServerProfileDialog(bool edit)
    {
        this.InitializeComponent();
        //RequestedTheme = Service.Platform.Theme.ToXamlTheme();
        ProfileFitMode.Items.Add("Center");
        ProfileFitMode.Items.Add("Scale to fit");

        if (edit)
            PrimaryButtonText = "Save";
        else
            FitMode = FitMode.Center;
    }

    public string ProfName { get => ProfileName.Text; set => ProfileName.Text = value; }
    public string Address { get => ProfileAddress.Text; set => ProfileAddress.Text = value; }
    public int Port { get => (int)ProfilePort.Value; set => ProfilePort.Value = value; }
    public string Password { get => ProfilePassword.Password; set => ProfilePassword.Password = value; }
    public bool AutoResizeGuest { get => ProfileAutoResizeGuest.IsOn; set => ProfileAutoResizeGuest.IsOn = value; }
    public bool AutoResizeViewer { get => ProfileAutoResizeViewer.IsOn; set => ProfileAutoResizeViewer.IsOn = value; }
    public FitMode FitMode { get => (FitMode)ProfileFitMode.SelectedIndex; set => ProfileFitMode.SelectedIndex = (int)value; }

    private void ProfileName_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
    {
        bool allow = true;
        ProfileError.Text = "";
        if (string.IsNullOrEmpty(ProfileName.Text))
        {
            ProfileError.Text = "Empty profile name";
            allow = false;
        }
        IsPrimaryButtonEnabled = allow && ValidateServerAddress() && ValidatePort();
    }

    private void ProfileServerAddress_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
    {
        bool allow = true;
        ProfileError.Text = "";
        if (string.IsNullOrEmpty(ProfileAddress.Text))
        {
            ProfileError.Text = "Empty address";
            allow = false;
        }
        IsPrimaryButtonEnabled = allow && ValidateProfileName() && ValidatePort();
    }

    private void ProfilePort_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if (ProfileError == null)
            return;
        bool allow = true;
        ProfileError.Text = "";
        if (double.IsNaN(args.NewValue))
        {
            ProfileError.Text = "Invalid port";
            allow = false;
        }
        IsPrimaryButtonEnabled = allow && ValidateProfileName() && ValidateServerAddress();
    }

    private bool ValidateProfileName()
    {
        return !string.IsNullOrEmpty(ProfileName.Text);
    }

    private bool ValidateServerAddress()
    {
        return !string.IsNullOrEmpty(ProfileAddress.Text);
    }

    private bool ValidatePort()
    {
        return Port >= 0 && Port <= 65535;
    }

}
