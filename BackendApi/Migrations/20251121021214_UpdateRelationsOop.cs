using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendApi.Migrations
{
    public partial class UpdateRelationsOop : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Users_DoctorId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Users_UserId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_PatientNotes_Users_DoctorId",
                table: "PatientNotes");

            migrationBuilder.DropForeignKey(
                name: "FK_PatientNotes_Users_UserId",
                table: "PatientNotes");

            migrationBuilder.DropIndex(
                name: "IX_PatientNotes_UserId",
                table: "PatientNotes");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_UserId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "PatientNotes");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Appointments");

            migrationBuilder.RenameColumn(
                name: "DoctorId",
                table: "PatientNotes",
                newName: "AuthorUserId");

            migrationBuilder.RenameIndex(
                name: "IX_PatientNotes_DoctorId",
                table: "PatientNotes",
                newName: "IX_PatientNotes_AuthorUserId");

            migrationBuilder.RenameColumn(
                name: "DoctorId",
                table: "Appointments",
                newName: "StaffUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Appointments_DoctorId_StartsAt",
                table: "Appointments",
                newName: "IX_Appointments_StaffUserId_StartsAt");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "PatientNotes",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Users_StaffUserId",
                table: "Appointments",
                column: "StaffUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PatientNotes_Users_AuthorUserId",
                table: "PatientNotes",
                column: "AuthorUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Users_StaffUserId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_PatientNotes_Users_AuthorUserId",
                table: "PatientNotes");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "PatientNotes");

            migrationBuilder.RenameColumn(
                name: "AuthorUserId",
                table: "PatientNotes",
                newName: "DoctorId");

            migrationBuilder.RenameIndex(
                name: "IX_PatientNotes_AuthorUserId",
                table: "PatientNotes",
                newName: "IX_PatientNotes_DoctorId");

            migrationBuilder.RenameColumn(
                name: "StaffUserId",
                table: "Appointments",
                newName: "DoctorId");

            migrationBuilder.RenameIndex(
                name: "IX_Appointments_StaffUserId_StartsAt",
                table: "Appointments",
                newName: "IX_Appointments_DoctorId_StartsAt");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "tinyint(1)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldDefaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "PatientNotes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Appointments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PatientNotes_UserId",
                table: "PatientNotes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_UserId",
                table: "Appointments",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Users_DoctorId",
                table: "Appointments",
                column: "DoctorId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Users_UserId",
                table: "Appointments",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PatientNotes_Users_DoctorId",
                table: "PatientNotes",
                column: "DoctorId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PatientNotes_Users_UserId",
                table: "PatientNotes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }
    }
}
