using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Storage;
using ExpensesAppCpp.ErrorHandling;
using ExpensesAppCpp.Helper;
using ExpensesAppCpp.Models;
using ExpensesAppCpp.PopUp;
using ExpensesAppCpp.ViewModel;
using FuzzySharp;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.PlatformConfiguration;
using SkiaSharp;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Serialization;





#if ANDROID
using Com.Googlecode.Tesseract.Android;
using Android.Graphics;
#endif

namespace ExpensesAppCpp.Helper
{
    public class FileManipulationHelper
    {
        public static async Task<string?> GetUserSelectedImagePath()
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
        public static async Task SaveBitmapAsync(SKBitmap bitmap)
        {
            // Convert SKBitmap to PNG stream
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            using var stream = new MemoryStream(data.ToArray());

            // Ask user where to save the file
            var result = await FileSaver.Default.SaveAsync(
                "image.png",                  // default filename
                stream,                       // content stream
                new CancellationToken());     // optional cancellation token

            if (result.IsSuccessful)
            {
                Console.WriteLine($"File saved: {result.FilePath}");
            }
            else
            {
                Console.WriteLine("File saving failed.");
            }
        }
    }
}
namespace ExpensesAppCpp.PreProcessor
{
    public class PreProcessorHelper
    {

        public static SKBitmap ApplyExifOrientation(SKBitmap source, SKEncodedOrigin origin)
        {
            if (origin == SKEncodedOrigin.TopLeft) // Normal, no rotation
                return source;

            SKBitmap rotated;

            switch (origin)
            {
                case SKEncodedOrigin.RightTop: // 90° CW
                    rotated = new SKBitmap(source.Height, source.Width);
                    using (var surface = new SKCanvas(rotated))
                    {
                        surface.Translate(rotated.Width, 0);
                        surface.RotateDegrees(90);
                        surface.DrawBitmap(source, 0, 0);
                    }
                    return rotated;

                case SKEncodedOrigin.LeftBottom: // 90° CCW
                    rotated = new SKBitmap(source.Height, source.Width);
                    using (var surface = new SKCanvas(rotated))
                    {
                        surface.Translate(0, rotated.Height);
                        surface.RotateDegrees(-90);
                        surface.DrawBitmap(source, 0, 0);
                    }
                    return rotated;

                case SKEncodedOrigin.BottomRight: // 180°
                    rotated = new SKBitmap(source.Width, source.Height);
                    using (var surface = new SKCanvas(rotated))
                    {
                        surface.Translate(rotated.Width, rotated.Height);
                        surface.RotateDegrees(180);
                        surface.DrawBitmap(source, 0, 0);
                    }
                    return rotated;

                default:
                    return source; // Other mirrored cases not needed for OCR
            }
        }

        public static double EstimateSkewAngle(SKBitmap binaryBitmap)
        {
            int width = binaryBitmap.Width;
            int height = binaryBitmap.Height;
            int angleSteps = 30; // test angles from -15 to +15 degrees
            double angleStepSize = 1.0; // degree

            double bestScore = double.MinValue;
            double bestAngle = 0;

            for (int i = -angleSteps; i <= angleSteps; i++)
            {
                double angle = i * angleStepSize;
                using var rotated = RotateBitmap(binaryBitmap, angle);

                double score = ComputeHorizontalProjectionScore(rotated);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestAngle = angle;
                }
            }

            return bestAngle;
        }

        public static SKBitmap RotateBitmap(SKBitmap src, double angleDegrees)
        {
            double angleRadians = angleDegrees * Math.PI / 180.0;

            using var surface = SKSurface.Create(new SKImageInfo(src.Width, src.Height));
            var canvas = surface.Canvas;

            canvas.Clear(SKColors.White);
            canvas.Translate(src.Width / 2, src.Height / 2);
            canvas.RotateDegrees((float)angleDegrees);
            canvas.Translate(-src.Width / 2, -src.Height / 2);
            canvas.DrawBitmap(src, 0, 0);
            canvas.Flush();

            using var img = surface.Snapshot();
            return SKBitmap.FromImage(img);
        }

