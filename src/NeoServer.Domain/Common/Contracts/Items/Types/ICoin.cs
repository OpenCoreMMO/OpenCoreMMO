namespace NeoServer.Domain.Common.Contracts.Items.Types;

public interface ICoin : ICumulative
{
    uint Worth { get; }
}