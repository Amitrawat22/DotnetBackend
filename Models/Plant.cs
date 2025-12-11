using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace dotnetapp.Models
{
    public class Plant
    {
        [Key]
        public int PlantId { get; set; } = default!;

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string Category { get; set; } = null!;

        [Required]
        [Range(1, 100000, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; } = default!;

        public string? Tips { get; set; }

        public string? PlantImage { get; set; }

    }
}