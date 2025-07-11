namespace ExpensesAppCpp
{
    public partial class MainPage : ContentPage
    {


        public MainPage()
        {
            InitializeComponent();
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
                        await DisplayAlert("Photo Captured", $"Photo saved to: {photo.FullPath}", "OK");
                        // You can use the photo file here (e.g., display or process it)
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
        private async void OnExpensesClicked(object? sender, EventArgs e)
        {
            // Navigate to the Expenses page
           await Shell.Current.GoToAsync(nameof(ExpenditureCollection));

        }
    }
}
