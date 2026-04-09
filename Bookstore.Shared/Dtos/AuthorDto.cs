using Bookstore.Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bookstore.Shared.Dtos
{
    public class AuthorDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Biography { get; set; }
        public string? ImageUrl { get; set; } = null;
        public ICollection<BookResponseDto> Books { get; set; } = new List<BookResponseDto>();
    }
}
