using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Vale.Server.Data;

#nullable disable

namespace Vale.Server.Migrations
{
    [DbContext(typeof(GameDbContext))]
    [Migration("202605110001_InitialCreate")]
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Structures",
                columns: table => new
                {
                    StructureId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChunkX = table.Column<int>(type: "INTEGER", nullable: false),
                    ChunkY = table.Column<int>(type: "INTEGER", nullable: false),
                    TileX = table.Column<int>(type: "INTEGER", nullable: false),
                    TileY = table.Column<int>(type: "INTEGER", nullable: false),
                    StructureType = table.Column<int>(type: "INTEGER", nullable: false),
                    OwnerPlayerId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    PlacedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Structures", x => x.StructureId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Structures_ChunkX_ChunkY_TileX_TileY",
                table: "Structures",
                columns: new[] { "ChunkX", "ChunkY", "TileX", "TileY" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Structures");
        }
    }
}
