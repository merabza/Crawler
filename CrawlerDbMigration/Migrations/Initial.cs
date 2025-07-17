#nullable disable

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CrawlerDbMigration.Migrations;

/// <inheritdoc />
public partial class Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable("batches",
            table => new
            {
                batchId = table.Column<int>("int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                batchName = table.Column<string>("nvarchar(50)", maxLength: 50, nullable: false),
                isOpen = table.Column<bool>("bit", nullable: false, defaultValue: false),
                autoCreateNextPart = table.Column<bool>("bit", nullable: false, defaultValue: false)
            }, constraints: table => { table.PrimaryKey("PK_batches", x => x.batchId); });

        migrationBuilder.CreateTable("extensions",
            table => new
            {
                extId = table.Column<int>("int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                extName = table.Column<string>("nvarchar(50)", maxLength: 50, nullable: false),
                extProhibited = table.Column<bool>("bit", nullable: false, defaultValue: false)
            }, constraints: table => { table.PrimaryKey("PK_extensions", x => x.extId); });

        migrationBuilder.CreateTable("hosts",
            table => new
            {
                hostId = table.Column<int>("int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                hostName = table.Column<string>("nvarchar(253)", maxLength: 253, nullable: false),
                hostProhibited = table.Column<bool>("bit", nullable: false, defaultValue: false)
            }, constraints: table => { table.PrimaryKey("PK_hosts", x => x.hostId); });

        migrationBuilder.CreateTable("schemes",
            table => new
            {
                schId = table.Column<int>("int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                schName = table.Column<string>("nvarchar(50)", maxLength: 50, nullable: false),
                schProhibited = table.Column<bool>("bit", nullable: false, defaultValue: false)
            }, constraints: table => { table.PrimaryKey("PK_schemes", x => x.schId); });

        migrationBuilder.CreateTable("termTypes",
            table => new
            {
                ttId = table.Column<int>("int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                ttKey = table.Column<string>("nvarchar(50)", maxLength: 50, nullable: false),
                ttName = table.Column<string>("nvarchar(50)", maxLength: 50, nullable: true)
            }, constraints: table => { table.PrimaryKey("PK_termTypes", x => x.ttId); });

        migrationBuilder.CreateTable("batchParts",
            table => new
            {
                bpId = table.Column<int>("int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                batchId = table.Column<int>("int", nullable: false),
                created = table.Column<DateTime>("datetime2", nullable: false),
                finished = table.Column<DateTime>("datetime2", nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_batchParts", x => x.bpId);
                table.ForeignKey("FK_BatchParts_Batches", x => x.batchId, "batches", "batchId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable("hostsByBatches",
            table => new
            {
                hbbId = table.Column<int>("int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                batchId = table.Column<int>("int", maxLength: 50, nullable: false),
                schemeId = table.Column<int>("int", nullable: false),
                hostId = table.Column<int>("int", nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_hostsByBatches", x => x.hbbId);
                table.ForeignKey("FK_HostsByBatches_Batches", x => x.batchId, "batches", "batchId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_HostsByBatches_HostModels", x => x.hostId, "hosts", "hostId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_HostsByBatches_SchemeModels", x => x.schemeId, "schemes", "schId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable("urls",
            table => new
            {
                urlId = table.Column<int>("int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                urlName = table.Column<string>("nvarchar(2048)", maxLength: 2048, nullable: false),
                hostId = table.Column<int>("int", nullable: false),
                extensionId = table.Column<int>("int", nullable: false),
                schemeId = table.Column<int>("int", nullable: false),
                urlHashCode = table.Column<int>("int", nullable: false),
                isSiteMap = table.Column<bool>("bit", nullable: false, defaultValue: false),
                isAllowed = table.Column<bool>("bit", nullable: false, defaultValue: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_urls", x => x.urlId);
                table.ForeignKey("FK_Urls_ExtensionModels", x => x.extensionId, "extensions", "extId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_Urls_HostModels", x => x.hostId, "hosts", "hostId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_Urls_SchemeModels", x => x.schemeId, "schemes", "schId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable("terms",
            table => new
            {
                trmId = table.Column<int>("int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                termText = table.Column<string>("nvarchar(50)", maxLength: 50, nullable: false,
                    collation: "SQL_Latin1_General_CP1_CS_AS"),
                termTypeId = table.Column<int>("int", nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_terms", x => x.trmId);
                table.ForeignKey("FK_Terms_TermTypes", x => x.termTypeId, "termTypes", "ttId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable("robots",
            table => new
            {
                rbtId = table.Column<int>("int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                batchPartId = table.Column<int>("int", maxLength: 50, nullable: false),
                schemeId = table.Column<int>("int", nullable: false),
                hostId = table.Column<int>("int", nullable: false),
                robotsTxt = table.Column<string>("ntext", nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_robots", x => x.rbtId);
                table.ForeignKey("FK_Robots_BatchParts", x => x.batchPartId, "batchParts", "bpId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_Robots_HostModels", x => x.hostId, "hosts", "hostId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_Robots_SchemeModels", x => x.schemeId, "schemes", "schId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable("contentAnalyses",
            table => new
            {
                caId = table.Column<int>("int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                batchPartId = table.Column<int>("int", nullable: false),
                urlId = table.Column<int>("int", nullable: false),
                responseStatusCode = table.Column<int>("int", nullable: false),
                finish = table.Column<DateTime>("datetime2", nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_contentAnalyses", x => x.caId);
                table.ForeignKey("FK_ContentAnalyses_BatchParts", x => x.batchPartId, "batchParts", "bpId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_ContentAnalyses_UrlModels", x => x.urlId, "urls", "urlId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable("urlGraphNodes",
            table => new
            {
                ugnId = table.Column<int>("int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                batchPartId = table.Column<int>("int", nullable: false),
                fromUrlId = table.Column<int>("int", nullable: false),
                gotUrlId = table.Column<int>("int", nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_urlGraphNodes", x => x.ugnId);
                table.ForeignKey("FK_UrlGraphNodes_BatchParts", x => x.batchPartId, "batchParts", "bpId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_UrlGraphNodes_UrlGraphNodes_FromUrlId", x => x.fromUrlId, "urls", "urlId");
                table.ForeignKey("FK_UrlGraphNodes_UrlGraphNodes_GotUrlId", x => x.gotUrlId, "urls", "urlId");
            });

        migrationBuilder.CreateTable("termsByUrls",
            table => new
            {
                tbuId = table.Column<int>("int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                batchPartId = table.Column<int>("int", nullable: false),
                urlId = table.Column<int>("int", nullable: false),
                termId = table.Column<int>("int", nullable: false),
                position = table.Column<int>("int", nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_termsByUrls", x => x.tbuId);
                table.ForeignKey("FK_TermsByUrls_BatchParts", x => x.batchPartId, "batchParts", "bpId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_TermsByUrls_Terms", x => x.termId, "terms", "trmId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_TermsByUrls_UrlModels", x => x.urlId, "urls", "urlId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex("IX_Batches_batchName_Unique", "batches", "batchName", unique: true);

        migrationBuilder.CreateIndex("IX_BatchParts_batchId_created_Unique", "batchParts",
            new[] { "batchId", "created" }, unique: true);

        migrationBuilder.CreateIndex("IX_ContentAnalyses_batchPartId_urlId_Unique", "contentAnalyses",
            new[] { "batchPartId", "urlId" }, unique: true);

        migrationBuilder.CreateIndex("IX_contentAnalyses_urlId", "contentAnalyses", "urlId");

        migrationBuilder.CreateIndex("IX_Extensions_extName_Unique", "extensions", "extName", unique: true);

        migrationBuilder.CreateIndex("IX_Hosts_hostName_Unique", "hosts", "hostName", unique: true);

        migrationBuilder.CreateIndex("IX_HostsByBatches_batchId_schemeId_hostId_Unique", "hostsByBatches",
            new[] { "batchId", "schemeId", "hostId" }, unique: true);

        migrationBuilder.CreateIndex("IX_hostsByBatches_hostId", "hostsByBatches", "hostId");

        migrationBuilder.CreateIndex("IX_hostsByBatches_schemeId", "hostsByBatches", "schemeId");

        migrationBuilder.CreateIndex("IX_Robots_batchPartId_schemeId_hostId_Unique", "robots",
            new[] { "batchPartId", "schemeId", "hostId" }, unique: true);

        migrationBuilder.CreateIndex("IX_robots_hostId", "robots", "hostId");

        migrationBuilder.CreateIndex("IX_robots_schemeId", "robots", "schemeId");

        migrationBuilder.CreateIndex("IX_Schemes_schName_Unique", "schemes", "schName", unique: true);

        migrationBuilder.CreateIndex("IX_Terms_termText_Unique", "terms", "termText");

        migrationBuilder.CreateIndex("IX_terms_termTypeId", "terms", "termTypeId");

        migrationBuilder.CreateIndex("IX_TermsByUrls_batchPartId_urlId_position_Unique", "termsByUrls",
            new[] { "batchPartId", "urlId", "position" }, unique: true);

        migrationBuilder.CreateIndex("IX_termsByUrls_termId", "termsByUrls", "termId");

        migrationBuilder.CreateIndex("IX_termsByUrls_urlId", "termsByUrls", "urlId");

        migrationBuilder.CreateIndex("IX_TermTypes_ttKey_Unique", "termTypes", "ttKey", unique: true);

        migrationBuilder.CreateIndex("IX_UrlGraphNodes_batchPartId_fromUrlId_gotUrlId_Unique", "urlGraphNodes",
            new[] { "batchPartId", "fromUrlId", "gotUrlId" }, unique: true);

        migrationBuilder.CreateIndex("IX_urlGraphNodes_fromUrlId", "urlGraphNodes", "fromUrlId");

        migrationBuilder.CreateIndex("IX_urlGraphNodes_gotUrlId", "urlGraphNodes", "gotUrlId");

        migrationBuilder.CreateIndex("IX_urls_extensionId", "urls", "extensionId");

        migrationBuilder.CreateIndex("IX_urls_hostId", "urls", "hostId");

        migrationBuilder.CreateIndex("IX_urls_schemeId", "urls", "schemeId");

        migrationBuilder.CreateIndex("IX_Urls_urlHashCode_hostId_extensionId_schemeId_Unique", "urls",
            new[] { "urlHashCode", "hostId", "extensionId", "schemeId" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("contentAnalyses");

        migrationBuilder.DropTable("hostsByBatches");

        migrationBuilder.DropTable("robots");

        migrationBuilder.DropTable("termsByUrls");

        migrationBuilder.DropTable("urlGraphNodes");

        migrationBuilder.DropTable("terms");

        migrationBuilder.DropTable("batchParts");

        migrationBuilder.DropTable("urls");

        migrationBuilder.DropTable("termTypes");

        migrationBuilder.DropTable("batches");

        migrationBuilder.DropTable("extensions");

        migrationBuilder.DropTable("hosts");

        migrationBuilder.DropTable("schemes");
    }
}