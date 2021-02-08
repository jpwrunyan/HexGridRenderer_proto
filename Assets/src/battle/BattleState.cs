using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains a mutable battle state.
/// Also processes changes to battle state.
/// In the future may split this logic between a battle state and a battle state manager.
/// </summary>
public class BattleState {

	public HexGrid hexGrid;

	//All entities in this combat, active or otherwise.
	//The index of this list is used as the id of the combatant.
	//This list should remain static.
	public List<BattlefieldEntity> battlefieldEntities;

	//A dictionary tracking a list of cards associated with each combatant id.
	//TODO: replace the List<Card> with a Deck which manages draw, discard, and in-hand data.
	//Doubles as a means to keep track of active combatants.
	public Dictionary<int, Deck> combatantIdDecks;

	
	//The current round of combat.
	//Combat begins at round 0. 
	public int round = 0;
	//The current turn in this round of combat.
	//Each round starts at turn 0. But initialization increments it by 1.
	public int turn = 0;

	//A counter for explicit number of characters who have actively passed/folded. Not sure how to get around this yet...
	//But running out of cards is distinct from actually passing...
	public int passCount = 0;

	//Pending actions that must be resolved during the current turn.
	public List<Card.CardAction> pendingActions = new List<Card.CardAction>();

	//The index of the pending action currently resolving.
	public int actionPhase = 0;

	//A list of combatant ids and the order in which they take their turn for the current round.
	//The id corresponds to the index of the battlefieldEntities list.
	//The turn value determines which combatant in the list is currently taking their turn.
	public List<int> combatantIdTurnOrder;

	private InputSource inputSource;

	public void setInputSource(InputSource inputSource, bool start=true) {
		this.inputSource = inputSource;
		if (start) {
			inputSource.processNextAction(this);
		}
	}

	/// <summary>
	/// Called by the input module.
	/// </summary>
	/// <param name="index">selected index</param>
	public void processActionInput(int index) {
		processActionInput(new Vector3Int(index, 0, 0));
	}

	/// <summary>
	/// Called by the input module.
	/// </summary>
	/// <param name="p">selected point</param>
	public void processActionInput(Vector2Int p) {
		//Assumes we're moving the current turn combatant.
		Debug.Log("processActionInput: " + getCurrentCombatant().name + p.x + ", " + p.y + " z set to: " + getCurrentCombatantId());
		processActionInput(new Vector3Int(p.x, p.y, getCurrentCombatantId()));
	}

