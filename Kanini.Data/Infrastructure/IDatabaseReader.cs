namespace Kanini.Data.Infrastructure;

public interface IDatabaseReader
{
    Task<T?> QuerySingleOrDefaultAsync<T>(string storedProcedure, object? parameters = null);
    Task<IEnumerable<T>> QueryAsync<T>(string storedProcedure, object? parameters = null);
}