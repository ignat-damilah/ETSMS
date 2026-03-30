using Foundation.Application.DTOs;
using Foundation.Application.Interfaces;
using Foundation.Domain.Entities;
using Foundation.Application.Services;

namespace Foundation.Application.Tests;

public class SkillServiceTests
{
    private readonly ISkillRepository _repository = new InMemorySkillRepository();
    private readonly SkillService _service;

    public SkillServiceTests()
    {
        _service = new SkillService(_repository);
    }

    [Fact]
    public async Task CreateAsync_requires_parent_for_secondary()
    {
        var dto = new SkillDto
        {
            Name = "Secondary",
            Category = SkillCategory.Secondary,
            IsActive = true,
            IsDeleted = false
        };

        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto));
    }

    [Fact]
    public async Task UpdateAsync_removes_parent_from_primary()
    {
        var primary = await _service.CreateAsync(new SkillDto
        {
            Name = "Primary",
            Category = SkillCategory.Primary,
            IsActive = true,
            IsDeleted = false
        });

        var updated = await _service.UpdateAsync(primary.Id, new SkillDto
        {
            Id = primary.Id,
            Name = "Primary Updated",
            Category = SkillCategory.Primary,
            IsActive = true,
            IsDeleted = false
        });

        Assert.Equal("Primary Updated", updated.Name);
        Assert.Null(updated.ParentSkillId);
    }
}
