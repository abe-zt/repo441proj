﻿using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace proj441
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PrescriptionsPage : ContentPage
    {
        public PrescriptionsPage()
        {
            InitializeComponent();
            PopulateMyPrescriptionsList();
        }

        private void PopulateMyPrescriptionsList()
        {
            List<Prescription> prescriptionsSortedMultiple = App.MyPrescrpitions.OrderBy(x => x.ProperName).ThenBy(x => x.Strength).ToList();
            MyLogList.ItemsSource = prescriptionsSortedMultiple;  
        }

        private async void AddPrescriptionButton_Clicked(object sender, EventArgs e)
        {
            Button button = (Button)sender;

            await Navigation.PushAsync(new AddPerscriptionPage());
        }

        async void Handle_ContextMenuInfoButton(object sender, EventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var contextSelected = (Prescription)menuItem.CommandParameter;
            //await Navigation.PushAsync(new PrescriptionInfoPage(contextSelected));
            await PopupNavigation.Instance.PushAsync(new PrescriptionInfoPage(contextSelected));
        }

        async void Handle_ContextMenuEditButton(object sender, EventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var contextSelected = (Prescription)menuItem.CommandParameter;
            await Navigation.PushModalAsync(new EditPrescriptionPage(contextSelected));
        }

        private async void Handle_ContextMenuDeleteButton(object sender, EventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var contextSelected = (Prescription)menuItem.CommandParameter;
            //DisplayAlert("Deleted:", contextSelected.Name, "OK");
            bool answer = await DisplayAlert("Confirm:", "Delete '" + contextSelected.Name + "' ?", "Yes", "No");
            if (answer)
            {
                App.MyPrescrpitions.Remove(contextSelected);
                await App.MyPrescriptionDatabase.DeleteItemAsync(contextSelected);
            }
            MyLogList_Refreshing(MyLogList, null);
        }

        private async void MyLogList_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            ListView l = (ListView)sender;

            Prescription p = (Prescription)l.SelectedItem;
            if (l.SelectedItem != null)
            {
                if (p.Remaining > 0)
                {
                    //await PopupNavigation.Instance.PushAsync(new LogPopup(p));
                    await Navigation.PushAsync(new LogDosagePage(p));
                }
                else
                {
                    await DisplayAlert("Error:", "Prescription is empty", "OK");
                }
            }

            l.SelectedItem = null;
        }

        private void MyLogList_Refreshing(object sender, EventArgs e)
        {
            var listViewToRefresh = (ListView)sender;
            ClearListView();
            PopulateMyPrescriptionsList();
            MyLogList.IsRefreshing = false;
        }

        private void ClearListView()
        {
            MyLogList.ItemsSource = null;
        }

        private async void NavToHistoryButton_Clicked(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            Navigation.InsertPageBefore(new HistoryPage(), this);
            //await Navigation.PushAsync(new HistoryPage());
            await Navigation.PopAsync();
        }

        protected override void OnAppearing()
        {
            MyLogList_Refreshing(MyLogList, null);
        }
    }
}