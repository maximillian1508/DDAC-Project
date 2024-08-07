﻿using DDAC_Project.Areas.Identity.Data;
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
        
        public int ? CategoryId { get; set; }
        
        public int ? GoalId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Required(ErrorMessage = "Amount is required")]
        [Range(typeof(decimal), "0.01", "79228162514264337593543950335", ErrorMessage = "Amount must be more than 0", ParseLimitsInInvariantCulture = true)]
        public required decimal Amount { get; set; }

        [Required]
        public required DateTime Date { get; set; }

        public String ? Description { get; set; }


        [ForeignKey("ClientId")]
        public virtual Client? Client { get; set; }
        
        [ForeignKey("CategoryId")]
        public virtual Category ? Category { get; set; } 
        
        [ForeignKey("GoalId")]
        public virtual Goal ? Goal { get; set; }
    }
}
