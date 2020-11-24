using System.Collections.Generic;

public class NodePathMap {
	
	protected Dictionary<PathableNode, HashSet<PathableNode>> nodeMap;

	public NodePathMap() {
		nodeMap = new Dictionary<PathableNode, HashSet<PathableNode>>();
	}

	public PathableNode addNode(PathableNode node, HashSet<PathableNode> adjacentNodes) {
		nodeMap[node] = adjacentNodes;
		return node;
	}

	public void setOrigin(PathableNode origin) {
		PathableNode currentNode = origin;
		
		//Assume origin is in the nodeMap...

		IEnumerable<PathableNode> keys = nodeMap.Keys;
		List<PathableNode> priorityQueue = new List<PathableNode>();
		
		foreach (PathableNode node in keys) {
			node.prevNode = null;
			if (node == currentNode) {
				currentNode.travelCost = 0;
			} else {
				node.travelCost = float.MaxValue;
				priorityQueue.Add(node);
			}
		}

		while (priorityQueue.Count > 0) {
			HashSet<PathableNode> adjNodes = nodeMap[currentNode];
			foreach (PathableNode adjNode in adjNodes) {
				float segmentCost = getSegmentCost(currentNode, adjNode);
				float travelCost = currentNode.travelCost + segmentCost;
				if (travelCost < adjNode.travelCost) {
					adjNode.travelCost = travelCost;
					adjNode.prevNode = currentNode;
				}
			}
			priorityQueue.Sort((PathableNode a, PathableNode b) => a.travelCost.CompareTo(b.travelCost));
			currentNode = priorityQueue[0];
			priorityQueue.RemoveAt(0);
		}
	}

	/// <summary>
	/// To be overridden.
	/// </summary>
	/// <param name="start"></param>
	/// <param name="finish"></param>
	/// <returns></returns>
	virtual protected float getSegmentCost(PathableNode start, PathableNode finish) {
		return 1;
	}
}
