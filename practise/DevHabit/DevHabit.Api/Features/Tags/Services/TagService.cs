using DevHabit.Api.Common.DataShaping;
using DevHabit.Api.Common.Hateoas;
using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Common;
using DevHabit.Api.DTOs.Habits;
using DevHabit.Api.Extensions;
using DevHabit.Api.Features.Habits.Controller;
using DevHabit.Api.Features.Habits.DTO.Queries;
using DevHabit.Api.Features.Tags.Controller;
using DevHabit.Api.Features.Tags.DTO;
using DevHabit.Api.Features.Tags.Entities;
using DevHabit.Api.Features.Tags.Mappings;
using DevHabit.Api.Features.Tags.Operations;
using DevHabit.Api.Features.Tags.Parameters;
using DevHabit.Api.Features.Tags.Queries;
using DevHabit.Api.Generics;
using DevHabit.Api.Services;
using IdentityModel.Client;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Features.Tags.Services;

public sealed class TagService : CrudServiceBase<Tag, TagDto, TagDto, CreateTagDto, UpdateTagDto, TagsParameters>, ITagService
{

    public readonly ICacheService _cacheService;
    private readonly PostgresAdvisoryLockService _lockService;
    public TagService(ApplicationDbContext db, LinkService linkService, ICacheService cacheService, PostgresAdvisoryLockService lockService) : base(db, linkService)
    {
        _cacheService = cacheService;
        _lockService = lockService;
    }
    public override ICollection<LinkDto> GetItemLinks(string id, string? fields = null) =>
        new List<LinkDto>
        {
            _linkService.Create(nameof(TagsController.GetTag), LinkRelations.Self, HttpMethods.Get, new { id, fields, version = "1.0" }, "Tags"),
            _linkService.Create(nameof(TagsController.UpdateTag), LinkRelations.Update, HttpMethods.Put, new { id, version = "1.0" }, "Tags"),
            _linkService.Create(nameof(TagsController.CreateTag), LinkRelations.Create, HttpMethods.Post, new { version = "1.0" }, "Tags"),
            _linkService.Create(nameof(TagsController.DeleteTag), LinkRelations.Delete, HttpMethods.Delete, new { id, version = "1.0" }, "Tags"),
        };
    public override ICollection<LinkDto> GetCollectionLinks(TagsParameters parameters, bool hasPreviousPage, bool hasNextPage)
    {
        var links = new List<LinkDto>
        {
            _linkService.Create(nameof(TagsController.GetTags), LinkRelations.Self, HttpMethods.Get, new
            {
                q = parameters.SearchTerm,
                fields = parameters.Fields,
                sort = parameters.Sort,
                page = parameters.Page,
                page_size = parameters.PageSize,
                version = "1.0"
            }),
            _linkService.Create(nameof(TagsController.CreateTag), LinkRelations.Create, HttpMethods.Post, new { version = "1.0" })
        };
        if (hasPreviousPage)
        {
            links.Add(
                _linkService.Create(nameof(TagsController.GetTags), LinkRelations.PreviousPage, HttpMethods.Get, new
                {
                    q = parameters.SearchTerm,
                    fields = parameters.Fields,
                    sort = parameters.Sort,
                    page = parameters.Page - 1,
                    page_size = parameters.PageSize,
                    version = "1.0"
                }));
        }
        if (hasNextPage)
        {
            links.Add(
                _linkService.Create(nameof(TagsController.GetTags), LinkRelations.NextPage, HttpMethods.Get, new
                {
                    q = parameters.SearchTerm,
                    fields = parameters.Fields,
                    sort = parameters.Sort,
                    page = parameters.Page + 1,
                    page_size = parameters.PageSize,
                    version = "1.0"
                }));
        }
        return links;
    }
    public override async Task<ShapedPaginationResult<TagDto>> GetAllAsync(
        string userId, TagsParameters queryParameters, CancellationToken cancellationToken)
    {
        string? normalizedSearch = queryParameters.SearchTerm?.Trim().ToLowerInvariant();

        // Kreiraj cache key baziran na parametrima query-a
        var cacheKey = $"Tags:{userId}:{normalizedSearch}:{queryParameters.SearchTerm}:{queryParameters.Sort}:{queryParameters.Page}:{queryParameters.PageSize}:{queryParameters.Fields}";

        // Pokušaj da uzmeš iz cache-a
        var cached = await _cacheService.GetAsync<ShapedPaginationResult<TagDto>>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        // Ako nema u cache-u, idi u bazu
        var query = _db.Tags.AsNoTracking()
         
            .Where(h => h.UserId == userId)
            .Where(h => normalizedSearch == null
                        || h.Name.ToLower().Contains(normalizedSearch)
                        || h.Description != null && h.Description.ToLower().Contains(normalizedSearch))
            //.Where(h => queryParameters. == null || h.Type == queryParameters.Type)
            //.Where(h => queryParameters.Status == null || h.Status == queryParameters.Status)
            .SortByQueryString(queryParameters.Sort, TagMappings.SortMapping.Mappings)
            .Select(TagQueries.ProjectToDto());

        var result = await query
            .ToShapedPaginationResultAsync(queryParameters.Page, queryParameters.PageSize, queryParameters.Fields, cancellationToken)
            .WithHateoasAsync(new HateoasPaginationOptions<TagDto>
            {
                ItemLinksFactory = x => GetItemLinks(x.Id, queryParameters.Fields),
                CollectionLinksFactory = x => GetCollectionLinks(queryParameters, x.HasPreviousPage, x.HasNextPage),
                AcceptHeader = queryParameters.Accept
            }, cancellationToken);

        // Sačuvaj rezultat u cache
        await _cacheService.SetAsync(cacheKey, result, cancellationToken);

        return result;
    }
    public override async Task<ShapedResult<TagDto>?> GetAsync(string id, string userId, string? fields, string? acceptHeader, CancellationToken cancellationToken)
    {
        var cacheKey = $"Tag:{userId}:{id}:{fields}";
        var cached = await _cacheService.GetAsync<ShapedResult<TagDto>>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var query = _db.Tags.AsNoTracking()
            .Where(h => h.Id == id && h.UserId == userId)
            .Select(TagQueries.ProjectToDto());

        var result = await query
            .ToShapedFirstOrDefaultAsync(fields, cancellationToken)
            .WithHateoasAsync(GetItemLinks(id, fields), acceptHeader, cancellationToken);

        if (result is not null)
        {
            // Convert ShapedResult<TagDto> to ShapedResult<object>
            var shapedResultObject = new ShapedResult<TagDto>
            {
                Item = result.Item,
                OriginalItem = result.OriginalItem
            };
            await _cacheService.SetAsync(cacheKey, shapedResultObject, cancellationToken);
            return shapedResultObject;
        }

        return null;
    }
    public override async Task<TagDto> CreateAsync(CreateTagDto dto, string userId)
    {
        var tag = dto.ToEntity(userId);
        // Opcionalno: evict / obriši relevantan cache
        // Na primer, lista svih habit-a za korisnika
        var listCachePrefix = $"Tags:{userId}:";
        await _cacheService.RemoveByPrefixAsync(listCachePrefix); // vidi napomenu ispod
        _db.Tags.Add(tag);
        await _db.SaveChangesAsync();
        return tag.ToDto();
    }
    public override async Task<TagDto?> UpdateAsync(string id, UpdateTagDto dto)
    {
        await _lockService.AcquireLockAsync(id); // uzmi distributed lock
        try
        {
            var tag = await _db.Tags.FirstOrDefaultAsync(h => h.Id == id);

            if (tag is null)
            {
                return null;
            }

            tag.UpdateFromDto(dto);
            await _db.SaveChangesAsync();

            var habitCacheKey = $"tag:{tag.UserId}:{id}:*"; // wildcard za fields
            await _cacheService.RemoveByPrefixAsync(habitCacheKey);

            // Obriši keš liste za tog korisnika
            var listCachePrefix = $"tag:{tag.UserId}:";
            await _cacheService.RemoveByPrefixAsync(listCachePrefix);

            return tag.ToDto();
        }
        finally
        {
            // OBAVEZNO oslobodi lock
            await _lockService.ReleaseLockAsync(id);
        }
    }

    public override async Task<bool> DeleteAsync(string id)
    {
        var tag = await _db.Tags.FirstOrDefaultAsync(h => h.Id == id);
        if (tag is null)
        {

            return false;
        }

        _db.Tags.Remove(tag);
        await _db.SaveChangesAsync();

        var habitCacheKey = $"tag:{tag.UserId}:{id}:*";
        await _cacheService.RemoveByPrefixAsync(habitCacheKey);

        var listCachePrefix = $"tags:{tag.UserId}:";
        await _cacheService.RemoveByPrefixAsync(listCachePrefix);

        return true;
    }





}
