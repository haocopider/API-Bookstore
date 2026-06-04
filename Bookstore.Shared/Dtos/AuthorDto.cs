using Bookstore.Shared.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Bookstore.Shared.Dtos
{
    public class AuthorDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; } = null;
        public List<BookDto> Books { get; set; } = new List<BookDto>();
    }

    public class CreateAuthorDto
    {
        [Required(ErrorMessage = "Tên tác giả không được để trống")]
        public string Name { get; set; } = null!;
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
    }

    public class UpdateAuthorDto
    {
        [Required(ErrorMessage = "Tên tác giả không được để trống")]
        public string Name { get; set; } = null!;
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
