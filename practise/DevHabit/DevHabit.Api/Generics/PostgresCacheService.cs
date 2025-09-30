using Dapper;
using Npgsql;
using Newtonsoft.Json;

namespace DevHabit.Api.Generics;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default);
    Task RemoveByPrefixAsync(string key, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implements the generic ICacheService using PostgreSQL as the backing store.
/// </summary>
public class PostgresCacheService : ICacheService
{
    private readonly NpgsqlDataSource _dataSource;

    public PostgresCacheService(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
    }

    /// <inheritdoc/>
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        var jsonString = await connection.QueryFirstOrDefaultAsync<string>(
            new CommandDefinition(
                "SELECT value FROM cache WHERE key = @Key",
                new { Key = key },
                cancellationToken: cancellationToken)
            );

        if (string.IsNullOrEmpty(jsonString))
        {
            return default;
        }

        try
        {
            return JsonConvert.DeserializeObject<T>(jsonString);
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error deserializing JSON for key {key} to type {typeof(T).Name}: {ex.Message}");
            return default;
        }
    }

    /// <inheritdoc/>
    public async Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        string jsonString;
        try
        {
            jsonString = JsonConvert.SerializeObject(value);
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error serializing value for key {key} of type {typeof(T).Name}: {ex.Message}");
            throw;
        }

        await connection.ExecuteAsync(
             new CommandDefinition(
                 """
                 INSERT INTO cache(key, value)
                 VALUES (@Key, @Value::jsonb)
                 ON CONFLICT (key) DO UPDATE
                 SET value = excluded.value;
                 """,
                 new { Key = key, Value = jsonString },
                 cancellationToken: cancellationToken)
             );
    }

    /// <inheritdoc/>
    public async Task RemoveByPrefixAsync(string key, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        await connection.ExecuteAsync(
            new CommandDefinition(
                "DELETE FROM cache WHERE key LIKE @Prefix",
                new { Prefix = key + "%" },
                cancellationToken: cancellationToken)
        );
    }
}
