using BeachTennis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeachTennis.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261017190000_AddIsAdminToAtleta")]
public partial class AddIsAdminToAtleta : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsAdmin",
            table: "Atletas",
            type: "boolean",
            nullable: false,
            defaultValue: false);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "IsAdmin",
            table: "Atletas");
    }
}
