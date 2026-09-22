using System.Collections.Generic;
using NeoServer.Domain.Common.Contracts.Items;
using Serilog;

namespace NeoServer.Domain.Houses.Services;

/// <summary>
///     Wakes players sleeping on beds inside a house.
/// </summary>
public class HouseBedWakerService : IHouseBedWaker
{
    private readonly ILogger _logger;

    public HouseBedWakerService(ILogger logger)
    {
        _logger = logger;
    }

    public void WakeAll(IEnumerable<IItem> beds)
    {
        _logger.Verbose("HouseBedWakerService.WakeAll called with {BedCount} beds — no-op in Phase 2", beds);
    }
}
