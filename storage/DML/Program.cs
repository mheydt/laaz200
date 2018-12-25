﻿using System; 
using System.Linq;
using System.IO;
using System.Threading; 
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage; 
using Microsoft.WindowsAzure.Storage.Blob; 
using Microsoft.WindowsAzure.Storage.DataMovement;

namespace DML
{
    class Program
    {
        static void Main(string[] args)
        {
            TransferFilesAsync().Wait();
        }

        static async Task TransferFilesAsync()
        {
            var primaryStorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=laaz200dmlprimary;AccountKey=hL3PQZmxmZEnYld8eGreOg/REk0ZgD/bmCWGmSgC3ktINsivocoz0J1Vz9mmqhznYY5W4dRCJB/JwJP33WYAAQ==;EndpointSuffix=core.windows.net"; 
            var secondaryStorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=laaz200dmlsecondary;AccountKey=BeXPdAagqD17tHhxgSa+46zz3jYwX90Y6yG8vqxAVXAIFwGd/xQF9rXGE6n1gUX9ISMri2xW62OlXn/Vlf2jyQ==;EndpointSuffix=core.windows.net"; 

            var primaryAccount = CloudStorageAccount.Parse(primaryStorageConnectionString); 
            var primaryBlobClient = primaryAccount.CreateCloudBlobClient(); 
            var primaryBlobContainer = primaryBlobClient.GetContainerReference("files");
            await primaryBlobContainer.CreateIfNotExistsAsync(); 

            var secondaryAccount = CloudStorageAccount.Parse(secondaryStorageConnectionString); 
            var secondaryBlobClient = secondaryAccount.CreateCloudBlobClient(); 
            var secondaryBlobContainer = secondaryBlobClient.GetContainerReference("files");
            await secondaryBlobContainer.CreateIfNotExistsAsync(); 

            var files = Directory.GetFiles("files").ToList();
            var tasks = files.Select(file =>
            {
                var copyOptions = new CopyOptions { };
                var context = new SingleTransferContext();
                context.ShouldOverwriteCallbackAsync = (source, destination) => Task.FromResult(true);

                return Task.Run(async () =>
                {
                    var fileName = Path.GetFileName(file);
                    var primaryBlob = primaryBlobContainer.GetBlockBlobReference(fileName);

                    await TransferManager.UploadAsync(file, primaryBlob, null, context);

                    var secondaryBlob = secondaryBlobContainer.GetBlockBlobReference(fileName);
                    await TransferManager.CopyAsync(primaryBlob, secondaryBlob, isServiceCopy: true, context: context, options: copyOptions);
                });
            }).ToArray();
            
            Task.WaitAll(tasks);
        }
    }
}
