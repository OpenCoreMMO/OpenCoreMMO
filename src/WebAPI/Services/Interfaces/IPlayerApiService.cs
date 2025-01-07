using NeoServer.Web.API.Response;
using NeoServer.Web.Shared.ViewModels.Request;

namespace NeoServer.Web.API.Services.Interfaces;

public interface IPlayerApiService
{
    Task<IEnumerable<PlayerResponseViewModel>> GetAll();
    Task<PlayerResponseViewModel> GetById(int playerId);
    Task Create(PlayerPostRequest playerPostRequest);
}