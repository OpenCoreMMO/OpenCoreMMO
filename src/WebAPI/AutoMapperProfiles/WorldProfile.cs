using AutoMapper;
using NeoServer.Data.Entities;
using NeoServer.Web.API.Response.World;

namespace NeoServer.Web.API.AutoMapperProfiles;

public class WorldProfile : Profile
{
    public WorldProfile()
    {
        CreateMap<WorldEntity, WorldResponseViewModel>();
    }
}