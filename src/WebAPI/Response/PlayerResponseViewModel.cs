namespace NeoServer.Web.API.Response;

[Serializable]
public class PlayerResponseViewModel
{
    public int PlayerId { get; set; }
    public string Name { get; set; }
    public ushort Level { get; set; }
    public bool Online { get; set; }
}