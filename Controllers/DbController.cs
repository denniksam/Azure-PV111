using Azure_PV111.Models.Home.Db;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;

namespace Azure_PV111.Controllers
{
    [Route("api/db")]
    [ApiController]
    public class DbController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public DbController(IConfiguration configuration)
        {
            _configuration = configuration;
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


    }
}
/* Д.З. Реалізувати відображення даних з контейнера CosmosDb
 * з типом "Producer" у вигляді HTML таблиці (№  Ім'я)
 */
