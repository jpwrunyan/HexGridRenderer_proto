using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Card instance class
/// Each card will have a <code>cardMapId</code>, <code>combatantId</code>, and an <code>sourceId</code>.
/// The card map is the source of a card's base stats.
/// The source is the character/item the card is attached to and where the stats modifiers come from.
/// The combatant is the character in battle that uses the card (can also be the source).
/// Card will access the game state to derive actual values with this information.
/// 
/// Cards will need to be generated from card templates at the beginning of battle.
/// Doing anything more abstract will probably be an immense headache.
/// </summary>
public class Card {

	public string cardMapId;
	public int sourceId;

	//This value may not strictly be necessary but here as a convenience.
	public int combatantId;

	//This may not be necessary considering the way I'm restructuring the system to be char-by-char
	//May change, but for now using this to tie to battlefield entity:
	//public BattlefieldEntity entity;

	//For now I will keep CardStats and Card together as one instance. Will revert to using the id format later if I *really* think it's best.

	//----------------------------------------
	// Card values
	//----------------------------------------
	public string title = "Card Title";

	//----------------------------------------
	// Card stats/actions
	//----------------------------------------
	//Attack action values:
	public int attack = 0;
	public int minRange = 0;
	public int maxRange = 0;
	public int radius = 0;
	//Move action values:
	public int move = 0;
	public int moveType = 0; //unused

	public List<CardAction> getCardActions() {
		List<CardAction> cardActions = new List<CardAction>();
		if (move > 0) {
			CardAction cardAction = new CardAction();
			cardAction.type = CombatActionType.MOVE;
			//cardAction.value = move;
			cardAction.maxRange = move;
			cardAction.minRange = 0;
			cardActions.Add(cardAction);
		}
		if (attack > 0) {
			CardAction cardAction = new CardAction();
			cardAction.type = minRange == 1 && maxRange == 1 ? CombatActionType.MELEE_ATTACK : CombatActionType.RANGE_ATTACK;
			cardAction.value = attack;
			cardAction.minRange = minRange;
			cardAction.maxRange = maxRange;
			cardAction.radius = radius;
			cardActions.Add(cardAction);
			/*
			//temporary test using move.
			CardAction cardAction = new CardAction();
			cardAction.type = CombatAction.MOVE;
			//cardAction.value = move;
			cardAction.maxRange = attack;
			cardAction.minRange = 0;
			cardActions.Add(cardAction);
			*/
		}
		if (cardActions.Count == 0) {
			cardActions.Add(new CardAction()); // do nothing
		}
		return cardActions;
	}

	//As opposed to battlefield effects like actually moving/repositioning an entity and taking damage...
	public class CardAction {
		//Default 0 means this is a non-action, regardless of other values.
		public CombatActionType type = CombatActionType.UNKNOWN;

		//Generic value
		public int value = 0; //used by attack type, will be replaced by more complex damage information

		//potentially rename these max/minValue to reflect their generic use.
		public int minRange = 0; //min range, movement minimum (usually 0)
		public int maxRange = 0; //max range, movement (max movement range, not necessarily the movement *cost* though?)
		public int radius = 0; //used by attack type
		public int initiative = 1; //the cumulative initiative value
	}
}
