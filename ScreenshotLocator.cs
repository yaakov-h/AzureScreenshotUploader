using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using static System.Environment;

namespace AzureUpload
{
    class ScreenshotLocator
    {
        public static string FindMostRecentScreenshot()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                throw new PlatformNotSupportedException("Only macOS implemented.");
            }

            var desktop = Environment.GetFolderPath(SpecialFolder.DesktopDirectory);
            var screenshots = new DirectoryInfo(desktop).EnumerateFiles("Screen Shot ????-??-?? at ??.??.?? ?m.png", SearchOption.TopDirectoryOnly);
            return screenshots.OrderByDescending(s => s.CreationTimeUtc).FirstOrDefault()?.FullName;
        }
    }
}