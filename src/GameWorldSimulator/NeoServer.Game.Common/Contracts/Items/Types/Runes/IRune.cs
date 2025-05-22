using System.Text;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Creatures.Structs;

namespace NeoServer.Game.Common.Contracts.Items.Types.Runes;

public interface IRune : IUsableRequirement, IFormula
{
    public CooldownTime Cooldown { get; }

    public string ValidationError
    {
        get
        {
            var text = new StringBuilder();
            text.Append("Only ");
            //todo
            //for (int i = 0; i < Vocations.Length; i++)
            //{
            //    text.Append($"{VocationTypeParser.Parse(Vocations[i]).ToLower()}s");
            //    if (i + 1 < Vocations.Length)
            //    {
            //        text.Append(", ");
            //    }
            //}
            text.Append($" of magic level {MinLevel} or above may use or consume this item");
            return text.ToString();
        }
    }

    bool CanBeUsedBy(IPlayer player);
}