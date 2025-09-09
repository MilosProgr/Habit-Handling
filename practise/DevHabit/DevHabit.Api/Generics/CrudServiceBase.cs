using DevHabit.Api.Common.DataShaping;
using DevHabit.Api.Common.Hateoas;
using DevHabit.Api.Common.Pagination;
using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Common;
using DevHabit.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Generics;

public abstract class CrudServiceBase<TEntity, TEntityDto, TWithTagsDto, TCreateDto, TUpdateDto, TQueryParameters>
    : ICrudService<TEntityDto, TWithTagsDto, TCreateDto, TUpdateDto, TQueryParameters>
    where TEntity : class
{
    protected readonly ApplicationDbContext _db;
    protected readonly LinkService _linkService;

    protected CrudServiceBase(ApplicationDbContext db, LinkService linkService)
    {
        _db = db;
        _linkService = linkService;
    }

    public abstract ICollection<LinkDto> GetItemLinks(string id, string? fields = null);
    public abstract ICollection<LinkDto> GetCollectionLinks(TQueryParameters parameters, bool hasPreviousPage, bool hasNextPage);
    public abstract Task<TEntityDto> CreateAsync(TCreateDto dto, string userId);
    public abstract Task<TEntityDto?> UpdateAsync(string id, TUpdateDto dto);
    public abstract Task<bool> DeleteAsync(string id);

    public abstract Task<ShapedPaginationResult<TEntityDto>> GetAllAsync(string userId, TQueryParameters queryParameters, CancellationToken cancellationToken);

    public abstract Task<ShapedResult<TWithTagsDto>?> GetAsync(string id, string userId, string? fields, string? acceptHeader, CancellationToken cancellationToken);
}
