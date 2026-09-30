using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Migrations;
namespace Wallabee.Infrastructure.Persistence.Migrations;
internal static class SqlServerBaseline
{
    private static string Read(string name)
    {
        using var stream = typeof(SqlServerBaseline).Assembly.GetManifestResourceStream("Wallabee.Infrastructure.Persistence.Migrations.Baseline." + name)!;
        using var reader = new StreamReader(stream); return reader.ReadToEnd();
    }
    private static IEnumerable<string> Batches(string text) => Regex.Split(text, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase)
        .Select(s => Regex.Replace(s, @"USE\s+\[DiveViewLab\];", "", RegexOptions.IgnoreCase)).Where(s => !string.IsNullOrWhiteSpace(s));
    public static void Apply(MigrationBuilder migration)
    {
        var create = string.Join("\n", new[] { "01_CreateSchemas.sql", "02_CreateTables.sql", "03_CreateConstraints.sql", "04_CreateIndexes.sql" }
            .SelectMany(name => Batches(Read(name))).Select(batch => "EXEC(N'" + batch.Replace("'", "''") + "');"));
        // A pre-existing baseline is adopted only after every expected table/column is verified.
        migration.Sql("IF OBJECT_ID(N'lab.Scans', N'U') IS NULL BEGIN\n" + create + "\nEND;\n" + Read("ValidateBaseline.sql"));
        foreach (var batch in Batches(Read("05_EfAdditions.sql"))) migration.Sql(batch);
    }
}
