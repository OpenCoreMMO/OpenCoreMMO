namespace NeoServer.Web.API.Response.Constants;

public static class ErrorMessage
{
    public static  string AccountEmailAlreadyExist => "Account email already exist.";
    public static  string AccountDoesNotExist => "Account does not exist.";
    public static  string AccountAlreadyBanished => "Account already banished.";
    public static string AccountInvalidPassword => "Invalid password.";
    public static string AccountNameAlreadyExist => "Account name already exist.";
    public static string PlayerAlreadyExist => "Player already exist.";
    public static string PlayerNameAlreadyExist => "Player name already exist.";
    public static string PlayerNotFound  => "Player not found.";
    public static string IpBanished => "Ip already banished.";
    public static string WorldAlreadyDeleted => "World already was deleted successfully.";
    public static string WorldNotFound  => "World not found.";
    public static string WorldAlreadyExist => "World already exist.";
}

public static class SuccessMessage
{
    public static  string AccountCreated => "Account created {id} successfully.";
    public static  string PlayerCreated => "Player created {id} successfully.";
    public static  string AddedPremiumDays => "Added premium days successfully.";
    public static string AccountBanned => "Account banned successfully.";
    public static string PasswordChanged => "Password changed successfully.";
    public static string PlayerInfosUpdated => "Player infos updated successfully.";
    public static string PlayerSkillsUpdated => "Player skills updated successfully.";
    public static string IpBanned => "Ip banned successfully.";
    public static string WorldDeleted => "World deleted successfully.";
    public static  string WorldCreated => "World created {id} successfully.";
    
}
