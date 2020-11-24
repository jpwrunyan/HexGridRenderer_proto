using System.Collections.Generic;

/// <summary>
/// A Pathable node that points to a previous node on a path to its origin.
/// The travel cost is the "distance" from that origin to this node.
/// </summary>
public interface PathableNode {
	/// <summary>
	/// The node prior to this one leading to the origin.
	/// </summary>
	PathableNode prevNode { get; set; }
	/// <summary>
	/// The travel cost to this node form the origin.
	/// </summary>
	float travelCost { get; set; }
}