	/// <summary>
	/// Input is recieved from the input processor (whether it's manual UI or ai logic).
	/// In the future, I may abstract the input processor and make it a held reference for battle state...
	/// </summary>
	/// <param name="input"></param>
	public void processActionInput(Vector3Int input) {
		switch (getActionState()) {
			case CombatAction.SELECT_ACTION: {
				//Card index is the input.x value.
				//change the deck accordingly.
				Debug.Log("process select action");
				if (input.x == -1) {
					//Player folded.
					pendingActions = new List<Card.CardAction>();
					//Discard all cards now?
					while (getCurrentCombatantDeck().getHand().Count > 0) {
						getCurrentCombatantDeck().discard(getCurrentCombatantDeck().getHand()[0]);
					}
				} else {
					Card card = getCurrentCombatantDeck().getHand()[input.x];
					pendingActions = card.getCardActions();
					//May possibly handle discards via CombatEffect
					getCurrentCombatantDeck().discard(card);
				}
				actionPhase = 0;
				break;
			}
			case CombatAction.MOVE: {
				//In the future I may require that the actual node data is also provided so validation can occur...
				//I may also need to know the subject of the action in case it isn't always the current combatant.
				//This will require updating the input parameter to a more complicated structure.
				//But otherwise I won't have a way to keep a history of "inputs".
				//Alternatively, just make sure you validate input via the UI.
				//The "z" value corresponds to the entity id.
				//Debug.Log("process move action");

				//The "clicked" pos could be anywhere. It's a goal. Find the destination along the path that can actually be moved to.
				HexNodePathMap pathMap = new HexNodePathMap(hexGrid, getBlockedHexes());
				pathMap.setOrigin(getCurrentCombatant().pos);
				//Can only move within max range of the action.
				HexNode dest = pathMap.getClosestHexNodeTo((Vector2Int)input, getCurrentAction().maxRange);

				if (dest != null) {
					battlefieldEntities[input.z].pos = dest.hexPos;
				} else {
					Debug.Log("Invalid input. Do not move.");
				}

				actionPhase++;
				break;
			}
			case CombatAction.MELEE_ATTACK:
			case CombatAction.RANGE_ATTACK: {
				Debug.Log("process attack");
				if (isTargetValid((Vector2Int)input)) {
					//Valid input.
					applyCombatEffects(determineCombatEffects((Vector2Int)input));
				}
				actionPhase++;
				break;
			}
		}

		//Debug.Log("Process Action phase: " + actionPhase + " actions count: " + pendingActions.Count);

		//Check end of turn and process.
		//Previous action phase and actions are not yet cleared.
		if (actionPhase == pendingActions.Count) {

			//If not done properly, how do we keep from skipping the *next* person whose turn it is when removed from the list? Keep track of passed count?
			//If this was the last card in hand, then remove combatant from turn order.
			//This is a potentially buggy way to do this... be careful.
			if (getCurrentCombatantDeck().getHand().Count == 0) {
				//Ran out of cards on last action.
				combatantIdTurnOrder.Remove(getCurrentCombatantId());

				//Always account for this character's missing place as an offset?
				passCount++;
			}

			//Proceed to next character...
			turn++;
			
			//Astoundingly, this seems to be working smoothly. But if a bug emerges later, don't beat yourself up.
			//The system may need to become more complex.
			for (int i = 0; i < combatantIdTurnOrder.Count; i++) {
				int combatantId = combatantIdTurnOrder[i];
				//string combatantName = getCombatantById(combatantId).name;
				//int health = getCombatantById(combatantId).health;
				//int damage = getCombatantById(combatantId).damage;
				//Debug.Log("combatant: " + combatantName + " " + (health - damage) + "/" + health);
				if (getCombatantById(combatantId).isAlive() == false) {
					bool removed = combatantIdTurnOrder.Remove(combatantId);
					passCount++;
					//Debug.Log("tried to remove: " + combatantName + " " + removed);
					i--;
				}
			}

			if (combatantIdTurnOrder.Count == 0) {
				round++;
				turn = 0;
				passCount = 0;
				//Debug.Log("----------------new round");
				//New round means draw new cards...
				//For now all players draw immediately??? Will probably change this.
				foreach (KeyValuePair<int, Deck> entry in combatantIdDecks) {
					int combatantId = entry.Key;
					if (getCombatantById(combatantId).isAlive()) {
						if (entry.Value.getDrawsRemaining() < 4) {
							entry.Value.reshuffle();
						}
						entry.Value.drawHand();
						combatantIdTurnOrder.Add(combatantId);
					}
				}
			}
		}
		if (round < 20) {
			//Input module - instance.processNextAction()
			inputSource.processNextAction(this);
		} else {
			Debug.Log("need to stop");
			return;
		}
	}

	/// <summary>
	/// Determines whether a target hex will be valid or not.
	/// Currently only checks visibility, but may be expanded later.
	/// </summary>
	/// <param name="target">the target hex</param>
	/// <returns></returns>
	public bool isTargetValid(Vector2Int target) {
		return new List<Vector2Int>(
			HexGrid.getVisibleHexes(
				getCurrentCombatant().pos,
				getBlockedHexes(1).ToArray(),
				getCurrentAction().minRange,
				getCurrentAction().maxRange
			)
		).Contains(target);
	}

