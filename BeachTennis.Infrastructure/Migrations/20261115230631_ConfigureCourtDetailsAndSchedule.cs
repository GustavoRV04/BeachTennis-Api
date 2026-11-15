using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeachTennis.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureCourtDetailsAndSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cobertura",
                table: "Quadras",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Ao ar livre");

            migrationBuilder.AddColumn<string[]>(
                name: "Comodidades",
                table: "Quadras",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<string>(
                name: "Descricao",
                table: "Quadras",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImagemUrl",
                table: "Quadras",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HorarioQuadra",
                columns: table => new
                {
                    QuadraId = table.Column<Guid>(type: "uuid", nullable: false),
                    DiaSemana = table.Column<int>(type: "integer", nullable: false),
                    Aberta = table.Column<bool>(type: "boolean", nullable: false),
                    AbreAs = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    FechaAs = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    ValorHoraAntesDasDezoito = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    ValorHoraAposDezoito = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorarioQuadra", x => new { x.QuadraId, x.DiaSemana });
                    table.ForeignKey(
                        name: "FK_HorarioQuadra_Quadras_QuadraId",
                        column: x => x.QuadraId,
                        principalTable: "Quadras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("""
                INSERT INTO "HorarioQuadra"
                    ("QuadraId", "DiaSemana", "Aberta", "AbreAs", "FechaAs",
                     "ValorHoraAntesDasDezoito", "ValorHoraAposDezoito")
                SELECT quadra."Id", dias."DiaSemana", TRUE, TIME '07:00', TIME '23:00', 100.00, 120.00
                FROM "Quadras" AS quadra
                CROSS JOIN generate_series(0, 6) AS dias("DiaSemana");
                """);

            migrationBuilder.Sql("""
                UPDATE "Quadras"
                SET "Descricao" = CASE
                        WHEN lower("Nome") LIKE '%central%' THEN 'Areia tratada e cobertura termoacústica'
                        WHEN lower("Nome") LIKE '%sunset%' THEN 'Vista aberta com brisa suave'
                        WHEN lower("Nome") LIKE '%prime%' THEN 'Perfeita para jogos e resenha'
                        ELSE 'Quadra disponível para reservas na arena'
                    END,
                    "Cobertura" = CASE
                        WHEN lower("Nome") LIKE '%central%' THEN 'Coberta'
                        WHEN lower("Nome") LIKE '%prime%' THEN 'Semi-coberta'
                        ELSE 'Ao ar livre'
                    END,
                    "Comodidades" = CASE
                        WHEN lower("Nome") LIKE '%central%' THEN ARRAY['Iluminação LED', 'Areia fina tipo A']
                        WHEN lower("Nome") LIKE '%sunset%' THEN ARRAY['Refletores Pro', 'Ducha externa']
                        WHEN lower("Nome") LIKE '%prime%' THEN ARRAY['Bebedouro próximo', 'Banco na sombra']
                        ELSE ARRAY[]::text[]
                    END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HorarioQuadra");

            migrationBuilder.DropColumn(
                name: "Cobertura",
                table: "Quadras");

            migrationBuilder.DropColumn(
                name: "Comodidades",
                table: "Quadras");

            migrationBuilder.DropColumn(
                name: "Descricao",
                table: "Quadras");

            migrationBuilder.DropColumn(
                name: "ImagemUrl",
                table: "Quadras");
        }
    }
}
