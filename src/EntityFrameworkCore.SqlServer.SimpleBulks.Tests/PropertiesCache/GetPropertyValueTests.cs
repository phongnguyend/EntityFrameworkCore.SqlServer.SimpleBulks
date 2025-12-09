namespace EntityFrameworkCore.SqlServer.SimpleBulks.Tests.PropertiesCache;

public class GetPropertyValueTests
{
    [Fact]
    public void GetPropertyValue_Multiple_Levels()
    {
        var orders = new List<Order>
        {
            new() {},
            new()
            {
                ShippingAddress = new Address
                {
                }
            },
            new()
            {
                ShippingAddress = new Address
                {
                    Street = "123 Main St"
                }
            },
            new()
            {
                ShippingAddress = new Address
                {
                    Location = new Location
                    {

                    }
                }
            },
            new()
            {
                ShippingAddress = new Address
                {
                    Location = new Location
                    {
                        Lat = 40.7128,
                        Lng = -74.0060
                    }
                }
            }
        };

        foreach (var order in orders)
        {
            var id = PropertiesCache<Order>.GetPropertyValueReflection("Id", order);
            var street = PropertiesCache<Order>.GetPropertyValueReflection("ShippingAddress.Street", order);
            var lat = PropertiesCache<Order>.GetPropertyValueReflection("ShippingAddress.Location.Lat", order);
            var lng = PropertiesCache<Order>.GetPropertyValueReflection("ShippingAddress.Location.Lng", order);

            Assert.Equal(order.Id, id);
            Assert.Equal(street, order?.ShippingAddress?.Street);
            Assert.Equal(lat, order?.ShippingAddress?.Location?.Lat);
            Assert.Equal(lng, order?.ShippingAddress?.Location?.Lng);
        }

        foreach (var order in orders)
        {
            var id = PropertiesCache<Order>.GetPropertyValueOptimized("Id", order);
            var street = PropertiesCache<Order>.GetPropertyValueOptimized("ShippingAddress.Street", order);
            var lat = PropertiesCache<Order>.GetPropertyValueOptimized("ShippingAddress.Location.Lat", order);
            var lng = PropertiesCache<Order>.GetPropertyValueOptimized("ShippingAddress.Location.Lng", order);

            Assert.Equal(order.Id, id);
            Assert.Equal(street, order?.ShippingAddress?.Street);
            Assert.Equal(lat, order?.ShippingAddress?.Location?.Lat);
            Assert.Equal(lng, order?.ShippingAddress?.Location?.Lng);
        }
    }
}
