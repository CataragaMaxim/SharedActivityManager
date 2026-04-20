using SharedActivityManager.Data;
using SharedActivityManager.Models;

namespace SharedActivityManager.Models
{
    public class ActivityCategory : ActivityComponent
    {
        private readonly Category _category;
        private readonly List<ActivityComponent> _children = new();

        public ActivityCategory(Category category)
        {
            _category = category;
        }

        public override string Id => _category.Id.ToString();
        public override string Name => _category.Name;
        public override string Description => $"Category: {_category.Name}";
        public override int CategoryId => _category.Id;

        // 🔥 PROPRIETATE NOUĂ
        public override bool IsComposite => true;

        public override void Add(ActivityComponent component)
        {
            _children.Add(component);
        }

        public override void Remove(ActivityComponent component)
        {
            _children.Remove(component);
        }

        public override ActivityComponent GetChild(int index)
        {
            if (index >= 0 && index < _children.Count)
                return _children[index];
            return null;
        }

        public override IReadOnlyList<ActivityComponent> GetChildren()
        {
            return _children.AsReadOnly();
        }

        public override int GetTotalActivitiesCount()
        {
            return _children.Sum(child => child.GetTotalActivitiesCount());
        }

        public override List<Activity> GetAllActivities()
        {
            var allActivities = new List<Activity>();
            foreach (var child in _children)
            {
                allActivities.AddRange(child.GetAllActivities());
            }
            return allActivities;
        }

        public override int GetCompletedCount()
        {
            return _children.Sum(child => child.GetCompletedCount());
        }

        public override async Task SaveAsync(ActivityDataBase database)
        {
            foreach (var child in _children)
            {
                await child.SaveAsync(database);
            }
        }

        public Category GetCategory() => _category;

        // ========== METODE RECURSIVE ==========

        public ActivityComponent FindComponentById(string id)
        {
            if (Id == id) return this;

            foreach (var child in _children)
            {
                if (child.Id == id) return child;

                if (child is ActivityCategory category)
                {
                    var found = category.FindComponentById(id);
                    if (found != null) return found;
                }
            }
            return null;
        }

        public ActivityLeaf FindActivityById(int activityId)
        {
            foreach (var child in _children)
            {
                if (child is ActivityLeaf leaf && leaf.GetActivity().Id == activityId)
                    return leaf;

                if (child is ActivityCategory category)
                {
                    var found = category.FindActivityById(activityId);
                    if (found != null) return found;
                }
            }
            return null;
        }

        public bool RemoveActivityById(int activityId)
        {
            for (int i = 0; i < _children.Count; i++)
            {
                if (_children[i] is ActivityLeaf leaf && leaf.GetActivity().Id == activityId)
                {
                    _children.RemoveAt(i);
                    return true;
                }

                if (_children[i] is ActivityCategory category)
                {
                    if (category.RemoveActivityById(activityId))
                        return true;
                }
            }
            return false;
        }

        public List<int> GetAllActivityIds()
        {
            var ids = new List<int>();
            foreach (var child in _children)
            {
                if (child is ActivityLeaf leaf)
                    ids.Add(leaf.GetActivity().Id);

                if (child is ActivityCategory category)
                    ids.AddRange(category.GetAllActivityIds());
            }
            return ids;
        }

        public List<Activity> GetIncompleteActivities()
        {
            var incomplete = new List<Activity>();
            foreach (var child in _children)
            {
                if (child is ActivityLeaf leaf && !leaf.GetActivity().IsCompleted)
                    incomplete.Add(leaf.GetActivity());

                if (child is ActivityCategory category)
                    incomplete.AddRange(category.GetIncompleteActivities());
            }
            return incomplete;
        }

        public List<Activity> GetCompletedActivities()
        {
            var completed = new List<Activity>();
            foreach (var child in _children)
            {
                if (child is ActivityLeaf leaf && leaf.GetActivity().IsCompleted)
                    completed.Add(leaf.GetActivity());

                if (child is ActivityCategory category)
                    completed.AddRange(category.GetCompletedActivities());
            }
            return completed;
        }

        public int GetMaxDepth(int currentDepth = 0)
        {
            if (!_children.Any()) return currentDepth;

            int maxChildDepth = currentDepth;
            foreach (var child in _children)
            {
                if (child is ActivityCategory category)
                {
                    int childDepth = category.GetMaxDepth(currentDepth + 1);
                    if (childDepth > maxChildDepth) maxChildDepth = childDepth;
                }
            }
            return maxChildDepth;
        }

        public void ForEach(Action<ActivityComponent> action)
        {
            action(this);
            foreach (var child in _children)
            {
                if (child is ActivityCategory category)
                    category.ForEach(action);
                else
                    action(child);
            }
        }

        public async Task ForEachAsync(Func<ActivityComponent, Task> action)
        {
            await action(this);
            foreach (var child in _children)
            {
                if (child is ActivityCategory category)
                    await category.ForEachAsync(action);
                else
                    await action(child);
            }
        }
    }
}