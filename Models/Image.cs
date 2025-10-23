using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CosmeticShopAPI.Models;

public partial class Image
{
    [Key]
    public int ID_Image { get; set; }

    [Required]
    public int ProductID { get; set; }

    [Required]
    public string ImageURL { get; set; } = null!;

    public string? DescriptionIMG { get; set; }


}
