using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PEIU.Service.WebApiService.Migrations
{
    public partial class AddStatusPropertyOnAccountModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalAvaliableESSMountKW",
                table: "AssetLocation");

            migrationBuilder.DropColumn(
                name: "TotalAvaliablePCSMountKW",
                table: "AssetLocation");

            migrationBuilder.DropColumn(
                name: "TotalAvaliablePVMountKW",
                table: "AssetLocation");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ReservedAssetLocation",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Address1 = table.Column<string>(nullable: true),
                    Address2 = table.Column<string>(nullable: true),
                    AccountId = table.Column<string>(nullable: true),
                    ControlOwner = table.Column<bool>(nullable: false),
                    SiteInformation = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservedAssetLocation", x => x.ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReservedAssetLocation");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<float>(
                name: "TotalAvaliableESSMountKW",
                table: "AssetLocation",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "TotalAvaliablePCSMountKW",
                table: "AssetLocation",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "TotalAvaliablePVMountKW",
                table: "AssetLocation",
                nullable: false,
                defaultValue: 0f);
        }
    }
}