        private static double ComputeHorizontalProjectionScore(SKBitmap bmp)
        {
            int width = bmp.Width;
            int height = bmp.Height;
            double score = 0;

            for (int y = 0; y < height; y++)
            {
                int rowSum = 0;
                for (int x = 0; x < width; x++)
                {
                    var pixel = bmp.GetPixel(x, y);
                    rowSum += pixel.Red == 0 ? 1 : 0; // black pixels only
                }
                score += rowSum * rowSum;
            }

            return score;
        }


        public static SKBitmap ResizeBitmap(SKBitmap originalBitmap, int targetWidth)
        {
            // Maintain aspect ratio
            int targetHeight = (int)(targetWidth / (float)originalBitmap.Width * originalBitmap.Height);

            // Create a new resized bitmap
            SKImageInfo info = new SKImageInfo(targetWidth, targetHeight);
            SKBitmap resized = new SKBitmap(info);

            using (var canvas = new SKCanvas(resized))
            {
                canvas.Clear(SKColors.White); // Optional: clear background
                canvas.DrawBitmap(originalBitmap, new SKRect(0, 0, targetWidth, targetHeight));
            }

            return resized;
        }

        public static async Task<SKBitmap?> CreateBitmap(string path)
        {

                SKBitmap bitmap;
                using (var stream = File.OpenRead(path))
                {
                    using var codec = SKCodec.Create(stream);
                    bitmap = SKBitmap.Decode(codec);
                    bitmap = PreProcessorHelper.ApplyExifOrientation(bitmap, codec.EncodedOrigin);
                }

                if (bitmap == null) return null;
                return bitmap;
        }

        public static async Task<SKBitmap?> ApplyGrayScale(SKBitmap bitmap, bool treshold)
        {
            return await Task.Run(async () =>
            {
                if (bitmap == null)
                    return null;
                var pixmap = bitmap.PeekPixels();


                // Loop through bitmap
                unsafe
                {
                    // Assume format is BGRA8888 (default in Skia)
                    for (int y = 0; y < pixmap.Height; y++)
                    {
                        byte* row = (byte*)pixmap.GetPixels().ToPointer() + y * pixmap.RowBytes;
                        for (int x = 0; x < pixmap.Width; x++)
                        {
                            byte* pixel = row + x * 4;
                            byte blue = pixel[0];
                            byte green = pixel[1];
                            byte red = pixel[2];
                            byte gray = (byte)(0.3 * red + 0.59 * green + 0.11 * blue);

                            //byte binary = (gray >= threshold) ? (byte)255 : (byte)0;


                            pixel[0] = pixel[1] = pixel[2] = gray; //removed binary to gray

                            if (treshold)
                            {
                                int threshold = 128; // Threshold for binarization
                                byte binary = (gray >= threshold) ? (byte)255 : (byte)0;
                                pixel[0] = pixel[1] = pixel[2] = binary;
                            }
                        }
                    }
                }
            if (bitmap == null)
                    return null;
                return bitmap;
            });
        }
        public static async void StripExifData(SKBitmap bitmap, string path)
        {
            using (var image = SKImage.FromBitmap(bitmap))
            using (var data = image.Encode(SKEncodedImageFormat.Png, 100)) // PNG strips EXIF by design
            {
                using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
                data.SaveTo(fs);
            }
        }

    }
}
namespace ExpensesAppCpp.Tesseract
{
    public class TesseractHelper
    {
        public static async Task<string> RunTesseractAsync(string imagePath)
        {
#if ANDROID
            // Copy traineddata to AppDataDirectory
            string tessDir = System.IO.Path.Combine(FileSystem.AppDataDirectory, "tessdata");
            Directory.CreateDirectory(tessDir);

            foreach (var lang in new[] { "eng.traineddata", "deu.traineddata" }) // add "deu.traineddata" if needed
            {
                string target = System.IO.Path.Combine(tessDir, lang);
                if (!File.Exists(target))
                {
                    using var input = await FileSystem.OpenAppPackageFileAsync($"tessdata/{lang}");
                    using var output = File.Create(target);
                    await input.CopyToAsync(output);
                }
            }

            var api = new TessBaseAPI();
            if (!api.Init(FileSystem.AppDataDirectory, "eng+deu")) // or "eng+deu"
                throw new Exception("Tesseract init failed");

            using var bmp = BitmapFactory.DecodeFile(imagePath);
            api.SetImage(bmp);

            string text = api.UTF8Text;
            api.Clear();
            api.Dispose();
            if (text == null)
                throw new Exception("Tesseract returned null text");
            return text;
#else
            return "Tesseract OCR only works on Android.";
#endif
        }

