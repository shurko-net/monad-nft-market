using MonadNftMarket.Models.DTO;

namespace MonadNftMarket.Models.EndpointsCursors;

public record UserTokenCursor(DateTime AcquiredAt, OrderDirection OrderBy);