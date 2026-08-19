using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DareToDance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLoginCodeToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "login_code_created_at_utc",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "login_code_expires_at_utc",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "login_code_failed_attempts",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "login_code_hash",
                table: "users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "user_role",
                table: "users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "login_code_created_at_utc",
                table: "users");

            migrationBuilder.DropColumn(
                name: "login_code_expires_at_utc",
                table: "users");

            migrationBuilder.DropColumn(
                name: "login_code_failed_attempts",
                table: "users");

            migrationBuilder.DropColumn(
                name: "login_code_hash",
                table: "users");

            migrationBuilder.DropColumn(
                name: "user_role",
                table: "users");
        }
    }
}
