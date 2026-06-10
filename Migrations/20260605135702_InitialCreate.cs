using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpaceAgro.DotNetApi.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TB_LEITURA_SENSOR",
                columns: table => new
                {
                    ID_LEITURA = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    TEMPERATURA = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    UMIDADE_AR = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    UMIDADE_SOLO = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    DATA_HORA = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    ID_DISPOSITIVO = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_LEITURA_SENSOR", x => x.ID_LEITURA);
                });

            migrationBuilder.CreateTable(
                name: "TB_TALHAO",
                columns: table => new
                {
                    ID_TALHAO = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NOME_TALHAO = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    CULTURA = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    AREA_HECTARES = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    LATITUDE = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    LONGITUDE = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    ID_PRODUTOR = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_TALHAO", x => x.ID_TALHAO);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TB_LEITURA_SENSOR");

            migrationBuilder.DropTable(
                name: "TB_TALHAO");
        }
    }
}
