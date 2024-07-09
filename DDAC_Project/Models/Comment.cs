using DDAC_Project.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DDAC_Project.Models
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }

        [Required]
        public required int ClientId { get; set; }      
        
        public int ? AdvisorId { get; set; }  
        

        [Required]
        public required string CommentText { get; set; }

        [Required]
        public required DateTime Date { get; set; }


        [ForeignKey("ClientId")]
        public virtual required Client Client { get; set; } 
        
        [ForeignKey("AdvisorId")]
        public virtual Advisor ? Advisor { get; set; }
    }
}