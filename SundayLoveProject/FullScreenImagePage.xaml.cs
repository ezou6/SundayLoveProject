namespace SundayLoveProject;

public partial class FullScreenImagePage : ContentPage
{
	
	public FullScreenImagePage(ImageSource image)
	{
		InitializeComponent();
		FullScreenImage.Source = image;
	}

    void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e) {
        // Handle the pinch
    }
}