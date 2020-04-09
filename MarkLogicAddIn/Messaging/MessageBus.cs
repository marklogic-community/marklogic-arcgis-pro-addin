using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace MarkLogic.Esri.ArcGISPro.AddIn.Messaging
{
    public class MessageBus
    {
        private class Subscribers
        {
            public ArrayList List { get; } = new ArrayList();

            public Mutex CollectionLock { get; } = new Mutex();
        }

        private Dictionary<Type, Subscribers> _subscriberMap;

        public MessageBus()
        {
            _subscriberMap = new Dictionary<Type, Subscribers>();
        }

        private void Subscribe(Type messageType, Delegate messageHandler)
        {
            Debug.Assert(messageType != null);
            Debug.Assert(messageHandler != null);
            if (!_subscriberMap.TryGetValue(messageType, out Subscribers subscribers))
            {
                subscribers = new Subscribers();
                _subscriberMap.Add(messageType, subscribers);
            }
            subscribers.CollectionLock.WaitOne();
            subscribers.List.Add(messageHandler);
            subscribers.CollectionLock.ReleaseMutex();
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
            if (_subscriberMap.TryGetValue(typeof(T), out Subscribers subscribers))
            {
                Debug.WriteLine($"Message published by {callerMember}: {typeof(T).Name}");

                try
                {
                    subscribers.CollectionLock.WaitOne();
                    foreach (var handler in subscribers.List.ToArray()) // get an array snapshot
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
                finally
                {
                    subscribers.CollectionLock.ReleaseMutex();
                }
            }
            return message;
        }
    }
}
