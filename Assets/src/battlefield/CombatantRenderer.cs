using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CombatantRenderer : MonoBehaviour {

	public Combatant combatant;

	public Text infoText;
	public Text harassmentText;
	public Text evasionText;

	public void updateDisplay() {
		infoText.text = combatant.name;
		harassmentText.text = combatant.harassment.ToString();
		evasionText.text = combatant.evasion.ToString();
	}
}
