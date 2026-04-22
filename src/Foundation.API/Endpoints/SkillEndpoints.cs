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

        // POST /api/skills
        group.MapPost("/", CreateSkillAsync)
             .WithName("CreateSkill")
             .WithSummary("Create a new skill")
             .WithDescription(
                 "Creates a new skill. Primary skills have no ParentSkillId. " +
                 "Secondary skills must reference an existing primary skill via ParentSkillId. " +
                 "IsActive is always set to true on creation.")
             .Produces<SkillDto>(StatusCodes.Status201Created)
             .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        // GET /api/skills
        group.MapGet("/", ListSkillsAsync)
             .WithName("ListSkills")
             .WithSummary("List and filter skills")
             .WithDescription(
                 "Returns a list of skills. Supports optional filtering by category, " +
                 "parentSkillId, and isActive. To retrieve only primary skills, " +
                 "pass parentSkillId=null together with the nullParentSkillId=true flag.")
             .Produces<IEnumerable<SkillDto>>(StatusCodes.Status200OK);

        // GET /api/skills/{id}
        group.MapGet("/{id:guid}", GetSkillByIdAsync)
             .WithName("GetSkillById")
             .WithSummary("Get a skill by ID")
             .WithDescription(
                 "Returns the full skill record. Secondary skills include parent metadata " +
                 "(parentSkillId and parentSkillName).")
             .Produces<SkillDto>(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status404NotFound);

        // PUT /api/skills/{id}
        group.MapPut("/{id:guid}", UpdateSkillAsync)
             .WithName("UpdateSkill")
             .WithSummary("Update an existing skill")
             .WithDescription(
                 "Updates Name, Category, ParentSkillId, and IsActive. " +
                 "All hierarchy constraints are re-validated on update.")
             .Produces<SkillDto>(StatusCodes.Status200OK)
             .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
             .Produces(StatusCodes.Status404NotFound);

        // DELETE /api/skills/{id}
        group.MapDelete("/{id:guid}", DeleteSkillAsync)
             .WithName("DeleteSkill")
             .WithSummary("Permanently delete a skill")
             .WithDescription(
                 "Physically removes the skill record. Deletion is blocked (409 Conflict) " +
                 "when the skill has active (IsActive = true) secondary skills. " +
                 "Inactive secondary children do not block deletion.")
             .Produces(StatusCodes.Status204NoContent)
             .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
             .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        // GET /api/skills/{id}/children
        group.MapGet("/{id:guid}/children", GetChildrenAsync)
             .WithName("GetSkillChildren")
             .WithSummary("Get secondary skills of a primary skill")
             .WithDescription(
                 "Returns all direct secondary skills of the specified primary skill. " +
                 "Supports an optional isActive filter. " +
                 "Returns 400 if the addressed skill is itself a secondary skill.")
             .Produces<IEnumerable<SkillDto>>(StatusCodes.Status200OK)
             .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
             .Produces(StatusCodes.Status404NotFound);

        // GET /api/skills/{id}/parent
        group.MapGet("/{id:guid}/parent", GetParentAsync)
             .WithName("GetSkillParent")
             .WithSummary("Get the parent skill of a secondary skill")
             .WithDescription(
                 "Returns the parent primary skill of the specified secondary skill. " +
                 "Returns 400 if the addressed skill is itself a primary skill (has no parent).")
             .Produces<SkillDto>(StatusCodes.Status200OK)
             .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
             .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    // -------------------------------------------------------------------------
    // Handler implementations
    // -------------------------------------------------------------------------

    private static async Task<IResult> CreateSkillAsync(
        [FromBody] CreateSkillRequest request,
        [FromServices] ISkillService service,
        CancellationToken cancellationToken)
    {
        var (skill, error) = await service.CreateAsync(request, cancellationToken);

        if (error is not null)
        {
            return Results.Problem(
                detail: error,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Validation Error");
        }

        return Results.CreatedAtRoute("GetSkillById", new { id = skill!.Id }, skill);
    }

    private static async Task<IResult> ListSkillsAsync(
        [FromServices] ISkillService service,
        [FromQuery] string? category = null,
        [FromQuery] Guid? parentSkillId = null,
        [FromQuery] bool nullParentSkillId = false,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        // filterByParentSkillId is true when the caller explicitly passes the
        // parentSkillId param (even as null) to list primary skills only.
        var filterByParent = parentSkillId.HasValue || nullParentSkillId;

        var skills = await service.ListAsync(
            category,
            parentSkillId,
            filterByParent,
            isActive,
            cancellationToken);

        return Results.Ok(skills);
    }

    private static async Task<IResult> GetSkillByIdAsync(
        Guid id,
        [FromServices] ISkillService service,
        CancellationToken cancellationToken)
    {
        var skill = await service.GetByIdAsync(id, cancellationToken);
        return skill is null ? Results.NotFound() : Results.Ok(skill);
    }

    private static async Task<IResult> UpdateSkillAsync(
        Guid id,
        [FromBody] UpdateSkillRequest request,
        [FromServices] ISkillService service,
        CancellationToken cancellationToken)
    {
        var (skill, error, notFound) = await service.UpdateAsync(id, request, cancellationToken);

        if (notFound)
        {
            return Results.NotFound();
        }

        if (error is not null)
        {
            return Results.Problem(
                detail: error,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Validation Error");
        }

        return Results.Ok(skill);
    }

    private static async Task<IResult> DeleteSkillAsync(
        Guid id,
        [FromServices] ISkillService service,
        CancellationToken cancellationToken)
    {
        var (deleted, error, notFound) = await service.DeleteAsync(id, cancellationToken);

        if (notFound)
        {
            return Results.NotFound();
        }

        if (error is not null)
        {
            return Results.Problem(
                detail: error,
                statusCode: StatusCodes.Status409Conflict,
                title: "Conflict");
        }

        return deleted ? Results.NoContent() : Results.NotFound();
    }

    private static async Task<IResult> GetChildrenAsync(
        Guid id,
        [FromServices] ISkillService service,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var (children, error, notFound) = await service.GetChildrenAsync(id, isActive, cancellationToken);

        if (notFound)
        {
            return Results.NotFound();
        }

        if (error is not null)
        {
            return Results.Problem(
                detail: error,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid Operation");
        }

        return Results.Ok(children);
    }

    private static async Task<IResult> GetParentAsync(
        Guid id,
        [FromServices] ISkillService service,
        CancellationToken cancellationToken)
    {
        var (parent, error, notFound) = await service.GetParentAsync(id, cancellationToken);

        if (notFound)
        {
            return Results.NotFound();
        }

        if (error is not null)
        {
            return Results.Problem(
                detail: error,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid Operation");
        }

        return Results.Ok(parent);
    }
}
