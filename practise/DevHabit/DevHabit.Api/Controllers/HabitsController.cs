using System.Net.Mime;
using Asp.Versioning;
using DevHabit.Api.Common.Auth;
using DevHabit.Api.Common.DataShaping;
using DevHabit.Api.Common.Hateoas;
using DevHabit.Api.Common.Pagination;
using DevHabit.Api.Common.Telemetry;
using DevHabit.Api.DTOs.Common;
using DevHabit.Api.DTOs.Habits;
using DevHabit.Api.Entities;
using DevHabit.Api.Services.Habit;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevHabit.Api.Controllers;

[ApiController]
[Route("api/habits")]
[Authorize(Roles = "member")]
[RequireUserId]
[ApiVersion(1.0)]
[Produces(MediaTypeNames.Application.Json, CustomMediaTypeNames.Application.HateoasJson)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class HabitsController : ControllerBase
{
    private readonly IHabitService _habitService;

    public HabitsController(IHabitService habitService)
    {
        _habitService = habitService;
    }
    //private readonly IHabitService _habitService;

    //public HabitsController(IHabitService habitService)
    //{
    //    _habitService = habitService;
    //}

    [HttpGet]
    [EndpointSummary("Get all habits")]
    [EndpointDescription("Retrieves a paginated list of habits with optional filtering, sorting, and field selection.")]
    [ProducesResponseType<PaginationResult<HabitDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetHabits(
        [FromQuery] HabitsQueryParameters habitParameters,
        IValidator<HabitsQueryParameters> validator,
        [FromServices] DevHabitMetrics devHabitMetrics,
        CancellationToken cancellationToken)
    {
        if (HttpContext.Items[AuthConstants.UserId] is not string userId)
        {

            return Unauthorized("User identity not found in token.");
        }

        await validator.ValidateAndThrowAsync(habitParameters,cancellationToken);

        var paginationResult = await _habitService.GetAllAsync(userId, habitParameters,cancellationToken);

        devHabitMetrics.IncreaseHabitsRequestCount([new("UserId", userId)]);

        return Ok(paginationResult);
    }

    [HttpGet("{id}")]
    [MapToApiVersion(1.0)]
    [EndpointSummary("Get a habit by ID")]
    [EndpointDescription("Retrieves a specific habit by its unique identifier with optional field selection.")]
    [ProducesResponseType<HabitWithTagsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHabit(
        string id,
        [FromQuery] HabitsQueryParameters habitParameters,
        CancellationToken cancellationToken)
    {
        if (HttpContext.Items[AuthConstants.UserId] is not string userId)
        {

            return Unauthorized("User identity not found in token.");
        }

        var result = await _habitService.GetAsync(id, userId, habitParameters.Fields, habitParameters.Accept, cancellationToken);

        return result is null ? NotFound() : Ok(result.Item);
    }

    [HttpPost]
    [Authorize(Roles = "member")]
    [EndpointSummary("Create a new habit")]
    [EndpointDescription("Creates a new habit with the provided details.")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<HabitDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateHabit(
        CreateHabitDto createHabitDto,
        AcceptHeaderDto acceptHeaderDto,
        IValidator<CreateHabitDto> validator)
    {
        if (HttpContext.Items[AuthConstants.UserId] is not string userId)
        {

            return Unauthorized("User identity not found in token.");
        }

        await validator.ValidateAndThrowAsync(createHabitDto);

        var habitDto = await _habitService.CreateAsync(createHabitDto, userId);

        if (HateoasHelpers.ShouldIncludeHateoas(acceptHeaderDto.Accept))
        {
            var shaped = DataShaper.ShapeData(habitDto, _habitService.GetItemLinks(habitDto.Id));
            return CreatedAtAction(nameof(GetHabit), new { id = habitDto.Id }, shaped);
        }

        return CreatedAtAction(nameof(GetHabit), new { id = habitDto.Id }, habitDto);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "member")]
    [EndpointSummary("Update a habit")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateHabit(string id, UpdateHabitDto dto, IValidator<UpdateHabitDto> validator)
    {
        await validator.ValidateAndThrowAsync(dto);

        var updated = await _habitService.UpdateAsync(id, dto);

        return updated is null ? NotFound() : NoContent();
    }

    [HttpDelete("{id}")]
    [EndpointSummary("Delete a habit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteHabit(string id)
    {
        var deleted = await _habitService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
