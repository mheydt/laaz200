using System;
using System.IO;
using Microsoft.WindowsAzure.Storage; 
using Microsoft.WindowsAzure.Storage.Blob; 
using Microsoft.WindowsAzure.Storage.File; 

namespace LinuxAcademy.AZ200.StorageSamples
{
    public class Common
    {
        public static string getConnectionString()
        {
            var acctName = "laaz200stg"; //Environment.GetEnvironmentVariable("LAAZ200STGACCTNAME");
            var acctKey = "XT9jit6sY/iDU3VPnquj6N/jHoymAW7N9R/GPvP5ihoTTTm+9aExlQo9bNU7w84gKHIIPk4C+qPHKoRU5cpIBQ=="; //Environment.GetEnvironmentVariable("LAAZ200STGACCTKEY");
            var connectionString = 
                $"DefaultEndpointsProtocol=https;AccountName={acctName};AccountKey={acctKey};EndpointSuffix=core.windows.net";

            return connectionString;
        }

        public static CloudStorageAccount getCloudStorageAccount()
        {
            var connectionString = getConnectionString();
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            return storageAccount;
        }
    }
}