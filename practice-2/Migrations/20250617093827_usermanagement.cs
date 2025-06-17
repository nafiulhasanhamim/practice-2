using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace practice_2.Migrations
{
    /// <inheritdoc />
    public partial class usermanagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "772d891f-49c9-450e-b14a-bbacf3b762b8");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a1d27a62-dfd8-4137-be35-f51825c41612");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "2eafb47c-7296-43be-81c0-827c46c2591d", "1", "Admin", "ADMIN" },
                    { "b1f977ef-dab2-4831-8ab3-722adc77fded", "2", "User", "USER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2eafb47c-7296-43be-81c0-827c46c2591d");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b1f977ef-dab2-4831-8ab3-722adc77fded");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "772d891f-49c9-450e-b14a-bbacf3b762b8", "1", "Admin", "ADMIN" },
                    { "a1d27a62-dfd8-4137-be35-f51825c41612", "2", "User", "USER" }
                });
        }
    }
}
