public class HexNode : PathableNode {

	public UnityEngine.Vector2Int hexPos;

	public float travelCost { get; set; } = 1;

	public int costModifier = 0;

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
		return "(" + hexPos.x + ", " + hexPos.y + ") cost: " + travelCost + (_prevNode == null ? " origin" : " prev: " + _prevNode.hexPos);
	}
}
