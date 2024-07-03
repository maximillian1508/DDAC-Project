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

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public required decimal Amount { get; set; }

        [Required]
        public required DateTime Month { get; set; }

        [ForeignKey("ClientId")]
        public virtual required Client Client { get; set; }
    }
}
