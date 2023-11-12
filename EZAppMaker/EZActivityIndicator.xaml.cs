namespace EZAppMaker;

public partial class EZActivityIndicator : ContentView
{
	public EZActivityIndicator()
	{
		BindingContext = this;
		InitializeComponent();
	}

	public void Show()
	{
		IsVisible = true;
		Spinner.IsSpinning = true;
	}

	public async void Hide()
	{
        await Task.Delay(500);

        IsVisible = false;
        Spinner.IsSpinning = false;
    }
}
