namespace DevHabit.Api.Common.Pagination;

public interface ICollectionResult<T>
{
     IReadOnlyCollection<T> Data { get; init; }
}
