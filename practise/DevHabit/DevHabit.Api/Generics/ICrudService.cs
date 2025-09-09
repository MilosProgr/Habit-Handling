
using DevHabit.Api.Common.DataShaping;
using DevHabit.Api.Common.Pagination;
using DevHabit.Api.DTOs.Common;
using Microsoft.AspNetCore.Http;

namespace DevHabit.Api.Generics;
public interface ICrudService<TEntityDto, TWithTagsDto, TCreateDto, TUpdateDto, TQueryParameters>
{
    Task<ShapedPaginationResult<TEntityDto>> GetAllAsync(
        string userId,
        TQueryParameters queryParameters,
        CancellationToken cancellationToken);

    Task<ShapedResult<TWithTagsDto>?> GetAsync(
        string id,
        string userId,
        string? fields,
        string? acceptHeader,
        CancellationToken cancellationToken);

    Task<TEntityDto> CreateAsync(TCreateDto dto, string userId);

    Task<TEntityDto?> UpdateAsync(string id, TUpdateDto dto);

    Task<bool> DeleteAsync(string id);

    ICollection<LinkDto> GetItemLinks(string id, string? fields = null);

    ICollection<LinkDto> GetCollectionLinks(TQueryParameters parameters, bool hasPreviousPage, bool hasNextPage);
}
