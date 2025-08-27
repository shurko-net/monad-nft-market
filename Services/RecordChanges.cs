using System.Numerics;
using Microsoft.EntityFrameworkCore;
using MonadNftMarket.Context;
using MonadNftMarket.Models;
using MonadNftMarket.Models.ContractEvents;
using MonadNftMarket.Models.DTO;
using MonadNftMarket.Providers;
using MonadNftMarket.Services.EventParser;
using MonadNftMarket.Services.Monad;
using MonadNftMarket.Services.Notifications;
using Nethereum.Util;
using Nethereum.Web3;

namespace MonadNftMarket.Services;

public class RecordChanges : BackgroundService
{
    private readonly IEventParser _eventParser;
    private readonly IHyperSyncQuery _hyperSyncQuery;
    private readonly ILogger<RecordChanges> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMonadService _monadService;
    public RecordChanges(IEventParser eventParser,
        IHyperSyncQuery hyperSyncQuery,
        ILogger<RecordChanges> logger,
        IServiceScopeFactory scopeFactory,
        IMonadService monadService)
    {
        _eventParser = eventParser;
        _hyperSyncQuery = hyperSyncQuery;
        _logger = logger;
        _scopeFactory = scopeFactory;
        _monadService = monadService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
                var notifyService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                var nextBlock = await db.Indexer.FirstAsync(i => i.Id == 1, stoppingToken);

                var data = await _hyperSyncQuery.GetLogs(nextBlock.LastProcessedBlock);

                if (data.Data.Count == 0)
                {
                    _logger.LogWarning("Got zero records");
                    await Task.Delay(500, stoppingToken);
                    continue;
                }

                nextBlock.LastProcessedBlock = data.NextBlock.GetValueOrDefault(0) + 1;
                nextBlock.UpdatedAt = DateTime.UtcNow;

                _logger.LogInformation($"NextBlock: {nextBlock.LastProcessedBlock}");

                var parsedEvents = new List<ParsedEvent>();

                foreach (var dt in data.Data)
                {
                    foreach (var log in dt.Logs)
                    {
                        var blk = dt.Blocks.First(b => b.Number == log.BlockNumber);
                        var tx = dt.Transactions.First(t =>
                            t.BlockNumber == log.BlockNumber &&
                            t.TransactionIndex == log.TransactionIndex);

                        var evt = _eventParser.ParseEvent(log);

                        if (evt is null) continue;

                        decimal priceEth = evt switch
                        {
                            ListingCreatedEvent e => Web3.Convert.FromWei(e.Price),
                            _ => 0m
                        };

                        parsedEvents.Add(new ParsedEvent
                        {
                            Event = evt,
                            BlockNumber = blk.Number,
                            BlockHash = blk.Hash!,
                            BlockTimestamp = DateTimeOffset
                                .FromUnixTimeSeconds(Convert.ToInt64(blk.Timestamp, 16))
                                .UtcDateTime,
                            TransactionHash = tx.Hash!,
                            TransactionFrom = tx.From!,
                            TransactionTo = tx.To!,
                            Price = priceEth,
                            LogIndex = log.LogIndex,
                            TransactionIndex = log.TransactionIndex,
                            LogData = log.Data!,
                            Topic0 = log.Topic0,
                            Topic1 = log.Topic1,
                            Topic2 = log.Topic2,
                            Topic3 = log.Topic3
                        });
                    }
                }

                foreach (var pe in parsedEvents)
                {
                    switch (pe.Event)
                    {
                        case ListingCreatedEvent e:
                        {
                            try
                            {
                                if (await db.Listings
                                        .AsNoTracking()
                                        .AnyAsync(l => l.ListingId == e.Id, cancellationToken: stoppingToken))
                                {
                                    break;
                                }

                                var lst = new Listing
                                {
                                    EventMetadata = new EventMetadata
                                    {
                                        BlockNumber = pe.BlockNumber,
                                        BlockHash = pe.BlockHash,
                                        Timestamp = pe.BlockTimestamp,
                                        TransactionHash = pe.TransactionHash
                                    },
                                    ListingId = e.Id,
                                    NftContractAddress = e.NftContract,
                                    Price = Web3.Convert.FromWei(e.Price),
                                    TokenId = e.TokenId,
                                    SellerAddress = e.Seller,
                                    IsSold = false,
                                    IsActive = true,
                                    BuyerAddress = string.Empty
                                };

                                await db.Listings.AddAsync(lst, cancellationToken: stoppingToken);
                                await db.SaveChangesAsync(stoppingToken);

                                await notifyService
                                    .NotifyAsync(lst.SellerAddress,
                                        NotificationType.ListingCreated,
                                        "Listing created",
                                        $"You created listing #{lst.ListingId}. Price: {lst.Price} ETH",
                                        lst.EventMetadata.TransactionHash);

                                _logger.LogInformation($"New listing: {e.Id}");
                            }
                            catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException pg
                                                                && pg.SqlState == "23505")
                            {
                                _logger.LogWarning("Listing already exists, skipping insert(unique constraint)");
                            }

                            break;
                        }
                        case ListingRemovedEvent e:
                        {
                            var lst = await db.Listings.FirstOrDefaultAsync(l =>
                                l.ListingId == e.Id, cancellationToken: stoppingToken);

                            if (lst is not null)
                            {
                                lst.IsActive = false;
                                await db.SaveChangesAsync(stoppingToken);
                                
                                await notifyService
                                    .NotifyAsync(lst.SellerAddress,
                                        NotificationType.ListingRemoved,
                                        "Listing removed",
                                        $"Your listing #{lst.ListingId} has been removed. Price: {lst.Price} ETH",
                                        lst.EventMetadata.TransactionHash);
                            }

                            break;
                        }
                        case ListingSoldEvent e:
                        {
                            var lst = await db.Listings.FirstOrDefaultAsync(l =>
                                l.ListingId == e.Id, cancellationToken: stoppingToken);

                            if (lst is not null)
                            {
                                lst.BuyerAddress = e.Buyer;
                                lst.IsSold = true;
                                lst.IsActive = false;

                                await db.SaveChangesAsync(stoppingToken);
                                
                                await notifyService
                                    .NotifyAsync(lst.SellerAddress,
                                        NotificationType.ListingSold,
                                        "Listing sold",
                                        $"Your listing #{lst.ListingId} was bought by {lst.BuyerAddress} for {lst.Price} ETH",
                                        lst.EventMetadata.TransactionHash);
                            }

                            break;
                        }
                        case TradeCreatedEvent e:
                        {
                            var tradeData =
                                await _monadService.GetTradeDataAsync(e.TradeId, cancellationToken: stoppingToken);
                            
                            if(tradeData.To.User.Equals(AddressUtil.ZERO_ADDRESS) ||
                               tradeData.From.User.Equals(AddressUtil.ZERO_ADDRESS))
                                break;
                            
                            var trade = new Trade
                            {
                                TradeId = e.TradeId,
                                EventMetadata = new EventMetadata
                                {
                                    BlockNumber = pe.BlockNumber,
                                    BlockHash = pe.BlockHash,
                                    Timestamp = pe.BlockTimestamp,
                                    TransactionHash = pe.TransactionHash
                                },
                                From = new Peer
                                {
                                    Address = tradeData.From.User,
                                    TokenIds = tradeData.From.TokenIds,
                                    NftContracts = tradeData.From.NftContracts
                                },
                                To = new Peer
                                {
                                    Address = tradeData.To.User,
                                    TokenIds = tradeData.To.TokenIds,
                                    NftContracts = tradeData.To.NftContracts
                                },
                                IsActive = true
                            };

                            await db.Trades.AddAsync(trade, cancellationToken: stoppingToken);
                            await db.SaveChangesAsync(stoppingToken);
                            
                            var toAddress = trade.To.Address.ToLowerInvariant();
                            await notifyService.NotifyAsync(toAddress,
                                NotificationType.TradeCreated,
                                "Incoming trade",
                                $"You received trade #{trade.TradeId} from {trade.From.Address}",
                                trade.EventMetadata.TransactionHash);

                            break;
                        }
                        case TradeAcceptedEvent e:
                        {
                            await CloseTradeAsync(e.TradeId, db, notifyService,
                                NotificationType.TradeAccepted,
                                $"Trade accepted",
                                $"Your trade #{e.TradeId} has been accepted",
                                pe.TransactionHash,
                                stoppingToken);
                            
                            break;
                        }
                        case TradeCompletedEvent e:
                        {
                            await CloseTradeAsync(e.TradeId, db, notifyService,
                                NotificationType.TradeCompleted,
                                $"Trade completed",
                                $"Your trade #{e.TradeId} final preparations for trade confirmation",
                                pe.TransactionHash,
                                stoppingToken);
                            
                            break;
                        }
                        case TradeRejectedEvent e:
                        {
                            await CloseTradeAsync(e.TradeId, db, notifyService,
                                NotificationType.TradeRejected,
                                $"Trade rejected",
                                $"Your trade #{e.TradeId} has been rejected by second side",
                                pe.TransactionHash,
                                stoppingToken);
                            
                            break;
                        }
                    }
                }
            }

            await Task.Delay(200, cancellationToken: stoppingToken);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogCritical($"Operation cancelled exception: {ex}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected exception: {ex}");
        }
    }
    private static async Task CloseTradeAsync(
        BigInteger tradeId,
        ApiDbContext db,
        INotificationService notifyService,
        NotificationType notificationType,
        string title,
        string message,
        string txHash,
        CancellationToken stoppingToken)
    {
        var trade = await db.Trades.FirstOrDefaultAsync(t => t.TradeId == tradeId,
            cancellationToken: stoppingToken);

        if (trade is null) return;

        trade.IsActive = false;
        await db.SaveChangesAsync(stoppingToken);

        if (notificationType == NotificationType.TradeCreated)
        {
            if (!string.IsNullOrEmpty(trade.From.Address))
            {
                var fromAddress =  trade.From.Address.ToLowerInvariant();
                await notifyService.NotifyAsync(fromAddress, notificationType,
                    "Outcoming trade",
                    $"You sent trade #{trade.Id} to {trade.To.Address}",
                    txHash);
            }
        }
        
        var toAddress = trade.To.Address.ToLowerInvariant();
        await notifyService.NotifyAsync(toAddress, notificationType, title, message, txHash);
    }
}