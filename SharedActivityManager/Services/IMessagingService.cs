// Services/IMessagingService.cs
using SharedActivityManager.Models;

namespace SharedActivityManager.Services
{
    public interface IMessagingService
    {
        void Send<TMessage>(TMessage message) where TMessage : class;
        void Subscribe<TMessage>(object subscriber, Action<TMessage> handler) where TMessage : class;
        void Unsubscribe<TMessage>(object subscriber) where TMessage : class;
    }

    public class MessagingService : IMessagingService
    {
        private readonly Dictionary<Type, List<WeakReference>> _subscribers = new();

        public void Send<TMessage>(TMessage message) where TMessage : class
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            var messageType = typeof(TMessage);
            if (!_subscribers.ContainsKey(messageType)) return;

            var deadEntries = new List<WeakReference>();
            foreach (var weakRef in _subscribers[messageType])
            {
                if (weakRef.Target is Action<TMessage> handler)
                {
                    handler(message);
                }
                else
                {
                    deadEntries.Add(weakRef);
                }
            }

            // Curăță intrările moarte
            foreach (var dead in deadEntries)
            {
                _subscribers[messageType].Remove(dead);
            }
        }

        public void Subscribe<TMessage>(object subscriber, Action<TMessage> handler) where TMessage : class
        {
            var messageType = typeof(TMessage);
            if (!_subscribers.ContainsKey(messageType))
            {
                _subscribers[messageType] = new List<WeakReference>();
            }

            _subscribers[messageType].Add(new WeakReference(handler));
        }

        public void Unsubscribe<TMessage>(object subscriber) where TMessage : class
        {
            var messageType = typeof(TMessage);
            if (_subscribers.ContainsKey(messageType))
            {
                _subscribers.Remove(messageType);
            }
        }
    }

    // Mesaje
    public class ActivitiesChangedMessage
    {
        public string Action { get; set; } // "Added", "Updated", "Deleted", "Copied", "DeletedAll", "ResetEverything", "Imported"
        public Activity Activity { get; set; }
        public int ActivityCount { get; set; }
    }
}