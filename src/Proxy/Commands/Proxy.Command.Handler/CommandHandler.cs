using Amazon.Lambda.SNSEvents;
using Discord;
using Microsoft.Extensions.Logging;
using Proxy.Core.Model;
using Proxy.Core.Services;
using Proxy.DiscordProxy;

namespace Proxy.Command.Handler;

public class CommandHandler
{
  private readonly IJsonService _jsonService;
  private readonly IVariablesModel _varModel;
  private readonly DiscordHandler _discordHandler;
  private readonly ILogger<CommandHandler> _logger;

  public CommandHandler(IJsonService jsonService,
    IVariablesModel varModel,
    DiscordHandler discordHandler,
    ILogger<CommandHandler> logger)
  {
    _jsonService = jsonService;
    _varModel = varModel;
    _discordHandler = discordHandler;
    _logger = logger;
  }

  public async Task Handle(SNSEvent snsEvent, Func<DiscordInteraction, Task<IReadOnlyList<EmbedBuilder>>> task)
  {
    DiscordInteraction interaction = default;

    try
    {
      if (!Validate(snsEvent, out interaction))
      {
        await _discordHandler.HandleError(interaction, "Validation failed :/ Check logs");
        return;
      }

      var discordContent = await task(interaction);

      await _discordHandler.Handle(interaction, discordContent);

      _logger.LogInformation("Great success!");
    }
    catch (Exception ex)
    {
      await _discordHandler.HandleError(interaction, ex.Message);
    }
  }

  private bool Validate(SNSEvent request, out DiscordInteraction interaction)
  {
    interaction = default;

    if (_varModel.DebugEnabled) _logger.LogDebug(_jsonService.Serialize(request));

    // Validate SNS Record
    var snsRecord = request.Records?.FirstOrDefault();

    if (snsRecord is null)
    {
      _logger.LogCritical("Failed to get SNS Record");
      return false;
    }

    if (request.Records.Count > 1)
    {
      _logger.LogWarning($"SNS received with {request.Records.Count} records");
    }

    var messageHealthy = _jsonService.TryDeserialize(snsRecord.Sns.Message, out interaction);
    if (messageHealthy) return true;

    _logger.LogCritical("SNS Message is unhealthy");
    return false;
  }
}
