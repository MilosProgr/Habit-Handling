using DevHabit.Api.Features.Tags.DTO;
using DevHabit.Api.Features.Tags.Operations;
using DevHabit.Api.Features.Tags.Parameters;
using DevHabit.Api.Generics;

namespace DevHabit.Api.Features.Tags.Services;

public interface ITagService : ICrudService<TagDto, TagDto, CreateTagDto, UpdateTagDto, TagsParameters>
{
}
