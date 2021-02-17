using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents an entity on the battlefield, not necessarily a combatant.
/// </summary>
public class BattlefieldEntity {

	public string name;
	public string image;
	//Potentially, only terrain will use this value. It is not meant to be dynamic.
	public uint color = 0xFFFFFF;

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
	/// </summary>
	public int movementModifier {
		get {
			return _movementModifier;
		}
		set {
			_movementModifier = value;
		}
	}

	private bool _blocksMovement;

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

	//These values live here for now. It assumes all entities can potentially be destroyed.
	private int _harassPenalty = 0;

	public int harassPenalty {
		get {
			return _harassPenalty;
		}
		set {
			_harassPenalty = value;
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
