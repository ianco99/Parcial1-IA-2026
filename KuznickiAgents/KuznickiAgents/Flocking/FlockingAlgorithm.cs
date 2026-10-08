using System;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace KuznickiAgents.Flocking
{
    public class FlockingAlgorithm
    {
        private List<IFlockingAgent> agents = new List<IFlockingAgent>();

        public void RemoveAgent(IFlockingAgent agent)
        {
            agents.Remove(agent);
        }

        public void AddAgent(IFlockingAgent agent)
        {
            agents.Add(agent);
        }

        public Vector3 Alignment(IFlockingAgent agent)
        {
            var neighbors = GetBoidsInsideRadius(agent);
            if (neighbors.Count == 0)
                return Vector3.Zero;

            Vector3 avg = Vector3.Zero;
            foreach (IFlockingAgent b in neighbors)
            {
                avg += Vector3.UnitY;
            }

            return Vector3.Normalize(avg / neighbors.Count);
        }

        public Vector3 Cohesion(IFlockingAgent agent)
        {
            List<IFlockingAgent> neighbors = GetBoidsInsideRadius(agent);
            if (neighbors.Count == 0)
                return Vector3.Zero;

            Vector3 avg = Vector3.Zero;
            foreach (IFlockingAgent b in neighbors)
            {
                avg += (Vector3)b.GetPosition();
            }

            avg /= neighbors.Count;
            return Vector3.Normalize((avg - agent.GetPosition()));
        }

        public Vector3 Separation(IFlockingAgent agent)
        {
            List<IFlockingAgent> neighbors = GetBoidsInsideRadius(agent);
            if (neighbors.Count == 0)
                return Vector3.Zero;

            Vector3 avg = Vector3.Zero;
            foreach (IFlockingAgent a in neighbors)
            {
                avg += ((Vector3)agent.GetPosition() - (Vector3)a.GetPosition());
            }

            return Vector3.Normalize((avg / neighbors.Count));
        }

        public Vector3 Direction(IFlockingAgent agent, Vector3 target)
        {
            return Vector3.Normalize(target - agent.GetPosition());
        }

        public List<IFlockingAgent> GetBoidsInsideRadius(IFlockingAgent agent)
        {
            List<IFlockingAgent> insideRadiusBoids = new List<IFlockingAgent>();

            foreach (IFlockingAgent a in agents)
            {
                if (Vector3.Distance(agent.GetPosition(), a.GetPosition()) < agent.GetDetectionRadious())
                {
                    insideRadiusBoids.Add(a);
                }
            }

            return insideRadiusBoids;
        }
    }
}
