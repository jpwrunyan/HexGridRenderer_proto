using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Displays a card.
[RequireComponent(typeof(CanvasGroup))]
public class CardRenderer : MonoBehaviour {

	[SerializeField]
	private Text titleText = null;

	[SerializeField]
	private CardStatLineRenderer cardStatLineRendererPrefab;

	private List<CardStatLineRenderer> cardStatLineRenderers = new List<CardStatLineRenderer>();

	//Probably don't need these... will come from the prefab this is attached to...
	const float xDim = 2.5f;
	const float yDim = 3.5f;
	const int size = 40;

	public bool visible {
		get {
			return GetComponent<CanvasGroup>().alpha == 1;
		}
		set {
			GetComponent<CanvasGroup>().alpha = value ? 1 : 0;
		}
	}

	public void setCard(Card card) {
		titleText.text = card.title;
		updateDisplay(card);
	}

	public string getTitle() {
		return titleText.text;
	}

	private void updateDisplay(Card card) {
		List<Card.CardAction> cardActions = card.getCardActions();
		int n = cardActions.Count;
		for (int i = 0; i < n; i++) {
			switch (cardActions[i].type) {
				case CombatActionType.MOVE: getCardStatLineRenderer(i).setText("Move " + cardActions[i].maxRange); break;
				case CombatActionType.MELEE_ATTACK: getCardStatLineRenderer(i).setText("Melee Attack " + cardActions[i].value); break;
				case CombatActionType.RANGE_ATTACK: {
					getCardStatLineRenderer(i).setText(
						"Range " + cardActions[i].minRange + 
						"-" + cardActions[i].maxRange +
						" Attack " + cardActions[i].value +
						(cardActions[i].radius > 0 ? " Radius " + cardActions[i].radius : "")
					);
					break;
				}
			}
		}

		while (n < cardStatLineRenderers.Count) {
			//Do not destroy excess card renderers, just disable them.
			cardStatLineRenderers[n++].gameObject.SetActive(false);
		}
	}

	private CardStatLineRenderer getCardStatLineRenderer(int i) {
		CardStatLineRenderer cardStatLineRenderer;
		if (i < cardStatLineRenderers.Count) {
			cardStatLineRenderer = cardStatLineRenderers[i];
			cardStatLineRenderer.gameObject.SetActive(true);
		} else {
			cardStatLineRenderer = Instantiate(cardStatLineRendererPrefab) as CardStatLineRenderer;
			cardStatLineRenderer.transform.SetParent(this.transform, false);
			cardStatLineRenderers.Add(cardStatLineRenderer);
		}
		return cardStatLineRenderer;
	}
}
