using TesseractOcrMaui;
using TesseractOcrMaui.Results;


namespace ExpensesAppCpp
{
    public partial class MainPage : ContentPage
    {
        private readonly ITesseract _tesseract;

        public MainPage(ITesseract tesseract)
        {
            InitializeComponent();
            _tesseract = tesseract;

        }


        private async void OnClicked(object? sender, EventArgs e)
        {
            try
            {
                // Check and request camera permission
                var status = await Permissions.RequestAsync<Permissions.Camera>();
                if (status != PermissionStatus.Granted)
                {
                    await DisplayAlert("Permission Denied", "Camera permission is required.", "OK");
                    return;
                }

                // Take photo using MediaPicker
                if (MediaPicker.Default.IsCaptureSupported)
                {
                    var photo = await MediaPicker.Default.CapturePhotoAsync();
                    if (photo != null)
                    {
                        // You can use the photo file here (e.g., display or process it)
                        //preprocessing (grayscale, resize, etc.) can be done here if needed

                        await _tesseract.LoadTraineddataAsync();
                        var result = await _tesseract.RecognizeTextAsync(photo.FullPath);
                        if (result.Confidence < 0.5)
                        {
                            await DisplayAlert("OCR Failed", $"Reason: {result.Status}", "OK");
                            return;
                        }
                        await DisplayAlert("OCR Result", result.RecognisedText, "OK");

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
        }
        private async void OnUploadClicked(object? sender, EventArgs e)
        {
            // Navigate to the Expenses page
            var path = await GetUserSelectedImagePath();
            if (path == null)
            {
                await DisplayAlert("Error", "No image selected.", "OK");
                return;
            }
            await _tesseract.LoadTraineddataAsync();
            var result = await _tesseract.RecognizeTextAsync(path);
            if (result.Confidence < 0.5)
            {
                await DisplayAlert("OCR Failed", $"Reason: {result.Status}", "OK");
                return;
            }
            await DisplayAlert("OCR Result", result.RecognisedText, "OK");
        }


        private static async Task<string?> GetUserSelectedImagePath()
        {
            /* This method lets user to select image file by opening
               file selection dialog. */
#if IOS
        /* Note that this method uses conditional
           compilation for iOS because MediaPicker is better
           option to use for image picking on iOS. */
        var pickResult = await MediaPicker.PickPhotoAsync(new MediaPickerOptions()
        {
            Title = "Pick jpeg or png image"
        });
#else
            var pickResult = await FilePicker.PickAsync(new PickOptions()
            {
                PickerTitle = "Pick jpeg or png image",
                // Currently usable image types are png and jpeg
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>()
                {
                    [DevicePlatform.Android] = new List<string>() { "image/png", "image/jpeg" },
                    [DevicePlatform.WinUI] = new List<string>() { ".png", ".jpg", ".jpeg" },
                })
            });
#endif
            return pickResult?.FullPath;
        }
    }
}
