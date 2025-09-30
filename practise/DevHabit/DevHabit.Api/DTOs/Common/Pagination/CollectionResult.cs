using DevHabit.Api.Common.Hateoas;
using DevHabit.Api.Common.Pagination;

namespace DevHabit.Api.DTOs.Common.Pagination;

public sealed record CollectionResult<T> : ICollectionResult<T>, ILinksResponse
{
    public required IReadOnlyCollection<T> Data { get; init; }
    public IReadOnlyCollection<LinkDto> Links { get; init; } = [];
}
