using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PES.Service.WebApiService.Migrations
{
    public partial class ChangeSiteId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assets");

            migrationBuilder.DropTable(
                name: "Address");

            migrationBuilder.CreateTable(
                name: "AssetLocation",
                columns: table => new
                {
                    SiteId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RCC = table.Column<int>(nullable: false),
                    Longtidue = table.Column<double>(nullable: false),
                    Latitude = table.Column<double>(nullable: false),
                    LawFirstCode = table.Column<string>(nullable: true),
                    LawMiddleCode = table.Column<string>(nullable: true),
                    LawLastCode = table.Column<string>(nullable: true),
                    Address1 = table.Column<string>(nullable: true),
                    Address2 = table.Column<string>(nullable: true),
                    AssetName = table.Column<string>(nullable: true),
                    DLNo = table.Column<int>(nullable: true),
                    InstallDate = table.Column<DateTime>(nullable: false),
                    AccountId = table.Column<string>(nullable: true),
                    TotalAvaliableESSMountKW = table.Column<float>(nullable: false),
                    TotalAvaliablePVMountKW = table.Column<float>(nullable: false),
                    TotalAvaliablePCSMountKW = table.Column<float>(nullable: false),
                    ServiceCode = table.Column<int>(nullable: false),
                    Comment = table.Column<string>(nullable: true),
                    ControlOwner = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetLocation", x => x.SiteId);
                });

            migrationBuilder.CreateTable(
                name: "AssetDevices",
                columns: table => new
                {
                    PK = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SiteId = table.Column<int>(nullable: false),
                    DeviceType = table.Column<int>(nullable: true),
                    Device_Name = table.Column<string>(nullable: true),
                    VolumeKW = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetDevices", x => x.PK);
                    table.ForeignKey(
                        name: "FK_AssetDevices_AssetLocation_SiteId",
                        column: x => x.SiteId,
                        principalTable: "AssetLocation",
                        principalColumn: "SiteId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssetDevices_SiteId",
                table: "AssetDevices",
                column: "SiteId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssetDevices");

            migrationBuilder.DropTable(
                name: "AssetLocation");

            migrationBuilder.CreateTable(
                name: "Address",
                columns: table => new
                {
                    PK = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Address1 = table.Column<string>(nullable: true),
                    Address2 = table.Column<string>(nullable: true),
                    Latitude = table.Column<double>(nullable: false),
                    LawFirstCode = table.Column<string>(nullable: true),
                    LawLasttCode = table.Column<string>(nullable: true),
                    LawMiddleCode = table.Column<string>(nullable: true),
                    Longtidue = table.Column<double>(nullable: false),
                    RCC = table.Column<short>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Address", x => x.PK);
                });

            migrationBuilder.CreateTable(
                name: "Assets",
                columns: table => new
                {
                    PK = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AccountId = table.Column<string>(nullable: true),
                    AddressId = table.Column<int>(nullable: false),
                    AssetName = table.Column<string>(nullable: true),
                    DLNo = table.Column<short>(nullable: false),
                    InstallDate = table.Column<DateTime>(nullable: false),
                    ServiceCode = table.Column<int>(nullable: false),
                    SiteId = table.Column<short>(nullable: false),
                    TotalAvaliableESSMountKW = table.Column<float>(nullable: false),
                    TotalAvaliablePCSMountKW = table.Column<float>(nullable: false),
                    TotalAvaliablePVMountKW = table.Column<float>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assets", x => x.PK);
                    table.ForeignKey(
                        name: "FK_Assets_AspNetUsers_AccountId",
                        column: x => x.AccountId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Assets_Address_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Address",
                        principalColumn: "PK",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_AccountId",
                table: "Assets",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_AddressId",
                table: "Assets",
                column: "AddressId");
        }
    }
}
