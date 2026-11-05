using ASI.Basecode.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ASI.Basecode.IntegrationTests;

public class SqlServerSmokeTests
{
    [Fact]
    public async Task ConfiguredSqlServerAcceptsAQuery()
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        Assert.False(
            string.IsNullOrWhiteSpace(connectionString),
            "Set ConnectionStrings__DefaultConnection to a local SQL Server instance for integration tests.");

        var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
        var sqlServerPassword = Environment.GetEnvironmentVariable("MSSQL_SA_PASSWORD");
        if (!string.IsNullOrEmpty(sqlServerPassword))
        {
            connectionStringBuilder.Password = sqlServerPassword;
        }

        Assert.True(
            IsLocalSqlServer(connectionStringBuilder.DataSource),
            "Integration tests may create AttendTaMVP_Test only on a local SQL Server instance.");

        var cancellationToken = TestContext.Current.CancellationToken;
        connectionStringBuilder.InitialCatalog = "master";
        var masterOptions = new DbContextOptionsBuilder<AsiBasecodeDBContext>()
            .UseSqlServer(connectionStringBuilder.ConnectionString)
            .Options;

        await using (var masterContext = new AsiBasecodeDBContext(masterOptions))
        {
            await masterContext.Database.ExecuteSqlRawAsync(
                "IF DB_ID(N'AttendTaMVP_Test') IS NULL CREATE DATABASE [AttendTaMVP_Test];",
                cancellationToken);
        }

        connectionStringBuilder.InitialCatalog = "AttendTaMVP_Test";

        var options = new DbContextOptionsBuilder<AsiBasecodeDBContext>()
            .UseSqlServer(connectionStringBuilder.ConnectionString)
            .Options;

        await using var context = new AsiBasecodeDBContext(options);
        var result = await context.Database
            .SqlQueryRaw<int>("SELECT CAST(1 AS int) AS [Value]")
            .SingleAsync(cancellationToken);

        Assert.Equal(1, result);
    }

    private static bool IsLocalSqlServer(string dataSource)
    {
        return dataSource.StartsWith("(localdb)\\", StringComparison.OrdinalIgnoreCase)
            || dataSource.Equals(".", StringComparison.OrdinalIgnoreCase)
            || dataSource.StartsWith(".\\", StringComparison.OrdinalIgnoreCase)
            || dataSource.Equals("(local)", StringComparison.OrdinalIgnoreCase)
            || dataSource.Equals("localhost", StringComparison.OrdinalIgnoreCase)
            || dataSource.StartsWith("localhost,", StringComparison.OrdinalIgnoreCase)
            || dataSource.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase)
            || dataSource.StartsWith("127.0.0.1,", StringComparison.OrdinalIgnoreCase)
            || dataSource.Equals("::1", StringComparison.OrdinalIgnoreCase)
            || dataSource.StartsWith("::1,", StringComparison.OrdinalIgnoreCase);
    }
}
