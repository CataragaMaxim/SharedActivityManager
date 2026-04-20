using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedActivityManager.Models
{
    public class ShoppingItem
    {
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public decimal Price { get; set; } = 0;
        public bool IsPurchased { get; set; } = false;
        public string Category { get; set; } = string.Empty;
    }
}
