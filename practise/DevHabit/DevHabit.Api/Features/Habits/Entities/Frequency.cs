namespace DevHabit.Api.Features.Habits.Entities;

public sealed class Frequency
{
    public FrequencyType Type { get; set; }
    public int TimesPerPeriod { get; set; }
}
