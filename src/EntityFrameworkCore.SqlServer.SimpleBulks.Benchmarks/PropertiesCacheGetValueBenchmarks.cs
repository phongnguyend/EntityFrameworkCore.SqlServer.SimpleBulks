using BenchmarkDotNet.Attributes;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks;

public class PropertiesCacheGetValueBenchmarks
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
    public void GetPropertyValueReflection()
    {
        foreach (var order in _orders)
        {
            var id = PropertiesCache<Order>.GetPropertyValueReflection("Id", order);
            var street = PropertiesCache<Order>.GetPropertyValueReflection("ShippingAddress.Street", order);
            var lat = PropertiesCache<Order>.GetPropertyValueReflection("ShippingAddress.Location.Lat", order);
            var lng = PropertiesCache<Order>.GetPropertyValueReflection("ShippingAddress.Location.Lng", order);
        }
    }

    [Benchmark]
    public void GetPropertyValueOptimized()
    {
        foreach (var order in _orders)
        {
            var id = PropertiesCache<Order>.GetPropertyValueOptimized("Id", order);
            var street = PropertiesCache<Order>.GetPropertyValueOptimized("ShippingAddress.Street", order);
            var lat = PropertiesCache<Order>.GetPropertyValueOptimized("ShippingAddress.Location.Lat", order);
            var lng = PropertiesCache<Order>.GetPropertyValueOptimized("ShippingAddress.Location.Lng", order);
        }
    }

    [Benchmark]
    public void GetFlattenedPropertyValueReflection()
    {
        foreach (var order in _flattenedOrders)
        {
            var id = PropertiesCache<FlattenedOrder>.GetFlattenedPropertyValueReflection("Id", order);
            var street = PropertiesCache<FlattenedOrder>.GetFlattenedPropertyValueReflection("ShippingAddress_Street", order);
            var lat = PropertiesCache<FlattenedOrder>.GetFlattenedPropertyValueReflection("ShippingAddress_Location_Lat", order);
            var lng = PropertiesCache<FlattenedOrder>.GetFlattenedPropertyValueReflection("ShippingAddress_Location_Lng", order);
        }
    }

    [Benchmark]
    public void GetFlattenedPropertyValueOptimized()
    {
        foreach (var order in _flattenedOrders)
        {
            var id = PropertiesCache<FlattenedOrder>.GetFlattenedPropertyValueOptimized("Id", order);
            var street = PropertiesCache<FlattenedOrder>.GetFlattenedPropertyValueOptimized("ShippingAddress_Street", order);
            var lat = PropertiesCache<FlattenedOrder>.GetFlattenedPropertyValueOptimized("ShippingAddress_Location_Lat", order);
            var lng = PropertiesCache<FlattenedOrder>.GetFlattenedPropertyValueOptimized("ShippingAddress_Location_Lng", order);
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
