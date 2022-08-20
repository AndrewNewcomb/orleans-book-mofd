using System.ComponentModel.DataAnnotations;

namespace OrleansBook.WebApi.Models
{
    public class RobotsPostRequest
    {
        [Required]
        [StringLength(50)] 
        public string Instruction {get;set;} = "";
    }
}