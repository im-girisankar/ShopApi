// File: Models/Order.cs
using System;
using System.Collections.Generic;

namespace ShopApi.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // FK to user
        public int UserId { get; set; }
        public User? User { get; set; }

        public List<OrderItem> Items { get; set; } = new();
    }
}
