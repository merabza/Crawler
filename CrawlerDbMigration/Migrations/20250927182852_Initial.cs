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
                name: "Batches",
                columns: table => new
                {
                    BatchId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BatchName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsOpen = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AutoCreateNextPart = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Batches", x => x.BatchId);
                });

            migrationBuilder.CreateTable(
                name: "extensions",
                columns: table => new
                {
                    ExtId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExtName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ExtProhibited = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_extensions", x => x.ExtId);
                });

            migrationBuilder.CreateTable(
                name: "hosts",
                columns: table => new
                {
                    HostId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HostName = table.Column<string>(type: "nvarchar(253)", maxLength: 253, nullable: false),
                    HostProhibited = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hosts", x => x.HostId);
                });

            migrationBuilder.CreateTable(
                name: "schemes",
                columns: table => new
                {
                    SchId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SchProhibited = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_schemes", x => x.SchId);
                });

            migrationBuilder.CreateTable(
                name: "termTypes",
                columns: table => new
                {
                    TtId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TtKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TtName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_termTypes", x => x.TtId);
                });

            migrationBuilder.CreateTable(
                name: "BatchParts",
                columns: table => new
                {
                    BpId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BatchId = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false),
                    Finished = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatchParts", x => x.BpId);
                    table.ForeignKey(
                        name: "FK_BatchParts_Batches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "Batches",
                        principalColumn: "BatchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hostsByBatches",
                columns: table => new
                {
                    HbbId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BatchId = table.Column<int>(type: "int", nullable: false),
                    SchemeId = table.Column<int>(type: "int", nullable: false),
                    HostId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hostsByBatches", x => x.HbbId);
                    table.ForeignKey(
                        name: "FK_hostsByBatches_Batches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "Batches",
                        principalColumn: "BatchId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_hostsByBatches_hosts_HostId",
                        column: x => x.HostId,
                        principalTable: "hosts",
                        principalColumn: "HostId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_hostsByBatches_schemes_SchemeId",
                        column: x => x.SchemeId,
                        principalTable: "schemes",
                        principalColumn: "SchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "urls",
                columns: table => new
                {
                    UrlId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UrlName = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    HostId = table.Column<int>(type: "int", nullable: false),
                    ExtensionId = table.Column<int>(type: "int", nullable: false),
                    SchemeId = table.Column<int>(type: "int", nullable: false),
                    UrlHashCode = table.Column<int>(type: "int", nullable: false),
                    IsSiteMap = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsAllowed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_urls", x => x.UrlId);
                    table.ForeignKey(
                        name: "FK_urls_extensions_ExtensionId",
                        column: x => x.ExtensionId,
                        principalTable: "extensions",
                        principalColumn: "ExtId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_urls_hosts_HostId",
                        column: x => x.HostId,
                        principalTable: "hosts",
                        principalColumn: "HostId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_urls_schemes_SchemeId",
                        column: x => x.SchemeId,
                        principalTable: "schemes",
                        principalColumn: "SchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "terms",
                columns: table => new
                {
                    TrmId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TermText = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    termTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_terms", x => x.TrmId);
                    table.ForeignKey(
                        name: "FK_terms_termTypes_termTypeId",
                        column: x => x.termTypeId,
                        principalTable: "termTypes",
                        principalColumn: "TtId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "robots",
                columns: table => new
                {
                    RbtId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BatchPartId = table.Column<int>(type: "int", nullable: false),
                    SchemeId = table.Column<int>(type: "int", nullable: false),
                    HostId = table.Column<int>(type: "int", nullable: false),
                    RobotsTxt = table.Column<string>(type: "ntext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_robots", x => x.RbtId);
                    table.ForeignKey(
                        name: "FK_robots_BatchParts_BatchPartId",
                        column: x => x.BatchPartId,
                        principalTable: "BatchParts",
                        principalColumn: "BpId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_robots_hosts_HostId",
                        column: x => x.HostId,
                        principalTable: "hosts",
                        principalColumn: "HostId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_robots_schemes_SchemeId",
                        column: x => x.SchemeId,
                        principalTable: "schemes",
                        principalColumn: "SchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContentsAnalysis",
                columns: table => new
                {
                    CaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BatchPartId = table.Column<int>(type: "int", nullable: false),
                    UrlId = table.Column<int>(type: "int", nullable: false),
                    ResponseStatusCode = table.Column<int>(type: "int", nullable: false),
                    Finish = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentsAnalysis", x => x.CaId);
                    table.ForeignKey(
                        name: "FK_ContentsAnalysis_BatchParts_BatchPartId",
                        column: x => x.BatchPartId,
                        principalTable: "BatchParts",
                        principalColumn: "BpId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContentsAnalysis_urls_UrlId",
                        column: x => x.UrlId,
                        principalTable: "urls",
                        principalColumn: "UrlId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UrlGraphNodes",
                columns: table => new
                {
                    UgnId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BatchPartId = table.Column<int>(type: "int", nullable: false),
                    FromUrlId = table.Column<int>(type: "int", nullable: false),
                    GotUrlId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrlGraphNodes", x => x.UgnId);
                    table.ForeignKey(
                        name: "FK_UrlGraphNodes_BatchParts_BatchPartId",
                        column: x => x.BatchPartId,
                        principalTable: "BatchParts",
                        principalColumn: "BpId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UrlGraphNodes_urls_FromUrlId",
                        column: x => x.FromUrlId,
                        principalTable: "urls",
                        principalColumn: "UrlId");
                    table.ForeignKey(
                        name: "FK_UrlGraphNodes_urls_GotUrlId",
                        column: x => x.GotUrlId,
                        principalTable: "urls",
                        principalColumn: "UrlId");
                });

            migrationBuilder.CreateTable(
                name: "termsByUrls",
                columns: table => new
                {
                    TbuId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BatchPartId = table.Column<int>(type: "int", nullable: false),
                    UrlId = table.Column<int>(type: "int", nullable: false),
                    TermId = table.Column<int>(type: "int", nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_termsByUrls", x => x.TbuId);
                    table.ForeignKey(
                        name: "FK_termsByUrls_BatchParts_BatchPartId",
                        column: x => x.BatchPartId,
                        principalTable: "BatchParts",
                        principalColumn: "BpId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_termsByUrls_terms_TermId",
                        column: x => x.TermId,
                        principalTable: "terms",
                        principalColumn: "TrmId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_termsByUrls_urls_UrlId",
                        column: x => x.UrlId,
                        principalTable: "urls",
                        principalColumn: "UrlId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Batches_BatchName",
                table: "Batches",
                column: "BatchName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BatchParts_BatchId_Created",
                table: "BatchParts",
                columns: new[] { "BatchId", "Created" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContentsAnalysis_BatchPartId_UrlId",
                table: "ContentsAnalysis",
                columns: new[] { "BatchPartId", "UrlId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContentsAnalysis_UrlId",
                table: "ContentsAnalysis",
                column: "UrlId");

            migrationBuilder.CreateIndex(
                name: "IX_extensions_ExtName",
                table: "extensions",
                column: "ExtName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hosts_HostName",
                table: "hosts",
                column: "HostName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hostsByBatches_BatchId_SchemeId_HostId",
                table: "hostsByBatches",
                columns: new[] { "BatchId", "SchemeId", "HostId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hostsByBatches_HostId",
                table: "hostsByBatches",
                column: "HostId");

            migrationBuilder.CreateIndex(
                name: "IX_hostsByBatches_SchemeId",
                table: "hostsByBatches",
                column: "SchemeId");

            migrationBuilder.CreateIndex(
                name: "IX_robots_BatchPartId_SchemeId_HostId",
                table: "robots",
                columns: new[] { "BatchPartId", "SchemeId", "HostId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_robots_HostId",
                table: "robots",
                column: "HostId");

            migrationBuilder.CreateIndex(
                name: "IX_robots_SchemeId",
                table: "robots",
                column: "SchemeId");

            migrationBuilder.CreateIndex(
                name: "IX_schemes_SchName",
                table: "schemes",
                column: "SchName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_terms_TermText",
                table: "terms",
                column: "TermText");

            migrationBuilder.CreateIndex(
                name: "IX_terms_termTypeId",
                table: "terms",
                column: "termTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_termsByUrls_BatchPartId_UrlId_Position",
                table: "termsByUrls",
                columns: new[] { "BatchPartId", "UrlId", "Position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_termsByUrls_TermId",
                table: "termsByUrls",
                column: "TermId");

            migrationBuilder.CreateIndex(
                name: "IX_termsByUrls_UrlId",
                table: "termsByUrls",
                column: "UrlId");

            migrationBuilder.CreateIndex(
                name: "IX_termTypes_TtKey",
                table: "termTypes",
                column: "TtKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UrlGraphNodes_BatchPartId_FromUrlId_GotUrlId",
                table: "UrlGraphNodes",
                columns: new[] { "BatchPartId", "FromUrlId", "GotUrlId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UrlGraphNodes_FromUrlId",
                table: "UrlGraphNodes",
                column: "FromUrlId");

            migrationBuilder.CreateIndex(
                name: "IX_UrlGraphNodes_GotUrlId",
                table: "UrlGraphNodes",
                column: "GotUrlId");

            migrationBuilder.CreateIndex(
                name: "IX_urls_ExtensionId",
                table: "urls",
                column: "ExtensionId");

            migrationBuilder.CreateIndex(
                name: "IX_urls_HostId",
                table: "urls",
                column: "HostId");

            migrationBuilder.CreateIndex(
                name: "IX_urls_SchemeId",
                table: "urls",
                column: "SchemeId");

            migrationBuilder.CreateIndex(
                name: "IX_urls_UrlHashCode_HostId_ExtensionId_SchemeId",
                table: "urls",
                columns: new[] { "UrlHashCode", "HostId", "ExtensionId", "SchemeId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentsAnalysis");

            migrationBuilder.DropTable(
                name: "hostsByBatches");

            migrationBuilder.DropTable(
                name: "robots");

            migrationBuilder.DropTable(
                name: "termsByUrls");

            migrationBuilder.DropTable(
                name: "UrlGraphNodes");

            migrationBuilder.DropTable(
                name: "terms");

            migrationBuilder.DropTable(
                name: "BatchParts");

            migrationBuilder.DropTable(
                name: "urls");

            migrationBuilder.DropTable(
                name: "termTypes");

            migrationBuilder.DropTable(
                name: "Batches");

            migrationBuilder.DropTable(
                name: "extensions");

            migrationBuilder.DropTable(
                name: "hosts");

            migrationBuilder.DropTable(
                name: "schemes");
        }
    }
}
