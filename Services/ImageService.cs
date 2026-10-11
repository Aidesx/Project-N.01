using Microsoft.AspNetCore.Http;
using SkiaSharp;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Project_385.Services
{
    public interface IImageService
    {
        Task<string> SaveAndResizeImageAsync(IFormFile file, string targetDirectory, int maxWidth = 800, int maxHeight = 800);
        void DeleteImage(string targetDirectory, string fileName);
    }

    public class ImageService : IImageService
    {
        public async Task<string> SaveAndResizeImageAsync(IFormFile file, string targetDirectory, int maxWidth = 800, int maxHeight = 800)
        {
            if (file == null || file.Length == 0) return null;

            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileNameWithoutExtension(file.FileName)}{extension}";
            string fullPath = Path.Combine(targetDirectory, uniqueFileName);

            using var inputStream = file.OpenReadStream();
            using var originalBitmap = SKBitmap.Decode(inputStream);

            if (originalBitmap == null)
            {
                using var fileStream = new FileStream(fullPath, FileMode.Create);
                await file.CopyToAsync(fileStream);
                return uniqueFileName;
            }

            int originalWidth = originalBitmap.Width;
            int originalHeight = originalBitmap.Height;

            int newWidth = originalWidth;
            int newHeight = originalHeight;

            if (originalWidth > maxWidth || originalHeight > maxHeight)
            {
                float ratioX = (float)maxWidth / originalWidth;
                float ratioY = (float)maxHeight / originalHeight;
                float ratio = Math.Min(ratioX, ratioY);

                newWidth = Math.Max(1, (int)(originalWidth * ratio));
                newHeight = Math.Max(1, (int)(originalHeight * ratio));
            }

            using var resizedBitmap = originalBitmap.Resize(new SKImageInfo(newWidth, newHeight), SKSamplingOptions.Default);
            using var image = SKImage.FromBitmap(resizedBitmap ?? originalBitmap);

            SKEncodedImageFormat format = extension switch
            {
                ".png" => SKEncodedImageFormat.Png,
                ".webp" => SKEncodedImageFormat.Webp,
                _ => SKEncodedImageFormat.Jpeg
            };

            using var data = image.Encode(format, 85);
            using var outputStream = File.OpenWrite(fullPath);
            data.SaveTo(outputStream);

            return uniqueFileName;
        }

        public void DeleteImage(string targetDirectory, string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || string.Equals(fileName, "default.png", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            string fullPath = Path.Combine(targetDirectory, fileName);
            if (File.Exists(fullPath))
            {
                try
                {
                    File.Delete(fullPath);
                }
                catch
                {
                }
            }
        }
    }
}
