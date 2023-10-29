using SundayLoveProject.Models;
using System.Collections.ObjectModel;

namespace SundayLoveProject;


public partial class CustomReportPage : ContentPage
{
    public DateTime LowerBoundDate { get; set; }
    public DateTime UpperBoundDate { get; set; }
	public string ZipCodeEntered { get; set; }

	ObservableCollection<Customer> customers;

    public CustomReportPage(ObservableCollection<Customer> customers)
	{
		InitializeComponent();
        this.customers = customers;
		LowerBoundDate = DateTime.Today;
		UpperBoundDate = DateTime.Today;
		ZipCodeEntered = "";
		BindingContext = this;
        //daily report how many customers that day and new customers that day
        CalculateTimesShopped(customers, DateTime.Today);
		InvalidDateRangeSetText();
		//custom report how many total customers between two dates
		//GenerateReport(customers, LowerBoundDate, UpperBoundDate);
	}

	public void GenerateReport(ObservableCollection<Customer> customers, DateTime start, DateTime end) {
		var firstTimeShoppers = 0;
		var uniqueShoppers = 0;
		var totalTimesShopped = 0;
		for (int i = 0; i < customers.Count; i++) {
			var datesShopped = customers[i].DatesShopped;
			if (datesShopped.Count == 0 || datesShopped.First<DateTime>() > end || datesShopped.Last<DateTime>() < start)
				continue;

			if (!(customers[i].ZipCode==ZipCodeEntered || string.IsNullOrEmpty(ZipCodeEntered)))
				continue;

			if (datesShopped.First<DateTime>() >= start) {
				firstTimeShoppers++;
			}

			//Count dates in the range
			var shopped = false;
			for (int j = 0; j < datesShopped.Count; j++) {
				if (datesShopped[j] >= start && datesShopped[j] <= end) {
					totalTimesShopped++;
					shopped = true; 
				}
				else if (datesShopped[j] > end)
					break;
			}
			if (shopped) uniqueShoppers++;
		}
		SummaryFirst.Text = "Total first time shoppers in this date range: " + firstTimeShoppers;
        SummaryTotal.Text = "Total unique shoppers in this date range: " + uniqueShoppers;
        SummaryUnique.Text = "Total times shopped (by all shoppers) in this date range: " + totalTimesShopped;
    }

	public void CalculateTimesShopped(ObservableCollection<Customer> customers, DateTime day)
	{
		int[] data = new int[2];
		for (int i = 0; i < customers.Count; i++)
		{
			for (int j = 0; j < customers[i].DatesShopped.Count; j++)
			{
				if (customers[i].DatesShopped[j] == day)
				{
					data[0] += 1;
					if (customers[i].DatesShopped.First<DateTime>() == day) 
						data[1] += 1;
				}
				
			}
		}
        Total.Text = "Total Customers Shopped Today: " + data[0];
        NewCount.Text = "Total New Customers Shopped Today: " + data[1];
	}

    private void LowerBoundDatePickerDateSelected(object sender, DateChangedEventArgs e)
    {

        if (LowerBoundDate <= UpperBoundDate)
        {
			ErrorLabel.Text = "";
            GenerateReport(customers, LowerBoundDate, UpperBoundDate);

        }
        else
        {
            ErrorLabel.Text = "Invalid date range.";
			InvalidDateRangeSetText();
        }
    }

    private void UpperBoundDatePickerDateSelected(object sender, DateChangedEventArgs e)
    {
		if (LowerBoundDate <= UpperBoundDate) {
            ErrorLabel.Text = "";
            GenerateReport(customers, LowerBoundDate, UpperBoundDate);
		}
		else
		{
            ErrorLabel.Text = "Invalid date range.";
            InvalidDateRangeSetText();
        }
    }

    private void ZipCodeTextEntered(object sender, EventArgs e) {
        if (LowerBoundDate <= UpperBoundDate) {
            ErrorLabel.Text = "";
            GenerateReport(customers, LowerBoundDate, UpperBoundDate);
        }
        else {
            ErrorLabel.Text = "Invalid date range.";
            InvalidDateRangeSetText();
        }
    }

    private void InvalidDateRangeSetText() {
        SummaryFirst.Text = "Total first time shoppers in this date range: -";
        SummaryTotal.Text = "Total unique shoppers in this date range: -";
        SummaryUnique.Text = "Total times shopped (by all shoppers) in this date range: -";
    }

    private void ReportButton_Clicked(object sender, EventArgs e) {
		GenerateReport(customers, LowerBoundDate, UpperBoundDate);
    }
}