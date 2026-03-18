using System;
using System.Collections.Generic;
using System.Text;

namespace Bookstore.Shared.Dtos
{
    public class BookDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? AuthorName { get; set; }
        public string? Description { get; set; }
        public decimal MinPrice { get; set; } 
        public string? CoverImageUrl { get; set; }

        public List<string> Categories { get; set; } = new List<string>();
    }
}
