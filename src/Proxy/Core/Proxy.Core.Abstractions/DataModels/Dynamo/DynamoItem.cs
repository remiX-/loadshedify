using Amazon.DynamoDBv2.Model;

namespace Proxy.Core.DataModels.Dynamo;

public class DynamoItem : Dictionary<string, AttributeValue>
{
  public DynamoItem(IDictionary<string, AttributeValue> attributes) : base(attributes)
  { }
}
