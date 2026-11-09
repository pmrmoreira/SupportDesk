using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupportDesk.Web.Migrations
{
    /// <inheritdoc />
    public partial class RenameAiSuggestionsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketAiSuggestion_AspNetUsers_DecidedById",
                table: "TicketAiSuggestion");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketAiSuggestion_Tickets_TicketId",
                table: "TicketAiSuggestion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TicketAiSuggestion",
                table: "TicketAiSuggestion");

            migrationBuilder.RenameTable(
                name: "TicketAiSuggestion",
                newName: "AiSuggestions");

            migrationBuilder.RenameIndex(
                name: "IX_TicketAiSuggestion_TicketId",
                table: "AiSuggestions",
                newName: "IX_AiSuggestions_TicketId");

            migrationBuilder.RenameIndex(
                name: "IX_TicketAiSuggestion_DecidedById",
                table: "AiSuggestions",
                newName: "IX_AiSuggestions_DecidedById");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AiSuggestions",
                table: "AiSuggestions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AiSuggestions_AspNetUsers_DecidedById",
                table: "AiSuggestions",
                column: "DecidedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AiSuggestions_Tickets_TicketId",
                table: "AiSuggestions",
                column: "TicketId",
                principalTable: "Tickets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AiSuggestions_AspNetUsers_DecidedById",
                table: "AiSuggestions");

            migrationBuilder.DropForeignKey(
                name: "FK_AiSuggestions_Tickets_TicketId",
                table: "AiSuggestions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AiSuggestions",
                table: "AiSuggestions");

            migrationBuilder.RenameTable(
                name: "AiSuggestions",
                newName: "TicketAiSuggestion");

            migrationBuilder.RenameIndex(
                name: "IX_AiSuggestions_TicketId",
                table: "TicketAiSuggestion",
                newName: "IX_TicketAiSuggestion_TicketId");

            migrationBuilder.RenameIndex(
                name: "IX_AiSuggestions_DecidedById",
                table: "TicketAiSuggestion",
                newName: "IX_TicketAiSuggestion_DecidedById");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TicketAiSuggestion",
                table: "TicketAiSuggestion",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketAiSuggestion_AspNetUsers_DecidedById",
                table: "TicketAiSuggestion",
                column: "DecidedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketAiSuggestion_Tickets_TicketId",
                table: "TicketAiSuggestion",
                column: "TicketId",
                principalTable: "Tickets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
