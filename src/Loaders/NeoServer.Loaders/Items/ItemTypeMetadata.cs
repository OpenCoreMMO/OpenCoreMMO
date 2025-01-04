using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using NeoServer.Loaders.Converts;

namespace NeoServer.Loaders.Items
{
    [Serializable]
    public struct ItemTypeMetadata
    {
        [JsonConverter(typeof(UshortNullableConverter))]
        public ushort? Id { get; set; }
        public string Name { get; set; }
        
        [JsonConverter(typeof(UshortNullableConverter))]
        public ushort? Fromid { get; set; }
        
        [JsonConverter(typeof(UshortNullableConverter))]
        public ushort? Toid { get; set; }
        public IEnumerable<Attribute> Attributes { get; set; }

        [JsonPropertyName("onUse")]
        public IEnumerable<Attribute> OnUseEvent { get; set; }

        public string Article { get; set; }
        public string Plural { get; set; }
        public string Editorsuffix { get; set; }
        public Requirement[] Requirements { get; set; }
        public string[] Flags { get; set; }

        [Serializable]
        public struct Attribute
        {
            public string Key { get; set; }
            public dynamic Value { get; set; }
            public IEnumerable<Attribute> Attributes { get; set; }
        }

        [Serializable]
        public struct Requirement
        {
            public string Vocation { get; set; }
            public ushort MinLevel { get; set; }
        }
    }
}