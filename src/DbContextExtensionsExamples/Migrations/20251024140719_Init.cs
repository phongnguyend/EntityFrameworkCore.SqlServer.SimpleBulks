using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace DbContextExtensionsExamples.Migrations;

/// <inheritdoc />
public partial class Init : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "CompositeKeyRows",
            columns: table => new
            {
                Id1 = table.Column<int>(type: "int", nullable: false),
                Id2 = table.Column<int>(type: "int", nullable: false),
                Column1 = table.Column<int>(type: "int", nullable: false),
                Column2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Column3 = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CompositeKeyRows", x => new { x.Id1, x.Id2 });
            });

        migrationBuilder.CreateTable(
            name: "ConfigurationEntries",
            columns: table => new
            {
                Id1 = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newsequentialid()"),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                CreatedDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                UpdatedDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                Key1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                IsSensitive = table.Column<bool>(type: "bit", nullable: false),
                SeasonAsInt = table.Column<int>(type: "int", nullable: true),
                SeasonAsString = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ConfigurationEntries", x => x.Id1);
            });

        migrationBuilder.CreateTable(
            name: "Rows",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Column1 = table.Column<int>(type: "int", nullable: false),
                Column2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Column3 = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Rows", x => x.Id);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "CompositeKeyRows");

        migrationBuilder.DropTable(
            name: "ConfigurationEntries");

        migrationBuilder.DropTable(
            name: "Rows");
    }
}
