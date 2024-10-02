namespace Help.ui;

public partial class MainPage : ContentPage
{
	int count = 0;

	public MainPage()
	{
		InitializeComponent();
	}

	private void OnCounterClicked(object sender, EventArgs e)
	{
		count++;

		if (count == 1)
			CounterBtn.Text = $"Presionaste {count} veces";
		else
			CounterBtn.Text = $"prsionado {count} veces";

		SemanticScreenReader.Announce(CounterBtn.Text);
	}
}

