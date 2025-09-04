using NeoServer.Domain.Common.Contracts.Spells;

namespace NeoServer.Domain.Spells;

public class SpellListManager
{
    private Dictionary<string, ISpell> Spells { get; } = new(StringComparer.InvariantCultureIgnoreCase);
    private Dictionary<string, string> SpellNameWordMap { get; } = new(StringComparer.InvariantCultureIgnoreCase);

    public void Add(string words, ISpell spell)
    {
        if (spell is ICommandSpell commandSpell)
        {
            var command = GetCommand(words);
            commandSpell.Params = command.Params;

            Spells.Add(command.Words, commandSpell);
            SpellNameWordMap.Add(commandSpell.Name, command.Words);
            return;
        }

        Spells.Add(words, spell);
        SpellNameWordMap.TryAdd(spell.Name, words);
    }

    public bool TryGet(string words, out ISpell spell)
    {
        spell = null;

        if (string.IsNullOrWhiteSpace(words)) return false;

        if (words.StartsWith('/'))
        {
            var command = GetCommand(words);
            if (Spells.TryGetValue(command.Item1, out spell) && spell is ICommandSpell commandSpell)
            {
                commandSpell.Params = command.Item2;
                return true;
            }

            return false;
        }

        return Spells.TryGetValue(words, out spell);
    }

    public bool TryGetInstantSpell(string words, out ISpell spell)
    {
        spell = null;

        if (string.IsNullOrWhiteSpace(words))
            return false;

        if (TryGet(words, out spell))
        {
            if (spell.HasParams) // Spell requires params but none were given
            {
                return false;
            }
            
            return true;
        }

        var param = string.Empty;
        var spellWord = string.Empty;

        var paramsIndex = words.IndexOf('"');

        if (paramsIndex < 0)
        {
            var lastSpaceIndex = words.LastIndexOf(' ');
            if (lastSpaceIndex >= 0 && words.StartsWith($"/"))
            {
                spellWord = words[..lastSpaceIndex];
                param = words[(lastSpaceIndex + 1)..];
            }
            else
            {
                spellWord = words;
                param = string.Empty;
            }
        }
        else
        {
            var endOfParam = words.LastIndexOf('"');
            var paramLength = endOfParam > -1 && endOfParam != paramsIndex ? endOfParam - paramsIndex : words.Length - paramsIndex;
            param = words.Substring(paramsIndex, paramLength);
            spellWord = words[..paramsIndex];
        }

        if (!TryGet(spellWord.Trim(), out spell))
            return false;

        param = param.Replace("\"", "").Replace("\'", "").Trim();

        if (!string.IsNullOrWhiteSpace(param)) spell.Params = [param];

        return true;
    }

    public ISpell GetByName(string name)
    {
        if (!SpellNameWordMap.TryGetValue(name, out var words)) return null;

        Spells.TryGetValue(words, out var spell);
        return spell;
    }

    public void Clear()
    {
        Spells.Clear();
        SpellNameWordMap.Clear();
    }

    private (string Words, object[] Params) GetCommand(string words)
    {
        var firstWhiteSpace = words.IndexOf(' ');

        if (firstWhiteSpace == -1) return (words, null);

        var command = words[..firstWhiteSpace];
        var @params = words[firstWhiteSpace..].Trim().Split(",");

        return (command, @params);
    }
}