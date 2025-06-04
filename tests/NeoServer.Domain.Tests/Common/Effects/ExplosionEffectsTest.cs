using NeoServer.Domain.Common.Effects.Magical;

namespace NeoServer.Domain.Tests.Common.Effects;

public class ExplosionEffectsTest
{
    [Theory]
    [InlineData(1)]
    public void Create_Should_Create_Matrix_Of_Locations(int radius)
    {
        var a = ExplosionEffect.Create(radius);
    }
}