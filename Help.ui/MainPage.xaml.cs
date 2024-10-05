namespace Help.ui;
 
public partial class MainPage : ContentPage
{

	public MainPage()
	{
		InitializeComponent();
        AssistanOn.IsToggled = (Application.Current as App)?.IsAssistantActive ?? false;


    }

    void SwitchChanged(object sender, ToggledEventArgs e)
    {
        if (e.Value)
        {
            // must display the button
            Preferences.Set("IsAssistantActive", e.Value);
            DisplayAlert("Estado del asistente", "Asistente activado.", "OK");
        }
        else
        {
            //must delete the button
            Preferences.Set("IsAssistantActive", e.Value);
            DisplayAlert("Estado del asistente", "Asistente desactivado.", "OK");
        }
    }

}

