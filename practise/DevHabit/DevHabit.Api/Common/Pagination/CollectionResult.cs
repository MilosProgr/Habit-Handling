using DevHabit.Api.Common.Hateoas;
using DevHabit.Api.DTOs.Common;
using ILinksResponse = DevHabit.Api.Common.Hateoas.ILinksResponse;


namespace DevHabit.Api.Common.Pagination;

public sealed record CollectionResult<T> : ICollectionResult<T>, ILinksResponse
{
    public required IReadOnlyCollection<T> Data { get; init; }
    public IReadOnlyCollection<LinkDto> Links { get; init; } = [];
}
