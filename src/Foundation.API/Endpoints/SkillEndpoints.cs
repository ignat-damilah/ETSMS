using Foundation.Application.DTOs;
using Foundation.Application.Services;
using Foundation.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Foundation.API.Endpoints;

public static class SkillEndpoints
{
    public static void MapSkillEndpoints(this WebApplication app)
    {
        app.MapGet("/skills", [Authorize(Policy = "EmployeePolicy")] async (
            [FromServices] ISkillService skillService,
            [FromQuery] SkillCategory? category,
            [FromQuery] Guid? parentSkillId,
            [FromQuery] bool? isActive,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
            => Results.Ok(await skillService.ListAsync(category, parentSkillId, isActive, page, pageSize)));

        app.MapGet("/skills/{id}", [Authorize(Policy = "EmployeePolicy")] async (
            [FromServices] ISkillService skillService,
            Guid id)
            => await skillService.GetAsync(id) is { } skill
                ? Results.Ok(skill)
                : Results.NotFound());

        app.MapPost("/skills", [Authorize(Policy = "AdminPolicy")] async (
            [FromServices] ISkillService skillService,
            [FromBody] SkillCreateDto request)
            => await HandleAsync(async () =>
            {
                var skill = await skillService.CreateAsync(request);
                return Results.Created($"/skills/{skill.Id}", skill);
            }));

        app.MapPut("/skills/{id}", [Authorize(Policy = "AdminPolicy")] async (
            [FromServices] ISkillService skillService,
            Guid id,
            [FromBody] SkillUpdateDto request)
            => await HandleAsync(async () =>
            {
                var updated = await skillService.UpdateAsync(id, request);
                return Results.Ok(updated);
            }));

        app.MapDelete("/skills/{id}", [Authorize(Policy = "AdminPolicy")] async (
            [FromServices] ISkillService skillService,
            Guid id)
            => await HandleAsync(async () =>
            {
                await skillService.DeleteAsync(id);
                return Results.NoContent();
            }));

        app.MapGet("/skills/{id}/children", [Authorize(Policy = "EmployeePolicy")] async (
            [FromServices] ISkillService skillService,
            Guid id)
            => await HandleAsync(async () =>
            {
                var children = await skillService.GetChildrenAsync(id);
                return Results.Ok(children);
            }));

        app.MapGet("/skills/{id}/parent", [Authorize(Policy = "EmployeePolicy")] async (
            [FromServices] ISkillService skillService,
            Guid id)
            => await HandleAsync(async () =>
            {
                var parent = await skillService.GetParentAsync(id);
                return parent is { } ? Results.Ok(parent) : Results.NoContent();
            }));
    }

    private static async Task<IResult> HandleAsync(Func<Task<IResult>> operation)
    {
        try
        {
            return await operation();
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return Results.NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Results.Conflict(new { message = ex.Message });
        }
    }
}
