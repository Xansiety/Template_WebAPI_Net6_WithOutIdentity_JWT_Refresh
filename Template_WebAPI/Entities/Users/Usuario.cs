namespace Template_WebAPI.Entities.Users
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombres { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string EmailNormalized { get; set; }
        public string Password { get; set; }

        //RefreshToken
        public string RefreshToken { get; set; }
        public DateTime? ExpireTimeRefreshToken { get; set; }
        public DateTime? CreatedRefreshToken { get; set; }
        public bool IsExpiredRefreshToken => DateTime.UtcNow >= ExpireTimeRefreshToken;
        public bool IsActiveRefreshToken => RefreshToken != null && !IsExpiredRefreshToken;

        //relación
        public ICollection<Rol> Roles { get; set; } = new HashSet<Rol>();
        //clase relaciones
        //public ICollection<RefreshToken> RefreshTokens { get; set; } = new HashSet<RefreshToken>();
        public ICollection<UsuariosRoles> UsuariosRoles { get; set; }

        public bool Activo { get; set; } = true;
        public DateTime Created { get; set; }
    }
}
