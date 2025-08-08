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
    private readonly IDbContextFactory<ApiDbContext> _dbContextFactory;

    public RecordChanges(IEventParser eventParser,
        IHyperSyncQuery hyperSyncQuery,
        ILogger<RecordChanges> logger,
        IDbContextFactory<ApiDbContext> dbContextFactory)
    {
        _eventParser = eventParser;
        _hyperSyncQuery = hyperSyncQuery;
        _logger = logger;
        _dbContextFactory = dbContextFactory;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await using var db = await _dbContextFactory.CreateDbContextAsync(stoppingToken);

                var nextBlock = await db.Indexer.FirstAsync(i => i.Id == 1, stoppingToken);
                
                var data = await _hyperSyncQuery.GetLogs(nextBlock.LastProcessedBlock);

                nextBlock.LastProcessedBlock = data.NextBlock.GetValueOrDefault(0) + 1;
                nextBlock.UpdatedAt = DateTime.UtcNow;

                var parsedEvents = data.Logs.Select(log =>
                {
                    var blk = data.Blocks
                        .First(b => b.Number == log.BlockNumber);
                    
                    var tx = data.Transactions
                        .First(t => t.BlockNumber == log.BlockNumber 
                        && t.TransactionIndex == log.TransactionIndex);

                    var evt = _eventParser.ParseEvent(log);

                    return new ParsedEvent
                    {
                        Event = evt!,
                        BlockNumber = blk.Number,
                        BlockHash = blk.Hash!,
                        BlockTimestamp = DateTimeOffset
                            .FromUnixTimeSeconds(Convert.ToInt64(blk.Timestamp!, 16))
                            .UtcDateTime,
                        TransactionHash = tx.Hash!,
                        TransactionFrom = tx.From!,
                        TransactionTo = tx.To!,
                        TransactionValue = Web3.Convert
                            .FromWei(BigInteger.Parse(tx.Value!.RemoveHexPrefix())),
                        LogIndex = log.LogIndex,
                        TransactionIndex = log.TransactionIndex,
                        LogData = log.Data!,
                        Topic0 = log.Topic0,
                        Topic1 = log.Topic1,
                        Topic2 = log.Topic2,
                        Topic3 = log.Topic3
                    };
                }).ToList();

                foreach (var pe in parsedEvents)
                {
                    switch (pe.Event)
                    {
                        case ListingCreatedEvent e:
                        {
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
                                TokenId = e.TokenId.ToString(),
                                SellerAddress = e.Seller,
                                IsSold = false,
                                IsActive = true,
                                BuyerAddress = string.Empty
                            };
                            
                            await db.Listings.AddAsync(lst, cancellationToken:  stoppingToken);
                            await db.SaveChangesAsync(stoppingToken);
                            
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
                                    /*Address = pe.,
                                    TokenIds = e,*/
                                    NftContracts = null
                                }
                            };
                            
                            break;
                        }
                    }
                }
            }

            await Task.Delay(1000, cancellationToken: stoppingToken);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogCritical($"Operation cancelled exception: {ex}");
        }
    }
}