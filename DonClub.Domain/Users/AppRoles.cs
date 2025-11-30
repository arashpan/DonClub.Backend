namespace Donclub.Domain.Users;

public static class AppRoles
{
    public const string SuperUser = "SuperUser";
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Player = "Player";

    public const string SuperUserOrAdmin = SuperUser + "," + Admin;
    public const string ManagerOrAbove = SuperUser + "," + Admin + "," + Manager;
}
