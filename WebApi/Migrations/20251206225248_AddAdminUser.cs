using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "password",
                value: "$2a$11$YGQ4M7F8z7t9UwQXxYzVxO4cOE9IzXRz9HYN.R8zG8XwZ7T9QwZ8u");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "password",
                value: "$2a$11$YGQ4M7F8z7t9UwQXxYzVxO4cOE9IzXRz9HYN.R8zG8XwZ7T9QwZ8u");

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "email", "name", "password" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), "admin@example.com", "Admin", "$2a$11$gR.MbESvO35KWLXJOZnWtuvs6fz4l1XE7bjj3u4NgiaqYiBZb8Xdy" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "password",
                value: "$2a$11$K2z3vJqKpZ8X1qZ8X1qZ8O5J3Z8X1qZ8X1qZ8X1qZ8X1qZ8X1qZ8O");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "password",
                value: "$2a$11$K2z3vJqKpZ8X1qZ8X1qZ8O5J3Z8X1qZ8X1qZ8X1qZ8X1qZ8X1qZ8O");
        }
    }
}
