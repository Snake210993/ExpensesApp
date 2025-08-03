using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpensesAppCpp.ErrorHandling;
using ExpensesAppCpp.FotoManagement;
using ExpensesAppCpp.Helper;
using ExpensesAppCpp.Models;
using ExpensesAppCpp.PopUp;
using ExpensesAppCpp.PreProcessor;
using ExpensesAppCpp.Tesseract;
using ExpensesAppCpp.ViewModel;
using Microsoft.Maui.ApplicationModel;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;




#if ANDROID
using Com.Googlecode.Tesseract.Android;
using Android.Graphics;
#endif

namespace ExpensesAppCpp.ViewModel
{
    public partial class MainPageViewModel : ObservableObject
    {
        private readonly BudgetData _budgetData;


        public bool USE_TRESHOLDING { get; set; } = false;
        public bool USE_SKEW_CORRECTION { get; set; } = false;
        public bool USE_DEVELOPMENT_MODE { get; set; } = false;
        public int SET_MAX_DIMENSIONS { get; set; } = 1500;
        public int SET_STATUS_DELAY { get; set; } = 2000; // milliseconds

        public string[] POSSIBLE_STORES { get; set; } = { "Aldi", "Migros", "Lidl", "Coop", "Volg" }; //populate trough entries of the user in the future
        public string[] POSSIBLE_AMOUNT_KEYWORDS = { "total", "summe", "gesamt", "amount due", "betrag", "amount", "due", "zu zahlen" };


        public MainPageViewModel(BudgetData budgetData)
        {
            _budgetData = budgetData;
            VisibleOverlay = false;
            IndicatorVisible = false;

        }
        [ObservableProperty]
        public string statusLabel;

        [ObservableProperty]
        public bool indicatorVisible;

        [ObservableProperty]
        public bool visibleOverlay;

        [RelayCommand]
        public async Task UploadReceipt()
        {
            if (!await CheckIfPeriodHasBeenCreated()) return;
            SKBitmap bitmap;
            var path = await FotoManagementHelper.LoadImageFromGalery();
            string ocrResult = string.Empty;
            //start preprocessing (grayscale, resize, etc.) can be done here if needed 
            //start UI Spinner and loading overlay
            if (string.IsNullOrEmpty(path))
            {
                await ErrorHandlingHelper.ShowPopup("No image selected. Please select an image to upload.");
                return;
            }
            try
            {
                VisibleOverlay = true;
                StatusLabel = "Processing image...";
                IndicatorVisible = true;

                bitmap = await PreProcessorHelper.CreateBitmap(path);
                bitmap = await PreProcessorHelper.ApplyGrayScale(bitmap, USE_TRESHOLDING);
                if (USE_SKEW_CORRECTION)
                {
                    double skewAngle = PreProcessorHelper.EstimateSkewAngle(bitmap);
                    var deskewedBitmap = PreProcessorHelper.RotateBitmap(bitmap, -skewAngle);
                }

                if (bitmap.Width > SET_MAX_DIMENSIONS || bitmap.Height > SET_MAX_DIMENSIONS)
                {
                    // Resize bitmap to a maximum width or height of 1500px, maintaining aspect ratio
                    bitmap = PreProcessorHelper.ResizeBitmap(bitmap, 1500);
                }
                PreProcessorHelper.StripExifData(bitmap, path);
                if (USE_DEVELOPMENT_MODE)
                {
                    // Save the preprocessed image for debugging purposes
                    await FileManipulationHelper.SaveBitmapAsync(bitmap);
                }
                //cleanup
                bitmap.Dispose();
                ocrResult = await TesseractHelper.RunTesseractAsync(path);
                // TODO: Display OCR result or pass it on as needed
                Trace.WriteLine($"Image OCR'd {ocrResult}");
                StatusLabel = "Done!";
                //delay for user to see the status
                await Task.Delay(SET_STATUS_DELAY);
                string store = await TesseractHelper.ReturnStoreName(ocrResult, POSSIBLE_STORES);
                await ErrorHandlingHelper.ShowPopup($"Store detected: {store}");
                var amount = await TesseractHelper.ReturnAmount(ocrResult, POSSIBLE_AMOUNT_KEYWORDS);
                Trace.WriteLine($"returned Amount: {amount}");
                await ErrorHandlingHelper.ShowPopup($"Amount detected: {amount}");
                IndicatorVisible = false;
                VisibleOverlay = false;


            }
            catch (Exception ex)//where to define this?
            {
                await ErrorHandlingHelper.ShowPopup($"Error processing image: {ex.Message}");
                return;
            }


        }
        [RelayCommand]
        public async Task ScanReceipt()
        {
            if (!await CheckIfPeriodHasBeenCreated()) return;
            SKBitmap bitmap;
            var path = await FotoManagementHelper.LoadImageFromCamera();
            string ocrResult = string.Empty;
            //start preprocessing (grayscale, resize, etc.) can be done here if needed 
            //start UI Spinner and loading overlay
            if (string.IsNullOrEmpty(path))
            {
                await ErrorHandlingHelper.ShowPopup("No image captured. Please capture an image to scan.");
                return;
            }
            await ErrorHandlingHelper.ShowPopup("Feature not implemented yet. Please use the upload feature instead.");



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
