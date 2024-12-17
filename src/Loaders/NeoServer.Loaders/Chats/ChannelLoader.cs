using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using NeoServer.Game.Chats;
using NeoServer.Game.Chats.Rules;
using NeoServer.Game.Common.Chats;
using NeoServer.Game.Common.Contracts.Chats;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Game.Common.Helpers;
using NeoServer.Loaders.Helpers;
using NeoServer.Loaders.Interfaces;
using NeoServer.Server.Configurations;
using Serilog;

namespace NeoServer.Loaders.Chats;

// ReSharper disable once UnusedType.Global
public class ChannelLoader : IStartupLoader
{
    private readonly ChatChannelFactory _chatChannelFactory;
    private readonly IChatChannelStore _chatChannelStore;
    private readonly ILogger _logger;
    private readonly ServerConfiguration _serverConfiguration;

    public ChannelLoader(ServerConfiguration serverConfiguration, ILogger logger,
        ChatChannelFactory chatChannelFactory, IChatChannelStore chatChannelStore)
    {
        _serverConfiguration = serverConfiguration;
        _logger = logger;
        _chatChannelFactory = chatChannelFactory;
        _chatChannelStore = chatChannelStore;
    }

    public void Load()
    {
        if (Guard.AnyNull(_serverConfiguration, _chatChannelFactory, _chatChannelStore))
        {
            LogErrorsIfNullParameters();
            return;
        }

        var path = Path.Combine(_serverConfiguration.Data, "channels.json");
        if (!File.Exists(path))
        {
            _logger.Error("channels.json file not found at {Path}", path);
            return;
        }

        var jsonString = File.ReadAllText(path);

        var channels = JsonSerializer.Deserialize<List<ChannelModel>>(jsonString, JsonSettings.Options);

        if (channels != null)
            foreach (var channel in channels.Where(x => x.Enabled))
            {
                IChatChannel createdChannel;
                if (!string.IsNullOrWhiteSpace(channel.Script))
                {
                    var type = ScriptSearch.Get(channel.Script);
                    createdChannel = _chatChannelFactory.Create(type, channel.Name);
                }
                else
                {
                    createdChannel = _chatChannelFactory.Create(channel.Name, channel.Description, channel.Opened,
                        ParseColor(channel.Color?.Default),
                        channel.Color?.ByVocation?.ToDictionary(x => (byte)x.Key, x => ParseColor(x.Value)) ??
                        default,
                        new ChannelRule
                        {
                            AllowedVocations = channel.Vocations,
                            MinMaxAllowedLevel = (channel.Level?.BiggerThan ?? 0, channel.Level?.LowerThan ?? 0)
                        },
                        new ChannelRule
                        {
                            AllowedVocations = channel.Write?.Vocations ?? default,
                            MinMaxAllowedLevel = (channel.Write?.Level?.BiggerThan ?? 0,
                                channel.Write?.Level?.LowerThan ?? 0)
                        },
                        channel.MuteRule is null
                            ? default
                            : new MuteRule
                            {
                                CancelMessage = channel.MuteRule.CancelMessage,
                                MessagesCount = channel.MuteRule.MessagesCount,
                                TimeMultiplier = channel.MuteRule.TimeMultiplier,
                                TimeToBlock = channel.MuteRule.TimeToBlock,
                                WaitTime = channel.MuteRule.WaitTime
                            });
                }

                if (createdChannel is null) continue;

                _chatChannelStore.Add(createdChannel.Id, createdChannel);
            }

        _logger.Verbose("Channels loaded!");
    }

    private void LogErrorsIfNullParameters()
    {
        if (_serverConfiguration is null) _logger.Error("Server configuration not found");
        if (_chatChannelFactory is null) _logger.Error("ChatChannelFactory not found");
        if (_chatChannelStore is null) _logger.Error("ChatChannelStore not found");

        _logger.Error("Unable to load channels");
    }

    private static SpeechType ParseColor(string color)
    {
        return color switch
        {
            "red" => SpeechType.ChannelRed1Text,
            "yellow" => SpeechType.ChannelYellowText,
            "white" => SpeechType.ChannelWhiteText,
            "orange" => SpeechType.ChannelOrangeText,
            _ => SpeechType.ChannelYellowText
        };
    }
}