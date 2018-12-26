using System;
using System.IO;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel; 

namespace LinuxAcademy.AZ200.CosmosDbSample
{ 
    class Program
    {
        private static DocumentClient _client;
        private const string _databaseId = "SqlSample";
        private const string _collectionId = "Families";

        static void Main(string[] args)
        {
            _client = new DocumentClient(new Uri(ConfigurationManager.AppSettings["accountEndpoint"]), 
                                                 ConfigurationManager.AppSettings["accountKey"]);

            CreateDataAsync(_databaseId, _collectionId).Wait(); 
            ExecuteSqlQuery(_databaseId, _collectionId, 
            @"
SELECT *
FROM Families f
WHERE f.id = 'AndersenFamily'
");
            ExecuteSqlQuery(_databaseId, _collectionId, 
            @"
SELECT {""Name"":f.id, ""City"":f.address.city} AS Family
    FROM Families f
    WHERE f.address.city = f.address.state
");  
            ExecuteSqlQuery(_databaseId, _collectionId, 
            @"
SELECT c.givenName
    FROM Families f
    JOIN c IN f.children
    WHERE f.id = 'WakefieldFamily'
    ORDER BY f.address.city ASC");  

 //           ExecuteSqlQueryAsync(_databaseId, _collectionId, "SELECT * FROM Families f WHERE f.id = 'AndersenFamily'").Wait();
            //GetDocumentByIdAsync(_databaseId, _collectionId, ).Wait();
            //ReadUserDocumentAsync(_databaseId, _collectionId, "1").Wait();
            //executeLinqQuery("OnlineOrdering", "WebOrders");
            //executeSqlQuery("OnlineOrdering", "WebOrders");
            //executeJoinQuery("OnlineOrdering", "WebOrders");
        }

        private static async Task CreateDataAsync(string databaseId, string collectionId)
        {
            await _client.CreateDatabaseIfNotExistsAsync(
                new Database { 
                    Id = databaseId
                });

            await _client.CreateDocumentCollectionIfNotExistsAsync(
                UriFactory.CreateDatabaseUri(databaseId), 
                new DocumentCollection 
                { 
                    Id = collectionId,
                    PartitionKey = new PartitionKeyDefinition() { 
                        Paths = new Collection<string>(new [] { "/id" })
                    }
                });

            var family1 = JObject.Parse(File.ReadAllText("data/andersen.json"));
            var family2 = JObject.Parse(File.ReadAllText("data/wakefield.json"));

            //await CreateDocumentIfNotExistsAsync(databaseId, collectionId, family1["id"].ToString(), family1);
            await CreateDocumentIfNotExistsAsync(databaseId, collectionId, family2["id"].ToString(), family2);
/* 
            await CreateEntitiesAsync();

            var user = await ReadUserDocumentAsync("Users", "WebCustomers", "1", "mheydt");
            user.FirstName = "Bleu Braque";
            await ReplaceUserDocumentAsync("Users", "WebCustomers", user);

            await DeleteUserDocumentAsync("Users", "WebCustomers", user);
            */
            //ExecuteJoinQuery("Users", "WebCustomers");

        }

        private static void ExecuteSqlQuery(string databaseId, string collectionId, string sql)
        {
            System.Console.WriteLine("SQL: " + sql);
            // Set some common query options
            var queryOptions = new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true };

            // Here we find nelapin via their LastName
            var sqlQuery = _client.CreateDocumentQuery<JObject>(
                    UriFactory.CreateDocumentCollectionUri(databaseId, collectionId),
                    sql, queryOptions );

            foreach (var result in sqlQuery)
            {
                Console.WriteLine(result);
            }
            //foreach (var order in orderQueryInSql)
            //{
              //  Console.WriteLine("\tRead {0}", order);
            //}
        }

/* 
        private static async Task CreateOrdersAsync()
        {
            var order1 = new Order
            {
                Id = "1",
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
                Id = "2",
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
*/
        private static async Task CreateDocumentIfNotExistsAsync(
            string databaseId, 
            string collectionId,
            string documentId,
            JObject data)
        {
            try
            {
                await _client.ReadDocumentAsync(UriFactory.CreateDocumentUri(
                    databaseId, collectionId, documentId),
                    new RequestOptions { 
                        PartitionKey = new PartitionKey(documentId) 
                    });
                Console.WriteLine($"Order {documentId} already exists in the database");
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await _client.CreateDocumentAsync(
                        UriFactory.CreateDocumentCollectionUri(databaseId, collectionId), data);
                    Console.WriteLine($"Created Order {documentId}");
                }
                else
                {
                    throw;
                }
            }
        }

        private static async Task<string> GetDocumentByIdAsync(
            string databaseId, 
            string collectionId,
            string documentId)
        {
            var response = await _client.ReadDocumentAsync(
                UriFactory.CreateDocumentUri(databaseId, collectionId, documentId),
                new RequestOptions { 
                    PartitionKey = new PartitionKey(Undefined.Value) 
                }
            );

            Console.WriteLine(response.Resource);

            return "";
        }

        private static async Task<Order> ReadUserDocumentAsync(string databaseName, string collectionName, string orderId)
        {
            try
            {
                var result = await _client.ReadDocumentAsync(
                    UriFactory.CreateDocumentUri(databaseName, collectionName, orderId), 
                    new RequestOptions { PartitionKey = new PartitionKey(orderId) }
                );

                Console.WriteLine($"Read user {orderId}");

                return null;
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    Console.WriteLine($"User {orderId} not read");
                }
                throw;
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
