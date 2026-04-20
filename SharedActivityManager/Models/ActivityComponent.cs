using SharedActivityManager.Data;
using SharedActivityManager.Models;

namespace SharedActivityManager.Models
{
    public abstract class ActivityComponent
    {
        public abstract string Id { get; }
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract int CategoryId { get; }

        // 🔥 PROPRIETĂȚI NOI PENTRU UI
        public virtual bool IsComposite => false;
        public virtual bool IsExpanded { get; set; } = false;

        // Proprietăți pentru binding
        public virtual int TotalActivitiesCount => GetTotalActivitiesCount();
        public virtual int CompletedCount => GetCompletedCount();
        public virtual double CompletionPercentage => TotalActivitiesCount > 0 ? (double)CompletedCount / TotalActivitiesCount * 100 : 0;

        // Operații principale
        public abstract int GetTotalActivitiesCount();
        public abstract List<Activity> GetAllActivities();
        public abstract int GetCompletedCount();
        public abstract Task SaveAsync(ActivityDataBase database);

        // Management copii (pentru Composite)
        public virtual void Add(ActivityComponent component) => throw new NotImplementedException();
        public virtual void Remove(ActivityComponent component) => throw new NotImplementedException();
        public virtual ActivityComponent GetChild(int index) => throw new NotImplementedException();
        public virtual IReadOnlyList<ActivityComponent> GetChildren() => new List<ActivityComponent>();
    }
}