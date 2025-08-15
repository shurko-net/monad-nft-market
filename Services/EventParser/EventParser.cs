using Microsoft.Extensions.Options;
using MonadNftMarket.Configuration;
using MonadNftMarket.Models.ContractEvents;
using MonadNftMarket.Models.DTO.HyperSync;
using Nethereum.ABI.FunctionEncoding;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;

namespace MonadNftMarket.Services.EventParser;

public class EventParser : IEventParser
{
    private readonly Dictionary<string, Func<FilterLog, IEventDTO>> _decoders;
    private readonly Web3 _web3;
    private readonly string _contractAddress;
    private readonly ILogger<EventParser> _logger;
    
    public EventParser(
        IOptions<EnvVariables> env,
        ILogger<EventParser> logger)
    {
        _contractAddress = env.Value.ContractAddress;
        _web3 = new Web3(env.Value.MonadRpcUrl);
        _logger = logger;
        
        _decoders = new Dictionary<string, Func<FilterLog, IEventDTO>>(StringComparer.OrdinalIgnoreCase);
        
        Register<ListingCreatedEvent>();
        Register<ListingRemovedEvent>();
        Register<ListingSoldEvent>();
        Register<TradeAcceptedEvent>();
        Register<TradeCompletedEvent>();
        Register<TradeCreatedEvent>();
        Register<TradeRejectedEvent>();
    }
    
    private void Register<T>() where T : IEventDTO, new()
    {
        var ev = _web3.Eth.GetEvent<T>(_contractAddress);
        var sig = ev.EventABI.Sha3Signature.EnsureHexPrefix();

        var topicDecoder = new EventTopicDecoder();
        
        _decoders[sig] = log => topicDecoder.DecodeTopics<T>(log.Topics, log.Data);
    }

    public IEventDTO? ParseEvent(Log log)
    {
        var topics = new List<object?>();
        if(!string.IsNullOrEmpty(log.Topic0)) topics.Add(log.Topic0);
        if(!string.IsNullOrEmpty(log.Topic1)) topics.Add(log.Topic1);
        if(!string.IsNullOrEmpty(log.Topic2)) topics.Add(log.Topic2);
        if(!string.IsNullOrEmpty(log.Topic3)) topics.Add(log.Topic3);

        var fl = new FilterLog
        {
            Topics = topics.ToArray(),
            Data = log.Data
        };

        if (string.IsNullOrEmpty(log.Topic0)) return null;

        if (!_decoders.TryGetValue(log.Topic0, out var decoder)) return null;
        try
        {
            return decoder(fl);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to decode event {log.Topic0}: {ex.Message}");
            return null;
        }
    }
}