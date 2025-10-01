using System.Security.Cryptography;
using System.Text;
using Dapper;
using Npgsql;

namespace DevHabit.Api.Services;

public class PostgresAdvisoryLockService
{
    private readonly NpgsqlDataSource _dataSource;

    public PostgresAdvisoryLockService(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<bool> TryAcquireLockAsync(string key)
    {
        await using var connection = await _dataSource.OpenConnectionAsync();

        var lockKey = HashKey(key);

        return await connection.ExecuteScalarAsync<bool>(
            "SELECT pg_try_advisory_lock(@key)", new { key = lockKey });
    }

    public async Task ReleaseLockAsync(string key)
    {
        await using var connection = await _dataSource.OpenConnectionAsync();

        var lockKey = HashKey(key);

        await connection.ExecuteAsync(
            "SELECT pg_advisory_unlock(@key)", new { key = lockKey });
    }

    private static long HashKey(string key) =>
        BitConverter.ToInt64(SHA256.HashData(Encoding.UTF8.GetBytes(key)), 0);
}
