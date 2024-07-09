using DDAC_Project.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DDAC_Project.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        public int ? ClientId { get; set; }

        [Required]
        public required string Name { get; set; }

        [Required]
        public required string Type { get; set; }

        [Required]
        public required bool IsDefault { get; set; }

        [ForeignKey("ClientId")]
        public virtual  Client ? Client { get; set; }
    }
}
