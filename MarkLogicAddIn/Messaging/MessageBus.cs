using System;
using System.Collections;
using System.Collections.Generic;

namespace MarkLogic.Esri.ArcGISPro.AddIn.Messaging
{
    public class MessageBus
    {
        private Dictionary<Type, ArrayList> _subscriberMap;

        public MessageBus()
        {
            _subscriberMap = new Dictionary<Type, ArrayList>();
        }

        public MessageBus Subscribe<T>(Action<T> messageHandler) where T : Message
        {
            if (messageHandler == null) throw new ArgumentNullException("messageHandler");
            var messageType = typeof(T);
            if (!_subscriberMap.TryGetValue(messageType, out ArrayList subscribers))
            {
                subscribers = new ArrayList();
                _subscriberMap.Add(messageType, subscribers);
            }
            subscribers.Add(messageHandler);
            return this;   
        }

        public void Publish<T>(T message) where T : Message
        {
            if (message == null) throw new ArgumentNullException("message");
            if (_subscriberMap.TryGetValue(message.GetType(), out ArrayList subscribers))
            {
                foreach(Action<T> messageHandler in subscribers)
                {
                    messageHandler.Invoke(message);
                    if (message.Resolved)
                        break;
                }
            }
        }
    }
}
