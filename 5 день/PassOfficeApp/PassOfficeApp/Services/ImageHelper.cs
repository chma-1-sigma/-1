using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace PassOfficeApp.Services
{
    public static class ImageHelper
    {
        private static string _imagePath;

        public static void Initialize(string basePath)
        {
            _imagePath = Path.Combine(basePath, "Images");
            if (!Directory.Exists(_imagePath))
            {
                Directory.CreateDirectory(_imagePath);
            }
        }

        public static string GetImagePath(string imageName)
        {
            return Path.Combine(_imagePath, imageName);
        }

        public static BitmapImage LoadImage(string imageName)
        {
            try
            {
                string path = GetImagePath(imageName);
                if (File.Exists(path))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(path, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    return bitmap;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading image {imageName}: {ex.Message}");
            }
            return null;
        }
    }
}