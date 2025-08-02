using System.ComponentModel.DataAnnotations.Schema;

namespace userinterface.Models
{
    public class UserRegistrationRequest
    {
        public int? ID { get; set; }
        public string username { get; set; }
        public string Password { get; set; }
        public string email { get; set; }
        public string? status { get; set; }
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
        public string? Status { get; set; }
    }
    public class UserBasicInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Status { get; set; }
        public string email { get; set; } = "";
    }

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        [Column("email")]
        public string email { get; set; } = "";
        [Column("password_hash")]
        public string PasswordHash { get; set; } = "";
        public string Msg { get; set; } = "";
        public string? Status { get; set; } = "user";
    }

    public class UserUpdateRequest
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Status { get; set; }
    }

    public class UserPasswordUpdateRequest
    {
        public int Id { get; set; }
        public string OldPassword { get; set; } 
        public string NewPassword { get; set; }
    }
}