using System.Collections.Generic;
/// <summary>
/// Base class to represent any entity that participates in a battle.
/// Combatants have a deck and allies in addition to an entity on the battlefield.
/// This means it appears in the active turn order and takes actions.
/// </summary>
public class Combatant : BattlefieldEntity {
	public Deck deck;
	public List<int> allyIds = new List<int>();
	public bool isAI = false;
	public int initiative = 0;
	
	//This score determines how hard it is to target a combatant.
	//In rare cases it might be negative.
	//It is increased by movement.
	public int evasion = 0;
	//This determines how much evasion is penalized.
	//It is increased mostly by being targeted by melee attacks.
	public int harassment = 0;
	
	override public int movementModifier {
		get {
			return isAlive() ? base.movementModifier : 0;
		}
		set {
			base.movementModifier = value;
		}
	}
}
