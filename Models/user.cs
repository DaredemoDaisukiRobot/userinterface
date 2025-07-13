namespace userinterface.Models
{
    public class UserRegistrationRequest
    {  
        public int? ID { get; set; } 
        public string username { get; set; }     
        public string Password { get; set; }  
    }

    public class UserLoginRequest
    {
        public int ID { get; set; }      
        public string? Password { get; set; }   
    }
}
