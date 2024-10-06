namespace Help.ui;
using Android.Content;
public partial class MainPage : ContentPage
{

	public MainPage()
	{
		InitializeComponent();
        // checks the stored value in the system if not exists then sets to false
        AssistanOn.IsToggled = (Application.Current as App)?.IsAssistantActive ?? false; 


    }

    void SwitchChanged(object sender, ToggledEventArgs e)
    {
        if (e.Value)
        {
            // must display the button
            Preferences.Set("IsAssistantActive", e.Value);
            StartFloatingButtonService();
        }
        else
        {
            //must delete the button
            Preferences.Set("IsAssistantActive", e.Value);
            StopFloatingButtonService();
        }
    }

    private void StartFloatingButtonService()
    {
        // Crear un intent para iniciar el servicio
        var intent = new Intent(Android.App.Application.Context, typeof(FloatingButtonService));
        Android.App.Application.Context.StartService(intent);
        DisplayAlert("Estado del asistente", "Servicio activado", "OK");
    }

    private void StopFloatingButtonService()
    {
        // Crear un intent para detener el servicio
        var intent = new Intent(Android.App.Application.Context, typeof(FloatingButtonService));
        Android.App.Application.Context.StopService(intent);
        DisplayAlert("Estado del asistente", "Servicio desactivado.", "OK");
    }

}

