using DevHabit.Api.DTOs.Common;
using DevHabit.Api.Entities;
using Microsoft.AspNetCore.Mvc;

namespace DevHabit.Api.DTOs.Habits;

public sealed record HabitsQueryParameters : AcceptHeaderDto
{
    [FromQuery(Name = "q")]
    public string? Search { get; set; }
    public HabitType? Type { get; init; }
    public HabitStatus? Status { get; init; }
    public string? Sort { get; init; }
    public string? Fields { get; init; }
    public int Page { get; init; } = 1;
    //public int PageSize { get; init; } = 10;

    [FromQuery(Name = "page_size")]
    public int PageSize { get; init; } = 10;

    public void Deconstruct(
        out string? search,
        out HabitType? type,
        out HabitStatus? status,
        out string? sort,
        out string? fields,
        out int page,
        out int pageSize)
    {
        search = Search;
        type = Type;
        status = Status;
        sort = Sort;
        fields = Fields;
        page = Page;
        pageSize = PageSize;
    }
    //[FromHeader(Name = "Accept")]
    //public string? Accept { get; init; }
}
