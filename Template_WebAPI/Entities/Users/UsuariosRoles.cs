namespace Template_WebAPI.Entities.Users
{
    public class UsuariosRoles
    {
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }
        public int RolId { get; set; }
        public Rol Rol { get; set; }

    }
}
