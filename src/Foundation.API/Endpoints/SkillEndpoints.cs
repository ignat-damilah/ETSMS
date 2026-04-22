using Foundation.Application.DTOs;
using Foundation.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Foundation.API.Endpoints;

public static class SkillEndpoints
{
    public static IEndpointRouteBuilder MapSkillEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/skills")
            .WithTags("Skills")
            .WithOpenApi();

        // POST /api/skills — Create a new skill
        group.MapPost("/", CreateSkillAsync)
            .WithName("CreateSkill")
            .WithSummary("Create a new skill")
            .WithDescription(
                "Creates a new primary or secondary skill. " +
                "IsActive and IsDeleted are always set to true and false respectively, regardless of any payload values. " +
                "When ParentSkillId is provided the parent must exist, be undeleted, and be a primary skill.")
            .Produces<SkillDto>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        // GET /api/skills/{id} — Get a single skill by ID
        group.MapGet("/{id:guid}", GetSkillByIdAsync)
            .WithName("GetSkillById")
            .WithSummary("Get a skill by ID")
            .WithDescription("Returns the skill with the specified ID, provided it is not soft-deleted.")
            .Produces<SkillDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        // PUT /api/skills/{id} — Update a skill
        group.MapPut("/{id:guid}", UpdateSkillAsync)
            .WithName("UpdateSkill")
            .WithSummary("Update an existing skill")
            .WithDescription(
                "Updates Name, Category, ParentSkillId, and IsActive for the specified skill. " +
                "IsDeleted cannot be modified via this endpoint. " +
                "Rejected with 400 Bad Request if the skill is soft-deleted or hierarchy rules are violated.")
            .Produces<SkillDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        // DELETE /api/skills/{id} — Soft-delete a skill
        group.MapDelete("/{id:guid}", SoftDeleteSkillAsync)
            .WithName("DeleteSkill")
            .WithSummary("Soft-delete a skill")
            .WithDescription(
                "Sets IsDeleted = true on the skill. No physical record removal is performed. " +
                "Returns 409 Conflict if the skill has active, undeleted child skills.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        // GET /api/skills — List and filter skills
        group.MapGet("/", ListSkillsAsync)
            .WithName("ListSkills")
            .WithSummary("List and filter skills")
            .WithDescription(
                "Returns skills matching the supplied filters. " +
                "When isDeleted is not supplied it defaults to false, excluding soft-deleted skills.")
            .Produces<IEnumerable<SkillDto>>(StatusCodes.Status200OK);

        // GET /api/skills/{id}/children — Get secondary skills of a primary skill
        group.MapGet("/{id:guid}/children", GetChildrenAsync)
            .WithName("GetSkillChildren")
            .WithSummary("Get secondary (child) skills of a primary skill")
            .WithDescription(
                "Returns all secondary skills whose ParentSkillId matches the given ID. " +
                "Returns 400 Bad Request if the target skill is itself a secondary skill. " +
                "isDeleted defaults to false when not supplied.")
            .Produces<IEnumerable<SkillDto>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        // GET /api/skills/{id}/parent — Get the parent skill of a secondary skill
        group.MapGet("/{id:guid}/parent", GetParentAsync)
            .WithName("GetSkillParent")
            .WithSummary("Get the parent skill of a secondary skill")
            .WithDescription(
                "Returns the primary parent skill of the identified secondary skill. " +
                "Returns 404 Not Found if the skill has no parent, the parent is soft-deleted, " +
                "or the skill itself does not exist.")
            .Produces<SkillDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        return app;
    }

    // ── Handler implementations ──────────────────────────────────────────────

    private static async Task<IResult> CreateSkillAsync(
        [FromBody] CreateSkillRequest request,
        [FromServices] ISkillService service,
        CancellationToken cancellationToken)
    {
        try
        {
            var (skill, location) = await service.CreateAsync(request, cancellationToken);
            return Results.Created(location, skill);
        }
        catch (ArgumentException ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Validation Error");
        }
    }

    private static async Task<IResult> GetSkillByIdAsync(
        Guid id,
        [FromServices] ISkillService service,
        CancellationToken cancellationToken)
    {
        var skill = await service.GetByIdAsync(id, cancellationToken);

        return skill is not null
            ? Results.Ok(skill)
            : Results.Problem(
                detail: $"Skill with id '{id}' was not found or has been deleted.",
                statusCode: StatusCodes.Status404NotFound,
                title: "Not Found");
    }

    private static async Task<IResult> UpdateSkillAsync(
        Guid id,
        [FromBody] UpdateSkillRequest request,
        [FromServices] ISkillService service,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await service.UpdateAsync(id, request, cancellationToken);
            return Results.Ok(updated);
        }
        catch (KeyNotFoundException ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound,
                title: "Not Found");
        }
        catch (InvalidOperationException ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Validation Error");
        }
        catch (ArgumentException ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Validation Error");
        }
    }

    private static async Task<IResult> SoftDeleteSkillAsync(
        Guid id,
        [FromServices] ISkillService service,
        CancellationToken cancellationToken)
    {
        try
        {
            await service.SoftDeleteAsync(id, cancellationToken);
            return Results.NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound,
                title: "Not Found");
        }
        catch (InvalidOperationException ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status409Conflict,
                title: "Conflict");
        }
    }

    private static async Task<IResult> ListSkillsAsync(
        [FromServices] ISkillService service,
        CancellationToken cancellationToken,
        [FromQuery] string? category = null,
        [FromQuery] Guid? parentSkillId = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] bool? isDeleted = null)
    {
        var filters = new SkillFilterParams
        {
            Category = category,
            ParentSkillId = parentSkillId,
            IsActive = isActive,
            IsDeleted = isDeleted
        };

        var skills = await service.ListAsync(filters, cancellationToken);
        return Results.Ok(skills);
    }

    private static async Task<IResult> GetChildrenAsync(
        Guid id,
        [FromServices] ISkillService service,
        CancellationToken cancellationToken,
        [FromQuery] bool? isActive = null,
        [FromQuery] bool? isDeleted = null)
    {
        try
        {
            var children = await service.GetChildrenAsync(id, isActive, isDeleted, cancellationToken);
            return Results.Ok(children);
        }
        catch (KeyNotFoundException ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound,
                title: "Not Found");
        }
        catch (InvalidOperationException ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad Request");
        }
    }

    private static async Task<IResult> GetParentAsync(
        Guid id,
        [FromServices] ISkillService service,
        CancellationToken cancellationToken)
    {
        var skill = await service.GetByIdAsync(id, cancellationToken);

        if (skill is null)
        {
            return Results.Problem(
                detail: $"Skill with id '{id}' was not found or has been deleted.",
                statusCode: StatusCodes.Status404NotFound,
                title: "Not Found");
        }

        var parent = await service.GetParentAsync(id, cancellationToken);

        return parent is not null
            ? Results.Ok(parent)
            : Results.Problem(
                detail: $"Skill with id '{id}' has no parent, or the parent has been deleted.",
                statusCode: StatusCodes.Status404NotFound,
                title: "Not Found");
    }
}
