// Models/Decorators/ActivityDecorator.cs

namespace SharedActivityManager.Models.Decorators
{
    /// <summary>
    /// Decorator abstract - clasele concrete vor moșteni din aceasta
    /// </summary>
    public abstract class ActivityDecorator : IActivityExtra
    {
        protected readonly IActivityExtra _inner;

        protected ActivityDecorator(IActivityExtra inner)
        {
            _inner = inner;
        }

        // 🔥 PROPRIETATE PUBLICĂ PENTRU ACCES
        public IActivityExtra Inner => _inner;

        public virtual string Name => _inner.Name;
        public virtual bool IsEnabled => _inner.IsEnabled;

        public virtual string GetDescription()
        {
            return _inner.GetDescription();
        }

        public virtual int GetExtraCost()
        {
            return _inner.GetExtraCost();
        }

        public virtual string GetIcon()
        {
            return _inner.GetIcon();
        }

        public virtual async Task ExecuteAsync(Activity activity)
        {
            await _inner.ExecuteAsync(activity);
        }

        public T FindDecorator<T>() where T : ActivityDecorator
        {
            if (this is T decorator)
                return decorator;

            if (_inner is ActivityDecorator innerDecorator)
                return innerDecorator.FindDecorator<T>();

            return null;
        }
    }
}