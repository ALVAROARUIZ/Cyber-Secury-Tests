using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wallabee.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PreserveVersionContractLengths : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Version",
                schema: "lab",
                table: "WallabeeVersions",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldUnicode: false,
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Version",
                schema: "lab",
                table: "HoneyBadgerVersions",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldUnicode: false,
                oldMaxLength: 50);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) =>
            throw new NotSupportedException("Version columns cannot be narrowed automatically; restore a verified backup.");
    }
}
