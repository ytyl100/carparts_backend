using System.Collections.Generic;

namespace CarPartsInventory.API.Models.DTOs
{
    public class BatchUpdateRequest
    {
        public List<Part> UPDATED_PARTS { get; set; } = new();
        public SubCategoryUpdateDto? SUB_CATEGORIES_UPDATE { get; set; }
    }

    public class SubCategoryUpdateDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
    }
}
