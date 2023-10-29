namespace SundayLoveProject;

using SundayLoveProject.Models;
using System.Collections.ObjectModel;

/// <summary>
/// Represents a page for viewing customer information.
/// </summary>
public partial class CustomerViewPage : ContentPage {
    private Customer customer;
    private Button[] buttons;

    private static Color TODAY_OUTLINE_COLOR = Colors.DarkRed;
    private static Color SHOPPED_COLOR = Colors.Maroon;
    private static Color TODAY_NOT_SHOPPED_COLOR = Colors.Gray;
    private static Color PAST_NOT_SHOPPED_COLOR = Colors.DarkGray;
    private static Color FUTURE_NOT_SHOPPED_COLOR = Color.FromArgb("979171");


    /// <summary>
    /// Initializes a new instance of the <see cref="CustomerViewPage"/> class.
    /// </summary>
    /// <param name="customer">The customer object.</param>
    public CustomerViewPage(Customer customer) {
        InitializeComponent();
        BindingContext = customer;
        Title = "Profile for " + customer.Name;
        this.customer = customer;
        buttons = new Button[] { MondayButton, TuesdayButton, WednesdayButton, ThursdayButton, FridayButton };
    }

    protected override void OnAppearing() {
        base.OnAppearing();

        setup();
    }

    /// <summary>
    /// Sets up the page with customer information and button colors.
    /// </summary>
    public async void setup() {
        if (customer == null) return;

        this.customer.CalculateEligibility(); //reset for the new week
        //get all the dates in the week
        //customer dates list
        var today = DateTime.Today;
        int dayNumber = (int)today.DayOfWeek - 1;
        var day = today.AddDays(-dayNumber);

        //sort list before taking last two
        var copy = new ObservableCollection<DateTime>(customer.DatesShopped.OrderBy(date => date));
        customer.DatesShopped.Clear();
        foreach (var d in copy) {
            customer.DatesShopped.Add(d);
        }

        var lastDatesShopped = customer.DatesShopped.TakeLast(Customer.NUMBER_OF_DAYS_PER_WEEK_CAN_SHOP).ToList();
        
        
        Color backgroundColor = null; //default
        foreach (var button in buttons) {
            if (lastDatesShopped.Contains(day))
                backgroundColor = SHOPPED_COLOR;
            else if (today == day)
                backgroundColor = TODAY_NOT_SHOPPED_COLOR; //current day
            else if (today > day)
                backgroundColor = PAST_NOT_SHOPPED_COLOR; //past days
            else if (today < day)
                backgroundColor = FUTURE_NOT_SHOPPED_COLOR; //future days

            button.BackgroundColor = backgroundColor;
  
       

            button.TextColor = Colors.Black; 
            button.Text = day.Date.ToString("dddd") + " " + day.Date.ToString("M/d");

            if (today == day) //highlight current day 
            {
                button.BorderColor = TODAY_OUTLINE_COLOR;
                button.BorderWidth = 4;
            }

            day = day.AddDays(1);
        }

        NameLabel.Text = "Name: " + customer.Name;
        GenderLabel.Text = "Gender: " + customer.Gender;
        CommentLabel.Text = "Comment: " + customer.CustomerComment;
        if (customer.DateOfBirth != null)
            DOBLabel.Text = "Birthday: " + customer.DateOfBirth.Value.ToString("MM/dd/yy");
        else
            DOBLabel.Text = "Birthday: N/A";
        DaysShoppedLabel.Text = "Days shopped: " + customer.DatesShopped.Count;
        IsEligibleLabel.Text = "Eligible to shop: " + (customer.IsEligible ? "Yes" : "No");
        if (customer.IsEligible == false)
        {
            await DisplayAlert("Alert", "Customer is not Eligible to Shop", "OK");
        }
    }

