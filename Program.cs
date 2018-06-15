using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureUpload
{
    class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var screenshotPath = ScreenshotLocator.FindMostRecentScreenshot();
            if (screenshotPath == null)
            {
                Console.Error.WriteLine("No screenshot found to upload.");
                return -1;
            }

            var storageConnectionString = Environment.GetEnvironmentVariable("AZ_STORAGE_CONNECTION_STRING");
            if (string.IsNullOrEmpty(storageConnectionString))
            {
                Console.Error.WriteLine("An Azure Storage connection string must be specified using the AZ_STORAGE_CONNECTION_STRING environment variable.");
                return -2;
            }

            var storageContainerName = Environment.GetEnvironmentVariable("AZ_STORAGE_CONTAINER_NAME");
            if (string.IsNullOrEmpty(storageContainerName))
            {
                Console.Error.WriteLine("An Azure Storage account name  must be specified using the AZ_STORAGE_CONTAINER_NAME environment variable.");
                return -3;
            }


            if (!CloudStorageAccount.TryParse(storageConnectionString, out var account))
            {
                Console.Error.WriteLine("Failed to parse Azure Storage connection string.");
                return -4;
            }

            try
            {
                var blobClient = account.CreateCloudBlobClient();
                var container = blobClient.GetContainerReference(storageContainerName);
                
                var fileName = Path.GetFileName(screenshotPath);
                var blob = container.GetBlockBlobReference(fileName);
                await blob.UploadFromFileAsync(screenshotPath, AccessCondition.GenerateIfNotExistsCondition(), default(BlobRequestOptions), default(OperationContext)).ConfigureAwait(false);

                if (Environment.GetEnvironmentVariable("AZ_CDN_ROOT") is var cdnRoot && !string.IsNullOrEmpty(cdnRoot))
                {
                    var uri = new UriBuilder(cdnRoot);
                    uri.Path = fileName;
                    Console.WriteLine(uri.ToString());
                }
                else
                {
                    Console.WriteLine(blob.Uri);
                }

                return 0;
            }
            catch (StorageException ex)
            {
                Console.Error.WriteLine(ex.ToString());
                return -5;
            }
        }
    }
}
