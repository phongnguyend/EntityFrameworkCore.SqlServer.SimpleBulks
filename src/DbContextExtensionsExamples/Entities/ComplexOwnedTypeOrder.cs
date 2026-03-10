using System.ComponentModel.DataAnnotations;

namespace DbContextExtensionsExamples.Entities;

public class ComplexOwnedTypeOrder
{
    public int Id { get; set; }

    [Required]
    public ComplexTypeAddress ComplexShippingAddress { get; set; }

    [Required]
    public OwnedTypeAddress OwnedShippingAddress { get; set; }
}

public class JsonComplexOwnedTypeOrder
{
    public int Id { get; set; }

    [Required]
    public ComplexTypeAddress ComplexShippingAddress { get; set; }

    [Required]
    public OwnedTypeAddress OwnedShippingAddress { get; set; }
}