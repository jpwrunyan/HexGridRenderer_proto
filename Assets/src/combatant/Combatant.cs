/// <summary>
/// Base class to represent any entity that participates in a battle.
/// Combatants have a deck and allies in addition to an entity on the battlefield.
/// This means it appears in the active turn order and takes actions.
/// </summary>
public class Combatant {
	BattlefieldEntity character;
	Deck deck;
	System.Collections.Generic.List<int> allies;
}
