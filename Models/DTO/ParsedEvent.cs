using Nethereum.ABI.FunctionEncoding.Attributes;

namespace MonadNftMarket.Models.DTO;

public class ParsedEvent
{
    public IEventDTO Event { get; set; } = null!;
    
    public long   BlockNumber { get; set; }
    public string BlockHash   { get; set; } = null!;
    public DateTime BlockTimestamp { get; set; }
    
    public string TransactionHash { get; set; } = null!;
    public string TransactionFrom { get; set; } = null!;
    public string TransactionTo   { get; set; } = null!;
    public decimal Price { get; set; }
    
    public long LogIndex         { get; set; }
    public long TransactionIndex { get; set; }
    public string LogData        { get; set; } = null!;
    public string? Topic0        { get; set; }
    public string? Topic1        { get; set; }
    public string? Topic2        { get; set; }
    public string? Topic3        { get; set; }
}