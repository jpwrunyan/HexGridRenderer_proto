using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CombatantRenderer : MonoBehaviour {

	public Combatant combatant;

	public Text infoText;
	public Text hitpointsText;
	public Text initiativeText;
	public Text harassmentText;
	public Text evasionText;

	public void updateDisplay() {
		infoText.text = combatant.name;
		hitpointsText.text = (combatant.hitpoints - combatant.damage) + "/" + combatant.hitpoints;
		initiativeText.text = combatant.initiative.ToString();
		harassmentText.text = combatant.harassment.ToString();
		evasionText.text = combatant.evasion.ToString();
	}
}
