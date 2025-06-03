using AutoMapper;
using NeoServer.Data.Entities;
using NeoServer.Web.API.Response.IpBans;

namespace NeoServer.Web.API.AutoMapperProfiles;

public class IpBanProfile : Profile
{
    public IpBanProfile()
    {
        CreateMap<IpBanEntity, IpBanResponseViewModel>();
    }
}