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
	//This variable may not be necessary. Can be inferred from BattleState::combatantIdTurnOrder
	public bool turnDone = false;
}
