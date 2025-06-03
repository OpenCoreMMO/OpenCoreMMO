using NeoServer.Domain.Common.Helpers;

namespace NeoServer.Domain.Tests.Common.Helpers;

public class DictionaryExtensionsTest
{
    [Fact]
    public void AddOrUpdate_When_Map_Has_No_Key_Add_Value()
    {
        IDictionary<string, int> sut = new Dictionary<string, int>();
        sut.AddOrUpdate("1", 1);

        Assert.Single(sut);
        Assert.Equal(1, sut["1"]);
    }

    [Fact]
    public void AddOrUpdate_When_Map_Has_Key_Update_Value()
    {
        IDictionary<string, int> sut = new Dictionary<string, int> { { "a", 1 } };
        sut.AddOrUpdate("a", 2);

        Assert.Single(sut);
        Assert.Equal(2, sut["a"]);
    }
}