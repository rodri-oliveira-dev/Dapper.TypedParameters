using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;
using Xunit;

namespace Dapper.TypedParameters.SqlServer.IntegrationTests;

public static class SqlServerIntegrationCollectionNames
{
    public const string Default = "SqlServerIntegration";
}

[CollectionDefinition(SqlServerIntegrationCollectionNames.Default, DisableParallelization = true)]
public sealed class SqlServerIntegrationCollection : ICollectionFixture<SqlServerContainerFixture>
{
}

public sealed class SqlServerContainerFixture : IAsyncLifetime
{
    private const string SqlServerImage = "mcr.microsoft.com/mssql/server:2022-CU20-ubuntu-22.04";
    private const string AcceptEulaEnvironmentVariable = "ACCEPT_EULA";
    private static readonly TimeSpan ReadyTimeout = TimeSpan.FromSeconds(90);
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(1);

    private readonly MsSqlContainer container = new MsSqlBuilder(SqlServerImage)
        .WithEnvironment(
            AcceptEulaEnvironmentVariable,
            Environment.GetEnvironmentVariable(AcceptEulaEnvironmentVariable) ?? "Y")
        .Build();

    public SqlConnection CreateConnection() =>
        new(container.GetConnectionString());

    public async Task InitializeAsync()
    {
        await container.StartAsync();
        await WaitUntilReadyAsync();
    }

    public async Task DisposeAsync()
    {
        await container.DisposeAsync();
    }

    private async Task WaitUntilReadyAsync()
    {
        using var timeout = new CancellationTokenSource(ReadyTimeout);
        Exception? lastException = null;

        while (!timeout.IsCancellationRequested)
        {
            try
            {
                await using var connection = CreateConnection();
                await connection.OpenAsync(timeout.Token);

                using var command = connection.CreateCommand();
                command.CommandText = "SELECT 1";
                command.CommandTimeout = 5;

                object? result = await command.ExecuteScalarAsync(timeout.Token);

                if (result is int value && value == 1)
                {
                    return;
                }
            }
            catch (Exception exception) when (
                exception is SqlException ||
                exception is InvalidOperationException ||
                exception is OperationCanceledException ||
                exception is TimeoutException)
            {
                lastException = exception;
            }

            await Task.Delay(RetryDelay);
        }

        throw new TimeoutException(
            "SQL Server container did not become ready before the configured timeout.",
            lastException);
    }
}
