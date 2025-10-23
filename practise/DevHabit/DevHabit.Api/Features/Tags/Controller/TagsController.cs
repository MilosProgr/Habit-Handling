using System.Net.Mime;
using Asp.Versioning;
using DevHabit.Api.Common.Auth;
using DevHabit.Api.Common.DataShaping;
using DevHabit.Api.Common.Hateoas;
using DevHabit.Api.Common.Pagination;
using DevHabit.Api.Common.Telemetry;
using DevHabit.Api.DTOs.Common;
using DevHabit.Api.Features.Tags.DTO;
using DevHabit.Api.Features.Tags.Operations;
using DevHabit.Api.Features.Tags.Parameters;
using DevHabit.Api.Features.Tags.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevHabit.Api.Features.Tags.Controller;

[ApiController]
[Route("api/tags")]
[Authorize(Roles = "member")]
[RequireUserId]
[ApiVersion(1.0)]
[Produces(MediaTypeNames.Application.Json, CustomMediaTypeNames.Application.HateoasJson)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ResponseCache(Duration = 120)]
public sealed class TagsController : ControllerBase
{
    private readonly ITagService _tagService;

    public TagsController(ITagService tagService)
    {
        _tagService = tagService;
    }

    // -------------------- GET ALL --------------------
    [HttpGet]
    [EndpointSummary("Get all tags")]
    [EndpointDescription("Retrieves a paginated list of tags with optional filtering, sorting, and field selection.")]
    [ProducesResponseType<PaginationResult<TagDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTags(
        [FromQuery] TagsParameters tagParameters,
        IValidator<TagsParameters> validator,
        //[FromServices] DevHabitMetrics devHabitMetrics,
        CancellationToken cancellationToken)
    {
        if (HttpContext.Items[AuthConstants.UserId] is not string userId)
        {
            return Unauthorized("User identity not found in token.");
        }

        await validator.ValidateAndThrowAsync(tagParameters, cancellationToken);

        var paginationResult = await _tagService.GetAllAsync(userId, tagParameters, cancellationToken);

        //devHabitMetrics.IncreaseTagsRequestCount([new("UserId", userId)]);

        return Ok(paginationResult);
    }

    // -------------------- GET BY ID --------------------
    [HttpGet("{id}")]
    [MapToApiVersion(1.0)]
    [EndpointSummary("Get a tag by ID")]
    [EndpointDescription("Retrieves a specific tag by its unique identifier with optional field selection.")]
    [ProducesResponseType<TagDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTag(
        string id,
        [FromQuery] TagsParameters tagParameters,
        CancellationToken cancellationToken)
    {
        if (HttpContext.Items[AuthConstants.UserId] is not string userId)
        {
            return Unauthorized("User identity not found in token.");
        }

        var result = await _tagService.GetAsync(id, userId, tagParameters.Fields, tagParameters.Accept, cancellationToken);

        return result is null ? NotFound() : Ok(result.Item);
    }

    // -------------------- CREATE --------------------
    [HttpPost]
    [Authorize(Roles = "member")]
    [EndpointSummary("Create a new tag")]
    [EndpointDescription("Creates a new tag with the provided details.")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<TagDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTag(
        CreateTagDto createTagDto,
        AcceptHeaderDto acceptHeaderDto,
        IValidator<CreateTagDto> validator)
    {
        if (HttpContext.Items[AuthConstants.UserId] is not string userId)
        {
            return Unauthorized("User identity not found in token.");
        }

        await validator.ValidateAndThrowAsync(createTagDto);

        var tagDto = await _tagService.CreateAsync(createTagDto, userId);

        if (HateoasHelpers.ShouldIncludeHateoas(acceptHeaderDto.Accept))
        {
            var shaped = DataShaper.ShapeData(tagDto, _tagService.GetItemLinks(tagDto.Id));
            return CreatedAtAction(nameof(GetTag), new { id = tagDto.Id }, shaped);
        }

        return CreatedAtAction(nameof(GetTag), new { id = tagDto.Id }, tagDto);
    }

    // -------------------- UPDATE --------------------
    [HttpPut("{id}")]
    [Authorize(Roles = "member")]
    [EndpointSummary("Update a tag")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTag(string id, UpdateTagDto dto, IValidator<UpdateTagDto> validator)
    {
        await validator.ValidateAndThrowAsync(dto);

        var updated = await _tagService.UpdateAsync(id, dto);

        return updated is null ? NotFound() : NoContent();
    }

    // -------------------- DELETE --------------------
    [HttpDelete("{id}")]
    [EndpointSummary("Delete a tag")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTag(string id)
    {
        var deleted = await _tagService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
