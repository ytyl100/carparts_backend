using System.ComponentModel.DataAnnotations;

namespace CarPartsInventory.API.Models
{
    public class Brand
    {
        public string Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [Url]
        public string Logo { get; set; }

        public string FirstLetter { get; set; }
        public bool IsHot { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}