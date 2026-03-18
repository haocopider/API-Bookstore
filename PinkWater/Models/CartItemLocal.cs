using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace PinkWater.Models
{
    public class CartItemLocal
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int BookId { get; set; }

        public string Title { get; set; }
        public string CoverImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
