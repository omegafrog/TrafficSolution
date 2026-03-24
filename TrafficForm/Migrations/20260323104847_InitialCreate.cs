using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrafficForm.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CoordinateFavorites",
                columns: table => new
                {
                    FavoriteId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: false),
                    longitude = table.Column<double>(type: "double precision", nullable: false),
                    zoom_level = table.Column<int>(type: "integer", nullable: false),
                    min_longitude = table.Column<double>(type: "double precision", nullable: false),
                    min_latitude = table.Column<double>(type: "double precision", nullable: false),
                    max_longitude = table.Column<double>(type: "double precision", nullable: false),
                    max_latitude = table.Column<double>(type: "double precision", nullable: false),
                    SavedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoordinateFavorites", x => x.FavoriteId);
                });

            migrationBuilder.CreateTable(
                name: "HighwayFavorites",
                columns: table => new
                {
                    FavoriteId = table.Column<string>(type: "text", nullable: false),
                    HighwayNo = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: false),
                    longitude = table.Column<double>(type: "double precision", nullable: false),
                    zoom_level = table.Column<int>(type: "integer", nullable: false),
                    min_longitude = table.Column<double>(type: "double precision", nullable: false),
                    min_latitude = table.Column<double>(type: "double precision", nullable: false),
                    max_longitude = table.Column<double>(type: "double precision", nullable: false),
                    max_latitude = table.Column<double>(type: "double precision", nullable: false),
                    SavedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HighwayFavorites", x => x.FavoriteId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CoordinateFavorites");

            migrationBuilder.DropTable(
                name: "HighwayFavorites");
        }
    }
}
