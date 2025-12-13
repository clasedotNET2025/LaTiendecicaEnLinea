namespace LaTiendecicaEnLinea.Shared.Constants;

/// <summary>
/// Defines application role constants
/// </summary>
public static class Roles
{
    /// <summary>
    /// Administrator role
    /// </summary>
    public const string Admin = "Admin";

    /// <summary>
    /// Customer role
    /// </summary>
    public const string Customer = "Customer";

    /// <summary>
    /// Manager role
    /// </summary>
    public const string Manager = "Manager";

    /// <summary>
    /// Seller role
    /// </summary>
    public const string Seller = "Seller";

    /// <summary>
    /// Gets all defined roles
    /// </summary>
    /// <returns>Enumerable of all role names</returns>
    public static IEnumerable<string> GetAll()
    {
        return new[] { Admin, Customer, Manager, Seller };
    }

    /// <summary>
    /// Checks if a role name is valid
    /// </summary>
    /// <param name="roleName">Role name to validate</param>
    /// <returns>True if the role exists</returns>
    public static bool IsValidRole(string roleName)
    {
        return GetAll().Any(r => r.Equals(roleName, StringComparison.OrdinalIgnoreCase));
    }
}