using DevHabit.Api.Common.DataShaping;
using DevHabit.Api.Common.Hateoas;
using DevHabit.Api.Common.Pagination;
using DevHabit.Api.Controllers;
using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Common;
using DevHabit.Api.DTOs.Habits;
using DevHabit.Api.Entities;
using DevHabit.Api.Extensions;
using DevHabit.Api.Generics;
using Microsoft.EntityFrameworkCore;


namespace DevHabit.Api.Services.Habit;

public sealed class HabitService : CrudServiceBase<DevHabit.Api.Entities.Habit, HabitDto, HabitWithTagsDto, CreateHabitDto, UpdateHabitDto, HabitsQueryParameters>,IHabitService

{
    public readonly ICacheService _cacheService;
    public HabitService(ApplicationDbContext db, LinkService linkService, ICacheService cacheService) : base(db, linkService)
    {
        _cacheService = cacheService;
    }

    public override ICollection<LinkDto> GetItemLinks(string id, string? fields = null) =>
        new List<LinkDto>
        {
            _linkService.Create(nameof(HabitsController.GetHabit), LinkRelations.Self, HttpMethods.Get, new {  id, fields, version = "1.0" }, "Habits"),
            _linkService.Create(nameof(HabitsController.UpdateHabit), LinkRelations.Update, HttpMethods.Put, new { id, version = "1.0" }, "Habits"),
            _linkService.Create(nameof(HabitsController.CreateHabit), LinkRelations.Create, HttpMethods.Post, new { version = "1.0" }, "Habits"),
            _linkService.Create(nameof(HabitsController.DeleteHabit), LinkRelations.Delete, HttpMethods.Delete, new { id, version = "1.0" }, "Habits"),
        };

    public override ICollection<LinkDto> GetCollectionLinks(HabitsQueryParameters parameters, bool hasPreviousPage, bool hasNextPage)
    {
        var links = new List<LinkDto>
        {
            _linkService.Create(nameof(HabitsController.GetHabits), LinkRelations.Self, HttpMethods.Get, new
            {
                q = parameters.Search,
                type = parameters.Type,
                status = parameters.Status,
                fields = parameters.Fields,
                sort = parameters.Sort,
                page = parameters.Page,
                page_size = parameters.PageSize,
                version = "1.0"
            }),
            _linkService.Create(nameof(HabitsController.CreateHabit), LinkRelations.Create, HttpMethods.Post, new { version = "1.0" })
        };

        if (hasPreviousPage)
        { 
            links.Add(_linkService.Create(nameof(HabitsController.GetHabits), LinkRelations.PreviousPage, HttpMethods.Get, new { page = parameters.Page - 1, version = "1.0" }));
        }
        if (hasNextPage)
        {

            links.Add(_linkService.Create(nameof(HabitsController.GetHabits), LinkRelations.NextPage, HttpMethods.Get, new { page = parameters.Page + 1, version = "1.0" }));
        }

        return links;
    }

    public override async Task<ShapedPaginationResult<HabitDto>> GetAllAsync(
    string userId, HabitsQueryParameters parameters, CancellationToken cancellationToken)
    {
        string? normalizedSearch = parameters.Search?.Trim().ToLowerInvariant();

        // Kreiraj cache key baziran na parametrima query-a
        var cacheKey = $"habits:{userId}:{normalizedSearch}:{parameters.Type}:{parameters.Status}:{parameters.Sort}:{parameters.Page}:{parameters.PageSize}:{parameters.Fields}";

        // Pokušaj da uzmeš iz cache-a
        var cached = await _cacheService.GetAsync<ShapedPaginationResult<HabitDto>>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        // Ako nema u cache-u, idi u bazu
        var query = _db.Habits.AsNoTracking()
            .Where(h => h.UserId == userId)
            .Where(h => normalizedSearch == null
                        || h.Name.ToLower().Contains(normalizedSearch)
                        || h.Description != null && h.Description.ToLower().Contains(normalizedSearch))
            .Where(h => parameters.Type == null || h.Type == parameters.Type)
            .Where(h => parameters.Status == null || h.Status == parameters.Status)
            .SortByQueryString(parameters.Sort, HabitMappings.SortMapping.Mappings)
            .Select(HabitQueries.ProjectToDto());

        var result = await query
            .ToShapedPaginationResultAsync(parameters.Page, parameters.PageSize, parameters.Fields, cancellationToken)
            .WithHateoasAsync(new HateoasPaginationOptions<HabitDto>
            {
                ItemLinksFactory = x => GetItemLinks(x.Id, parameters.Fields),
                CollectionLinksFactory = x => GetCollectionLinks(parameters, x.HasPreviousPage, x.HasNextPage),
                AcceptHeader = parameters.Accept
            }, cancellationToken);

        // Sačuvaj rezultat u cache
        await _cacheService.SetAsync(cacheKey, result, cancellationToken);

        return result;
    }