    /// <summary>
    /// Handles the click event of the edit button.
    /// </summary>
    /// <param name="sender">The sender object.</param>
    /// <param name="e">The event arguments.</param>
    private async void EditButtonClicked(object sender, EventArgs e)
    {
        //bool answer = await DisplayAlert("", "Would you like to edit this customer's information?", "Ok", "Cancel");
        bool answer = true;
        if (answer)
        {
            var page = new CustomerDetailPage(customer);
            page.CustomerSaved += async (source, customerCopy) => 
            {
                customer.Name = customerCopy.Name;
                customer.Gender = customerCopy.Gender;
                customer.DateOfBirth = customerCopy.DateOfBirth;
                customer.ZipCode = customerCopy.ZipCode;
                customer.CustomerComment = customerCopy.CustomerComment;
                customer.IsEligible = customerCopy.IsEligible;

                if (customer.ImageUrl != customerCopy.ImageUrl) //new photo
                {
                    File.Delete(customer.ImageUrl); //Delete old photo
                    var dirPath = Path.Combine(FileSystem.AppDataDirectory, "" + customer.ID);
                    if (!Directory.Exists(dirPath))
                        Directory.CreateDirectory(dirPath);
                    var newFileName = Path.GetFileName(customerCopy.ImageUrl);
                    var newFilePath = Path.Combine(dirPath, newFileName);
                    File.Move(customerCopy.ImageUrl, newFilePath, true); //Move cached photo to app data
                    customer.ImageUrl = newFilePath; //Save new path
                    App.Firebase.UploadFileAsync(customer.ImageUrl, @"images/" + newFileName);                    
                }
                if (customer.IDImageUrl != customerCopy.IDImageUrl) //new ID photo
                {
                    File.Delete(customer.IDImageUrl); //Delete old photo
                    var dirPath = Path.Combine(FileSystem.AppDataDirectory, "" + customer.ID);
                    if (!Directory.Exists(dirPath))
                        Directory.CreateDirectory(dirPath);
                    var newFileName = Path.GetFileName(customerCopy.IDImageUrl);
                    var newFilePath = Path.Combine(dirPath, newFileName);
                    File.Move(customerCopy.IDImageUrl, newFilePath, true); //Move cached photo to app data
                    customer.IDImageUrl = newFilePath; //Save new path
                    App.Firebase.UploadFileAsync(customer.IDImageUrl, @"images/" + newFileName);
                }
                customer.DatesShopped.Clear();
                foreach (var d in customerCopy.DatesShopped) {
                    customer.DatesShopped.Add(d);
                }

                //add to database
                await App.Database.SaveCustomerAsync(customer);
                //backup database to Firebase
                App.Firebase.UploadFileAsync(Constants.DatabasePath, Constants.DatabaseFilename);

            };
            Navigation.PushAsync(page);
        }
    }

    /// <summary>
    /// Handles the click event of a Day Button.
    /// </summary>
    /// <param name="sender">The sender object.</param>
    /// <param name="e">The event arguments.</param>
    private async void ButtonClicked(object sender, EventArgs e)
    {
        var buttonClicked = sender as Button;
        for (int i = 0; i < buttons.Length; i++) {
            if (buttons[i] == buttonClicked && (int) DateTime.Today.DayOfWeek == i+1) //date of button clicked must be today  
            {
                if (customer.IsEligible&&!customer.ShoppedToday()) {
                    customer.AddDateShopped(DateTime.Today);
                    buttons[i].BackgroundColor = SHOPPED_COLOR;
                }
                else {
                    customer.RemoveDateShopped(DateTime.Today);
                    buttons[i].BackgroundColor = TODAY_NOT_SHOPPED_COLOR;
                }

                //add to database
                await App.Database.SaveCustomerAsync(customer);
            }
        }
        IsEligibleLabel.Text = "Eligible to shop: "+ (customer.IsEligible ? "Yes" : "No");
        
    }

    private async void Image_Clicked(object sender, EventArgs e) {
        if (sender is ImageButton imageClicked) {
            // Create a new FullScreenImagePage and pass the image source
            var fullScreenImagePage = new FullScreenImagePage(imageClicked.Source);

            // Navigate to the FullScreenImagePage
            await Navigation.PushAsync(fullScreenImagePage);
        }

    }
}