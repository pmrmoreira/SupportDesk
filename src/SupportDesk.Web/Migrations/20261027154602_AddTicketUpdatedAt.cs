using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupportDesk.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketUpdatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Tickets_Status",
                table: "Tickets");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Tickets",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            // Os pedidos que já existiam ficam com a data de criação (e não com o ano 1).
            migrationBuilder.Sql("UPDATE \"Tickets\" SET \"UpdatedAt\" = \"CreatedAt\";");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Tickets",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Tickets_Status",
                table: "Tickets",
                sql: "\"Status\" IN ('Open', 'InProgress', 'Resolved')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Tickets_Status",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Tickets");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Tickets_Status",
                table: "Tickets",
                sql: "\"Status\" IN ('Open','InProgress','Resolved')");
        }
    }
}
