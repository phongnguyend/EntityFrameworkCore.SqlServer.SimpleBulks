using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DbContextExtensionsExamples.Entities;

public class ComplexTypeOrder
{
    public int Id { get; set; }

    [Required]
    public ComplexTypeAddress ShippingAddress { get; set; }
}

[ComplexType]
public class ComplexTypeAddress
{
    public string Street { get; set; }

    public ComplexTypeLocation Location { get; set; }
}

[ComplexType]
public class ComplexTypeLocation
{
    public double Lat { get; set; }

    public double Lng { get; set; }
}

public class JsonComplexTypeOrder
{
    public int Id { get; set; }

    [Required]
    public ComplexTypeAddress ShippingAddress { get; set; }
}