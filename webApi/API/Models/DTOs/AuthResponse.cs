namespace CarPartsInventory.API.Models.DTOs
{
    public class AuthResponse
    {
        public string Token { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string FullName { get; set; }
        public bool IsAuthenticated { get; set; }
        public string Message { get; set; }
        public DateTime TokenExpiry { get; set; }
    }
}