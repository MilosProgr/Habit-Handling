using DevHabit.Api.DTOs.Common;

namespace DevHabit.Api.Common.Hateoas;

public interface ILinksResponse
{
     IReadOnlyCollection<LinkDto> Links { get; init; }
}
