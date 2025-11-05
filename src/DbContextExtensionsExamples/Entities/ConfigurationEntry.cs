using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DbContextExtensionsExamples.Entities;

public class ConfigurationEntry
{
    public Guid Id { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; }

    public DateTimeOffset CreatedDateTime { get; set; }

    public DateTimeOffset? UpdatedDateTime { get; set; }

    public string Key { get; set; }

    public string Value { get; set; }

    public string Description { get; set; }

    public bool IsSensitive { get; set; }

    public Season? SeasonAsInt { get; set; }

    public Season? SeasonAsString { get; set; }


    [NotMapped]
    public string TestNotMapped { get; set; }
}
