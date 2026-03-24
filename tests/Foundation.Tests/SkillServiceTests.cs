using System;
using System.Linq;
using Foundation.Application.DTOs;
using Foundation.Application.Services;
using Foundation.Infrastructure.Data;
using Foundation.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Foundation.Tests;

public sealed class SkillServiceTests : IDisposable
{
    private readonly FoundationDbContext _context;
    private readonly SkillService _service;

    public SkillServiceTests()
    {
        var options = new DbContextOptionsBuilder<FoundationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new FoundationDbContext(options);
        var repository = new SkillRepository(_context);
        _service = new SkillService(repository);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task CannotDeleteSkillWithActiveChildren()
    {
        var parent = await _service.CreateAsync(new SkillCreateDto { Name = "Parent", Category = "Core" });
        await _service.CreateAsync(new SkillCreateDto { Name = "Child", Category = "Core", ParentSkillId = parent.Id });

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteAsync(parent.Id));

        Assert.Contains("active child", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CanDeleteSkillAfterChildrenDeactivated()
    {
        var parent = await _service.CreateAsync(new SkillCreateDto { Name = "Parent", Category = "Core" });
        var child = await _service.CreateAsync(new SkillCreateDto { Name = "Child", Category = "Core", ParentSkillId = parent.Id });
        await _service.UpdateAsync(child.Id, new SkillUpdateDto { Name = "Child", Category = "Core", ParentSkillId = parent.Id, IsActive = false });

        await _service.DeleteAsync(parent.Id);

        var result = await _service.GetAsync(parent.Id);
        Assert.Null(result);
    }

    [Fact]
    public async Task CannotCreateCircularRelationships()
    {
        var parent = await _service.CreateAsync(new SkillCreateDto { Name = "Parent", Category = "Core" });
        var child = await _service.CreateAsync(new SkillCreateDto { Name = "Child", Category = "Core", ParentSkillId = parent.Id });

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAsync(parent.Id, new SkillUpdateDto
        {
            Name = "Parent",
            Category = "Core",
            ParentSkillId = child.Id,
            IsActive = true
        }));

        Assert.Contains("Circular skill", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HierarchyIncludesAncestorsAndDescendants()
    {
        var root = await _service.CreateAsync(new SkillCreateDto { Name = "Root", Category = "Core" });
        var child = await _service.CreateAsync(new SkillCreateDto { Name = "Child", Category = "Core", ParentSkillId = root.Id });
        var grandChild = await _service.CreateAsync(new SkillCreateDto { Name = "GrandChild", Category = "Core", ParentSkillId = child.Id });

        var hierarchy = await _service.GetHierarchyAsync(root.Id);

        Assert.Equal(root.Id, hierarchy?.Current.Id);
        Assert.Contains(hierarchy!.Descendants.Select(d => d.Id), id => id == child.Id);
        Assert.Contains(hierarchy.Descendants.Select(d => d.Id), id => id == grandChild.Id);
    }
}
