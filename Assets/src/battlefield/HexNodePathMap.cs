using System.Collections.Generic;
using UnityEngine;

public class HexNodePathMap : NodePathMap {
	private Dictionary<Vector2Int, HexNode> hexNodes;
	private Dictionary<Vector2Int, int> hexCosts;

	public HexNodePathMap(HexGrid hexGrid, BattleState battleState, int movementMode=0) : base() {
		//TODO: hex costs will vary depending on movementMode
		hexCosts = new Dictionary<Vector2Int, int>();
		foreach (BattlefieldEntity entity in battleState.battlefieldEntities) {
			if (hexCosts.ContainsKey(entity.pos)) {
				hexCosts[entity.pos] += entity.movementModifier;
			} else {
				hexCosts[entity.pos] = entity.movementModifier;
			}
		}

		hexNodes = new Dictionary<Vector2Int, HexNode>();
		foreach (Vector2Int hexPos in hexGrid.hexPos) {
			HexNode hexNode;
			if (hexNodes.ContainsKey(hexPos)) {
				hexNode = hexNodes[hexPos];
			} else {
				hexNodes[hexPos] = hexNode = new HexNode();
				hexNode.hexPos = hexPos;
			}

			HashSet<PathableNode> adjHexNodes = new HashSet<PathableNode>();
			foreach (Vector2Int dir in HexGrid.HEX_ROTATIONS) {
				Vector2Int adjHexPos = new Vector2Int(hexPos.x + dir.x, hexPos.y + dir.y);
				HexNode adjHexNode;
				if (hexNodes.ContainsKey(adjHexPos)) {
					adjHexNode = hexNodes[adjHexPos];
				} else if (hexGrid.indexOfPos(adjHexPos) != -1) {
					hexNodes[adjHexPos] = adjHexNode = new HexNode();
					adjHexNode.hexPos = adjHexPos;
				} else {
					continue;
				}
				adjHexNodes.Add(adjHexNode);
			}
			//Debug.Log("adjHexNodes: " + adjHexNodes.Count);
			addNode(hexNode, adjHexNodes);
		}

		//For now we treat terrain statically. But in the future will need a way to update it.
		//foreach (Vector2Int hex in blockedHexes) {
		//	this.blockedHexes.Add(hex);
		//}

		//Debug.Log("finished hex node path map nodes: " + hexNodes.Keys.Count);
	}

	public HexNode getHexNodeAt(Vector2Int hexPos) {
		if (hexNodes.ContainsKey(hexPos)) {
			return hexNodes[hexPos];
		} else {
			return null;
		}
	}

	public HexNode getClosestHexNodeTo(Vector2Int hexPos, int travelCostLimit) {
		HexNode dest = getHexNodeAt(hexPos);
		//Remember: if the travel cost is infinite due to being  blocked terrain, its prevNode will also be null.
		//See NodePathMap::getSegmentCost for why float.Maxvalue becomes infinity (resulting in essentially orphaned nodes).
		while (dest != null && dest.travelCost > travelCostLimit) {
			dest = (HexNode)dest.prevNode;
		}
		return dest;
	}

	public void setOrigin(Vector2Int hexPos) {
		if (hexNodes.ContainsKey(hexPos)) {
			setOrigin(hexNodes[hexPos]);
		} else {
			throw new System.Exception("Invalid position. Does not exist in node map.");
		}
	}

	override protected float getSegmentCost(PathableNode start, PathableNode finish) {
		Vector2Int hexPos = ((HexNode)finish).hexPos;
		if (hexCosts.ContainsKey(hexPos)) {
			return base.getSegmentCost(start, finish) + hexCosts[hexPos];
		} else {
			return base.getSegmentCost(start, finish);
		}
	}
}