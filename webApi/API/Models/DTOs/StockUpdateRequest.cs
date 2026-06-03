using System.ComponentModel.DataAnnotations;

namespace CarPartsInventory.API.Models.DTOs
{
    public class StockUpdateRequest
    {
        [Required(ErrorMessage = "QuantityChange 是必需的")]
        [Range(-1000, 1000, ErrorMessage = "库存变更量必须在 -1000 到 1000 之间")]
        public int QuantityChange { get; set; }
    }
}