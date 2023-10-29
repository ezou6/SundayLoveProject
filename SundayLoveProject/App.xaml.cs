using SundayLoveProject.Models;

namespace SundayLoveProject;

public partial class App : Application
{	
	public static CustomerDatabase Database { get; set; }
    public static FirebaseUtility Firebase { get; set; }
    public App()
	{
		InitializeComponent();
		App.Current.Resources["HeadingFontSize"] = Preferences.Default.Get("HeadingFontSize", 20); ;
        App.Current.Resources["TextFontSize"] = Preferences.Default.Get("TextFontSize", 18);
		App.Current.Resources["days_can_shop"] = Preferences.Default.Get("days_can_shop", 1);
		Customer.NUMBER_OF_DAYS_PER_WEEK_CAN_SHOP = Preferences.Default.Get("days_can_shop", 1);
		PhotoUtility.photoWidth = Preferences.Default.Get("photo_width", 540);
		PhotoUtility.photoHeight = Preferences.Default.Get("photo_height", 720);
        Database = new CustomerDatabase();
		Firebase = new FirebaseUtility();
		MainPage = new NavigationPage(new CustomerSearchPage());

    }

}
