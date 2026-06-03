using System.ComponentModel.DataAnnotations;

namespace CarPartsInventory.API.Models.DTOs
{
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}