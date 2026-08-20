using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DareToDance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SnakeCaseNaming : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.RenameTable(
                name: "Permissions",
                newName: "permissions");

            migrationBuilder.RenameTable(
                name: "UserPermissions",
                newName: "user_permissions");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Users",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "Users",
                newName: "phone");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Users",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Users",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserRole",
                table: "Users",
                newName: "user_role");

            migrationBuilder.RenameColumn(
                name: "UpdatedAtUtc",
                table: "Users",
                newName: "updated_at_utc");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "Users",
                newName: "last_name");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "Users",
                newName: "first_name");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "Users",
                newName: "created_at_utc");

            migrationBuilder.RenameIndex(
                name: "IX_Users_Email",
                table: "Users",
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
                table: "Users",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_permissions",
                table: "permissions",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_user_permissions",
                table: "user_permissions",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_users_phone",
                table: "Users",
                column: "phone",
                unique: true);

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
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_user_permissions_permissions_permission_id",
                table: "user_permissions");

            migrationBuilder.DropForeignKey(
                name: "fk_user_permissions_users_user_id",
                table: "user_permissions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_users",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "ix_users_phone",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "pk_permissions",
                table: "permissions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_user_permissions",
                table: "user_permissions");

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
                name: "user_role",
                table: "Users",
                newName: "UserRole");

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
    }
}
