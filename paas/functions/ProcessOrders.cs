using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace LaAz200Functions
{
    public static class OrderProcessor
    {
        [FunctionName("ProcessOrders")]
        public static void ProcessOrders(
            [QueueTrigger("incoming-orders", Connection = "AzureWebJobsStorage")]CloudQueueMessage myQueueItem,
            [Table("Orders")]ICollector<Order> tableBindings,
            ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
        
        }

        [FunctionName("ProcessOrders-Poison")]
        public static void ProcessFailedOrders([QueueTrigger("incoming-orders-poison", Connection = "AzureWebJobsStorage")]string myQueueItem, 
        ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
        }
    }
}
