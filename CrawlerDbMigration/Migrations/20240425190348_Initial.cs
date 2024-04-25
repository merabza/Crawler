using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrawlerDbMigration.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "batches",
                columns: table => new
                {
                    batchId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    batchName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    isOpen = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    autoCreateNextPart = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_batches", x => x.batchId);
                });

            migrationBuilder.CreateTable(
                name: "extensions",
                columns: table => new
                {
                    extId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    extName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    extProhibited = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_extensions", x => x.extId);
                });

            migrationBuilder.CreateTable(
                name: "hosts",
                columns: table => new
                {
                    hostId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    hostName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    hostProhibited = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hosts", x => x.hostId);
                });

            migrationBuilder.CreateTable(
                name: "schemes",
                columns: table => new
                {
                    schId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    schName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    schProhibited = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_schemes", x => x.schId);
                });

            migrationBuilder.CreateTable(
                name: "termTypes",
                columns: table => new
                {
                    ttId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ttKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ttName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_termTypes", x => x.ttId);
                });

            migrationBuilder.CreateTable(
                name: "batchParts",
                columns: table => new
                {
                    bpId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    batchId = table.Column<int>(type: "int", nullable: false),
                    created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    finished = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_batchParts", x => x.bpId);
                    table.ForeignKey(
                        name: "FK_BatchParts_Batches",
                        column: x => x.batchId,
                        principalTable: "batches",
                        principalColumn: "batchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hostsByBatches",
                columns: table => new
                {
                    hbbId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    batchId = table.Column<int>(type: "int", maxLength: 50, nullable: false),
                    schemeId = table.Column<int>(type: "int", nullable: false),
                    hostId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hostsByBatches", x => x.hbbId);
                    table.ForeignKey(
                        name: "FK_HostsByBatches_Batches",
                        column: x => x.batchId,
                        principalTable: "batches",
                        principalColumn: "batchId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HostsByBatches_HostModels",
                        column: x => x.hostId,
                        principalTable: "hosts",
                        principalColumn: "hostId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HostsByBatches_SchemeModels",
                        column: x => x.schemeId,
                        principalTable: "schemes",
                        principalColumn: "schId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "urls",
                columns: table => new
                {
                    urlId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    urlName = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    hostId = table.Column<int>(type: "int", nullable: false),
                    extensionId = table.Column<int>(type: "int", nullable: false),
                    schemeId = table.Column<int>(type: "int", nullable: false),
                    urlHashCode = table.Column<int>(type: "int", nullable: false),
                    isSiteMap = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_urls", x => x.urlId);
                    table.ForeignKey(
                        name: "FK_Urls_ExtensionModels",
                        column: x => x.extensionId,
                        principalTable: "extensions",
                        principalColumn: "extId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Urls_HostModels",
                        column: x => x.hostId,
                        principalTable: "hosts",
                        principalColumn: "hostId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Urls_SchemeModels",
                        column: x => x.schemeId,
                        principalTable: "schemes",
                        principalColumn: "schId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "terms",
                columns: table => new
                {
                    trmId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    termText = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    termTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_terms", x => x.trmId);
                    table.ForeignKey(
                        name: "FK_Terms_TermTypes",
                        column: x => x.termTypeId,
                        principalTable: "termTypes",
                        principalColumn: "ttId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "contentAnalyses",
                columns: table => new
                {
                    caId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    batchPartId = table.Column<int>(type: "int", nullable: false),
                    urlId = table.Column<int>(type: "int", nullable: false),
                    responseStatusCode = table.Column<int>(type: "int", nullable: false),
                    finish = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contentAnalyses", x => x.caId);
                    table.ForeignKey(
                        name: "FK_ContentAnalyses_BatchParts",
                        column: x => x.batchPartId,
                        principalTable: "batchParts",
                        principalColumn: "bpId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContentAnalyses_UrlModels",
                        column: x => x.urlId,
                        principalTable: "urls",
                        principalColumn: "urlId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "urlGraphNodes",
                columns: table => new
                {
                    ugnId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    batchPartId = table.Column<int>(type: "int", nullable: false),
                    fromUrlId = table.Column<int>(type: "int", nullable: false),
                    gotUrlId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_urlGraphNodes", x => x.ugnId);
                    table.ForeignKey(
                        name: "FK_UrlGraphNodes_BatchParts",
                        column: x => x.batchPartId,
                        principalTable: "batchParts",
                        principalColumn: "bpId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UrlGraphNodes_UrlGraphNodes_FromUrlId",
                        column: x => x.fromUrlId,
                        principalTable: "urls",
                        principalColumn: "urlId");
                    table.ForeignKey(
                        name: "FK_UrlGraphNodes_UrlGraphNodes_GotUrlId",
                        column: x => x.gotUrlId,
                        principalTable: "urls",
                        principalColumn: "urlId");
                });

            migrationBuilder.CreateTable(
                name: "termsByUrls",
                columns: table => new
                {
                    tbuId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    batchPartId = table.Column<int>(type: "int", nullable: false),
                    urlId = table.Column<int>(type: "int", nullable: false),
                    termId = table.Column<int>(type: "int", nullable: false),
                    position = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_termsByUrls", x => x.tbuId);
                    table.ForeignKey(
                        name: "FK_TermsByUrls_BatchParts",
                        column: x => x.batchPartId,
                        principalTable: "batchParts",
                        principalColumn: "bpId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TermsByUrls_Terms",
                        column: x => x.termId,
                        principalTable: "terms",
                        principalColumn: "trmId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TermsByUrls_UrlModels",
                        column: x => x.urlId,
                        principalTable: "urls",
                        principalColumn: "urlId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Batches_batchName_Unique",
                table: "batches",
                column: "batchName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BatchParts_batchId_created_Unique",
                table: "batchParts",
                columns: new[] { "batchId", "created" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContentAnalyses_batchPartId_urlId_Unique",
                table: "contentAnalyses",
                columns: new[] { "batchPartId", "urlId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_contentAnalyses_urlId",
                table: "contentAnalyses",
                column: "urlId");

            migrationBuilder.CreateIndex(
                name: "IX_Extensions_extName_Unique",
                table: "extensions",
                column: "extName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Hosts_hostName_Unique",
                table: "hosts",
                column: "hostName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HostsByBatches_batchId_schemeId_hostId_Unique",
                table: "hostsByBatches",
                columns: new[] { "batchId", "schemeId", "hostId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hostsByBatches_hostId",
                table: "hostsByBatches",
                column: "hostId");

            migrationBuilder.CreateIndex(
                name: "IX_hostsByBatches_schemeId",
                table: "hostsByBatches",
                column: "schemeId");

            migrationBuilder.CreateIndex(
                name: "IX_Schemes_schName_Unique",
                table: "schemes",
                column: "schName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Terms_termText_Unique",
                table: "terms",
                column: "termText",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_terms_termTypeId",
                table: "terms",
                column: "termTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TermsByUrls_batchPartId_urlId_position_Unique",
                table: "termsByUrls",
                columns: new[] { "batchPartId", "urlId", "position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_termsByUrls_termId",
                table: "termsByUrls",
                column: "termId");

            migrationBuilder.CreateIndex(
                name: "IX_termsByUrls_urlId",
                table: "termsByUrls",
                column: "urlId");

            migrationBuilder.CreateIndex(
                name: "IX_TermTypes_ttKey_Unique",
                table: "termTypes",
                column: "ttKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UrlGraphNodes_batchPartId_fromUrlId_gotUrlId_Unique",
                table: "urlGraphNodes",
                columns: new[] { "batchPartId", "fromUrlId", "gotUrlId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_urlGraphNodes_fromUrlId",
                table: "urlGraphNodes",
                column: "fromUrlId");

            migrationBuilder.CreateIndex(
                name: "IX_urlGraphNodes_gotUrlId",
                table: "urlGraphNodes",
                column: "gotUrlId");

            migrationBuilder.CreateIndex(
                name: "IX_urls_extensionId",
                table: "urls",
                column: "extensionId");

            migrationBuilder.CreateIndex(
                name: "IX_urls_hostId",
                table: "urls",
                column: "hostId");

            migrationBuilder.CreateIndex(
                name: "IX_urls_schemeId",
                table: "urls",
                column: "schemeId");

            migrationBuilder.CreateIndex(
                name: "IX_Urls_urlHashCode_hostId_extensionId_schemeId_Unique",
                table: "urls",
                columns: new[] { "urlHashCode", "hostId", "extensionId", "schemeId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contentAnalyses");

            migrationBuilder.DropTable(
                name: "hostsByBatches");

            migrationBuilder.DropTable(
                name: "termsByUrls");

            migrationBuilder.DropTable(
                name: "urlGraphNodes");

            migrationBuilder.DropTable(
                name: "terms");

            migrationBuilder.DropTable(
                name: "batchParts");

            migrationBuilder.DropTable(
                name: "urls");

            migrationBuilder.DropTable(
                name: "termTypes");

            migrationBuilder.DropTable(
                name: "batches");

            migrationBuilder.DropTable(
                name: "extensions");

            migrationBuilder.DropTable(
                name: "hosts");

            migrationBuilder.DropTable(
                name: "schemes");
        }
    }
}
