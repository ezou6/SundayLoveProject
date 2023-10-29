namespace SundayLoveProject;
using SundayLoveProject.Models;

/// <summary>
/// Represents a partial class for the customer detail page.
/// </summary>
public partial class CustomerDetailPage : ContentPage
{

    /// <summary>
    /// Event triggered when a customer is saved.
    /// </summary>
    public event EventHandler<Customer> CustomerSaved;
    /// <summary>
    /// Gets or sets the customer object.
    /// </summary>
    public Customer customer { get; set; }

    public bool newIDImage, newImage;
    public string oldIDImageURL, oldImageURL;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomerDetailPage"/> class.
    /// </summary>
    /// <param name="customer">The customer object.</param>
    public CustomerDetailPage(Customer customer) {
        InitializeComponent();

        this.customer = new Customer(customer);
        newIDImage = false;
        newImage = false;
        
        //default birthday
        if (this.customer.DateOfBirth == null) 
            this.customer.DateOfBirth = new DateTime(1950, 1, 1);

        DateOfBirthCellMonth.Text = "" + this.customer.DateOfBirth.Value.Month;
        DateOfBirthCellDay.Text = "" + this.customer.DateOfBirth.Value.Day;
        DateOfBirthCellYear.Text = "" + this.customer.DateOfBirth.Value.Year;

        SetVisitedCalendar();
        BindingContext = this.customer;
    }

    protected override bool OnBackButtonPressed() {
        Dispatcher.Dispatch(async () =>
        {
            var leave = await DisplayAlert("Changes will not be saved", "Click the 'Save' button at the bottom of the screen to save changes.", "Don't save", "Stay on page");

            if (leave) {
                //revert to old photos and delete new pictures taken
                if (newImage) {
                    if(customer.ImageUrl != Customer.NO_IMAGE_URL)
                        File.Delete(customer.ImageUrl);
                    customer.ImageUrl = oldImageURL;
                }
                if (newIDImage) {
                    if(customer.IDImageUrl != Customer.NO_IMAGE_URL)
                        File.Delete(customer.IDImageUrl);
                    customer.IDImageUrl = oldIDImageURL;
                }
                await Navigation.PopAsync();
            }
        });

        return true;

    }

