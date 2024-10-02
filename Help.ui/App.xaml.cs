namespace Help.ui;

public partial class App : Application
{
	public static bool buttonActive { get; set; } = false; // variable to store if the button is activate or not
	public App()
	{
		InitializeComponent();

		MainPage = new AppShell();
	}
}
