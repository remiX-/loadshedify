// using System.Net;
// using Amazon.DynamoDBv2;
// using Amazon.DynamoDBv2.Model;
// using Microsoft.Extensions.Logging;
// using Proxy.Core.DataModels.Dynamo;
//
// namespace Proxy.Core.Services;
//
// public class DynamoService : IDynamoService
// {
//   private readonly IAmazonDynamoDB _dynamoDbClient;
//   private readonly ILogger<DynamoService> _logger;
//
//   // TODO more commands here
//   // https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/LowLevelDotNetItemCRUD.html#GetItemLowLevelDotNET
//
//   public DynamoService(IAmazonDynamoDB dynamoDbClient, ILogger<DynamoService> logger)
//   {
//     _dynamoDbClient = dynamoDbClient;
//     _logger = logger;
//   }
//   public async Task<bool> PutItem(PutItemRequest request)
//   {
//     try
//     {
//       var response = await _dynamoDbClient.PutItemAsync(request);
//
//       if (response.HttpStatusCode is HttpStatusCode.OK)
//       {
//         _logger.LogInformation(" > PutItem: success");
//         return true;
//       }
//
//       _logger.LogInformation($" > PutItem failed: {response.HttpStatusCode}");
//     }
//     catch (Exception ex)
//     {
//       _logger.LogError($" > PutItem exception: {ex.Message}");
//     }
//
//     return false;
//   }
//
//   public async Task<(bool success, DynamoItem item)> GetItem(string table, string id)
//   {
//     try
//     {
//       var request = new GetItemRequest
//       {
//         TableName = table,
//         Key = new Dictionary<string, AttributeValue>
//         {
//           { TableNameKeys.Keys[table], new AttributeValue(id) },
//         }
//       };
//
//       var response = await _dynamoDbClient.GetItemAsync(request);
//
//       if (response.HttpStatusCode is HttpStatusCode.OK)
//       {
//         _logger.LogInformation(" > GetItem: success");
//         return (response.IsItemSet, new DynamoItem(response.Item));
//       }
//
//       _logger.LogInformation($" > GetItem failed: {response.HttpStatusCode}");
//     }
//     catch (Exception ex)
//     {
//       _logger.LogError($" > GetItem exception: {ex.Message}");
//     }
//
//     return (false, null);
//   }
//
//   public async Task<IList<DynamoItem>> ScanTable(string table)
//   {
//     try
//     {
//       var request = new ScanRequest
//       {
//         TableName = table
//       };
//       var response = await _dynamoDbClient.ScanAsync(request);
//
//       if (response.HttpStatusCode is HttpStatusCode.OK)
//       {
//         _logger.LogInformation($" > GetItemsByAttribute success");
//         return response.Items.Select(x => new DynamoItem(x)).ToList();
//       }
//
//       _logger.LogInformation($" > GetItemsByAttribute: failed: {response.HttpStatusCode}");
//     }
//     catch (Exception ex)
//     {
//       _logger.LogError($" > GetItemsByAttribute exception: {ex.Message}");
//     }
//
//     return default;
//   }
// }
