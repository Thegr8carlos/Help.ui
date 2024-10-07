namespace Help.ui;
using Android.Content;
public partial class MainPage : ContentPage
{

	public MainPage()
	{
		InitializeComponent();
        // checks the stored value in the system if not exists then sets to false
        AssistanOn.IsToggled = (Application.Current as App)?.IsAssistantActive ?? false;
        SetIsVoiceEnabled((Application.Current as App)?.IsVoiceEnabled ?? true);
    }

    void SwitchChanged(object sender, ToggledEventArgs e)
    {
        if (e.Value)
        {
            // must display the button
            Preferences.Set("IsAssistantActive", e.Value);
            SetStateMessage(e.Value);
            StartFloatingButtonService();
        }
        else
        {
            //must delete the button
            Preferences.Set("IsAssistantActive", e.Value);
            SetStateMessage(e.Value);
            StopFloatingButtonService();
        }

    }

    private void SetStateMessage(bool state)
    {
        if (state)
        {
            StateMessage.Text = "Activado";
            StateMessage.TextColor = Colors.Purple;
        }else
        {
            StateMessage.Text = "Desactivado";
            StateMessage.TextColor = Colors.Black;
        }
    }

    private void SetIsVoiceEnabled(bool enabled)
    {
        if (enabled)
        {
            AppMode.Source = "voice.svg";
        }else
        {
            AppMode.Source = "text.svg";
        }
    }
    private void StartFloatingButtonService()
    {
        // starts service
        var intent = new Intent(Android.App.Application.Context, typeof(FloatingButtonService));
        Android.App.Application.Context.StartService(intent);
        //DisplayAlert("Estado del asistente", "Servicio activado", "OK");
    }

    private void StopFloatingButtonService()
    {
        //stops the service
        var intent = new Intent(Android.App.Application.Context, typeof(FloatingButtonService));
        Android.App.Application.Context.StopService(intent);
        //DisplayAlert("Estado del asistente", "Servicio desactivado.", "OK");
    }

    private void OnModeTapped(object sender, EventArgs e)
    {

        (Application.Current as App).IsVoiceEnabled = !(Application.Current as App).IsVoiceEnabled;

        Preferences.Set("IsVoiceEnabled", (Application.Current as App).IsVoiceEnabled);

        SetIsVoiceEnabled((Application.Current as App).IsVoiceEnabled);
    }

    private void OnSettingsTapped(object sender, EventArgs e)
    {
        //
    }

}

