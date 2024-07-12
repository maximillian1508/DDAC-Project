using DDAC_Project.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DDAC_Project.Models
{
    public class Budget
    {
        [Key]
        public int BudgetId { get; set; }

        [Required]
        public required int ClientId { get; set; }

        [Required(ErrorMessage ="Amount is required")]
        [Range(typeof(decimal), "0.00", "79228162514264337593543950335", ErrorMessage = "Budget amount must be more than 0", ParseLimitsInInvariantCulture = true)]
        [Column(TypeName = "decimal(18,2)")]
        public required decimal Amount { get; set; }

        [Required]
        public required DateTime Month { get; set; }

        [ForeignKey("ClientId")]
        public virtual Client? Client { get; set; }
    }
}
