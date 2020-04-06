using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MarkLogic.Esri.ArcGISPro.AddIn.Messaging
{
    public class MessageBus
    {
        private Dictionary<Type, ArrayList> _subscriberMap;

        public MessageBus()
        {
            _subscriberMap = new Dictionary<Type, ArrayList>();
        }

        private void Subscribe(Type messageType, Delegate messageHandler)
        {
            Debug.Assert(messageType != null);
            Debug.Assert(messageHandler != null);
            if (!_subscriberMap.TryGetValue(messageType, out ArrayList subscribers))
            {
                subscribers = new ArrayList();
                _subscriberMap.Add(messageType, subscribers);
            }
            subscribers.Add(messageHandler);
        }

        public void Subscribe<T>(Action<T> messageHandler) where T : Message
        {
            if (messageHandler == null) throw new ArgumentNullException("messageHandler");
            Subscribe(typeof(T), messageHandler);
        }

        public void Subscribe<T>(Func<T, Task> messageHandler) where T : Message
        {
            if (messageHandler == null) throw new ArgumentNullException("messageHandler");
            Subscribe(typeof(T), messageHandler);
        }

        public async Task<T> Publish<T>(T message, [CallerMemberName] string callerMember = null) where T : Message
        {
            if (message == null) throw new ArgumentNullException("message");
            if (_subscriberMap.TryGetValue(typeof(T), out ArrayList subscribers))
            {
                Debug.WriteLine($"Message published by {callerMember}: {typeof(T).Name}");
                foreach(var handler in subscribers)
                {
                    var handlerType = handler.GetType().GetGenericTypeDefinition();
                    if (handlerType == typeof(Action<>))
                        (handler as Action<T>).Invoke(message);
                    else if (handlerType == typeof(Func<,>))
                        await (handler as Func<T, Task>).Invoke(message);
                    else
                        throw new InvalidOperationException("Unrecognized message handler.");
                    if (message.Resolved)
                        break;
                }
            }
            return message;
        }
    }
}
