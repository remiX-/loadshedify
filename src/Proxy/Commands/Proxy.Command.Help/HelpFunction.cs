using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Lambda.SNSEvents;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Proxy.Command.Handler;
using Proxy.Core;
using Proxy.DiscordProxy;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]
namespace Proxy.Command;

public class HelpFunction
{
  private readonly CommandHandler _commandHandler;

  private readonly ILogger<HelpFunction> _logger;

  public HelpFunction()
  {
    Console.WriteLine("StatusFunction.ctor");

    Shell.ConfigureServices(collection =>
    {
      collection
        .WithCommandProxy()
        .WithDiscordHandler();
    });

    _commandHandler = Shell.Get<CommandHandler>();

    _logger = Shell.Get<ILogger<HelpFunction>>();
  }

  public async Task FunctionHandler(SNSEvent request, ILambdaContext context)
  {
    await _commandHandler.Handle(request, Action);
  }

  private async Task<IReadOnlyList<EmbedBuilder>> Action(DiscordInteraction interaction)
  {
    return await Task.FromResult(new List<EmbedBuilder>()
    {
      GetBaseHelp(),
      GetStatusHelp(),
      GetSearchHelp(),
      GetScheduleHelp(),
      GetSubscribeHelp()
    });
  }

  private static EmbedBuilder GetBaseHelp()
  {
    return new EmbedBuilder()
      .WithTitle("Help")
      .WithDescription(
        "loadshedify is a discord bot create to notify gamers for loadshedding alerts by allowing them to subscribe to a given **area_id**.\n" +
        "\n" +
        "loadshedify is a Discord bot created for gamers to retrieve loadshedding schedules and to be notified at real time using EskomSePush Business API 2.0.\n" +
        "\n" +
        "Created for [EskomSePush Build Challenge](https://offerzen.gitbook.io/programmable-banking-community-wiki/build-events/eskomsepush-build-challenge)"
      )
      .WithColor(Color.Orange);
  }

  private static EmbedBuilder GetStatusHelp()
  {
    return new EmbedBuilder()
      .WithTitle("/status")
      .WithDescription(
        "This command shows the current Loadshedding status.\n" +
        "Some cities (currently only **Cape Town**) have municipal overrides which has them on a separated schedule.\n" +
        "The rest fall under **South Africa**."
      )
      .WithColor(Color.Blue);
  }

  private static EmbedBuilder GetSearchHelp()
  {
    return new EmbedBuilder()
      .WithTitle("/search {text}")
      .WithDescription(
        "In order to correctly view schedule information for an area, you will need to obtain the specific **area_id** using this command.\n" +
        "This can be done via `/search {area}` in which you can copy the `id` field.\n" +
        "\n" +
        ":information_source: Try paste this: `/search area:summerstrand`\n" +
        "\n" +
        "Now check out how to use this :)"
      )
      .WithColor(Color.Blue);
  }

  private static EmbedBuilder GetScheduleHelp()
  {
    return new EmbedBuilder()
      .WithTitle("/schedule {id}")
      .WithDescription(
        "After retrieving an **area_id** via `/search`, you can now provide it to `/schedule {id}`\n" +
        "This can be done via `/search {area}` in which you can copy the `id` field.\n" +
        "\n" +
        ":information_source: Using the example above, we can use the **area_id** that we get back from `/search`\n" +
        "Try paste this: `/schedule id:nelsonmandelabay-5-summerstranduptomarinehotelarea8`\n" +
        "\n" +
        ":grey_exclamation: **PROTIP:** You can also use `/schedulesim (current|future) {id}` to simulate active or upcoming loadshedding"
      )
      .WithColor(Color.Blue);
  }

  private static EmbedBuilder GetSubscribeHelp()
  {
    return new EmbedBuilder()
      .WithTitle("/subscribe add {id}")
      .WithDescription(
        "Cool, so we can manually check the schedule, but that's lame!!!\n" +
        "\n" +
        "This is where `/subscribe add` comes to save the day. You can subscribe to specific **area_id**'s in any text channel.\n" +
        "The bot checks periodically for any new alerts to send out (every hour)\n" +
        "\n" +
        ":information_source: Using the example above, let's subscribe to **Summerstrand**.\n" +
        "Try paste this: `/subscribe add id:nelsonmandelabay-5-summerstranduptomarinehotelarea8`\n" +
        "\n" +
        ":bangbang: **NOTE:** The alert threshold is currently set to 24 hours for testing & demo purposes\n" +
        ":bangbang: **NOTE:** There is currently no way to **list** your subscriptions or **remove** yourself from an area, sorry :sweat:"
      )
      .WithColor(Color.Magenta);
  }
}
