using DDAC_Project.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DDAC_Project.Models
{
    public class Advisor
    {
        [Key]
        public int AdvisorId { get; set; }

        [Required]
        public required string UserId { get; set; }

        public string ? YearsOfExperience { get; set; }

        public string ? Specialization { get; set; }

        [ForeignKey("UserId")]
        public virtual required DDAC_ProjectUser User { get; set; }
    }
}
