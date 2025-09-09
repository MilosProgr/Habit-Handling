using DevHabit.Api.Common.Hateoas;
using DevHabit.Api.DTOs.Common;

namespace DevHabit.Api.DTOs.NewFolder;

public sealed record UserDto 
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    public string IdentifyId { get; set; }

   
}