	/// <summary>
	/// Determines the combat effects of an action on a target.
	/// Does not validate and thus can be used for "hypothetical" results.
	/// </summary>
	/// <param name="target">Target hex position of the action</param>
	public List<CombatEffect> determineCombatEffects(Vector2Int targetHex) {
		//Debug.Log("determineCombatEffects");

		List<CombatEffect> combatEffects = new List<CombatEffect>();
		Card.CardAction combatAction = getCurrentAction();
		Vector2Int[] affectedHexes = HexGrid.getXYsWithinRadius(targetHex.x, targetHex.y, getCurrentAction().radius);
		//Debug.Log("affectedHexes: " + affectedHexes.Length);
		foreach (BattlefieldEntity entity in battlefieldEntities) {
			//We avoid terrain since it has 0 health and is never technically alive in the first place.
			if (entity.isAlive()) {
				for (int i = 0; i < affectedHexes.Length; i++) {
					//Debug.Log("Checking: " + entity.pos);
					if (entity.pos.Equals(affectedHexes[i])) {
						CombatEffect combatEffect = new CombatEffect();
						combatEffect.combatant = entity;
						combatEffect.damage = combatAction.value;
						combatEffects.Add(combatEffect);
					}
				}
			} else {
				//Debug.Log("Not alive: " + entity.name);
			}
		}
		//Debug.Log("combat effects: " + combatEffects.Count);
		return combatEffects;
	}

	private void applyCombatEffects(List<CombatEffect> combatEffects) {
		Debug.Log("apply combat effects");
		foreach (CombatEffect effect in combatEffects) {
			//For now that's it. We're just doing damage.
			effect.combatant.damage += effect.damage;
			//We'll update here for now until there's a more elegant solution.
			if (effect.combatant.isAlive() == false) {
				effect.combatant.blocksMovement = false;
			}
			Debug.Log("apply effect to: " + effect.combatant.name + " " + (effect.combatant.health - effect.combatant.damage) + "/" + effect.combatant.health);
		}
		Debug.Log("Turn order: " + System.String.Join(",", combatantIdTurnOrder));
	}

	//Input state will be used to figure out just exactly how to modify the state based on input commands.
	//In other words, a given input command will result in a change of state based on the input state.
	//Everything else needed should be here to do that (eg: current combatant, deck, etc.)
	//Then there will just be a process function to change state according to the input and the current state.
	public CombatAction getActionState() {
		if (actionPhase == pendingActions.Count) {
			//All pending actions resolved.
			//Process next turn, this increments turn, round, and requires card selection.
			return CombatAction.SELECT_ACTION;
			//} else if (actionPhase == pendingActions.Count) {
			//I think I need this for my own sanity.
			//	return Action.END_TURN;
		} else {
			return pendingActions[actionPhase].type;
		}
	}
	
	public int getCurrentCombatantId() {
		//Debug.Log("turn: " + turn + " pass count: " + passCount + " turn order count: " + combatantIdTurnOrder.Count);
		return combatantIdTurnOrder[getCurrentTurnIndex()];
		//return combatantIdTurnOrder[turn % combatantIdTurnOrder.Count];
	}

	private int getCurrentTurnIndex() {
		return (turn - passCount) % combatantIdTurnOrder.Count;
	}

	public BattlefieldEntity getCurrentCombatant() {
		return battlefieldEntities[getCurrentCombatantId()];
	}

	public Deck getCurrentCombatantDeck() {
		return combatantIdDecks[getCurrentCombatantId()];
	}

	public Card.CardAction getCurrentAction() {
		//Debug.Log("getCurrentAction phase: " + actionPhase + " actions avail: " + pendingActions.Count);
		return pendingActions[actionPhase];
	}

	public BattlefieldEntity getCombatantById(int id) {
		return battlefieldEntities[id];
	}

	public List<BattlefieldEntity> getCombatantsInTurnOrder() {
		List<BattlefieldEntity> combatants = new List<BattlefieldEntity>();
		for (int i = 0; i < combatantIdTurnOrder.Count; i++) {
			combatants.Add(battlefieldEntities[combatantIdTurnOrder[i]]);
		}
		return combatants;
	}

	public List<Vector2Int> getBlockedHexes(int mode=0) {
		//TODO: make mode an enum of some kind for various tests.
		List<Vector2Int> blockedHexes = new List<Vector2Int>();
		foreach (BattlefieldEntity entity in battlefieldEntities) {
			if ((mode == 0 && entity.blocksMovement) || (mode == 1 && entity.blocksVision)) {
				blockedHexes.Add(entity.pos);
			}
		}
		return blockedHexes;
	}

	//TODO: add a method for adding an active combatant...
	public void addActiveCombatant(BattlefieldEntity combatant, List<Card> deck) {
		//TODO
	}
}
