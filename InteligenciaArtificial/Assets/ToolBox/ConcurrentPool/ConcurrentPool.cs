using System;
using System.Collections;
using System.Collections.Concurrent;

namespace ianco99.ToolBox.Pool
{
    public sealed class ConcurrentPool
    {
        private readonly ConcurrentDictionary<Type, ConcurrentStack<IResettable>> concurrentPool =
            new ConcurrentDictionary<Type, ConcurrentStack<IResettable>>();

        public ResettableType Get<ResettableType>(params object[] parameters) where ResettableType : IResettable 
        {
            Type ressetteableType = typeof(ResettableType);
            ConcurrentStack<IResettable> stack =
                concurrentPool.GetOrAdd(ressetteableType, _ => new ConcurrentStack<IResettable>());

            ResettableType value;
            if (stack.TryPop(out IResettable resettable))
            {
                value = (ResettableType)resettable;
            }
            else
            {
                value = (ResettableType)Activator.CreateInstance(ressetteableType);
            }

            value.Assign(parameters);
            return value;
        }

        public void Release<ResettableType>(ResettableType ressettable) where ResettableType : IResettable 
        {
            ressettable.Reset();
            concurrentPool[typeof(ResettableType)].Push(ressettable);
        }
    }
}
