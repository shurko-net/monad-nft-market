using Microsoft.Extensions.Options;
using MonadNftMarket.Configuration;
using MonadNftMarket.Models.DTO.ContractEvents;
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
    
    public EventParser(IOptions<EnvVariables> env)
    {
        _contractAddress = env.Value.ContractAddress;
        _web3 = new Web3(env.Value.MonadRpcUrl);
        
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
        var fl = new FilterLog
        {
            Topics = [log.Topic0, log.Topic1, log.Topic2, log.Topic3],
            Data = log.Data
        };

        return _decoders.TryGetValue(log.Topic0!, out var decoder) ? decoder(fl) : null;
    }
}