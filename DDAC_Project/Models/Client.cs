using DDAC_Project.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DDAC_Project.Models
{
    public class Client
    {
        [Key]
        public int ClientId { get; set; }

        [Required]
        public required string UserId { get; set; }

        public int ? AdvisorId { get; set; }

        [ForeignKey("AdvisorId")]  
        public virtual Advisor ? Advisor { get; set; }
        

        [ForeignKey("UserId")]
        public virtual required DDAC_ProjectUser User { get; set; }
    }
}
