using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace dotnetapp.Models
{
    public class ErrorLog
    {
        [Key]
        public int Id {get; set;}
        
        [Required]
        public string Source {get; set;}=null!;
        
        [Required]
        public string Message {get; set;}=null!;

        [Required]
        public DateTime TimeStamp {get; set;} = DateTime.Now;
    }
}