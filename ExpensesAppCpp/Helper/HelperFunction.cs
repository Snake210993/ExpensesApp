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
        


        private static Task<double> ComputeHorizontalProjectionScoreAsync(SKBitmap bmp)
        {
            return Task.Run(() => ComputeHorizontalProjectionScore(bmp));
        }
        public static Task<SKBitmap> RotateBitmapAsync(SKBitmap src, double angleDegrees)
        {
            return Task.Run(() => RotateBitmap(src, angleDegrees));
        }

        public static SKBitmap RotateBitmap(SKBitmap src, double angleDegrees)
        {
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
            // SKBitmap.Pixels gives you an SKColor[] you can index directly:
            var colors = bmp.Pixels;
            int W = bmp.Width, H = bmp.Height;
            double score = 0;

            for (int y = 0; y < H; y++)
            {
                int rowSum = 0;
                int off = y * W;
                for (int x = 0; x < W; x++)
                {
                    if (colors[off + x].Red == 0)
                        rowSum++;
                }
                score += (long)rowSum * rowSum;
            }

            return score;
        }

        public static Task<double> EstimateSkewAngleAsync(SKBitmap binaryBitmap)
        {
            return Task.Run(() =>
            {
                int angleSteps = 30;    // test –30…+30 steps => –30°…+30°
                double angleStepSize = 1.0;  // degrees

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
            });
        }

        // replace your current EstimateSkewAngleFast with this:
        public static async Task<double> EstimateSkewAngleFast(SKBitmap binaryBitmap, int proxyWidth = 600)
        {
            // 1) make a small proxy
            using var proxy = ResizeBitmap(binaryBitmap, proxyWidth);

            // 2) await the async skew estimator on the proxy
            return await EstimateSkewAngleAsync(proxy);
        }


        public static SKBitmap ResizeBitmap(SKBitmap src, int targetWidth)
        {
            // preserve aspect ratio
            int targetHeight = (int)(targetWidth / (float)src.Width * src.Height);

            var info = new SKImageInfo(targetWidth, targetHeight, src.ColorType, src.AlphaType);

            // cubic resampler (Mitchell) is exposed as a property, not a method
            var sampling = new SKSamplingOptions(SKCubicResampler.Mitchell);

            return src.Resize(info, sampling);
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

        public static async Task<decimal?> ReturnAmount(string ocrResult, string[] keywords, string[] blackList)
        {
            var lines = ocrResult.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var amountRegex = new Regex(@"(CHF|Fr\.?)\s?(\d+[.,]\d{2})", RegexOptions.IgnoreCase);

            decimal? bestAmount = null;
            int bestScore = 0;

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                // Skip if blacklisted
                if (blackList.Any(black => line.Contains(black, StringComparison.OrdinalIgnoreCase)))
                    continue;

                var match = amountRegex.Match(line);
                if (!match.Success) continue;

                var numericPart = match.Groups[2].Value.Replace(',', '.');
                if (!decimal.TryParse(numericPart, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
                    continue;

                // Check this line + neighbors for total keywords
                string context = line;
                if (i > 0) context += " " + lines[i - 1];
                if (i < lines.Length - 1) context += " " + lines[i + 1];

                int score = 0;
                foreach (var keyword in keywords)
                {
                    score = Math.Max(score, Fuzz.PartialRatio(keyword.ToLower(), context.ToLower()));
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestAmount = value;
                }
            }

            return bestAmount;
        }


        public static async Task<DateTime> ReturnDate(string ocrResult)
        {
            // 1) Match exactly dd.MM.yy OR dd.MM.yyyy
            var dateRegex = new Regex(@"\b(\d{2}\.\d{2}\.\d{2,4})\b");
            var match = dateRegex.Match(ocrResult);
            if (!match.Success)
                return DateTime.MinValue;

            var dateString = match.Value;
            // 2) Split day, month, year parts
            var parts = dateString.Split('.');
            if (parts.Length != 3)
                return DateTime.MinValue;

            if (parts[2].Length == 2)
            {
                // Two-digit year → force 2000+
                if (int.TryParse(parts[0], out int day)
                 && int.TryParse(parts[1], out int month)
                 && int.TryParse(parts[2], out int yy))
                {
                    int year = 2000 + yy;
                    // Validate range (optional)
                    if (month >= 1 && month <= 12
                     && day >= 1 && day <= DateTime.DaysInMonth(year, month))
                    {
                        return new DateTime(year, month, day);
                    }
                }
            }
            else if (parts[2].Length == 4)
            {
                // Four-digit year → parse with exact format & German culture
                if (DateTime.TryParseExact(
                        dateString,
                        "dd.MM.yyyy",
                        CultureInfo.GetCultureInfo("de-CH"),
                        DateTimeStyles.None,
                        out DateTime fullYear))
                {
                    return fullYear;
                }
            }

            return DateTime.MinValue;
        }

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

        public static async Task<bool> ShowConfirmationPopup(string message)
        {
            var currentPage = Application.Current.Windows[0].Page;
            if (currentPage != null)
            {
                var viewModel = new PopUpViewModel(message, true);
                var result = await currentPage.ShowPopupAsync(new CustomPopUp(viewModel));
                return await viewModel.GetResultTask(); // Return the result of the confirmation
            }
            return false; // Default to false if no page is available
        }
    }
}





