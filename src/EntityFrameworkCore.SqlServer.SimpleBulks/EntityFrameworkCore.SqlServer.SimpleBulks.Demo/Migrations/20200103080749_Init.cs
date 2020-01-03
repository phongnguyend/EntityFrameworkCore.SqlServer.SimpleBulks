using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Demo.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompositeKeyRows",
                columns: table => new
                {
                    Id1 = table.Column<int>(nullable: false),
                    Id2 = table.Column<int>(nullable: false),
                    Column1 = table.Column<int>(nullable: false),
                    Column2 = table.Column<string>(nullable: true),
                    Column3 = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompositeKeyRows", x => new { x.Id1, x.Id2 });
                });

            migrationBuilder.CreateTable(
                name: "Rows",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Column1 = table.Column<int>(nullable: false),
                    Column2 = table.Column<string>(nullable: true),
                    Column3 = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rows", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompositeKeyRows");

            migrationBuilder.DropTable(
                name: "Rows");
        }
    }
}
