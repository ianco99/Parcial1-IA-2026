using System;
using ianco99.ToolBox.DataFlow;
using UnityEngine;

namespace Parcial.View.Scene
{
    public class ViewComponent : MonoBehaviour, IInitable, ITickable, IDisposable
    {
        public virtual void Init()
        {
        }

        public virtual void Init(params object[] parameters)
        {
            Init();
        }

        public virtual void LateInit()
        {
        }

        public virtual void Tick(float deltaTime)
        {
        }

        public virtual void Dispose()
        {
        }
    }
}