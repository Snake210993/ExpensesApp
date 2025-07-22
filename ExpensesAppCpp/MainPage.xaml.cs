using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Storage;
using Microsoft.Maui.Controls.PlatformConfiguration;
using SkiaSharp;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using ExpensesAppCpp.Helper;
using ExpensesAppCpp.PreProcessor;
using ExpensesAppCpp.Tesseract;
using ExpensesAppCpp.ViewModel;
using ExpensesAppCpp.Models;




#if ANDROID
using Com.Googlecode.Tesseract.Android;
using Android.Graphics;
#endif

namespace ExpensesAppCpp
{
    public partial class MainPage : ContentPage
    {

        public MainPage(MainPageViewModel vm)
        {
            InitializeComponent();
            Overlay.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() =>{}) });
            BindingContext = vm;
        }


        private async void OnClicked(object? sender, EventArgs e)
        {
            Stopwatch sw = Stopwatch.StartNew();
            var result = "{}";
            try
            {


                // Take photo using MediaPicker
                if (MediaPicker.Default.IsCaptureSupported)
                {
                    var photo = await MediaPicker.Default.CapturePhotoAsync();
                    if (photo != null)
                    {
                        // You can use the photo file here (e.g., display or process it)
                        //preprocessing (grayscale, resize, etc.) can be done here if needed

                        var preProcessedPicture = "{}";
                        ActivityIndicator activityIndicator = new ActivityIndicator { IsRunning = true };
                        Overlay.IsVisible = true;
                        StatusLabel.Text = "Processing image...";

                        preProcessedPicture = await Task.Run(() => PreProcessorHelper.PreProcessPicture(photo.FullPath, StatusLabel));
                        if (preProcessedPicture == null)
                        {
                           await DisplayAlert("Error", "Failed to preprocess the image.", "OK");
                           return;
                        }
                       result = await TesseractHelper.RunTesseractAsync(preProcessedPicture);
                        if (string.IsNullOrEmpty(result))
                        {
                            await DisplayAlert("Error", "OCR failed to recognize any text.", "OK");
                            return;
                        }
                    }
                }
                else
                {
                    await DisplayAlert("Not Supported", "Camera capture is not supported on this device.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
            await DisplayAlert("Elapsed Time", $"Time spent: {sw.Elapsed}", "OK");
            await DisplayAlert("Success", "Image processed successfully.", "OK");
            await DisplayAlert("Recognized Text", result, "OK");
            Spinner.IsRunning = false;
            Overlay.IsVisible = false;
            StatusLabel.Text = "Done!";

        }
        private async void OnUploadClicked(object? sender, EventArgs e)
        {
            Stopwatch sw = Stopwatch.StartNew();
            var result = "{}";
            // Navigate to the Expenses page
            var path = await FileManipulationHelper.GetUserSelectedImagePath();
            if (path == null)
            {
                await DisplayAlert("Error", "No image selected.", "OK");
                return;
            }
            var preProcessedPicture = "{}";
            ActivityIndicator activityIndicator = new ActivityIndicator { IsRunning = true };
            Overlay.IsVisible = true;
            StatusLabel.Text = "Processing image...";

            preProcessedPicture = await Task.Run(() => PreProcessorHelper.PreProcessPicture(path, StatusLabel));

           if (preProcessedPicture == null)
            {
                await DisplayAlert("Error", "Failed to preprocess the image.", "OK");
                return;
            }
            result = await TesseractHelper.RunTesseractAsync(preProcessedPicture);
            if (string.IsNullOrEmpty(result))
            {
                await DisplayAlert("Error", "OCR failed to recognize any text.", "OK");
                return;
            }
            await DisplayAlert("Success", result, "OK");
            Spinner.IsRunning = false;
            Overlay.IsVisible = false;
            StatusLabel.Text = "Done!";
            await DisplayAlert("Elapsed Time", $"Time spent: {sw.Elapsed}", "OK");

        }




    }



}
