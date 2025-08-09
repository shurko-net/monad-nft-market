using System.Numerics;
using Microsoft.EntityFrameworkCore;
using MonadNftMarket.Context;
using MonadNftMarket.Models;
using MonadNftMarket.Models.DTO;
using MonadNftMarket.Models.DTO.ContractEvents;
using MonadNftMarket.Providers;
using MonadNftMarket.Services.EventParser;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Web3;

namespace MonadNftMarket.Services;

public class RecordChanges : BackgroundService
{
    private readonly IEventParser _eventParser;
    private readonly IHyperSyncQuery _hyperSyncQuery;
    private readonly ILogger<RecordChanges> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public RecordChanges(IEventParser eventParser,
        IHyperSyncQuery hyperSyncQuery,
        ILogger<RecordChanges> logger,
        IServiceScopeFactory scopeFactory)
    {
        _eventParser = eventParser;
        _hyperSyncQuery = hyperSyncQuery;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
                
                var nextBlock = await db.Indexer.FirstAsync(i => i.Id == 1, stoppingToken);

                var data = await _hyperSyncQuery.GetLogs(nextBlock.LastProcessedBlock);

                if (!data.Data.Any())
                {
                    _logger.LogWarning("Got zero records");
                    await Task.Delay(6000, stoppingToken);
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

                        if (evt is not null)
                        {
                            decimal priceEth = 0;
                            switch (evt)
                            {
                                case ListingCreatedEvent or ListingRemovedEvent or ListingSoldEvent:
                                    dynamic lst = evt;
                                    
                                    priceEth = (decimal)Web3.Convert.FromWei(lst.Price);
                                    break;
                            }
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
                            
                                await db.Listings.AddAsync(lst, cancellationToken:  stoppingToken);
                                await db.SaveChangesAsync(stoppingToken);
                            
                                _logger.LogInformation($"New listing: {e.Id}");
                            }
                            catch (DbUpdateException ex) when((ex.InnerException is Npgsql.PostgresException pg 
                                                              && pg.SqlState == "23505"))
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
                                db.Remove(lst);
                                await db.SaveChangesAsync(stoppingToken);
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
                            }
                            
                            break;
                        }
                        case TradeCreatedEvent e:
                        {
                            var trade = new Trade
                            {
                                EventMetadata = new EventMetadata
                                {
                                    BlockNumber = pe.BlockNumber,
                                    BlockHash = pe.BlockHash,
                                    Timestamp = pe.BlockTimestamp,
                                    TransactionHash = pe.TransactionHash
                                },
                                From = new Peer
                                {
                                    //Address = pe.,
                                    //TokenIds = e,*/
                                    NftContracts = null
                                }
                            };
                            
                            break;
                        }
                    }
                }
            }

            await Task.Delay(3000, cancellationToken: stoppingToken);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogCritical($"Operation cancelled exception: {ex}");
        }
    }
}