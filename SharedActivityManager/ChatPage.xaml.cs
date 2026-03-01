namespace SharedActivityManager;

public partial class ChatPage : ContentPage
{
	public ChatPage()
	{
		InitializeComponent();
	}

	private async void OnGoBack(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("//MainPage");
    }
}