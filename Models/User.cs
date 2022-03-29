﻿namespace StockBand.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string HashPassword { get; set; }
        public int RoleId { get; set; }
        public virtual Role Role { get; set; }
        public bool Block { get; set; } = false;
    }
}
