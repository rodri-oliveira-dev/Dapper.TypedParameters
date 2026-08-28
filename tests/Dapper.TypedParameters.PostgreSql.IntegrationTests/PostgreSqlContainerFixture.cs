using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace Dapper.TypedParameters.PostgreSql.IntegrationTests;

public static class PostgreSqlIntegrationCollectionNames
{
    public const string Default = "PostgreSqlIntegration";
}

[CollectionDefinition(PostgreSqlIntegrationCollectionNames.Default, DisableParallelization = true)]
public sealed class PostgreSqlIntegrationCollection : ICollectionFixture<PostgreSqlContainerFixture>
{
}

public sealed class PostgreSqlContainerFixture : IAsyncLifetime
{
    // PostgreSQL 17 is a stable supported release; the Debian suite is pinned
    // to keep OS image changes explicit and reproducible.
    public const string PostgreSqlImage = "postgres:17.6-bookworm";

    private static readonly TimeSpan ReadyTimeout = TimeSpan.FromSeconds(90);
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(1);

    private readonly PostgreSqlContainer container = new PostgreSqlBuilder(PostgreSqlImage)
        .WithDatabase("typedparameters")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public NpgsqlConnection CreateConnection() =>
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

                await using var command = connection.CreateCommand();
                command.CommandText = "SELECT 1";
                command.CommandTimeout = 5;

                object? result = await command.ExecuteScalarAsync(timeout.Token);

                if (result is int value && value == 1)
                {
                    return;
                }
            }
            catch (Exception exception) when (
                exception is NpgsqlException ||
                exception is InvalidOperationException ||
                exception is OperationCanceledException ||
                exception is TimeoutException)
            {
                lastException = exception;
            }

            await Task.Delay(RetryDelay);
        }

        throw new TimeoutException(
            "PostgreSQL container did not become ready before the configured timeout.",
            lastException);
    }
}
