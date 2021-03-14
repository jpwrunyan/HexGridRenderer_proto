using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
	
	//The current round of combat.
	//Combat begins at round 0. 
	public int round = 0;
	//The current turn in this round of combat.
	//Each round starts at turn 0. But initialization increments it by 1.
	public int turn = 0;

	//Pending actions that must be resolved during the current turn.
	public List<Card.CardAction> pendingActions = new List<Card.CardAction>();

	//The index of the pending action currently resolving.
	public int actionPhase = 0;

	//A list of combatant ids and the order in which they take their turn for the current round.
	//The id corresponds to the index of the battlefieldEntities list.
	//The turn value determines which combatant in the list is currently taking their turn.
	private List<int> combatantIdTurnOrder = new List<int>();

	private List<int> activeCombatantIds = new List<int>();

	private InputSource inputSource;

	public BattleState() {
		//
	}

	public void setInputSource(InputSource inputSource, bool start=true) {
		this.inputSource = inputSource;
		if (start) {
			inputSource.processNextAction(this);
		}
	}

	public void addAlliedActiveCombatants(params Combatant[] allies) {
		foreach (Combatant combatant in allies) {
			addActiveCombatant(combatant);
		}
		setAllies(allies);
	}

	public void setAllies(params Combatant[] allies) {
		//people.Select(person => person.Age).ToArray()
		int[] allyIds = allies.Select(combatant => battlefieldEntities.IndexOf(combatant)).ToArray();
		foreach (Combatant combatant in allies) {
			int combatantId = battlefieldEntities.IndexOf(combatant);
			foreach (int allyId in allyIds) {
				if (combatantId != allyId && !combatant.allyIds.Contains(allyId)) {
					combatant.allyIds.Add(allyId);
				}
			}
		}
	}

	public int addActiveCombatant(Combatant combatant) {
		int combatantId = battlefieldEntities.Count;
		battlefieldEntities.Add(combatant);
		combatantIdTurnOrder.Add(combatantId);
		activeCombatantIds.Add(combatantId);
		combatant.deck.drawHand();
		return combatantId;
	}

	/// <summary>
	/// Determine if combat is resolved.
	/// Either everyone is dead or only allies remain.
	/// </summary>
	/// <returns></returns>
	public bool isCombatOver() {
		//Basically a simple check of each combatant against the other to see if they are friends.
		for (int i = 0; i < activeCombatantIds.Count - 1; i++) {
			int combatantId = activeCombatantIds[i];
			Combatant combatant = battlefieldEntities[combatantId] as Combatant;
			if (combatant.isAlive()) {
				List<int> allyIds = combatant.allyIds;
				for (int j = i + 1; j < activeCombatantIds.Count; j++) {
					if (battlefieldEntities[activeCombatantIds[j]].isAlive() && !allyIds.Contains(activeCombatantIds[j])) {
						//Debug.Log("X Combatant " + combatantId + " is NOT friends with " + activeCombatantIds[j]);
						return false;
					} else {
						//Debug.Log("Combatant " + combatantId + " is friends with " + activeCombatantIds[j]);
					}
				}
			}
		}
		return true;
	}

	/// <summary>
	/// Input is recieved from the input processor (whether it's manual UI or ai logic).
	/// In the future, I may abstract the input processor and make it a held reference for battle state...
	/// </summary>
	/// <param name="input"></param>
	public void processActionInput(BattleInput input) {

		Combatant currentCombatant = battlefieldEntities[combatantIdTurnOrder[0]] as Combatant;
		Deck currentCombatantDeck = currentCombatant.deck;

		switch (getActionState()) {
			case CombatActionType.SELECT_ACTION: {
				//Card index is the input.x value.
				//change the deck accordingly.
				//Debug.Log("process select action");
				int selectedIndex = input.value.x;
				if (selectedIndex == -1) {
					//Player folded.
					pendingActions = new List<Card.CardAction>();
					//Discard all cards now? This determines pass state further down.
					//
					while (currentCombatantDeck.getHand().Count > 0) {
						currentCombatantDeck.discard(currentCombatantDeck.getHand()[0]);
					}
				} else {
					Card card = currentCombatantDeck.getHand()[selectedIndex];
					pendingActions = card.getCardActions();
					//May possibly handle discards via CombatEffect
					currentCombatantDeck.discard(card);
					//Combatant's initiative score increases.
					currentCombatant.initiative += card.initiative;
				}
				actionPhase = 0;
				break;
			}
			case CombatActionType.MOVE: {
				//In the future I may require that the actual node data is also provided so validation can occur...
				//I may also need to know the subject of the action in case it isn't always the current combatant.
				//This will require updating the input parameter to a more complicated structure.
				//But otherwise I won't have a way to keep a history of "inputs".
				//Alternatively, just make sure you validate input via the UI.
				//The "z" value corresponds to the entity id.
				//Debug.Log("process move action");

				//The "clicked" pos could be anywhere. It's a goal. Find the destination along the path that can actually be moved to.
				HexNodePathMap pathMap = new HexNodePathMap(hexGrid, this);
				pathMap.setOrigin(getCurrentCombatant().pos);
				//Can only move within max range of the action.
				HexNode dest = pathMap.getClosestHexNodeTo(input.value, getCurrentAction().maxRange);

				if (dest != null) {
					//Combatant combatant = battlefieldEntities[input.combatantId] as Combatant;
					//combatant.pos = dest.hexPos;
					currentCombatant.pos = dest.hexPos;
					//Increment evasion for each hex traversed.
					while (dest.prevNode != null) {
						//combatant.evasion++;
						currentCombatant.evasion++;
						dest = (HexNode) dest.prevNode;
					}
				} else {
					Debug.Log("Invalid input. Do not move.");
				}

				actionPhase++;
				break;
			}
			case CombatActionType.MELEE_ATTACK:
			case CombatActionType.RANGE_ATTACK: {
				//Debug.Log("process attack");
				if (isTargetValid(input.value)) {
					//Valid input.
					applyCombatEffects(determineCombatEffects(input.value));
				}
				actionPhase++;
				break;
			}
		}

		//Debug.Log("Process Action phase: " + actionPhase + " actions count: " + pendingActions.Count);

		//Check end of turn and process.
		//Previous action phase and actions are not yet cleared.
		if (actionPhase == pendingActions.Count) {
			if (currentCombatantDeck.getHand().Count == 0) {
				//Ran out of cards on last action.
				Debug.Log(
					"passing this char since they have no more cards - id: " +
					getCurrentCombatantId() +
					" name: " +
					getCurrentCombatant().name +
					" deck: " +
					currentCombatant.name
				);
			} else if (currentCombatant.isAlive()) {
				combatantIdTurnOrder.Add(combatantIdTurnOrder[0]);
			}
			combatantIdTurnOrder.RemoveAt(0);

			//Next, remove other combatants who are no longer alive.
			int i = 0;
			while (i < combatantIdTurnOrder.Count) {
				if (battlefieldEntities[combatantIdTurnOrder[i]].isAlive() == false) {
					combatantIdTurnOrder.RemoveAt(i);
				} else {
					i++;
				}
			}

			if (combatantIdTurnOrder.Count > 0) {
				//Proceed to next turn.
				turn++;
			} else {
				startNewRound();
			}
		}

		if (isCombatOver()) {
			Debug.Log("combat ended on this process action");
		}

		if (round< 20) {
			//Input module - instance.processNextAction()
			inputSource.processNextAction(this);
		} else {
			Debug.Log("need to stop");
			return;
		}
	}
	private void startNewRound() {
		Debug.Log("----------------new round " + (round + 1));
		round++;
		turn = 0;
		//New round means draw new cards...
		//For now all players draw immediately??? Will probably change this.
		activeCombatantIds.Sort((a, b) => {
			return ((Combatant)battlefieldEntities[a]).initiative - ((Combatant)battlefieldEntities[b]).initiative;
		});
		for (int i = 0; i < activeCombatantIds.Count; i++) {
			Combatant combatant = battlefieldEntities[activeCombatantIds[i]] as Combatant;
			if (combatant.isAlive()) {
				if (combatant.deck.getDrawsRemaining() < combatant.deck.handSize) {
					combatant.deck.reshuffle();
				}
				combatant.deck.drawHand();
				//TODO determine position in turn order based on intiative.
				combatantIdTurnOrder.Add(activeCombatantIds[i]);
				//Reset evasion and harassment.
				combatant.evasion = 0;
				combatant.harassment = 0;
			}
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
						combatEffect.target = entity;
						//Calculate if it hits.
						if (entity is Combatant) {
							Combatant targetCombatant = ((Combatant)entity);
							//evasion + -harassment ensures that even if evasion is negative, applying harassment will further decrease it.
							//I do not want a situation where -1 - 1 = 0. So we use -1 + -1.
							if (targetCombatant.evasion + -targetCombatant.harassment > combatAction.accuracy) {
								combatEffect.damage = 0;
							} else {
								combatEffect.damage = combatAction.value;
							}
							combatEffect.harassment = combatAction.harassment;
						}
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
		//Debug.Log("apply combat effects");
		foreach (CombatEffect effect in combatEffects) {
			//For now that's it. We're just doing damage.
			effect.target.damage += effect.damage;
			if (effect.target is Combatant) {
				Combatant targetCombatant = ((Combatant)effect.target);
				targetCombatant.harassment += effect.harassment;
			}
			//We'll update here for now until there's a more elegant solution.
			if (effect.target.isAlive() == false) {
				effect.target.blocksMovement = false;
				effect.target.blocksVision = false;
			}
			//Debug.Log("apply effect to: " + effect.combatant.name + " " + (effect.combatant.health - effect.combatant.damage) + "/" + effect.combatant.health);
		}
		//Debug.Log("Turn order: " + System.String.Join(",", combatantIdTurnOrder));
	}

	//Input state will be used to figure out just exactly how to modify the state based on input commands.
	//In other words, a given input command will result in a change of state based on the input state.
	//Everything else needed should be here to do that (eg: current combatant, deck, etc.)
	//Then there will just be a process function to change state according to the input and the current state.
	public CombatActionType getActionState() {
		//Check if the battle is concluded first.
		if (isCombatOver()) {
			return CombatActionType.NONE;
		} else if (actionPhase == pendingActions.Count) {
			//All pending actions resolved.
			//Process next turn, this increments turn, round, and requires card selection.
			return CombatActionType.SELECT_ACTION;
			//} else if (actionPhase == pendingActions.Count) {
			//I think I need this for my own sanity.
			//	return Action.END_TURN;
		} else {
			return pendingActions[actionPhase].type;
		}
	}
	
	public int getCurrentCombatantId() {
		return combatantIdTurnOrder[0];
	}

	public Combatant getCurrentCombatant() {
		return battlefieldEntities[getCurrentCombatantId()] as Combatant;
	}

	public Card.CardAction getCurrentAction() {
		//Debug.Log("getCurrentAction phase: " + actionPhase + " actions avail: " + pendingActions.Count);
		return pendingActions[actionPhase];
	}

	public BattlefieldEntity getBattlefieldEntityById(int id) {
		return battlefieldEntities[id];
	}

	public Combatant getCombatantById(int id) {
		return getBattlefieldEntityById(id) as Combatant;
	}

	public List<Combatant> getCombatantsInTurnOrder() {
		List<Combatant> combatants = new List<Combatant>();
		foreach (int i in combatantIdTurnOrder) {
			combatants.Add(battlefieldEntities[i] as Combatant);
		}
		return combatants;
	}

	//Currently this is no longer used for movement. It is only used for vision arcs.
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
}
