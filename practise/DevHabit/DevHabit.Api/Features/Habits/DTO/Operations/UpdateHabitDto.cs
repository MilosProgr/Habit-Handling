using DevHabit.Api.DTOs.Habits;
using DevHabit.Api.Features.Habits.Entities;
using static DevHabit.Api.Features.Habits.Entities.Habit;

namespace DevHabit.Api.Features.Habits.DTO.Operations;

public sealed record UpdateHabitDto
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required HabitType Type { get; init; }
    public required FrequencyDto Frequency { get; init; }
    public required TargetDto Target { get; init; }
    public DateOnly? EndDate { get; init; }
    public UpdateMilestoneDto? Milestone { get; init; }

    public AutomationSource? AutomationSource { get; init; }

}

public sealed record UpdateMilestoneDto
{
    public required int Target { get; init; }
}
