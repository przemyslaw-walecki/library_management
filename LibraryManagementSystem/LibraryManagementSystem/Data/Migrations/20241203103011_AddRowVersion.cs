using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LibraryManagementSystem.Data.Migrations
{
    public partial class AddRowVersion : Migration
    {
            protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "books",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                defaultValueSql: "gen_random_bytes(16)");
        }


    protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
             name: "RowVersion",
             table: "books");
        }
    }
}
