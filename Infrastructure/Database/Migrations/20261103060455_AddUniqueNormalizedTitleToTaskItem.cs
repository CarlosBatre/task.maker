using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueNormalizedTitleToTaskItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NormalizedTitle",
                table: "TaskItem",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("UPDATE \"TaskItem\" SET \"NormalizedTitle\" = UPPER(TRIM(\"Title\"));");

            migrationBuilder.CreateIndex(
                name: "IX_TaskItem_NormalizedTitle",
                table: "TaskItem",
                column: "NormalizedTitle",
                unique: true,
                filter: "\"IsActive\" = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TaskItem_NormalizedTitle",
                table: "TaskItem");

            migrationBuilder.DropColumn(
                name: "NormalizedTitle",
                table: "TaskItem");
        }
    }
}
