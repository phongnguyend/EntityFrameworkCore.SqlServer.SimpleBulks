﻿using EntityFrameworkCore.SqlServer.SimpleBulks.Tests.CustomSchema;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Tests.Database
{
    [Table("ConfigurationEntries", Schema = TestConstants.Schema)]
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

        [NotMapped]
        public string TestNotMapped { get; set; }
    }
}