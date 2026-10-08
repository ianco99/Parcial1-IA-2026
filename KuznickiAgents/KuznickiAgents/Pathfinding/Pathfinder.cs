using System.Collections.Generic;

namespace KuznickiAgents.Pathfinding
{
    /// <summary>
    /// La open list es la lista de todos los nodos que abrimos, a la cual nos "expandimos"
    /// </summary>
    /// <typeparam name="TNodeType"></typeparam>
    public abstract class Pathfinder<TNodeType> where TNodeType : IPathfindingNode
    {
        protected ICollection<TNodeType> graph;
        public Pathfinder(ICollection<TNodeType> nodes)
        {
            graph = nodes;
        }
        
        public virtual List<TNodeType> FindPath(TNodeType startNode, TNodeType destinationNode, ICollection<TNodeType> graph)
        {
            //Diccionario de:
            //Llave -> instancia de nodo
            //Valor -> Parent del nodo, costo acumulado hacia ese nodo, heuristica de ese nodo dado
            Dictionary<TNodeType, (TNodeType Parent, int AcumulativeCost, int Heuristic)> nodes =
                new Dictionary<TNodeType, (TNodeType Parent, int AcumulativeCost, int Heuristic)>(graph.Count);

            //Para cada nodo en mi grafo, agregarlo a mi diccionario con valores default y sin parents
            foreach (TNodeType node in graph)
            {
                nodes.Add(node, (default, 0, 0));
            }

            //Creo dos listas de instancias de nodos
            List<TNodeType> openList = new List<TNodeType>();
            HashSet<TNodeType> closedList = new HashSet<TNodeType>();
            HashSet<TNodeType> openSet = new HashSet<TNodeType>();

            //Agrego a mi lista abierta el nodo de inicio del camino
            openList.Add(startNode);
            openSet.Add(startNode);

            while (openList.Count > 0)
            {
                //Me ubico en el primer nodo de mi lista abierta
                TNodeType currentNode = openList[0];
                int currentIndex = 0;
                int currentHeuristic = nodes[currentNode].Heuristic;
                int currentTotalCost = nodes[currentNode].AcumulativeCost + currentHeuristic;

                //Reviso toda mi lista abierta en los nodos que no son el mío
                for (int i = 1; i < openList.Count; i++)
                {
                    (TNodeType Parent, int AcumulativeCost, int Heuristic) candidate = nodes[openList[i]];
                    int candidateTotalCost = candidate.AcumulativeCost + candidate.Heuristic;

                    //Si el nodo que no es el mío tiene menor costo total que el que estoy actualmente usando
                    if (candidateTotalCost < currentTotalCost ||
                        (candidateTotalCost == currentTotalCost && candidate.Heuristic < currentHeuristic))
                    {
                        //mi nodo actual es el encontrado
                        currentNode = openList[i];
                        currentTotalCost = candidateTotalCost;
                        currentHeuristic = candidate.Heuristic;
                        //reviso el resto de la lista
                        currentIndex = i;
                    }
                }

                //borro el nodo ya revisado de mi lista de nodos abiertos
                openList.RemoveAt(currentIndex);
                openSet.Remove(currentNode);
                //lo agrego a mi lista de nodos cerrados
                closedList.Add(currentNode);

                //Si el mejor nodo al que me puedo mover es el destino
                if (NodesEquals(currentNode, destinationNode))
                {
                    //Genera el camino
                    return GeneratePath(startNode, destinationNode);
                }
                
                //Por cada nodo vecino a mi nodo actual (mejor nodo a moverme)
                foreach (TNodeType neighbor in GetNeighbors(currentNode))
                {
                    if (!nodes.ContainsKey(neighbor) || //si el vecino no existe en mi lista de nodos
                        IsBlocked(neighbor) ||          //si el vecino está bloqueado
                        closedList.Contains(neighbor))  //si el vecino ya fue revisado y no es el mejor nodo a moverme
                    {
                        continue;
                    }

                    int tentativeNewAcumulatedCost = 0; 
                    tentativeNewAcumulatedCost += nodes[currentNode].AcumulativeCost;
                    tentativeNewAcumulatedCost += MoveToNeighborCost(currentNode, neighbor);
                    //Genero un costo tentativo de moverme desde mi nodo actual al nodo vecino

                    
                    bool isOpen = openSet.Contains(neighbor);

                    if (!isOpen || tentativeNewAcumulatedCost < nodes[neighbor].AcumulativeCost)
                    {
                        //Abro un nodo vecino desde mi nodo actual, con su costo tentativo y su heuristica
                        nodes[neighbor] = (currentNode, tentativeNewAcumulatedCost, Distance(neighbor, destinationNode));

                        //Si mi lista de nodos abierta no contiene mi vecino
                        if (!isOpen)
                        {
                            //Agrego al vecino a mi lista abierta
                            openList.Add(neighbor);
                            openSet.Add(neighbor);
                        }
                    }
                }
            }

            return null;

            List<TNodeType> GeneratePath(TNodeType startNode, TNodeType goalNode)
            {
                List<TNodeType> path = new List<TNodeType>();
                TNodeType currentNode = goalNode;

                while (!NodesEquals(currentNode, startNode))
                {
                    path.Add(currentNode);
                    currentNode = nodes[currentNode].Parent;
                }

                path.Reverse();
                return path;
            }
        }

        /// <summary>
        /// Gets all neighbors for a given node.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected abstract ICollection<TNodeType> GetNeighbors(TNodeType node);

        /// <summary>
        /// Measures distance between any two given nodes.
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        protected abstract int Distance(TNodeType A, TNodeType B);

        /// <summary>
        /// Measures if two nodes are the same.
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        protected abstract bool NodesEquals(TNodeType A, TNodeType B);

        /// <summary>
        /// Measures cost for moving between two neighboring nodes.
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        protected abstract int MoveToNeighborCost(TNodeType A, TNodeType B);

        /// <summary>
        /// Returns if a given node is intransitable
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected abstract bool IsBlocked(TNodeType node);
    }
}