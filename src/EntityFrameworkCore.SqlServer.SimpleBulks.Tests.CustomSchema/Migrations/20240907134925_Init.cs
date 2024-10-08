﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Tests.CustomSchema.Migrations;

public partial class Init : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "test");

        migrationBuilder.CreateTable(
            name: "CompositeKeyRows",
            schema: "test",
            columns: table => new
            {
                Id1 = table.Column<int>(type: "int", nullable: false),
                Id2 = table.Column<int>(type: "int", nullable: false),
                Column1 = table.Column<int>(type: "int", nullable: false),
                Column2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Column3 = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CompositeKeyRows", x => new { x.Id1, x.Id2 });
            });

        migrationBuilder.CreateTable(
            name: "ConfigurationEntries",
            schema: "test",
            columns: table => new
            {
                Id1 = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newsequentialid()"),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                CreatedDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                UpdatedDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                Key1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                IsSensitive = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ConfigurationEntries", x => x.Id1);
            });

        migrationBuilder.CreateTable(
            name: "Customers",
            schema: "test",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newsequentialid()"),
                FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                CurrentCountryIsoCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Index = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Customers", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "SingleKeyRows",
            schema: "test",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Column1 = table.Column<int>(type: "int", nullable: false),
                Column2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Column3 = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SingleKeyRows", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Contacts",
            schema: "test",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newsequentialid()"),
                EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                CountryIsoCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Index = table.Column<int>(type: "int", nullable: false),
                CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Contacts", x => x.Id);
                table.ForeignKey(
                    name: "FK_Contacts_Customers_CustomerId",
                    column: x => x.CustomerId,
                    principalSchema: "test",
                    principalTable: "Customers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Contacts_CustomerId",
            schema: "test",
            table: "Contacts",
            column: "CustomerId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "CompositeKeyRows",
            schema: "test");

        migrationBuilder.DropTable(
            name: "ConfigurationEntries",
            schema: "test");

        migrationBuilder.DropTable(
            name: "Contacts",
            schema: "test");

        migrationBuilder.DropTable(
            name: "SingleKeyRows",
            schema: "test");

        migrationBuilder.DropTable(
            name: "Customers",
            schema: "test");
    }
}
