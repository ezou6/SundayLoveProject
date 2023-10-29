using SundayLoveProject.Models;
using System.Collections.ObjectModel;

namespace SundayLoveProject;

public partial class SettingsPage : ContentPage
{
    ObservableCollection<Customer> customers;
    List<Button> dayButtons;
    List<Button> textButtons;
    List<Button> photoButtons;
	public SettingsPage(ObservableCollection<Customer> customers)
	{
		InitializeComponent();
        this.customers = customers;
        dayButtons = new List<Button> { Days1Button, Days2Button, Days3Button, Days4Button, Days5Button };
        textButtons = new List<Button> { SmallButton, MediumButton, LargeButton };
        photoButtons = new List<Button> { LowResButton, MediumResButton, HighResButton };
        ResetDayButtonHighlights();
        ResetFontSizeButtonHighLights();
        ResetPhotoSizeButtonHighLights();
    }



    private async void Sync_Clicked(object sender, EventArgs e)
    {
        var answer = await DisplayAlert("Sync with Database", "This operation may take a few minutes. Be sure you have a wifi connection. Are you sure you want to sync with the database now?", "Yes", "No");
        if (!answer)
            return;

        SyncProgressBar.IsVisible = true;
        SyncProgressBar.Progress = 0;
        var totalOperations = customers.Count + 1;
        var operationsCompleted = 0;
        App.Firebase.UploadFileAsync(Constants.DatabasePath, Constants.DatabaseFilename);
        SyncProgressBar.Progress = operationsCompleted++/totalOperations;

        foreach (var customer in customers)
        {
            //download images from database if they aren't on local device
            if (customer.ImageUrl != Customer.NO_IMAGE_URL && !File.Exists(customer.ImageUrl))
                App.Firebase.DownloadFileAsync(@"images/" + Path.GetFileName(customer.ImageUrl), customer.ImageUrl);
            if (customer.IDImageUrl != Customer.NO_IMAGE_URL && !File.Exists(customer.IDImageUrl))
                App.Firebase.DownloadFileAsync(@"images/" + Path.GetFileName(customer.IDImageUrl), customer.IDImageUrl);

            //upload images to database if they aren't in firebase storage
            if (customer.ImageUrl != Customer.NO_IMAGE_URL)
                App.Firebase.SyncFileAsync(customer.ImageUrl, @"images/" + Path.GetFileName(customer.ImageUrl));
            if (customer.IDImageUrl != Customer.NO_IMAGE_URL)
                App.Firebase.SyncFileAsync(customer.IDImageUrl, @"images/" + Path.GetFileName(customer.IDImageUrl));
            
            SyncProgressBar.Progress = operationsCompleted++ / totalOperations;
        }
        await DisplayAlert("Sync Update", "Sync with firebase completed.", "Continue");
        SyncProgressBar.IsVisible = false;
    }

    private void TextSize_Button_Clicked(object sender, EventArgs e)
    {
        if ((sender as Button).Text == "Small")
        {
            Preferences.Default.Set("HeadingFontSize", 16);
            Preferences.Default.Set("TextFontSize", 14);
            App.Current.Resources["HeadingFontSize"] = 16;
            App.Current.Resources["TextFontSize"] = 14;
        }
        else if ((sender as Button).Text == "Medium")
        {
            Preferences.Default.Set("HeadingFontSize", 20);
            Preferences.Default.Set("TextFontSize", 18);
            App.Current.Resources["HeadingFontSize"] = 20;
            App.Current.Resources["TextFontSize"] = 18;
        }
        else if ((sender as Button).Text == "Large")
        {
            Preferences.Default.Set("HeadingFontSize", 24);
            Preferences.Default.Set("TextFontSize", 22);
            App.Current.Resources["HeadingFontSize"] = 24;
            App.Current.Resources["TextFontSize"] = 22;
        }
        ResetFontSizeButtonHighLights();
    }



    private void DaysSelected_Button_Clicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var success = int.TryParse(button.Text, out int days);
        if(success)
        {
            Customer.NUMBER_OF_DAYS_PER_WEEK_CAN_SHOP = days;
            Preferences.Default.Set("days_can_shop", days);
        }

        ResetDayButtonHighlights();
    }

    private void PhotoResolution_Button_Clicked(object sender, EventArgs e)
    {
        if ((sender as Button).Text == "Low (480 x 360)")
        {
            Preferences.Default.Set("photo_width", 360);
            Preferences.Default.Set("photo_height", 480);
        }
        else if ((sender as Button).Text == "Medium (720 x 540)")
        {
            Preferences.Default.Set("photo_width", 540);
            Preferences.Default.Set("photo_height", 720);
        }
        else if ((sender as Button).Text == "High (960 x 720)")
        {
            Preferences.Default.Set("photo_width", 720);
            Preferences.Default.Set("photo_height", 960);
        }
        PhotoUtility.photoWidth = Preferences.Default.Get("photo_width", 540);
        PhotoUtility.photoHeight = Preferences.Default.Get("photo_height", 720);
        ResetPhotoSizeButtonHighLights();
    }


    private void ResetDayButtonHighlights()
    {
        foreach (var b in dayButtons)
            b.BorderColor = Colors.Maroon;

        dayButtons[Customer.NUMBER_OF_DAYS_PER_WEEK_CAN_SHOP - 1].BorderColor = Colors.Gray;
    }

    private void ResetFontSizeButtonHighLights()
    {
        foreach (var b in textButtons)
            b.BorderColor = Colors.Maroon;

        var fontSize = Preferences.Default.Get("TextFontSize", 14);
        if (fontSize == 14)
            textButtons[0].BorderColor = Colors.Gray;
        else if (fontSize == 18)
            textButtons[1].BorderColor = Colors.Gray;
        else
            textButtons[2].BorderColor = Colors.Gray;
    }

    private void ResetPhotoSizeButtonHighLights()
    {
        foreach (var b in photoButtons)
            b.BorderColor = Colors.Maroon;

        var photoSize = Preferences.Default.Get("photo_width", 540);
        if (photoSize == 360)
            photoButtons[0].BorderColor = Colors.Gray;
        else if (photoSize == 540)
            photoButtons[1].BorderColor = Colors.Gray;
        else
            photoButtons[2].BorderColor = Colors.Gray;
    }
}