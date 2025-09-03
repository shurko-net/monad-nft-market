using System;
using System.Collections.Generic;
using System.Numerics;
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
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    body = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    is_read = table.Column<bool>(type: "boolean", nullable: false),
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
                    trade_id = table.Column<string>(type: "text", nullable: false),
                    listing_ids = table.Column<string>(type: "text", nullable: false),
                    from_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    from_token_ids = table.Column<string>(type: "text", nullable: false),
                    from_nft_contracts = table.Column<List<string>>(type: "text[]", nullable: false),
                    to_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    to_token_ids = table.Column<string>(type: "text", nullable: false),
                    to_nft_contracts = table.Column<List<string>>(type: "text[]", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trades", x => x.id);
                    table.UniqueConstraint("ak_trades_trade_id", x => x.trade_id);
                });

            migrationBuilder.CreateTable(
                name: "listings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nft_metadata_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nft_metadata_token_id = table.Column<BigInteger>(type: "numeric", nullable: false),
                    nft_metadata_nft_contract_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    nft_metadata_kind = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    nft_metadata_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    nft_metadata_image_original = table.Column<string>(type: "text", nullable: false),
                    nft_metadata_description = table.Column<string>(type: "text", nullable: false),
                    nft_metadata_price = table.Column<decimal>(type: "numeric", nullable: true),
                    nft_metadata_last_updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    listing_id = table.Column<string>(type: "text", nullable: false),
                    nft_contract_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    token_id = table.Column<string>(type: "text", nullable: false),
                    seller_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    buyer_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    trade_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_listings", x => x.id);
                    table.UniqueConstraint("ak_listings_listing_id", x => x.listing_id);
                    table.ForeignKey(
                        name: "fk_listings_trades_trade_id",
                        column: x => x.trade_id,
                        principalTable: "trades",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    to_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    event_metadata_block_number = table.Column<string>(type: "text", nullable: false),
                    event_metadata_block_hash = table.Column<string>(type: "character varying(70)", maxLength: 70, nullable: false),
                    event_metadata_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    event_metadata_transaction_hash = table.Column<string>(type: "character varying(70)", maxLength: 70, nullable: false),
                    listing_id = table.Column<string>(type: "text", nullable: true),
                    trade_id = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_history", x => x.id);
                    table.ForeignKey(
                        name: "fk_history_listings_listing_id",
                        column: x => x.listing_id,
                        principalTable: "listings",
                        principalColumn: "listing_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_history_trades_trade_id",
                        column: x => x.trade_id,
                        principalTable: "trades",
                        principalColumn: "trade_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "indexer",
                columns: new[] { "id", "last_processed_block", "updated_at" },
                values: new object[] { 1, "0", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.CreateIndex(
                name: "ix_history_from_address_status",
                table: "history",
                columns: new[] { "from_address", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_history_from_address_to_address_status",
                table: "history",
                columns: new[] { "from_address", "to_address", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_history_listing_id",
                table: "history",
                column: "listing_id");

            migrationBuilder.CreateIndex(
                name: "ix_history_trade_id",
                table: "history",
                column: "trade_id");

            migrationBuilder.CreateIndex(
                name: "ix_listings_listing_id",
                table: "listings",
                column: "listing_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_listings_status_listing_id",
                table: "listings",
                columns: new[] { "status", "listing_id" });

            migrationBuilder.CreateIndex(
                name: "ix_listings_trade_id",
                table: "listings",
                column: "trade_id");

            migrationBuilder.CreateIndex(
                name: "ix_trades_status_trade_id",
                table: "trades",
                columns: new[] { "status", "trade_id" });

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
                name: "history");

            migrationBuilder.DropTable(
                name: "indexer");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "listings");

            migrationBuilder.DropTable(
                name: "trades");
        }
    }
}
