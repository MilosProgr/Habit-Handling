//using DevHabit.Api.Common.DataShaping;
//using DevHabit.Api.Common.Pagination;
//using DevHabit.Api.DTOs.Common;
//using DevHabit.Api.DTOs.Habits;

//namespace DevHabit.Api.Services.Habit;

//public interface IHabitService
//{
//    Task<ShapedPaginationResult<HabitDto>> GetHabitsAsync(
//        string userId,
//        HabitsQueryParameters habitParameters,
//        CancellationToken cancellationToken);

//    Task<ShapedResult<HabitWithTagsDto>?> GetHabitAsync(
//        string habitId,
//        string userId,
//        string? fields,
//        string? acceptHeader,
//        CancellationToken cancellationToken);

//    Task<HabitDto> CreateHabitAsync(CreateHabitDto dto, string userId);

//    Task<HabitDto?> UpdateHabitAsync(string habitId, UpdateHabitDto dto);

//    Task<bool> DeleteHabitAsync(string habitId);

//    ICollection<LinkDto> GetHabitLinks(string habitId, string? fields = null);

//    ICollection<LinkDto> GetHabitsCollectionLinks(HabitsQueryParameters parameters, bool hasPreviousPage, bool hasNextPage);
//}
