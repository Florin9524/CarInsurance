using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarInsurance.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakePolicyEndDateRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            // Update EndDate to match StartDate where EndDate is null
            migrationBuilder.Sql(@"
                UPDATE Policies 
                SET EndDate = StartDate 
                WHERE EndDate IS NULL;
            ");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "EndDate",
                table: "Policies",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1),
                oldClrType: typeof(DateOnly),
                oldType: "TEXT",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateOnly>(
                name: "EndDate",
                table: "Policies",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "TEXT");
        }
    }
}
