using System.Net;
using System.Text;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Logging;
using NSec.Cryptography;
using Proxy.Core;
using Proxy.Core.Model;
using Proxy.Core.Services;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace Proxy.BotHandler;

public class BotProxyFunction
{
  private readonly IJsonService _jsonService;
  private readonly IVariablesModel _varModel;

  private readonly ILogger<BotProxyFunction> _logger;

  public BotProxyFunction()
  {
    Console.WriteLine("BotProxyFunction.ctor");

    Shell.ConfigureServices();

    _jsonService = Shell.Get<IJsonService>();
    _varModel = Shell.Get<IVariablesModel>();
    _logger = Shell.Get<ILogger<BotProxyFunction>>();
  }

  public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
  {
    if (_varModel.DebugEnabled) _logger.LogDebug(_jsonService.Serialize(request));

    // Checking signature (requirement 1.)
    var isVerified = VerifySignature(request.Body, request.Headers);
    if (!isVerified)
    {
      _logger.LogDebug("Signature verification failed");
      return new APIGatewayProxyResponse
      {
        StatusCode = (int)HttpStatusCode.Unauthorized,
        Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } },
        Body = "invalid request signature"
      };
    }

    // Replying to Discord bot ping (requirement 2.)
    var body = _jsonService.Deserialize<DiscordProxyRequest>(request.Body);
    if (body.Type == 1)
    {
      _logger.LogDebug("PONG!");
      return new APIGatewayProxyResponse
      {
        StatusCode = (int)HttpStatusCode.OK,
        Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
        Body = _jsonService.Serialize(new { type = 1 })
      };
    }

    // Verify command name is valid
    if (body.Data.Name is null)
    {
      return new APIGatewayProxyResponse { StatusCode = (int)HttpStatusCode.NotFound };
    }

    // Send notification to split to separate lambda function
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
          content = body.Data.Name == "ping" ? "pong!" : "*⏳ Loading...*"
        }
      })
    };
  }

  private static bool VerifySignature(string body, IDictionary<string, string> headers)
  {
    if (headers.ContainsKey("app-warm-ping")) return true;

    try
    {
      var signature = headers["x-signature-ed25519"];
      var timestamp = headers["x-signature-timestamp"];
      var botPublicKey = Environment.GetEnvironmentVariable("PUBLIC_KEY");

      var algorithm = SignatureAlgorithm.Ed25519;
      var publicKey = PublicKey.Import(algorithm, GetBytesFromHexString(botPublicKey), KeyBlobFormat.RawPublicKey);
      var data = Encoding.UTF8.GetBytes(timestamp + body);

      return algorithm.Verify(publicKey, data, GetBytesFromHexString(signature));
    }
    catch
    {
      return false;
    }
  }

  private static byte[] GetBytesFromHexString(string hex)
  {
    var length = hex.Length;
    var bytes = new byte[length / 2];

    for (int i = 0; i < length; i += 2)
      bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);

    return bytes;
  }

  private async Task SnsSend(string body, string commandName)
  {
    var topicArn = Environment.GetEnvironmentVariable("TOPIC_ARN");

    var snsParams = new PublishRequest
    {
      Message = body,
      Subject = $"Discord bot SNS message: {commandName}",
      TopicArn = topicArn,
      MessageAttributes = new Dictionary<string, MessageAttributeValue>
      {
        {
          "command",
          new MessageAttributeValue { DataType = "String", StringValue = commandName }
        }
      }
    };

    _logger.LogDebug("Sending SNS...");
    if (_varModel.DebugEnabled) _logger.LogDebug(_jsonService.Serialize(snsParams));

    var snsClient = new AmazonSimpleNotificationServiceClient();

    var response = await snsClient.PublishAsync(snsParams);
    if (_varModel.DebugEnabled) _logger.LogDebug(_jsonService.Serialize(response));

    _logger.LogDebug("SNS Sent!");
  }
}
