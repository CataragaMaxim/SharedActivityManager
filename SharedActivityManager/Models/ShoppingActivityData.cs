using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SharedActivityManager.Models
{
    public class ShoppingActivityData
    {
        public List<ShoppingItem> Items { get; set; } = new();
        public decimal Budget { get; set; } = 0;
        public string Store { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public DateTime? DeliveryDate { get; set; }

        public decimal GetTotalSpent() => Items.Where(i => i.IsPurchased).Sum(i => i.Price * i.Quantity);
        public decimal GetRemainingBudget() => Budget - GetTotalSpent();
        public int GetItemsLeftCount() => Items.Count(i => !i.IsPurchased);

        public string Serialize() => JsonSerializer.Serialize(this);
        public static ShoppingActivityData Deserialize(string json) => JsonSerializer.Deserialize<ShoppingActivityData>(json) ?? new ShoppingActivityData();
    }
}