    public override async Task<ShapedResult<HabitWithTagsDto>?> GetAsync(string id, string userId, string? fields, string? acceptHeader, CancellationToken cancellationToken)
    {
        var cacheKey = $"habit:{userId}:{id}:{fields}";
        var cached = await _cacheService.GetAsync<ShapedResult<HabitWithTagsDto>>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var query = _db.Habits.AsNoTracking()
            .Where(h => h.Id == id && h.UserId == userId)
            .Select(HabitQueries.ProjectToDtoWithTags());

        var result = await query
        .ToShapedFirstOrDefaultAsync(fields, cancellationToken)
        .WithHateoasAsync(GetItemLinks(id, fields), acceptHeader, cancellationToken);

        if (result is not null)
        {
            await _cacheService.SetAsync(cacheKey, result, cancellationToken);
        }

        return result;
    }

    public override async Task<HabitDto> CreateAsync(CreateHabitDto dto, string userId)
    {

        var habit = dto.ToEntity(userId);
        // Opcionalno: evict / obriši relevantan cache
        // Na primer, lista svih habit-a za korisnika
        var listCachePrefix = $"habits:{userId}:";
        await _cacheService.RemoveByPrefixAsync(listCachePrefix); // vidi napomenu ispod
        _db.Habits.Add(habit);
        await _db.SaveChangesAsync();
        return habit.ToDto();
    }

    public override async Task<HabitDto?> UpdateAsync(string id, UpdateHabitDto dto)
    {
        var habit = await _db.Habits.FirstOrDefaultAsync(h => h.Id == id);
        if (habit is null)
        {

            return null;
        }

        habit.UpdateFromDto(dto);
        await _db.SaveChangesAsync();
        var habitCacheKey = $"habit:{habit.UserId}:{id}:*"; // wildcard za fields
        await _cacheService.RemoveByPrefixAsync(habitCacheKey);

        // Obriši keš liste za tog korisnika
        var listCachePrefix = $"habits:{habit.UserId}:";
        await _cacheService.RemoveByPrefixAsync(listCachePrefix);
        return habit.ToDto();
    }

    public override async Task<bool> DeleteAsync(string id)
    {
        var habit = await _db.Habits.FirstOrDefaultAsync(h => h.Id == id);
        if (habit is null)
        {

            return false;
        }

        _db.Habits.Remove(habit);
        await _db.SaveChangesAsync();

        var habitCacheKey = $"habit:{habit.UserId}:{id}:*";
        await _cacheService.RemoveByPrefixAsync(habitCacheKey);

        var listCachePrefix = $"habits:{habit.UserId}:";
        await _cacheService.RemoveByPrefixAsync(listCachePrefix);

        return true;
    }

    public async Task<IEnumerable<HabitDto>> GetMostPopularHabitsAsync(int top, CancellationToken cancellationToken)
    {
        return await _db.Habits.AsNoTracking()
            .Take(top)
            .Select(HabitQueries.ProjectToDto())
            .ToListAsync(cancellationToken);
    }
}


