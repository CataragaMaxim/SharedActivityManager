using SharedActivityManager.Enums;
using SharedActivityManager.Models;

namespace SharedActivityManager.Factories
{
    public class ShoppingActivityCreator : ActivityCreator
    {
        public override Activity CreateActivity()
        {
            return new Activity
            {
                TypeId = ActivityType.Personal,
                SpecificDataJson = new ShoppingActivityData().Serialize()
            };
        }

        protected override void ConfigureSpecificProperties(Activity activity, Dictionary<string, object> additionalParams)
        {
            var shoppingData = ShoppingActivityData.Deserialize(activity.SpecificDataJson);

            shoppingData.Budget = GetParamValue(additionalParams, "Budget", 0m);
            shoppingData.Store = GetParamValue(additionalParams, "Store", "");
            shoppingData.DeliveryAddress = GetParamValue(additionalParams, "DeliveryAddress", "");

            activity.SpecificDataJson = shoppingData.Serialize();
        }

        // ========== METODE SPECIFICE SHOPPING ==========

        public ShoppingActivityData GetShoppingData(Activity activity)
        {
            return ShoppingActivityData.Deserialize(activity.SpecificDataJson);
        }

        public void SaveShoppingData(Activity activity, ShoppingActivityData data)
        {
            activity.SpecificDataJson = data.Serialize();
        }

        public void AddItem(Activity activity, string itemName, int quantity = 1, decimal price = 0, string category = "")
        {
            var data = GetShoppingData(activity);
            data.Items.Add(new ShoppingItem
            {
                Name = itemName,
                Quantity = quantity,
                Price = price,
                Category = category,
                IsPurchased = false
            });
            SaveShoppingData(activity, data);
        }

        public void RemoveItem(Activity activity, int itemIndex)
        {
            var data = GetShoppingData(activity);
            if (itemIndex >= 0 && itemIndex < data.Items.Count)
            {
                data.Items.RemoveAt(itemIndex);
                SaveShoppingData(activity, data);
            }
        }

        public async Task MarkItemAsPurchasedAsync(Activity activity, int itemIndex)
        {
            var data = GetShoppingData(activity);
            if (itemIndex >= 0 && itemIndex < data.Items.Count)
            {
                data.Items[itemIndex].IsPurchased = true;
                SaveShoppingData(activity, data);

                // Verifică dacă toate item-urile sunt achiziționate
                if (data.GetItemsLeftCount() == 0)
                {
                    activity.IsCompleted = true;
                }
            }
            await Task.CompletedTask;
        }

        public void UpdateItemQuantity(Activity activity, int itemIndex, int newQuantity)
        {
            var data = GetShoppingData(activity);
            if (itemIndex >= 0 && itemIndex < data.Items.Count && newQuantity > 0)
            {
                data.Items[itemIndex].Quantity = newQuantity;
                SaveShoppingData(activity, data);
            }
        }

        public decimal GetTotalSpent(Activity activity)
        {
            return GetShoppingData(activity).GetTotalSpent();
        }

        public decimal GetRemainingBudget(Activity activity)
        {
            return GetShoppingData(activity).GetRemainingBudget();
        }

        public int GetItemsLeftCount(Activity activity)
        {
            return GetShoppingData(activity).GetItemsLeftCount();
        }

        public List<ShoppingItem> GetItemsByCategory(Activity activity, string category)
        {
            return GetShoppingData(activity).Items.Where(i => i.Category == category).ToList();
        }

        public List<string> GetAllCategories(Activity activity)
        {
            return GetShoppingData(activity).Items.Select(i => i.Category).Where(c => !string.IsNullOrEmpty(c)).Distinct().ToList();
        }
    }
}