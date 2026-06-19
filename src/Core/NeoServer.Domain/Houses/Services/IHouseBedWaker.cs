using NeoServer.Domain.Common.Contracts.Items;

namespace NeoServer.Domain.Houses.Services;

/// <summary>Wakes players sleeping on beds inside a house.</summary>
public interface IHouseBedWaker
{
    void WakeAll(IEnumerable<IItem> beds);
}
