using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NeoServer.Game.Combat.Spells;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Common.Results;
using NeoServer.Game.Items;
using NeoServer.Game.Items.Bases;
using NeoServer.Server.Configurations;
using NeoServer.Server.Helpers;
using Newtonsoft.Json;

namespace NeoServer.Extensions.Spells.Commands;

public class ListCommandsCommand : CommandSpell
{
    private const string SPELL_TYPE = "command";

    public override bool OnCast(ICombatActor actor, string words, out InvalidOperation error)
    {
        error = InvalidOperation.NotPossible;

        if (actor is not IPlayer player) return false;

        var spells = LoadSpells();
        var text = BuildTextFromSpells(spells, words);
        var item = CreateItemBook();

        var window = new TextWindow(item, player.Location, text);

        player.Read(window);

        return true;
    }

    private static IItemType CreateItemBook()
    {
        var item = new ItemType();
        item.SetName("book");
        item.SetClientId(2821);
        return item;
    }

    private static string BuildTextFromSpells(List<IDictionary<string, object>> spells, string command)
    {
        var lines = new List<string>();
        foreach (var spell in spells)
        {
            var (words, name, description) = ExtractSpellAttributes(spell);

            if (string.IsNullOrEmpty(words) || words == command)
                continue;

            if (string.IsNullOrEmpty(description))
            {
                lines.Add($"{words} {name}");
                continue;
            }
            
            lines.Add($"{words} {name} - {description}");
        }

        return string.Join(Environment.NewLine + Environment.NewLine, lines);
    }

    private static (string, string, string) ExtractSpellAttributes(IDictionary<string, object> spell)
    {
        if (spell is null || !spell.ContainsKey("type") || spell["type"]?.ToString() != SPELL_TYPE)
            return (string.Empty, string.Empty, string.Empty);

        var words = spell["words"].ToString();
        var name = spell["name"].ToString();
        var description = spell["description"]?.ToString();

        return (words, name, description);
    }

    private static List<IDictionary<string, object>> LoadSpells()
    {
        var serverConfiguration = IoC.GetInstance<ServerConfiguration>();
        var path = Path.Combine(serverConfiguration.Data, "spells", "spells.json");
        var jsonString = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<List<IDictionary<string, object>>>(jsonString)?.ToList() ??
               new List<IDictionary<string, object>>(0);
    }

    public sealed class TextWindow : BaseItem, IReadable
    {
        public TextWindow(IItemType metadata, Location location, string text) : base(metadata, location)
        {
            Text = text?.ToString(CultureInfo.InvariantCulture);
        }

        public string Text { get; private set; }
        public ushort MaxLength => (ushort)(Text?.Length ?? 0);
        public bool CanWrite => false;
        public string WrittenBy { get; set; }
        public DateTime? WrittenOn { get; set; }

        public Result Write(string text, IPlayer writtenBy)
        {
            if (!CanWrite) return Result.Fail(InvalidOperation.NotPossible);

            if (text.IsNull()) return Result.Success;

            if (text.Length > MaxLength) return Result.Fail(InvalidOperation.NotPossible);

            Text = text;
            return Result.Success;
        }
    }
}