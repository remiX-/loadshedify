using Amazon;
using Amazon.DynamoDBv2;
using Microsoft.Extensions.DependencyInjection;
using Proxy.Core.Model;
using Proxy.Core.Services;

namespace Proxy.Core;

public static class DynamoIoC
{
  public static IServiceCollection WithDynamoDb(this IServiceCollection collection)
  {
    collection
      .AddSingleton<IAmazonDynamoDB>(sp =>
      {
        var envModel = sp.GetService<IEnvironmentModel>()!;
        var endpoint = RegionEndpoint.GetBySystemName(envModel.Get("AWS_REGION"));
        return new AmazonDynamoDBClient(endpoint);
      })
      .AddSingleton<IDynamoService, DynamoService>();

    return collection;
  }
}
