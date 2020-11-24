public class HexNode : PathableNode {

	public UnityEngine.Vector2Int hexPos;

	private float _travelCost = 1;
	
	public float travelCost {
		get {
			return _travelCost;
		}
		set {
			_travelCost = value;
		}
	}

	private HexNode _prevNode;

	public PathableNode prevNode {
		get {
			return _prevNode;
		}
		set {
			_prevNode = (HexNode) value;
		}
	}

	override public string ToString() {
		return "(" + hexPos.x + ", " + hexPos.y + ") cost: " + _travelCost + (_prevNode == null ? " origin" : " prev: " + _prevNode.hexPos);
	}
}
