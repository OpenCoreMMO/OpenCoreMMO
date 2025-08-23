using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Domain.Items.Items;

public class Letter(IItemType metadata, Location location) : Label(metadata, location);