using Bookstore.Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bookstore.Shared.Dtos
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
    }
}
