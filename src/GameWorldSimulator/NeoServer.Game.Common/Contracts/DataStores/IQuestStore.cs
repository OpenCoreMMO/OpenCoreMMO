﻿using NeoServer.Game.Common.Item;

namespace NeoServer.Game.Common.Contracts.DataStores;

public interface IQuestStore : IDataStore<(ushort ActionId, uint UniqueId), QuestData>
{
}