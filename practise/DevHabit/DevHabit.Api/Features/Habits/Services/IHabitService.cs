using DevHabit.Api.DTOs.Habits;
using DevHabit.Api.Features.Habits.DTO.Create;
using DevHabit.Api.Features.Habits.DTO.Operations;
using DevHabit.Api.Features.Habits.DTO.Queries;
using DevHabit.Api.Generics;

namespace DevHabit.Api.Features.Habits.Services;

public interface IHabitService : ICrudService<HabitDto, HabitWithTagsDto, CreateHabitDto, UpdateHabitDto, HabitsQueryParameters>
{
    // Dodatne metode specifične za Habit
    Task<IEnumerable<HabitDto>> GetMostPopularHabitsAsync(int top, CancellationToken cancellationToken);
}
