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
                name: "indexer",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    last_processed_block = table.Column<string>(type: "text", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_indexer", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "listings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_metadata_block_number = table.Column<string>(type: "text", nullable: false),
                    event_metadata_block_hash = table.Column<string>(type: "character varying(70)", maxLength: 70, nullable: false),
                    event_metadata_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    event_metadata_transaction_hash = table.Column<string>(type: "character varying(70)", maxLength: 70, nullable: false),
                    listing_id = table.Column<string>(type: "text", nullable: false),
                    nft_contract_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    token_id = table.Column<string>(type: "text", nullable: false),
                    seller_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    is_sold = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    buyer_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_listings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    body = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    is_read = table.Column<bool>(type: "boolean", nullable: false),
                    transaction_hash = table.Column<string>(type: "character varying(70)", maxLength: 70, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notifications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "trades",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_metadata_block_number = table.Column<string>(type: "text", nullable: false),
                    event_metadata_block_hash = table.Column<string>(type: "character varying(70)", maxLength: 70, nullable: false),
                    event_metadata_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    event_metadata_transaction_hash = table.Column<string>(type: "character varying(70)", maxLength: 70, nullable: false),
                    trade_id = table.Column<string>(type: "text", nullable: false),
                    from_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    from_token_ids = table.Column<string>(type: "text", nullable: false),
                    from_nft_contracts = table.Column<List<string>>(type: "text[]", nullable: false),
                    to_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    to_token_ids = table.Column<string>(type: "text", nullable: false),
                    to_nft_contracts = table.Column<List<string>>(type: "text[]", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trades", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "indexer",
                columns: new[] { "id", "last_processed_block", "updated_at" },
                values: new object[] { 1, "0", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.CreateIndex(
                name: "ix_listings_listing_id",
                table: "listings",
                column: "listing_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_trades_trade_id",
                table: "trades",
                column: "trade_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "indexer");

            migrationBuilder.DropTable(
                name: "listings");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "trades");
        }
    }
}
