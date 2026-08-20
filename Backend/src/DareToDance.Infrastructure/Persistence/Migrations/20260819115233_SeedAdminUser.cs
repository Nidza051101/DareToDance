using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DareToDance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_user_permissions_permissions_permission_id",
                table: "user_permissions");

            migrationBuilder.DropForeignKey(
                name: "fk_user_permissions_users_user_id",
                table: "user_permissions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_users",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "pk_permissions",
                table: "permissions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_user_permissions",
                table: "user_permissions");

            migrationBuilder.RenameTable(
                name: "users",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "permissions",
                newName: "Permissions");

            migrationBuilder.RenameTable(
                name: "user_permissions",
                newName: "UserPermissions");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "Users",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "phone",
                table: "Users",
                newName: "Phone");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "Users",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Users",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at_utc",
                table: "Users",
                newName: "UpdatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "last_name",
                table: "Users",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "first_name",
                table: "Users",
                newName: "FirstName");

            migrationBuilder.RenameColumn(
                name: "created_at_utc",
                table: "Users",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameIndex(
                name: "ix_users_email",
                table: "Users",
                newName: "IX_Users_Email");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Permissions",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Permissions",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Permissions",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at_utc",
                table: "Permissions",
                newName: "UpdatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "created_at_utc",
                table: "Permissions",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameIndex(
                name: "ix_permissions_name",
                table: "Permissions",
                newName: "IX_Permissions_Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "UserPermissions",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "UserPermissions",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "updated_at_utc",
                table: "UserPermissions",
                newName: "UpdatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "permission_id",
                table: "UserPermissions",
                newName: "PermissionId");

            migrationBuilder.RenameColumn(
                name: "created_at_utc",
                table: "UserPermissions",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameIndex(
                name: "ix_user_permissions_user_id_permission_id",
                table: "UserPermissions",
                newName: "IX_UserPermissions_UserId_PermissionId");

            migrationBuilder.RenameIndex(
                name: "ix_user_permissions_permission_id",
                table: "UserPermissions",
                newName: "IX_UserPermissions_PermissionId");

            migrationBuilder.AddColumn<string>(
                name: "UserRole",
                table: "Users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Permissions",
                table: "Permissions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserPermissions",
                table: "UserPermissions",
                column: "Id");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAtUtc", "Email", "FirstName", "LastName", "Phone", "Status", "UpdatedAtUtc", "UserRole" },
                values: new object[] { new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3301"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "nikolaandricw@gmail.com", "Nikola", "Andric", "0641059679", "Active", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Admin" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserPermissions_Permissions_PermissionId",
                table: "UserPermissions",
                column: "PermissionId",
                principalTable: "Permissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPermissions_Users_UserId",
                table: "UserPermissions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserPermissions_Permissions_PermissionId",
                table: "UserPermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPermissions_Users_UserId",
                table: "UserPermissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Permissions",
                table: "Permissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserPermissions",
                table: "UserPermissions");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3301"));

            migrationBuilder.DropColumn(
                name: "UserRole",
                table: "Users");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "users");

            migrationBuilder.RenameTable(
                name: "Permissions",
                newName: "permissions");

            migrationBuilder.RenameTable(
                name: "UserPermissions",
                newName: "user_permissions");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "users",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "users",
                newName: "phone");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "users",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "users",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAtUtc",
                table: "users",
                newName: "updated_at_utc");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "users",
                newName: "last_name");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "users",
                newName: "first_name");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "users",
                newName: "created_at_utc");

            migrationBuilder.RenameIndex(
                name: "IX_Users_Email",
                table: "users",
                newName: "ix_users_email");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "permissions",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "permissions",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "permissions",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAtUtc",
                table: "permissions",
                newName: "updated_at_utc");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "permissions",
                newName: "created_at_utc");

            migrationBuilder.RenameIndex(
                name: "IX_Permissions_Name",
                table: "permissions",
                newName: "ix_permissions_name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "user_permissions",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "user_permissions",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAtUtc",
                table: "user_permissions",
                newName: "updated_at_utc");

            migrationBuilder.RenameColumn(
                name: "PermissionId",
                table: "user_permissions",
                newName: "permission_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "user_permissions",
                newName: "created_at_utc");

            migrationBuilder.RenameIndex(
                name: "IX_UserPermissions_UserId_PermissionId",
                table: "user_permissions",
                newName: "ix_user_permissions_user_id_permission_id");

            migrationBuilder.RenameIndex(
                name: "IX_UserPermissions_PermissionId",
                table: "user_permissions",
                newName: "ix_user_permissions_permission_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_users",
                table: "users",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_permissions",
                table: "permissions",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_user_permissions",
                table: "user_permissions",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_user_permissions_permissions_permission_id",
                table: "user_permissions",
                column: "permission_id",
                principalTable: "permissions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_permissions_users_user_id",
                table: "user_permissions",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
