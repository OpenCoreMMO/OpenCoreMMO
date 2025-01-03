namespace NeoServer.Web.API.Response.Constants;

public static class ErrorMessage
{
    public static  string AccountEmailAlreadyExist => "Account email already exist.";
    public static  string AccountDoesNotExist => "Account does not exist.";
    public static  string AccountAlreadyBanished => "Account already banished.";
    public static string AccountInvalidPassword => "Invalid password.";
}

public static class SuccessMessage
{
    public static  string AccountCreated => "Account created successfully.";
    public static  string AddedPremiumDays => "Added premium days successfully.";
    public static string AccountBanned => "Account banned successfully.";
    public static string PasswordChanged => "Password changed successfully.";
}
