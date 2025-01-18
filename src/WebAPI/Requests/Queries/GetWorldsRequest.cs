using MediatR;
using NeoServer.Data.Entities;
using NeoServer.Web.API.Response;
using NeoServer.Web.API.Response.World;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NeoServer.Web.API.Requests.Queries;

public class GetWorldsRequest : IRequest<BasePagedResponseViewModel<IEnumerable<WorldResponseViewModel>>>
{
    public string Name { get; set; }
    public bool? RequiresPremium { get; set; }
    public bool? TransferEnabled { get; set; }
    public bool? AntiCheatEnabled { get; set; }
    [JsonConverter(typeof(StringEnumConverter))]
    public Continent? Continent { get; set; }
    [JsonConverter(typeof(StringEnumConverter))]
    public PvpType? PvpType { get; set; }
    [JsonConverter(typeof(StringEnumConverter))]
    public Mode? Mode { get; set; }
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 5;
}