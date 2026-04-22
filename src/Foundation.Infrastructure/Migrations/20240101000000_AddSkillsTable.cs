using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foundation.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddSkillsTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Skills",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Category = table.Column<int>(type: "integer", nullable: false),
                ParentSkillId = table.Column<Guid>(type: "uuid", nullable: true),
                IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Skills", x => x.Id);
                table.ForeignKey(
                    name: "FK_Skills_Skills_ParentSkillId",
                    column: x => x.ParentSkillId,
                    principalTable: "Skills",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Skills_Name_Category_ParentSkillId",
            table: "Skills",
            columns: new[] { "Name", "Category", "ParentSkillId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Skills_ParentSkillId",
            table: "Skills",
            column: "ParentSkillId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Skills");
    }
}
