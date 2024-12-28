namespace NeoServer.WebApi.Tests.Factories;

public sealed class NeoServerWebApiWebApplicationFactory : BaseWebApplicationFactory<Web.API.Program>
{
    #region private members

    private static NeoServerWebApiWebApplicationFactory _instance;

    #endregion

    #region constructors

    private NeoServerWebApiWebApplicationFactory()
    {
    }

    #endregion

    #region public methods implementation

    public static NeoServerWebApiWebApplicationFactory GetInstance()
    {
        if (_instance == null) _instance = new NeoServerWebApiWebApplicationFactory();

        return _instance;
    }

    #endregion
}