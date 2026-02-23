using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DbContextExtensionsExamples.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Blogs",
                columns: table => new
                {
                    BlogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Discriminator = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    RssUrl = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blogs", x => x.BlogId);
                });

            migrationBuilder.CreateTable(
                name: "ComplexOwnedTypeOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnedShippingAddress_Street = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OwnedShippingAddress_Location_Lat = table.Column<double>(type: "float", nullable: false),
                    OwnedShippingAddress_Location_Lng = table.Column<double>(type: "float", nullable: false),
                    ComplexShippingAddress_Street = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ComplexShippingAddress_Location_Lat = table.Column<double>(type: "float", nullable: true),
                    ComplexShippingAddress_Location_Lng = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplexOwnedTypeOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComplexTypeOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShippingAddress_Street = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShippingAddress_Location_Lat = table.Column<double>(type: "float", nullable: true),
                    ShippingAddress_Location_Lng = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplexTypeOrders", x => x.Id);
                });

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
                name: "JsonComplexOwnedTypeOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComplexShippingAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OwnedShippingAddress = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JsonComplexOwnedTypeOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JsonComplexTypeOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShippingAddress = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JsonComplexTypeOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JsonOwnedTypeOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShippingAddress = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JsonOwnedTypeOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OwnedTypeOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShippingAddress_Street = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShippingAddress_Location_Lat = table.Column<double>(type: "float", nullable: false),
                    ShippingAddress_Location_Lng = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OwnedTypeOrders", x => x.Id);
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
                name: "Blogs");

            migrationBuilder.DropTable(
                name: "ComplexOwnedTypeOrders");

            migrationBuilder.DropTable(
                name: "ComplexTypeOrders");

            migrationBuilder.DropTable(
                name: "CompositeKeyRows");

            migrationBuilder.DropTable(
                name: "ConfigurationEntries");

            migrationBuilder.DropTable(
                name: "JsonComplexOwnedTypeOrders");

            migrationBuilder.DropTable(
                name: "JsonComplexTypeOrders");

            migrationBuilder.DropTable(
                name: "JsonOwnedTypeOrders");

            migrationBuilder.DropTable(
                name: "OwnedTypeOrders");

            migrationBuilder.DropTable(
                name: "Rows");
        }
    }
}
