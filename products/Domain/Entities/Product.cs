using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    public class Product
    {
        public Guid Id { get; set; }

        public String Title { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public String Description { get; set; }

        public int Version { get; set; }
    }
}
