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

        [Required]
        public required string Name { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public required decimal TargetAmount { get; set; }


        [ForeignKey("ClientId")]
        public virtual required Client Client { get; set; }
    }
}
