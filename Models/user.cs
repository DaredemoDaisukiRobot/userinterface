using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace userinterface.Models
{
    public class UserRegistrationRequest
    {
        public int? ID { get; set; }
        public string username { get; set; }
        public string Password { get; set; }
        public string email { get; set; }
    }

    public class UserLoginRequest
    {
        public string email { get; set; }
        public string? Password { get; set; }
    }

    public class UserDeleteRequest
    {
        public int user_ID { get; set; }
    }

    public class UserRegistrationResult
    {
        public int UserId { get; set; }
        public IEnumerable<string> Roles { get; set; } = [];
    }

    public class UserBasicInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string email { get; set; } = "";
        public List<string> Roles { get; set; } = [];
    }

    [Table("users")]
    public class User
    {
        [Column("id")] public int Id { get; set; }
        [Column("name")] public string Name { get; set; } = "";
        [Column("email")] public string email { get; set; } = "";
        [Column("password_hash")] public string PasswordHash { get; set; } = "";
        [Column("msg")] public string Msg { get; set; } = "";
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }

    [Table("roles")]
    public class Role
    {
        [Column("id")] public int Id { get; set; }
        [Column("name")] public string Name { get; set; } = "";
        [Column("description")] public string? Description { get; set; }
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }

    [Table("permissions")]
    public class Permission
    {
        [Column("id")] public int Id { get; set; }
        [Column("name")] public string Name { get; set; } = "";
        [Column("description")] public string? Description { get; set; }
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }

    [Table("role_permissions")]
    public class RolePermission
    {
        [Column("role_id")] public int RoleId { get; set; }
        public Role Role { get; set; }
        [Column("permission_id")] public int PermissionId { get; set; }
        public Permission Permission { get; set; }
    }

    [Table("user_roles")]
    public class UserRole
    {
        [Column("user_id")] public int UserId { get; set; }
        public User User { get; set; }
        [Column("role_id")] public int RoleId { get; set; }
        public Role Role { get; set; }
    }

    public class UserUpdateRequest
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    public class AssignRoleRequest
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
    }
}