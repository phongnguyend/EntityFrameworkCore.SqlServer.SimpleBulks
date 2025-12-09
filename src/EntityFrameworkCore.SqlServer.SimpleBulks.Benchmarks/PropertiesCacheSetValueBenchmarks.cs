using BenchmarkDotNet.Attributes;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks;

public class PropertiesCacheSetValueBenchmarks
{
    private List<Order> _orders;
    private List<FlattenedOrder> _flattenedOrders;
    

    [IterationSetup]
    public void IterationSetup()
    {
        _orders = new List<Order>();
        _flattenedOrders = new List<FlattenedOrder>();

        for (int i = 0; i < 1_000_000; i++)
        {
            _orders.Add(new Order
            {
                Id = i,
                ShippingAddress = new Address
                {
                    Street = "Street " + i,
                    Location = new Location
                    {
                        Lat = 40.7128 + i,
                        Lng = -74.0060 - i
                    }
                }
            });

            _flattenedOrders.Add(new FlattenedOrder
            {
                Id = i,
                ShippingAddress_Street = "Street " + i,
                ShippingAddress_Location_Lat = 40.7128 + i,
                ShippingAddress_Location_Lng = -74.0060 - i
            });
        }
    }

    [IterationCleanup]
    public void IterationCleanup()
    {

    }

    [Benchmark]
    public void SetFlattenedValueReflection()
    {
        foreach (var order in _flattenedOrders)
        {
            PropertiesCache<FlattenedOrder>.SetFlattenedValueReflection("Id", order, order.Id + 1);
            PropertiesCache<FlattenedOrder>.SetFlattenedValueReflection("ShippingAddress_Street", order, "Updated");
            PropertiesCache<FlattenedOrder>.SetFlattenedValueReflection("ShippingAddress_Location_Lat", order, 41.0);
            PropertiesCache<FlattenedOrder>.SetFlattenedValueReflection("ShippingAddress_Location_Lng", order, -75.0);
        }
    }

    [Benchmark]
    public void SetFlattenedValueOptimized()
    {
        foreach (var order in _flattenedOrders)
        {
            PropertiesCache<FlattenedOrder>.SetFlattenedValueOptimized("Id", order, order.Id + 1);
            PropertiesCache<FlattenedOrder>.SetFlattenedValueOptimized("ShippingAddress_Street", order, "Updated");
            PropertiesCache<FlattenedOrder>.SetFlattenedValueOptimized("ShippingAddress_Location_Lat", order, 41.0);
            PropertiesCache<FlattenedOrder>.SetFlattenedValueOptimized("ShippingAddress_Location_Lng", order, -75.0);
        }
    }

    [Benchmark]
    public void SetPropertyValueReflection()
    {
        foreach (var order in _orders)
        {
            PropertiesCache<Order>.SetPropertyValueReflection("Id", order, order.Id + 1);
            PropertiesCache<Order>.SetPropertyValueReflection("ShippingAddress.Street", order, "Updated");
            PropertiesCache<Order>.SetPropertyValueReflection("ShippingAddress.Location.Lat", order, 41.0);
            PropertiesCache<Order>.SetPropertyValueReflection("ShippingAddress.Location.Lng", order, -75.0);
        }
    }

    [Benchmark]
    public void SetPropertyValueOptimized()
    {
        foreach (var order in _orders)
        {
            PropertiesCache<Order>.SetPropertyValueOptimized("Id", order, order.Id + 1);
            PropertiesCache<Order>.SetPropertyValueOptimized("ShippingAddress.Street", order, "Updated");
            PropertiesCache<Order>.SetPropertyValueOptimized("ShippingAddress.Location.Lat", order, 41.0);
            PropertiesCache<Order>.SetPropertyValueOptimized("ShippingAddress.Location.Lng", order, -75.0);
        }
    }

    public class Order
    {
        public int Id { get; set; }

        public Address ShippingAddress { get; set; }
    }

    public class Address
    {
        public string Street { get; set; }

        public Location Location { get; set; }
    }

    public class Location
    {
        public double Lat { get; set; }

        public double Lng { get; set; }
    }

    public class FlattenedOrder
    {
        public int Id { get; set; }

        public string ShippingAddress_Street { get; set; }

        public double ShippingAddress_Location_Lat { get; set; }

        public double ShippingAddress_Location_Lng { get; set; }
    }
}
