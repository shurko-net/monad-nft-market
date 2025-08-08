using MonadNftMarket.Models.DTO.HyperSync;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace MonadNftMarket.Services.EventParser;

public interface IEventParser
{
    IEventDTO? ParseEvent(Log log);
}