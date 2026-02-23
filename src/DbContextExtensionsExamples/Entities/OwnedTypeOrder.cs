using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DbContextExtensionsExamples.Entities;

public class OwnedTypeOrder
{
    public int Id { get; set; }

    [Required]
    public OwnedTypeAddress ShippingAddress { get; set; }
}

[Owned]
public class OwnedTypeAddress
{
    public string Street { get; set; }

    [Required]
    public OwnedTypeLocation Location { get; set; }
}

[Owned]
public class OwnedTypeLocation
{
    public double Lat { get; set; }

    public double Lng { get; set; }
}

public class JsonOwnedTypeOrder
{
    public int Id { get; set; }

    [Required]
    public OwnedTypeAddress ShippingAddress { get; set; }
}