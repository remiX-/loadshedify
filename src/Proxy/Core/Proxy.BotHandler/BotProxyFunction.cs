﻿using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Runtime.Internal;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Logging;
using Proxy.Core;
using Proxy.Core.Services;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace Proxy.BotHandler;

public class BotProxyFunction
{
  private readonly JsonService _jsonService;
  private readonly ILogger<BotProxyFunction> _logger;

  public BotProxyFunction()
  {
    Console.WriteLine("BotProxyFunction.ctor");

    Shell.ConfigureServices();

    _jsonService = Shell.Get<JsonService>();
    _logger = Shell.Get<ILogger<BotProxyFunction>>();
  }

  public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
  {
    // Checking signature (requirement 1.)
    var isVerified = VerifySignature(request.Body, request.Headers);
    if (!isVerified)
    {
      return new APIGatewayProxyResponse
      {
        StatusCode = (int)HttpStatusCode.Unauthorized,
        Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } },
        Body = "Invalid request signature"
      };
    }

    // Replying to Discord bot ping (requirement 2.)
    var body = _jsonService.Deserialize<DiscordProxyRequest>(request.Body);
    _logger.LogDebug(_jsonService.Serialize(body));
    if (body.Type == 1)
    {
      _logger.LogDebug("PONG!\n" + _jsonService.Serialize(body));
      return new APIGatewayProxyResponse
      {
        StatusCode = (int)HttpStatusCode.Unauthorized,
        Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
        Body = _jsonService.Serialize(new { type = 1 })
      };
    }

    // Verify command name is valid
    if (body.Data.Name is null)
    {
      return new APIGatewayProxyResponse { StatusCode = (int)HttpStatusCode.NotFound };
    }

    // Handle command in a new thread to be able to respond to Discord asap
    // Sends a notification via SNS and splits to one of Lambdas
    await SnsSend(request.Body, body.Data.Name);

    return new APIGatewayProxyResponse
    {
      StatusCode = (int)HttpStatusCode.OK,
      Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
      Body = _jsonService.Serialize(new
      {
        type = 4,
        data = new
        {
          content = "*⏳ Loading...*"
        }
      })
    };
  }

  private bool VerifySignature(string body, IDictionary<string, string> headers)
  {
    if (headers.ContainsKey("test")) return true;

    // TODO properly verify signature
    // var botPublicKey = Environment.GetEnvironmentVariable("PUBLIC_KEY");
    // var signature = headers["x-signature-ed25519"];
    // var timestamp = headers["x-signature-timestamp"];

    // const isVerified = nacl.sign.detached.verify(
    //   Buffer.from(timestamp + strBody),
    //   Buffer.from(signature, 'hex'),
    //   Buffer.from(PUBLIC_KEY, 'hex')
    // );
    //
    // if (!isVerified) {
    //   return {
    //     statusCode: 401,
    //     body: JSON.stringify('invalid request signature'),
    //   };
    // }

    return true;
  }

  private async Task SnsSend(string body, string commandName)
  {
    var topicArn = Environment.GetEnvironmentVariable("TOPIC_ARN");

    var snsParams = new PublishRequest
    {
      Message = body,
      Subject = $"Discord bot SNS message: {commandName}",
      TopicArn = topicArn,
      MessageAttributes = new Dictionary<string, MessageAttributeValue>()
      {
        {
          "command",
          new MessageAttributeValue
          {
            DataType = "String",
            StringValue = commandName
          }
        }
      },
    };

    _logger.LogDebug(_jsonService.Serialize(snsParams));
    _logger.LogDebug("Sending SNS...");

    using var snsClient = new AmazonSimpleNotificationServiceClient();
    var snsResponse = await snsClient.PublishAsync(snsParams);

    _logger.LogDebug(_jsonService.Serialize(snsResponse));
  }
}

internal struct DiscordProxyRequest
{
  public int Type { get; init; }
  public DiscordCommandData Data { get; init; }
}

internal struct DiscordCommandData
{
  public string Name { get; init; }
}
