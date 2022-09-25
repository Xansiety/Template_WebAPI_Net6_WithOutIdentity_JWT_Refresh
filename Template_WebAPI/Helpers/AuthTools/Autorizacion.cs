namespace Template_WebAPI.Helpers.AuthTools;

public class Autorizacion
{
    public enum AuthorizationRoles
    {
        Admin,
        Gerente,
        Empleado
    }

    public const AuthorizationRoles rol_predeterminado = AuthorizationRoles.Admin;
}