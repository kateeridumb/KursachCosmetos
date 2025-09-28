using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CosmeticShopAPI.Models;

public partial class Image
{
    [Key]
    public int Id_Image { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Required]
    public string ImageUrl { get; set; } = null!;

    public string? DescriptionImg { get; set; }


}
