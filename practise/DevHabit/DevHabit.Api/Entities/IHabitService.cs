using DevHabit.Api.DTOs.Habits;
using DevHabit.Api.Generics;

namespace DevHabit.Api.Entities;

public interface IHabitService : ICrudService<HabitDto, HabitWithTagsDto, CreateHabitDto, UpdateHabitDto, HabitsQueryParameters>
{
    // Dodatne metode specifične za Habit
    Task<IEnumerable<HabitDto>> GetMostPopularHabitsAsync(int top, CancellationToken cancellationToken);
}
