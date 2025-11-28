// File: Models/User.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShopApi.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string FullName { get; set; }

        [Required, MaxLength(150)]
        public string Email { get; set; }

        public List<Order> Orders { get; set; } = new();
    }
}
