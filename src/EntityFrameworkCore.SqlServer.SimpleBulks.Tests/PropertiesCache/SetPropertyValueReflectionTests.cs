namespace EntityFrameworkCore.SqlServer.SimpleBulks.Tests.PropertiesCache;

public class SetPropertyValueReflectionTests
{
    [Fact]
    public void SetPropertyValue_Current_Level()
    {
        var order = new Order();

        PropertiesCache<Order>.SetPropertyValueReflection("Id", order, 1);

        Assert.Equal(1, order.Id);
        Assert.Null(order.ShippingAddress);
        Assert.Null(order.ShippingAddress?.Street);
        Assert.Null(order.ShippingAddress?.Location);
        Assert.Null(order.ShippingAddress?.Location?.Lng);
        Assert.Null(order.ShippingAddress?.Location?.Lat);
    }

    [Fact]
    public void SetPropertyValue_One_Level()
    {
        var order = new Order();

        PropertiesCache<Order>.SetPropertyValueReflection("ShippingAddress.Street", order, "xxx");

        Assert.Equal(0, order.Id);
        Assert.NotNull(order.ShippingAddress);
        Assert.Equal("xxx", order.ShippingAddress!.Street);
        Assert.Null(order.ShippingAddress?.Location);
        Assert.Null(order.ShippingAddress?.Location?.Lng);
        Assert.Null(order.ShippingAddress?.Location?.Lat);
    }

    [Fact]
    public void SetPropertyValue_Two_Levels()
    {
        var order = new Order();

        PropertiesCache<Order>.SetPropertyValueReflection("ShippingAddress.Location.Lat", order, 40.7128);

        Assert.Equal(0, order.Id);
        Assert.NotNull(order.ShippingAddress);
        Assert.Null(order.ShippingAddress?.Street);
        Assert.NotNull(order.ShippingAddress?.Location);
        Assert.Equal(0, order.ShippingAddress?.Location?.Lng);
        Assert.Equal(40.7128, order.ShippingAddress!.Location.Lat);
    }

    [Fact]
    public void SetPropertyValue_Multiple_Levels_Down()
    {
        var order = new Order();

        PropertiesCache<Order>.SetPropertyValueReflection("ShippingAddress.Street", order, "xxx");
        PropertiesCache<Order>.SetPropertyValueReflection("ShippingAddress.Location.Lat", order, 40.7128);

        Assert.Equal(0, order.Id);
        Assert.NotNull(order.ShippingAddress);
        Assert.Equal("xxx", order.ShippingAddress!.Street);
        Assert.NotNull(order.ShippingAddress?.Location);
        Assert.Equal(0, order.ShippingAddress?.Location?.Lng);
        Assert.Equal(40.7128, order.ShippingAddress!.Location.Lat);
    }

    [Fact]
    public void SetPropertyValue_Multiple_Levels_Up()
    {
        var order = new Order();

        PropertiesCache<Order>.SetPropertyValueReflection("ShippingAddress.Location.Lat", order, 40.7128);
        PropertiesCache<Order>.SetPropertyValueReflection("ShippingAddress.Street", order, "xxx");

        Assert.Equal(0, order.Id);
        Assert.NotNull(order.ShippingAddress);
        Assert.Equal("xxx", order.ShippingAddress!.Street);
        Assert.NotNull(order.ShippingAddress?.Location);
        Assert.Equal(0, order.ShippingAddress?.Location?.Lng);
        Assert.Equal(40.7128, order.ShippingAddress!.Location.Lat);
    }
}
