﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Plugin.Connectivity;
using RecallJson;
using PostalCodesJSON;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

namespace proj441
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RecallPage : ContentPage
    {
        ObservableCollection<Result> myRecallsCollection = new ObservableCollection<Result>();

        public RecallPage()
        {
            InitializeComponent();
            D1.MinimumDate = DateTime.Now.AddYears(-1);
            D1.MaximumDate = DateTime.Now;
            D1.Date = DateTime.Now.AddMonths(-1);
            D2.MinimumDate = DateTime.Now.AddYears(-1);
            D2.MaximumDate = DateTime.Now;
        }

        private async void RecallLookup_Clicked(object sender, EventArgs e)
        {
            try
            {
                bool connection = CrossConnectivity.Current.IsConnected;


                if (connection)
                {
                    Indicator.IsRunning = true;
                    Indicator.IsVisible = true;

                    if (userZip.Text != null && userZip.Text != "")
                    {
                        HttpClient client = new HttpClient();

                        /*added an exception for iOS in info.plist https://docs.microsoft.com/en-us/xamarin/ios/app-fundamentals/ats#optout */

                        string dictionaryEndpoint = "http://api.geonames.org/postalCodeSearchJSON?formatted=true&country=US&postalcode=" + userZip.Text + "&username=abezat";
                        Uri dictionaryUri = new Uri(dictionaryEndpoint);
                        HttpResponseMessage response = await client.GetAsync(dictionaryEndpoint);

                        if (response.IsSuccessStatusCode)
                        {

                            string jsonString = await response.Content.ReadAsStringAsync();
                            var myPostal = Postal.FromJson(jsonString);

                            PostalCode result = myPostal.PostalCodes.First();
                            string ZipToCity = result.PlaceName;
                            userEntry.Text = ZipToCity;

                            Analytics.TrackEvent("Successfully Retrived zip code info");
                        }
                    }
                    if (userEntry.Text != null && userEntry.Text != "")
                    {
                        string d1 = D1.Date.ToString("yyyyMMdd");
                        string d2 = D2.Date.ToString("yyyyMMdd");

                        //string userString = userEntry.Text;
                        //userString = userString.ToLower();

                        HttpClient client = new HttpClient();
                        string dictionaryEndpoint = "https://api.fda.gov/drug/enforcement.json?search=report_date:[" + d1 + "+TO+" + d2 + "]+AND+city:" + userEntry.Text + "+AND+status:ongoing&limit=100";
                        Uri dictionaryUri = new Uri(dictionaryEndpoint);
                        HttpResponseMessage response = await client.GetAsync(dictionaryEndpoint);

                        if (response.IsSuccessStatusCode)
                        {
                            Indicator.IsRunning = false;
                            Indicator.IsVisible = false;
                            string jsonString = await response.Content.ReadAsStringAsync();
                            var myRecalls = MyRecalls.FromJson(jsonString);

                            //List<Define> myDefinitions = JsonConvert.DeserializeObject<List<Define>>(jsonContent);

                            //if (myRecalls.Meta.Results.Total > 0)
                            //{
                            myRecallsCollection.Clear();
                            myRecalls.Results.ForEach(myRecallsCollection.Add);
                            RecallsListView.ItemsSource = myRecallsCollection;
                            RecallsListView.IsVisible = true;
                            userLabel.IsVisible = false;

                            Analytics.TrackEvent("Successfully Retrived recall info");

                            //RecallsListView_Refreshing(RecallsListView, null);
                            //}

                            //else
                            //{
                            //    userLabel.Text = "NO RECALLS IN YOUR CITY WITHIN LAST YEAR";    //strings of letters that are not words, but still return success
                            //    userLabel.IsVisible = true;
                            //    RecallsListView.IsVisible = false;
                            //}
                        }
                        else
                        {
                            Indicator.IsRunning = false;
                            Indicator.IsVisible = false;
                            userLabel.Text = "YOUR ENTRY RETURNED ZERO RESULTS";    //for empty strings
                            userLabel.IsVisible = true;
                            RecallsListView.IsVisible = false;
                        }
                    }
                    else
                    {
                        Indicator.IsRunning = false;
                        Indicator.IsVisible = false;
                        userLabel.Text = "PLEASE ENTER A CITY NAME";
                        userLabel.IsVisible = true;
                    }
                }
                else
                {
                    Indicator.IsRunning = false;
                    Indicator.IsVisible = false;
                    await DisplayAlert("No Internet", "No Internet Connection Detected", "OK");
                    userLabel.IsVisible = true;
                    RecallsListView.IsVisible = false;
                }

                var s = userZip.Text;
                var s2 = userEntry.Text;

                if ((userZip.Text == null || userZip.Text == "") && (userEntry.Text == null || userEntry.Text == ""))
                {
                    userSearch.IsVisible = false;
                }
                else
                {
                    userSearch.IsVisible = true;
                    userSearch.Text = "You've searched: " + userEntry.Text;
                }
                if (userZip.Text != null && userZip.Text != "")
                    userSearch.Text += ", " + userZip.Text;
                userEntry.Text = null;
                userZip.Text = null;
            }
            catch (Exception exception)
            {
                Crashes.TrackError(exception);
            }
        }

        private void RecallsListView_Refreshing(object sender, EventArgs e)
        {
            var listViewToRefresh = (ListView)sender;
            RecallsListView.ItemsSource = null;
            RecallsListView.ItemsSource = myRecallsCollection;
            RecallsListView.IsRefreshing = false;
        }

        private void D1_DateSelected(object sender, DateChangedEventArgs e)
        {
            D2.MinimumDate = e.NewDate;
            if (D2.Date < D1.Date)
                D2.Date = D1.Date;
        }

        private void D2_DateSelected(object sender, DateChangedEventArgs e)
        {
            if (D2.Date < D1.Date)
                D2.Date = D1.Date;
        }

        protected override void OnAppearing()
        {

        }
    }
}