    protected override void OnAppearing() {
        base.OnAppearing();
        SetVisitedCalendar();
    }
    private void OnNameEntryLoaded(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(this.customer.Name))
        {
            NameCell.Focus();
        }
    }

    /// <summary>
    /// Sets up the visited calendar control.
    /// </summary>
    private void SetVisitedCalendar() {
        VisitedCalendar.View = Syncfusion.Maui.Calendar.CalendarView.Month;
        if(DeviceDisplay.MainDisplayInfo.Width > 400) {
            VisitedCalendar.HeightRequest = 400;
            VisitedCalendar.WidthRequest = 400;
        }
        VisitedCalendar.BackgroundColor = Colors.White;
        VisitedCalendar.SelectionMode = Syncfusion.Maui.Calendar.CalendarSelectionMode.Multiple;
        VisitedCalendar.SelectionShape = Syncfusion.Maui.Calendar.CalendarSelectionShape.Circle;
        VisitedCalendar.SelectedDates = customer.DatesShopped;
        VisitedCalendar.MaximumDate = DateTime.Today;
        VisitedCalendar.MonthView.TodayBackground = Colors.Pink;
        VisitedCalendar.MonthView.DisabledDatesBackground = Colors.Gray;
        VisitedCalendar.SelectionBackground = Colors.Maroon;
        VisitedCalendar.TodayHighlightBrush = Colors.Maroon;
        VisitedCalendar.DisplayDate = DateTime.Now;
        VisitedCalendar.SelectableDayPredicate = (date) =>
        {
            return (date.DayOfWeek != DayOfWeek.Sunday && date.DayOfWeek != DayOfWeek.Saturday);
        };
    }

    /// <summary>
    /// Handles the event when the Take Picture button is clicked.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    private async void TakePictureClicked(object sender, EventArgs e) {
        if (MediaPicker.Default.IsCaptureSupported) {
            FileResult photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo == null)
                return;

            var resizedPhotoPath = await PhotoUtility.ResizePhotoAsync(photo);
            // Link File path to customer
            if ((sender as Button).Text == "Take Profile Picture") {
                newImage = true;
                oldImageURL = customer.ImageUrl;
                customer.ImageUrl = resizedPhotoPath;
            }
            else // Picture of customer's ID
            {
                newIDImage = true;
                oldIDImageURL = customer.IDImageUrl;
                customer.IDImageUrl = resizedPhotoPath;
            }

            try
            {
                //Delete original photo
                File.Delete(photo.FullPath);

            }catch
            {
                Console.WriteLine("Couldn't delete file {0}...", photo.FullPath);
            }
        }
    }



    /// <summary>
    /// Handles the event when the Save button is clicked.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    private async void SaveButtonClicked(object sender, EventArgs e) {
        //Verify name field is filled in
        if (string.IsNullOrEmpty(customer.Name)) {
            await DisplayAlert("Invalid Entry", "Name field cannot be empty.", "Ok");
            return;
        }
        YearUnfocused();
        MonthUnfocused();
        DayUnfocused();

        CustomerSaved?.Invoke(this, customer);
        await Navigation.PopAsync();
    }



    private void NameCell_Completed(object sender, EventArgs e) {
        NameCell.Unfocus();
        GenderCell.Focus();
    }

    private void GenderCell_Completed(object sender, EventArgs e) {
        GenderCell.Unfocus();
        ZipCodeCell.Focus();
    }

    private void ZipCodeCell_Completed(object sender, EventArgs e) {
        ZipCodeCell.Unfocus();
        CustomerCommentCell.Focus();
    }

    private void CustomerCommentCell_Completed(object sender, EventArgs e)
    {
        CustomerCommentCell.Unfocus();
        MonthFocus();
    }
    private void DateOfBirthCellMonth_Focused(object sender, FocusEventArgs e)
    {
        MonthFocus();
    }
    private void DateOfBirthCellMonth_Unfocused(object sender, FocusEventArgs e)
    {
        MonthUnfocused();
    }
    private void DateOfBirthCellMonth_Completed(object sender, EventArgs e)
    {
        MonthUnfocused();
        DayFocus();
    }
    private void MonthFocus()
    {
        DateOfBirthCellMonth.Focus();
        DateOfBirthCellMonth.CursorPosition = 0;
        DateOfBirthCellMonth.SelectionLength = 2;
    }
    private void MonthUnfocused()
    {
        var success = int.TryParse(DateOfBirthCellMonth.Text, out var month);
        if (!success)
        {
            DateOfBirthCellMonth.Text = "01";
            month = 1;
        }
        var dob = customer.DateOfBirth.Value;
        try
        {
            customer.DateOfBirth = new DateTime(dob.Year, month, dob.Day);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            customer.DateOfBirth = dob;
            DateOfBirthCellMonth.Text = "" + dob.Month;
        }

        DateOfBirthCellMonth.Unfocus();
    }

    private void DateOfBirthCellDay_Focused(object sender, FocusEventArgs e)
    {
        DayFocus();
    }
    private void DateOfBirthCellDay_Unfocused(object sender, FocusEventArgs e)
    {
        DayUnfocused();
    }
    private void DateOfBirthCellDay_Completed(object sender, EventArgs e)
    {
        DayUnfocused();
        YearFocus();
    }
    private void DayFocus()
    {
        DateOfBirthCellDay.Focus();
        DateOfBirthCellDay.CursorPosition = 0;
        DateOfBirthCellDay.SelectionLength = 2;
    }
    private void DayUnfocused()
    {
        var success = int.TryParse(DateOfBirthCellDay.Text, out var day);
        if (!success)
        {
            DateOfBirthCellDay.Text = "01";
            day = 1;
        }
        var dob = customer.DateOfBirth.Value;
        try
        {
            customer.DateOfBirth = new DateTime(dob.Year, dob.Month, day);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            customer.DateOfBirth = dob;
            DateOfBirthCellDay.Text = "" + dob.Day;
        }
        DateOfBirthCellDay.Unfocus();
    }


    private void DateOfBirthCellYear_Focused(object sender, FocusEventArgs e)
    {
        YearFocus();
    }
    private void DateOfBirthCellYear_Unfocused(object sender, FocusEventArgs e)
    {
        YearUnfocused();
    }
    private void DateOfBirthCellYear_Completed(object sender, EventArgs e)
    {
        YearUnfocused();
        //hides keyboard
        DateOfBirthCellYear.IsEnabled = false;
        DateOfBirthCellYear.IsEnabled = true;
    }
    private void YearFocus() {
        DateOfBirthCellYear.Focus();
        DateOfBirthCellYear.CursorPosition = 0;
        DateOfBirthCellYear.SelectionLength = 4;
    }
    private void YearUnfocused()
    {
        var success = int.TryParse(DateOfBirthCellYear.Text, out var year);
        if (!success)
        {
            DateOfBirthCellYear.Text = "1950";
            year = 1950;
        }
        var dob = customer.DateOfBirth.Value;
        try
        {
            //correct 2 digit entries
            if (year < 100)
                year += DateTime.Now.Year / 100 * 100;
            if (year >= DateTime.Now.Year)
                year -= 100;
            customer.DateOfBirth = new DateTime(year, dob.Month, dob.Day);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            customer.DateOfBirth = dob;
            DateOfBirthCellYear.Text = "" + dob.Year;
        }
        DateOfBirthCellYear.Unfocus();
    }

    /// <summary>
    /// Handles the event when the date is selected in the date picker.
    /// </summary>
    private void DOBPickerDateSelected(object sender, DateChangedEventArgs e) {
        DateOfBirthCellDay.Text = "" + DOBPicker.Date.Day;
        DateOfBirthCellMonth.Text = "" + DOBPicker.Date.Month;
        DateOfBirthCellYear.Text = "" + DOBPicker.Date.Year;
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
