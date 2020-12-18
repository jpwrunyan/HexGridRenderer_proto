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

	private Vector2Int getInput(BattleState battleState) {
		switch (battleState.getActionState()) {
			case CombatAction.SELECT_ACTION: return getSelectCardInput(battleState);
			case CombatAction.MOVE: return getMoveInput(battleState);
			case CombatAction.MELEE_ATTACK:
			case CombatAction.RANGE_ATTACK: return getSelectTargetInput(battleState);
			default: return new Vector2Int(0, 0);
		}
	}

	public Vector2Int getSelectCardInput(BattleState battleState) {
		return new Vector2Int(0, 0);
	}

	public Vector2Int getMoveInput(BattleState battleState) {
		return battleState.hexGrid.hexPos[0];
	}

	public Vector2Int getSelectTargetInput(BattleState battleState) {
		return battleState.getCombatantsInTurnOrder()[0].pos;
	}
}
