using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using System.Collections.ObjectModel; 

namespace LinuxAcademy.AZ200.CosmosDbSample
{ 
    class Program
    {
        private static DocumentClient _client;

        static void Main(string[] args)
        {
            _client = new DocumentClient(new Uri(ConfigurationManager.AppSettings["accountEndpoint"]), 
                                                 ConfigurationManager.AppSettings["accountKey"]);


            //createData().Wait();            
            //executeLinqQuery("OnlineOrdering", "WebOrders");
            //executeSqlQuery("OnlineOrdering", "WebOrders");
            executeJoinQuery("OnlineOrdering", "WebOrders");
        }

        private static async Task createData()
        {
            await _client.CreateDatabaseIfNotExistsAsync(
                new Database { 
                    Id = "OnlineOrdering" 
                });

            await _client.CreateDocumentCollectionIfNotExistsAsync(
                UriFactory.CreateDatabaseUri("OnlineOrdering"), 
                new DocumentCollection 
                { 
                    Id = "WebOrders",
                    PartitionKey = new PartitionKeyDefinition() { 
                        Paths = new Collection<string>(new [] { "/OrderId" })
                    }
                });

                await CreateOrdersAsync();
/* 
            await CreateEntitiesAsync();

            var user = await ReadUserDocumentAsync("Users", "WebCustomers", "1", "mheydt");
            user.FirstName = "Bleu Braque";
            await ReplaceUserDocumentAsync("Users", "WebCustomers", user);

            await DeleteUserDocumentAsync("Users", "WebCustomers", user);
            */
            //ExecuteJoinQuery("Users", "WebCustomers");

        }

        private static async Task CreateOrdersAsync()
        {
            var order1 = new Order
            {
                OrderId = "1",
                Customer = new Customer
                {
                    CustomerId = "1",
                    Name = "Mike"
                },
                OrderItems = new []
                {
                    new OrderItem
                    {
                        OrderItemId = "1",
                        Description = "RAM",
                        Quantity = 2,
                        Price = 100.00
                    },
                    new OrderItem
                    {
                        OrderItemId = "2",
                        Description = "CPU",
                        Quantity = 1,
                        Price = 500.00
                    }
                },
                ShipTo = new Address
                {
                    City = "New York"
                }
            };


            await CreateUserDocumentIfNotExistsAsync("OnlineOrdering", "WebOrders", order1);

            var order2 = new Order
            {
                OrderId = "2",
                Customer = new Customer
                {
                    CustomerId = "2",
                    Name = "Bleu"
                },
                OrderItems = new []
                {
                    new OrderItem
                    {
                        OrderItemId = "1",
                        Description = "Dog Food",
                        Quantity = 1,
                        Price = 25.00
                    }
                },
                ShipTo = new Address
                {
                    City = "Los Angeles"
                }
            };

            await CreateUserDocumentIfNotExistsAsync("OnlineOrdering", "WebOrders", order2);
        }

        private static async Task CreateUserDocumentIfNotExistsAsync(
            string databaseName, 
            string collectionName,
            Order order)
        {
            try
            {
                await _client.ReadDocumentAsync(UriFactory.CreateDocumentUri(
                    databaseName, collectionName, order.OrderId), 
                    new RequestOptions { 
                        PartitionKey = new PartitionKey(order.OrderId) 
                });
                Console.WriteLine($"Order {0} already exists in the database", order.OrderId);
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await _client.CreateDocumentAsync(
                        UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), order);
                    Console.WriteLine($"Created Order {0}", order.OrderId);
                }
                else
                {
                    throw;
                }
            }
        }


        private static void executeLinqQuery(string databaseName, string collectionName)
        {
            // Set some common query options
            var queryOptions = new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true };

            // Here we find nelapin via their LastName
            var ordersQuery = _client.CreateDocumentQuery<Order>(
                    UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), queryOptions)
                    .Where(o => o.Customer.Name == "Mike");

            // The query is executed synchronously here, but can also be executed asynchronously via the IDocumentQuery<T> interface
            Console.WriteLine("Running LINQ query...");
            foreach (var order in ordersQuery)
            {
                Console.WriteLine("\tRead {0}", order);
            }
        }

        private static void executeSqlQuery(string databaseName, string collectionName)
        {
            // Set some common query options
            var queryOptions = new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true };

            // Here we find nelapin via their LastName
            var orderQueryInSql = _client.CreateDocumentQuery<User>(
                    UriFactory.CreateDocumentCollectionUri(databaseName, collectionName),
                    "SELECT * FROM c WHERE c.Customer.Name = 'Mike'", queryOptions );

            Console.WriteLine("Running direct SQL query...");
            foreach (var order in orderQueryInSql)
            {
                Console.WriteLine("\tRead {0}", order);
            }
        }

        private static void executeJoinQuery(string databaseName, string collectionName)
        {
            // Set some common query options
            var queryOptions = new FeedOptions { 
                MaxItemCount = -1, 
                EnableCrossPartitionQuery = true 
            };

            var joinSql = _client.CreateDocumentQuery<OrderItem>(
                    UriFactory.CreateDocumentCollectionUri(databaseName, collectionName),
//                    "SELECT oh.OrderId, oh.DateShipped, oh.Total FROM c JOIN oh IN c.OrderHistory ORDER BY oh.DateShipped", queryOptions );
                    "SELECT oi.Description, oi.Quantity, oi.Price FROM o JOIN oi IN o.OrderItems ORDER BY oi.Description", queryOptions );

            // wow, this is a question but the above throws an error as order by on correlated questions is not supported
            try
            {
                var results = joinSql.ToList();

                Console.WriteLine("Running direct SQL query...");
                foreach (var orderItem in joinSql)
                {
                    System.Console.WriteLine(orderItem);
                    //Console.WriteLine("\tRead {0} {1} {2}", orderHistory.OrderId, orderHistory.DateShipped, orderHistory.Total);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine(ex.InnerException.Message);
                }
            }
        }
    }

     /* 
        private async Task<Order> ReadUserDocumentAsync(
            string databaseName, string collectionName, string id, string userId)
        {
            try
            {
                var response = await _client.ReadDocumentAsync(
                    UriFactory.CreateDocumentUri(databaseName, collectionName, id),
                    new RequestOptions { PartitionKey = new PartitionKey(userId) });

                Console.WriteLine($"Read user {0}", userId);

                var order = (Order)(dynamic)response.Resource;
                return user;
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    Console.WriteLine($"User {0} not read", userId);
                }
                throw;
            }
        }

        private async Task ReplaceUserDocumentAsync(string databaseName, string collectionName, User updatedUser)
        {
            try
            {
                await _client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, updatedUser.Id), updatedUser, new RequestOptions { PartitionKey = new PartitionKey(updatedUser.UserId) });
                Console.WriteLine($"Replaced last name for {0}", updatedUser.LastName);
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    Console.WriteLine($"User {0} not found for replacement", updatedUser.Id);
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task DeleteUserDocumentAsync(string databaseName, string collectionName, User user)
        {
            try
            {
                await _client.DeleteDocumentAsync(
                    UriFactory.CreateDocumentUri(databaseName, collectionName, user.Id), 
                    new RequestOptions { PartitionKey = new PartitionKey(user.UserId) });
                Console.WriteLine("Deleted user {0}", user.Id);
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    Console.WriteLine("User {0} not found for deletion", user.Id);
                }
                else
                {
                    throw;
                }
            }
        }
*/

    }
