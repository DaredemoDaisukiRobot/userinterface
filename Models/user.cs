namespace userinterface.Models
{
    public class UserRegistrationRequest
    {
        public int? ID { get; set; }
        public string username { get; set; }
        public string Password { get; set; }
        public string? status { get; set; }
    }

    public class UserLoginRequest
    {
        public int ID { get; set; }
        public string? Password { get; set; }
    }

    public class UserDeleteRequest
    {
        public int admin_ID { get; set; }
        public string admin_pwd { get; set; }
        public int user_ID { get; set; }
    }
}
