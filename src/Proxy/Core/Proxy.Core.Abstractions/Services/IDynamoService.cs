// using Amazon.DynamoDBv2.Model;
// using Proxy.Core.DataModels.Dynamo;
//
// namespace Proxy.Core.Services;
//
// public interface IDynamoService
// {
//   Task<bool> PutItem(PutItemRequest request);
//
//   Task<(bool success, DynamoItem item)> GetItem(string table, string id);
//
//   Task<IList<DynamoItem>> ScanTable(string table);
// }
