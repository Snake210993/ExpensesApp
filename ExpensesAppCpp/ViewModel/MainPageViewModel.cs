using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpensesAppCpp.Helper;
using ExpensesAppCpp.ErrorHandling;
using ExpensesAppCpp.FotoManagement;
using ExpensesAppCpp.PreProcessor;
using ExpensesAppCpp.Tesseract;
using ExpensesAppCpp.Models;
using ExpensesAppCpp.PopUp;
using ExpensesAppCpp.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;




#if ANDROID
using Com.Googlecode.Tesseract.Android;
using Android.Graphics;
#endif

namespace ExpensesAppCpp.ViewModel
{
    public partial class MainPageViewModel : ObservableObject
    {
        private readonly BudgetData _budgetData;

        public MainPageViewModel(BudgetData budgetData)
        {
            _budgetData = budgetData;
        }

        [ObservableProperty]
        Label statusLabel;

        [RelayCommand]
        public async Task UploadReceipt()
        {
            if (!await CheckIfPeriodHasBeenCreated()) return;
            var path = await FotoManagementHelper.LoadImageFromGalery();
            //start preprocessing (grayscale, resize, etc.) can be done here if needed 
            //start UI Spinner and loading overlay

        }
        [RelayCommand]
        public async Task ScanReceipt()
        {
            if (!await CheckIfPeriodHasBeenCreated()) return;
            var path = await FotoManagementHelper.LoadImageFromCamera();
            //start preprocessing (grayscale, resize, etc.) can be done here if needed 
            //start UI Spinner and loading overlay

        }

        public async Task<bool> CheckIfPeriodHasBeenCreated()
        {
            if (_budgetData.BudgetPeriods.Any())
            {
                return true;

            }
            else
            {
                await ErrorHandlingHelper.ShowPopup("No budget period has been created yet. Please create a budget period first.");
                return false;
            }
        }
    }
}
