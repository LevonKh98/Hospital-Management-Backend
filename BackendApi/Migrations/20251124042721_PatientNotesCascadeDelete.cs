using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendApi.Migrations
{
    public partial class PatientNotesCascadeDelete : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PatientNotes_Patients_PatientId",
                table: "PatientNotes");

            migrationBuilder.AddForeignKey(
                name: "FK_PatientNotes_Patients_PatientId",
                table: "PatientNotes",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PatientNotes_Patients_PatientId",
                table: "PatientNotes");

            migrationBuilder.AddForeignKey(
                name: "FK_PatientNotes_Patients_PatientId",
                table: "PatientNotes",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