        public static async Task<string?> ReturnStoreName(string ocrResult, string[] possibleStores)
        {
            // Check if OCR result contains any of the possible store names
            // implement checking for partial store names & deducting the complete store name
            foreach (var store in possibleStores)
            {
                if (ocrResult.Contains(store, StringComparison.OrdinalIgnoreCase))
                {
                    return store;
                }
            }
            // If no store name found, return null
            //else use fuzzy
            string? bestMatch = null;
            int bestScore = 0;
            foreach (var store in possibleStores)
            {
                int score = Fuzz.WeightedRatio(ocrResult, store);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestMatch = store;
                }

                
            }
            return bestMatch;

        }

        public static async Task<decimal?> ReturnAmount(string ocrResult, string[] keywords)
        {
            // OCR result (simulated)
            var lines = ocrResult.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            string? totalLine = null;
            int bestScore = 0;

            // Match formats like 26.85, 1,234.50, or 12,50 (German/Swiss)
            var amountRegex = new Regex(@"(\d{1,3}(?:[.,]\d{3})*(?:[.,]\d{2}))");


            // Fuzzy match each line against total-related keywords
            foreach (var line in lines)
            {

                if (!amountRegex.IsMatch(line)) continue;

                foreach (var keyword in keywords)
                {
                    int score = Fuzz.PartialRatio(keyword.ToLower(), line.ToLower());
                    if (score > bestScore && score > 70) // threshold ~70 for fuzzy matching
                    {
                        bestScore = score;
                        totalLine = line;
                    }
                }
            }
            if (!string.IsNullOrEmpty(totalLine))
            {

                var match = amountRegex.Match(totalLine);
                if (match.Success)
                {
                    // Normalize 12,50 -> 12.50 for invariant parsing
                    var normalized = match.Value.Replace(',', '.');
                    if (decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
                    {
                        return result; // Return the parsed amount
                    }
                }
            }


            return null; // If no valid amount found
        }

        //public static async Task<DateTime> ReturnReceiptDate()
        //{

        //}
    }


}
namespace ExpensesAppCpp.FotoManagement
{
    public class FotoManagementHelper
    {
        public static async Task<string?> LoadImageFromGalery()
        {
            //check for access to storage
            var status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.StorageRead>();
                if (status != PermissionStatus.Granted)
                {
                    await ErrorHandlingHelper.ShowPopup("Storage permission is required to access the gallery.");
                    return null;
                }
            }

            var path = await FileManipulationHelper.GetUserSelectedImagePath();
            if (path == null)
            {
                await ErrorHandlingHelper.ShowPopup("No Image selected");
                return null;
            }

            return path;
        }
        public static async Task<string?> LoadImageFromCamera()
        {
            // Check and request camera permission
            var status = await Permissions.RequestAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                await ErrorHandlingHelper.ShowPopup("Camera permission is required to take a photo.");
                return null;
            }
            var path = await MediaPicker.Default.CapturePhotoAsync();
            if (path == null)
            {
                await ErrorHandlingHelper.ShowPopup("No photo taken.");
                return null;
            }
            // Take photo using MediaPicker
            return path.FullPath;
        }
    }
}
namespace ExpensesAppCpp.ErrorHandling
{
    public class ErrorHandlingHelper
    {
        public static async Task ShowPopup(string message)
        {
            var currentPage = Application.Current.Windows[0].Page;
            if (currentPage != null)
            {
                var result = await currentPage.ShowPopupAsync(new CustomPopUp(new PopUpViewModel(message)));
            }
        }
    }
}


