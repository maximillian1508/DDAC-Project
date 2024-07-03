using DDAC_Project.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DDAC_Project.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }

        [Required]
        public required int ClientId { get; set; }
        
        [Required]
        public required int CategoryId { get; set; }
        
        [Required]
        public required int GoalId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public required decimal Amount { get; set; }

        [Required]
        public required DateTime Date { get; set; }

        public String ? Description { get; set; }


        [ForeignKey("ClientId")]
        public virtual required Client Client { get; set; }
        
        [ForeignKey("ClientId")]
        public virtual required Category Category { get; set; } 
        
        [ForeignKey("ClientId")]
        public virtual required Goal Goal { get; set; }
    }
}
