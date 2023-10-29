using Newtonsoft.Json;
using SundayLoveProject.Models;
using System.Collections.ObjectModel;
using Firebase.Storage;
using System.Windows.Input;

namespace SundayLoveProject;

/// <summary>
/// Represents a page for searching and managing customers.
/// </summary>
public partial class CustomerSearchPage : ContentPage
{
    CustomerDatabase database; 
    bool databaseIsLoaded = false;
    int customersToShowInSearch = 20;
    
    ObservableCollection<Customer> customers = new ObservableCollection<Customer>();
    ObservableCollection<Customer> searchSubsetOfCustomers = new ObservableCollection<Customer>();

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomerSearchPage"/> class.
    /// </summary>
    public CustomerSearchPage()
	{
        InitializeComponent();
        database = App.Database;
        customerListView.ItemsSource = searchSubsetOfCustomers;
        //customerListView.ItemsSource = customers; //FOR DEBUGGING
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        //Clear database file
        //await database.DeleteAllCustomersAsync(); //DEBUG ONLY
        if (!databaseIsLoaded) {
            await LoadFromDatabase();
            databaseIsLoaded= true;
        }
        searchSubsetOfCustomers.Clear();
        if (string.IsNullOrEmpty(searchBar.Text))
            return;

        //update based on previous search bar contents
        foreach (var customer in customers) {
            if (customer.Name.Contains(searchBar.Text, StringComparison.CurrentCultureIgnoreCase))
                searchSubsetOfCustomers.Add(customer);
            if (searchSubsetOfCustomers.Count >= customersToShowInSearch)
                return;
        }

    }

    /// <summary>
    /// Loads customer data from the database.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task<bool> LoadFromDatabase() {
        customers.Clear();

        foreach (var customer in await database.GetCustomersAsync()) { 
            //deserialize the dates shopped collection for each customer
            customer.DatesShopped = JsonConvert.DeserializeObject<ObservableCollection<DateTime>>(customer.DateTimesBlob);
            customers.Add(customer);
        }
        return true;
    }


    /// <summary>
    /// Handles the event when the "Add Customer" button is clicked.
    /// </summary>
    private async void AddCustomerClicked(object sender, EventArgs e) {
        
        customerListView.SelectedItem = null;

        Customer customer = new Customer();
        var page = new CustomerDetailPage(customer);
        page.CustomerSaved += async (source, customerCopy) =>
        {
            customer.Name = customerCopy.Name;
            customer.Gender = customerCopy.Gender;
            customer.DateOfBirth = customerCopy.DateOfBirth;
            customer.IsEligible = customerCopy.IsEligible;
            customer.ImageUrl = customerCopy.ImageUrl;
            customer.IDImageUrl = customerCopy.IDImageUrl;
            customer.ZipCode = customerCopy.ZipCode;
            customer.CustomerComment = customerCopy.CustomerComment;

            //Save customer to get it an ID, then use the ID for new picture file names
            await database.SaveCustomerAsync(customer);

            //backup image to Firebase and save to app data; 
            if (customer.ImageUrl != Customer.NO_IMAGE_URL) {
                var dirPath = Path.Combine(FileSystem.AppDataDirectory, "" + customer.ID);
                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);
                var newFileName = Path.GetFileName(customer.ImageUrl);
                var newFilePath = Path.Combine(dirPath, newFileName);
                File.Move(customer.ImageUrl, newFilePath, true); //Move file from cache to app data
                customer.ImageUrl = newFilePath; //Save new path
                App.Firebase.UploadFileAsync(customer.ImageUrl, @"images/" + newFileName);
            }
            if(customer.IDImageUrl != Customer.NO_IMAGE_URL) {
                var dirPath = Path.Combine(FileSystem.AppDataDirectory, "" + customer.ID);
                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);
                var newFileName = Path.GetFileName(customer.IDImageUrl);
                var newFilePath = Path.Combine(dirPath, newFileName);
                File.Move(customer.IDImageUrl, newFilePath, true); //Save a copy of cached photo to app data
                customer.IDImageUrl = newFilePath; //Save new path
                App.Firebase.UploadFileAsync(customer.IDImageUrl, @"images/" + newFileName);
            }

            customer.DatesShopped.Clear();
            foreach (var d in customerCopy.DatesShopped)
            {
                customer.DatesShopped.Add(d);
            }

            //add to list
            customers.Add(customer);

            //add to database
            await database.SaveCustomerAsync(customer);
            //backup database to Firebase
            App.Firebase.UploadFileAsync(Constants.DatabasePath, Constants.DatabaseFilename);

        };        
        await Navigation.PushAsync(page);
    }

    /// <summary>
    /// Handles the event when the "View Report" button is clicked.
    /// </summary>
    private async void ViewReportClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CustomReportPage(customers));
    }

    /// <summary>
    /// Handles the event when the "Search Bar Text" is changed.
    /// </summary>
    private void SearchBarTextChanged(object sender, TextChangedEventArgs e) {
        if (string.IsNullOrEmpty(e.NewTextValue)) //search bar is empty
            searchSubsetOfCustomers.Clear(); 
        else
        {
            searchSubsetOfCustomers.Clear();
            foreach (var customer in customers)
            {
                if (customer.Name.Contains(e.NewTextValue, StringComparison.CurrentCultureIgnoreCase))
                    searchSubsetOfCustomers.Add(customer);
            }
      
        }
    }

    /// <summary>
    /// Handles the event when an item in the customer list view is tapped.
    /// </summary>
    private void CustomerListViewItemTapped(object sender, ItemTappedEventArgs e) {
        //should go to CustomerView page
        var customer = e.Item as Customer;
        customerListView.SelectedItem = null; //deselect
        Navigation.PushAsync(new CustomerViewPage(customer));
    }

    /// <summary>
    /// Handles the event when an item in the customer list view is deleted.
    /// </summary>
    private async void DeleteButtonClicked(object sender, EventArgs e) {
        bool delete = await DisplayAlert("Delete Customer", "Are you sure you want to delete the customer?", "Yes", "No");
        if (delete) {
            var customer = (sender as Button).CommandParameter as Customer;
            //delete photos from device
            File.Delete(customer.ImageUrl);
            File.Delete(customer.IDImageUrl);
            //delete photos from firebase
            App.Firebase.DeleteFileAsync(@"images/" + Path.GetFileName(customer.ImageUrl));
            App.Firebase.DeleteFileAsync(@"images/" + Path.GetFileName(customer.IDImageUrl));

            customers.Remove(customer);
            searchSubsetOfCustomers.Remove(customer);
            await database.DeleteCustomerAsync(customer);
        }
    }

    private void searchBar_SearchButtonPressed(object sender, EventArgs e) {
        (sender as SearchBar).Unfocus();
    }

    private async void CheckButton_Clicked(object sender, EventArgs e) {
        var customer = (sender as Button).CommandParameter as Customer;
        if (customer.IsEligible && !customer.ShoppedToday()) {
            customer.AddDateShopped(DateTime.Today);
        }
        else {
            customer.RemoveDateShopped(DateTime.Today);

        }
        //add to database
        await App.Database.SaveCustomerAsync(customer);
    }

    private async void Settings_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SettingsPage(customers));
    }
}