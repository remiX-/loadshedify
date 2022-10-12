﻿using Discord;

namespace Proxy.DiscordProxy;

public struct DiscordDataOption
{
  public string Name { get; set; }

  public object Value { get; set; }

  public ApplicationCommandOptionType Type { get; set; }
}
