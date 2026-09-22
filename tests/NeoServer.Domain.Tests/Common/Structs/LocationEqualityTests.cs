using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Domain.Tests.Common.Structs;

public class LocationEqualityTests
{
    [Fact]
    public void Equals_BoxedEqualLocation_ReturnsTrue()
    {
        var a = new Location(1, 2, 3);
        object boxed = new Location(1, 2, 3);

        // Must not overflow — this is the regression guard for the C1 bug.
        var result = a.Equals(boxed);

        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_BoxedDifferentLocation_ReturnsFalse()
    {
        var a = new Location(1, 2, 3);
        object boxed = new Location(1, 2, 4);

        a.Equals(boxed).Should().BeFalse();
    }

    [Fact]
    public void Equals_BoxedNonLocation_ReturnsFalse()
    {
        var a = new Location(1, 2, 3);

        a.Equals("not a location").Should().BeFalse();
    }

    [Fact]
    public void Equals_TypedOverload_MatchesOperator()
    {
        var a = new Location(1, 2, 3);
        var b = new Location(1, 2, 3);
        var c = new Location(9, 9, 9);

        (a.Equals(b) == (a == b)).Should().BeTrue();
        (a.Equals(c) == (a == c)).Should().BeTrue();
    }

    [Fact]
    public void Be_ViaFluentAssertions_DoesNotOverflow()
    {
        // FluentAssertions .Be() boxes through object.Equals — this is the exact crash path.
        var loc = new Location(1, 2, 3);
        loc.Should().Be(new Location(1, 2, 3));
    }

    [Fact]
    public void GetHashCode_EqualLocations_AreEqual()
    {
        var a = new Location(5, 10, 7);
        var b = new Location(5, 10, 7);

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void OperatorEquality_And_Inequality_AreConsistent()
    {
        var a = new Location(1, 2, 3);
        var b = new Location(1, 2, 3);
        var c = new Location(9, 9, 9);

        // (a == b) must equal !(a != b) for equal pair.
        (a == b).Should().Be(!(a != b));

        // (a == c) must equal !(a != c) for unequal pair.
        (a == c).Should().Be(!(a != c));
    }
}
