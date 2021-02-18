using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple automated source of combatant input
/// </summary>
public class SimpleAutoInput : InputSource {
	public void processNextAction(BattleState battleState) {
		battleState.processActionInput(getInput(battleState));
	}

	private BattleInput getInput(BattleState battleState) {
		switch (battleState.getActionState()) {
			case CombatActionType.SELECT_ACTION: return getSelectCardInput(battleState);
			case CombatActionType.MOVE: return getMoveInput(battleState);
			case CombatActionType.MELEE_ATTACK:
			case CombatActionType.RANGE_ATTACK: return getSelectTargetInput(battleState);
			default: {
				BattleInput input = new BattleInput();
				input.combatantId = battleState.getCurrentCombatantId();
				return input;
			}
		}
	}

	public BattleInput getSelectCardInput(BattleState battleState) {
		BattleInput input = new BattleInput();
		input.combatantId = battleState.getCurrentCombatantId();
		input.value = new Vector2Int(0, 0);
		return input;
	}

	public BattleInput getMoveInput(BattleState battleState) {
		//Temporary, should actually be able to move toward blocked hexes.
		int i = 0;
		while (battleState.getBlockedHexes().Contains(battleState.hexGrid.hexPos[i])) {
			//Find a hex that isn't blocked.
			i++;
		}

		BattleInput input = new BattleInput();
		input.combatantId = battleState.getCurrentCombatantId();
		input.value = battleState.hexGrid.hexPos[i];

		return input;
	}

	public BattleInput getSelectTargetInput(BattleState battleState) {
		BattleInput input = new BattleInput();
		input.combatantId = battleState.getCurrentCombatantId();
		input.value = battleState.getCombatantsInTurnOrder()[0].pos;
		return input;
	}
}
