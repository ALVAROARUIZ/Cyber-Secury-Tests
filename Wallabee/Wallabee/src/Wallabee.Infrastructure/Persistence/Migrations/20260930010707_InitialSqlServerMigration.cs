using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wallabee.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialSqlServerMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            SqlServerBaseline.Apply(migrationBuilder);
        }

        protected override void Down(MigrationBuilder migrationBuilder) =>
            throw new NotSupportedException("This migration adopts existing laboratory tables. Restore a backup instead of dropping real data.");
    }
}
