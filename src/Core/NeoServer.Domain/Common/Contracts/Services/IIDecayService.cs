using NeoServer.Domain.Common.Contracts.Items;

namespace NeoServer.Domain.Common.Contracts.Services;

public interface IDecayService
{
    void Decay(IItem item);
}