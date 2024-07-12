using DDAC_Project.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DDAC_Project.Models
{
    public class Goal
    {
        [Key]
        public int GoalId { get; set; }

        [Required]
        public required int ClientId { get; set; }

        [Required(ErrorMessage = "Goal name is required")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Target amount is required")]
        [Range(typeof(decimal), "0,01", "79228162514264337593543950335", ErrorMessage = "Target amount must be more than 0")]
        [Column(TypeName = "decimal(18,2)")]
        public required decimal TargetAmount { get; set; }


        [ForeignKey("ClientId")]
        public virtual Client? Client { get; set; }
    }
}
