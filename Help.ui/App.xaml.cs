namespace Help.ui;
using Microsoft.Maui.Storage; // used to store if the asssistant is still active
public partial class App : Application
{
    public bool IsAssistantActive { get; set; }
    public bool IsVoiceEnabled { get; set; }
    public App()
	{
		InitializeComponent();
        IsAssistantActive = Preferences.Get("IsAssistantActive", false);
        IsVoiceEnabled = Preferences.Get("IsVoiceEnabled",true);
        MainPage = new AppShell();
	}
}
