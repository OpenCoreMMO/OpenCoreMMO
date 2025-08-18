using NeoServer.Domain.Items;

namespace NeoServer.Domain.Tests.Items.ItemAttributeList;

public class BaseAttributeListTests
{
    public enum TestEnum
    {
        Dummy
    }

    [Fact]
    public void GetCustomAttribute_ShouldReturnStringValue()
    {
        var list = new TestAttributeList();
        var value = list.GetCustomAttribute<string>("str");
        Assert.Equal("test", value);
    }

    [Fact]
    public void GetCustomAttribute_ShouldReturnDecimalValue()
    {
        var list = new TestAttributeList();
        var value = list.GetCustomAttribute<decimal>("dec");
        Assert.Equal(123.45m, value);
    }

    [Fact]
    public void GetCustomAttribute_ShouldReturnIntValue()
    {
        var list = new TestAttributeList();
        var value = list.GetCustomAttribute<int>("int");
        Assert.Equal(42, value);
    }

    private class TestAttributeList : BaseAttributeList<TestEnum>
    {
        public TestAttributeList()
        {
            SetCustomAttribute("str", "test");
            SetCustomAttribute("dec", 123.45m);
            SetCustomAttribute("int", 42);
        }
    }
}