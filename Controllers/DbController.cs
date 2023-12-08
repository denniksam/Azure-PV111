using Azure;
using Azure_PV111.Models.Home.Db;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace Azure_PV111.Controllers
{
    [Route("api/db")]
    [ApiController]
    public class DbController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DbController> _logger;

        public DbController(IConfiguration configuration, ILogger<DbController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private static Container? _container;
        private async Task<Container> GetDbContainer()
        {
            if (_container != null) return _container;

            String? endpoint = _configuration.GetSection("CosmosDb").GetSection("Endpoint").Value;
            String? key = _configuration.GetSection("CosmosDb").GetSection("Key").Value;
            String? databaseId = _configuration.GetSection("CosmosDb").GetSection("DatabaseId").Value;
            String? containerId = _configuration.GetSection("CosmosDb").GetSection("ContainerId").Value;

            CosmosClient cosmosClient = new(
                endpoint, key,
                new CosmosClientOptions()
                {
                    ApplicationName = "Azure_PV111"
                });
            Database database = await cosmosClient
                .CreateDatabaseIfNotExistsAsync(databaseId);
            _container = await database
                .CreateContainerIfNotExistsAsync(containerId, "/partitionKey");
            return _container;
            /*
            int rnd = new Random().Next(100);

            Models.Home.Db.Test data = new()
            {
                Id = Guid.NewGuid().ToString(),
                PartitionKey = rnd.ToString(),
                Data = $"Random {rnd}"
            };
            ItemResponse<Models.Home.Db.Test> response =
                await container.CreateItemAsync<Models.Home.Db.Test>(
                    data,
                    new PartitionKey(data.PartitionKey)
                );*/
        }


        [HttpPost]
        public async Task<object> AddProducer(ProducerFormModel formModel)
        {
            Container dbContainer = await GetDbContainer();
            ProducerDataModel data = new() {
                Id = Guid.NewGuid(),
                Name = formModel.Name,
                Products = new(),
            };
            ItemResponse<ProducerDataModel> response =
                await dbContainer.CreateItemAsync<ProducerDataModel>(
                    data,
                    new PartitionKey(data.PartitionKey)
                );
            return response.StatusCode;
        }

        [HttpGet]
        public async Task<IEnumerable<object>> GetItemsAsync(String type)
        {
            Container dbContainer = await GetDbContainer();
            QueryDefinition query = new($"SELECT * FROM c WHERE c.type='{type}'");
            FeedIterator<ProducerDataModel> feedIterator = 
                dbContainer.GetItemQueryIterator<ProducerDataModel>(query);
            List<ProducerDataModel> res = new();
            while(feedIterator.HasMoreResults)
            {
                FeedResponse<ProducerDataModel> response = 
                    await feedIterator.ReadNextAsync();
                foreach(ProducerDataModel item in response)
                {
                    res.Add(item);
                }
            }
            return res;
        }

        [HttpDelete]
        public async Task<object> DeleteProducer(String producerId)
        {
            Container dbContainer = await GetDbContainer();
            ItemResponse<ProducerDataModel> response = await
                dbContainer.DeleteItemAsync<ProducerDataModel>(
                    producerId,
                    new PartitionKey(ProducerDataModel.DataType)
                );
            return new { status = response.StatusCode };
        }

        [HttpPut]
        public async Task<object> UpdateProducer(String producerId, String newName)
        {
            Container dbContainer = await GetDbContainer();
            ItemResponse<ProducerDataModel> response = await
                dbContainer.ReadItemAsync<ProducerDataModel>(
                    producerId,
                    new PartitionKey(ProducerDataModel.DataType));

            ProducerDataModel producer = response.Resource;

            producer.Name = newName;

            response = await
                dbContainer.ReplaceItemAsync<ProducerDataModel>(
                    producer,
                    producerId,
                    new PartitionKey(ProducerDataModel.DataType));

            return new { status = response.StatusCode };
        }

        public async Task<object> NonStdRouter()
        {
            _logger.LogInformation(
                "NonStdRouter method {method}",
                HttpContext.Request.Method);

            return HttpContext.Request.Method switch
            {
                "LINK" => await LinqGet(),
                "ADD" => await AddProduct(),
                _ => new { HttpContext.Request.Method }
            };
                
        }

        private async Task<object> LinqGet()
        {
            Container dbContainer = await GetDbContainer();
            return dbContainer
                .GetItemLinqQueryable<ProducerDataModel>(true)
                .Where(item => item.Type == ProducerDataModel.DataType)
                .ToList();
        }
        private async Task<object> AddProduct()
        {
            Container dbContainer = await GetDbContainer();
            using StreamReader stream = new(HttpContext.Request.Body);
            String body = await stream.ReadToEndAsync();
            var product =
                JsonConvert.DeserializeObject<ProductFormModel>(body);
            var producer = dbContainer
                .GetItemLinqQueryable<ProducerDataModel>(true)
                .ToList()
                .FirstOrDefault(p => p.Id == product.producerId);
            if(producer.Products == null)
            {
                producer.Products = new();
            }
            producer.Products.Add(new()
            {
                Id = Guid.NewGuid(),
                Name = product.name,
                Year = product.year
            });

            var response = await
                dbContainer.ReplaceItemAsync<ProducerDataModel>(
                    producer,
                    producer.Id.ToString(),
                    new PartitionKey(ProducerDataModel.DataType));

            return new { status = response.StatusCode };
        }
    }
}
/* Д.З. Реалізувати відображення "підлеглих" колекцій
 * "товари" з елементів контейнера "виробники"
 * у складі HTML, який відображає наявні позиції Products
 */
