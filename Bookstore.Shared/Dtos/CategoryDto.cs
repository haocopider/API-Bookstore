using Bookstore.Shared.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Bookstore.Shared.Dtos
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
    }

    public class CreateCategoryDto
    {
        [Required(ErrorMessage = "Tên thể loại không được để trống")]
        public string Name { get; set; } = null!;
        public string? Slug { get; set; }
        public int? ParentId { get; set; }
    }

    public class UpdateCategoryDto
    {
        [Required(ErrorMessage = "Tên thể loại không được để trống")]
        public string Name { get; set; } = null!;
        public string? Slug { get; set; }
        public int? ParentId { get; set; }
    }
}
