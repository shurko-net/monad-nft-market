using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MonadNftMarket.Migrations
{
    /// <inheritdoc />
    public partial class initialmigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Indexer",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LastProcessedBlock = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Indexer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Listings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventMetadata_BlockNumber = table.Column<long>(type: "bigint", nullable: false),
                    EventMetadata_BlockHash = table.Column<string>(type: "text", nullable: true),
                    EventMetadata_Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EventMetadata_TransactionHash = table.Column<string>(type: "text", nullable: true),
                    ListingId = table.Column<long>(type: "bigint", nullable: false),
                    NftContractAddress = table.Column<string>(type: "text", nullable: true),
                    TokenId = table.Column<string>(type: "text", nullable: true),
                    SellerAddress = table.Column<string>(type: "text", nullable: true),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    IsSold = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    BuyerAddress = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Listings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Trades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventMetadata_BlockNumber = table.Column<long>(type: "bigint", nullable: false),
                    EventMetadata_BlockHash = table.Column<string>(type: "text", nullable: true),
                    EventMetadata_Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EventMetadata_TransactionHash = table.Column<string>(type: "text", nullable: true),
                    From_Address = table.Column<string>(type: "text", nullable: true),
                    From_TokenIds = table.Column<List<string>>(type: "text[]", nullable: false),
                    From_NftContracts = table.Column<List<string>>(type: "text[]", nullable: false),
                    To_Address = table.Column<string>(type: "text", nullable: true),
                    To_TokenIds = table.Column<List<string>>(type: "text[]", nullable: false),
                    To_NftContracts = table.Column<List<string>>(type: "text[]", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trades", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Indexer",
                columns: new[] { "Id", "LastProcessedBlock", "UpdatedAt" },
                values: new object[] { 1, "0", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Indexer");

            migrationBuilder.DropTable(
                name: "Listings");

            migrationBuilder.DropTable(
                name: "Trades");
        }
    }
}
