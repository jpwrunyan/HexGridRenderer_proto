using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents an entity on the battlefield, not necessarily a combatant.
/// </summary>
public class BattlefieldEntity {

	public string name;
	public string image;

	//TODO: these values belong on a terrain subclass.
	public uint color = 0xFFFFFF;
	public bool erect = true;

	private int xPos;
	private int yPos;

	//The position in the arena
	public Vector2Int pos {
		get => new Vector2Int(xPos, yPos);
		set {
			xPos = value.x;
			yPos = value.y;
		}
	}
	
	private int _movementModifier = 0;

	/// <summary>
	/// This is the movement penalty to move through a hex occupied by this combatant.
	/// Needs work.
	/// Temporarily put isAlive() check here for movement modifier.
	/// </summary>
	virtual public int movementModifier {
		get {
			return _movementModifier;
		}
		set {
			_movementModifier = value;
		}
	}

	private bool _blocksMovement;

	/// <summary>
	/// Does this entity block normal movement? This will be replaced later by "elevation" types that block surface and/or air movement.
	/// Alternatively, there may be terrain types like "pit", "low wall", "high wall" added to Terrain Entities.
	/// </summary>
	public bool blocksMovement {
		get {
			return _blocksMovement;
		}
		set {
			_blocksMovement = value;
		}
	}

	private bool _blocksVision;

	public bool blocksVision {
		get {
			return _blocksVision;
		}
		set {
			_blocksVision = value;
		}
	}

	private int _damage = 0;

	public int damage {
		get {
			return _damage;
		}
		set {
			_damage = value;
		}
	}

	private int _health;

	public int health {
		get {
			return _health;
		}
		set {
			_health = value;
		}
	}

	public bool isAlive() {
		return damage < health;
	}
}
