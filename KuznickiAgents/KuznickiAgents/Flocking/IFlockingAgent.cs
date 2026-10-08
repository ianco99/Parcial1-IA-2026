using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace KuznickiAgents.Flocking
{
    public interface IFlockingAgent
    {
        public Vector3 GetPosition();
        public float GetDetectionRadious();
    }
}
