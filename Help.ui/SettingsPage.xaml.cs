namespace Help.ui
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();
        }
        public async void OnBackButtonClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("///MainPage");
        }

        public async void OnAceptarCambiosClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Configuraciones", "Configuraciones Guardadas.", "OK");
            await Shell.Current.GoToAsync("///MainPage");
        }
    }
}
