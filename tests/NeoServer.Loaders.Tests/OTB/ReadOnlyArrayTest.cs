using System;
using NeoServer.Loaders.OTB.DataStructures;
using Xunit;

namespace NeoServer.Loaders.Tests.OTB;

public class ReadOnlyArrayTest
{
    [Fact]
    public void WrapCollection_NullItems_Throws()
    {
        var sut = ReadOnlyArray<int>.WrapCollection(null);
        Assert.Null(sut);
    }

    [Fact]
    public void WrapCollection_NotNull_ReturnsInstance()
    {
        var instance = ReadOnlyArray<int>.WrapCollection([]);
        Assert.NotNull(instance);
        Assert.IsType<ReadOnlyArray<int>>(instance);
    }

    [Fact]
    public void GetByIndex_ReturnsValue()
    {
        var instance = ReadOnlyArray<int>.WrapCollection([4, 7, 5, 1]);

        Assert.Equal(5, instance[2]);
        Assert.Equal(7, instance[1]);
        Assert.Equal(4, instance[0]);
        Assert.Throws<IndexOutOfRangeException>(() => instance[8]);
        Assert.Throws<IndexOutOfRangeException>(() => instance[-5]);
    }

    [Fact]
    public void GetCount_ReturnsArrayLength()
    {
        var instance = ReadOnlyArray<int>.WrapCollection([4, 7, 5, 1]);

        Assert.Equal(4, instance.Count);
    }

    [Fact]
    public void GetEnumerator_ReturnsGenericEnumerator()
    {
        var data = new int[4] { 4, 7, 5, 1 };
        var instance = ReadOnlyArray<int>.WrapCollection(data);

        Assert.NotNull(instance.GetEnumerator());
    }
}