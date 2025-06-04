using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Spells;

namespace NeoServer.Extensions.Spells.Support;

public class Haste : HasteSpell
{
    public override EffectT Effect => EffectT.GlitterBlue;
    public override uint Duration => 20000;
    public override ushort SpeedBoost => 400;
}