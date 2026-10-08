using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ianco99.ToolBox.DataFlow;
using ianco99.ToolBox.Pool;
using ianco99.ToolBox.Services;

namespace ianco99.ToolBox.Events
{
    public sealed class EventBus : IService, ITickable
    {
        public delegate void EventCallback<EventType>(in EventType callback) where EventType : struct, IEvent;
        public bool IsPersistance => false;

        private readonly ConcurrentPool eventPool = new ConcurrentPool();
        private readonly ConcurrentDictionary<Type, List<Delegate>> subscribers = new ConcurrentDictionary<Type, List<Delegate>>();
        private readonly ConcurrentDictionary<Type, List<Delegate>> mainThreadOnlySubs = new ConcurrentDictionary<Type, List<Delegate>>();
        private readonly ConcurrentQueue<Action> mainThreadQueue = new ConcurrentQueue<Action>();

        public void Subscribe<EventType>(EventCallback<EventType> callback) where EventType : struct, IEvent
        {
            Type eventType = typeof(EventType);

            while (!subscribers.ContainsKey(eventType))
                subscribers.TryAdd(eventType, new List<Delegate>());

            List<Delegate> delegates = subscribers[eventType];

            lock (delegates)
                delegates.Add(callback);
        }

        public void SubscribeOnlyMainThread<EventType>(EventCallback<EventType> callback) where EventType : struct, IEvent
        {
            Type eventType = typeof(EventType);
            mainThreadOnlySubs.GetOrAdd(eventType, _ => new List<Delegate>()).Add(callback);
        }

        public EventCallback<EventType> SubscribeAndReturn<EventType>(EventCallback<EventType> callback) where EventType : struct, IEvent
        {
            Subscribe(callback);
            return callback;
        }

        public EventCallback<EventType> SubscribeAndReturnOnlyMainThread<EventType>(EventCallback<EventType> callback) where EventType : struct, IEvent
        {
            SubscribeOnlyMainThread(callback);
            return callback;
        }

        public void UnSubscribe<EventType>(EventCallback<EventType> callback) where EventType : struct, IEvent
        {
            Type eventType = typeof(EventType);
            if (subscribers.TryGetValue(eventType, out List<Delegate> subscriptions))
            {
                lock (subscriptions)
                    subscriptions.Remove(callback);
            }
        }

        public void UnsubscribeOnlyMainThread<EventType>(EventCallback<EventType> callback) where EventType : struct, IEvent
        {
            Type eventType = typeof(EventType);
            if (mainThreadOnlySubs.TryGetValue(eventType, out List<Delegate> subscriptions))
            {
                subscriptions.Remove(callback);
            }
        }

        public void Raise<EventType>(params object[] parameters) where EventType : struct, IEvent
        {
            Type eventType = typeof(EventType);
            EventType raisingEvent = eventPool.Get<EventType>(parameters);

            if (subscribers.TryGetValue(eventType, out List<Delegate> subscriptions))
            {
                Delegate[] snapshot;
                lock (subscriptions)
                    snapshot = subscriptions.ToArray();

                foreach (Delegate callback in snapshot)
                {
                    ((EventCallback<EventType>)callback)?.Invoke(raisingEvent);
                }
            }

            if (!mainThreadOnlySubs.TryGetValue(eventType, out List<Delegate> mainThreadSubscriptions) || mainThreadSubscriptions.Count == 0)
            {
                eventPool.Release(raisingEvent);
                return;
            }

            mainThreadQueue.Enqueue(() =>
            {
                RaiseMainThreadCallbacks(raisingEvent);
                eventPool.Release(raisingEvent);
            });
        }

        public void Clear()
        {
            subscribers.Clear();
            mainThreadOnlySubs.Clear();
        }

        public void Tick(float deltaTime)
        {
            while (mainThreadQueue.TryDequeue(out Action dispatch))
                dispatch();
        }

        private void RaiseMainThreadCallbacks<EventType>(EventType raisingEvent) where EventType : struct, IEvent
        {
            if (!mainThreadOnlySubs.TryGetValue(typeof(EventType), out List<Delegate> subscriptions))
                return;

            foreach (Delegate callback in subscriptions.ToArray())
            {
                ((EventCallback<EventType>)callback)?.Invoke(raisingEvent);
            }
        }
    }
}