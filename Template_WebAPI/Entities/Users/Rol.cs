namespace Template_WebAPI.Entities.Users
{
    public class Rol
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public bool Activo { get; set; }
        public ICollection<Usuario> Usuarios { get; set; } = new HashSet<Usuario>();
        public ICollection<UsuariosRoles> UsuariosRoles { get; set; }
    }
}
