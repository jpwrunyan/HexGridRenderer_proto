using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(HorizontalLayoutGroup))]
[RequireComponent(typeof(RectTransform))]
public class CardListDisplay : MonoBehaviour {
	GameInput controls;

	public GameObject pointer;
	public CardRenderer cardRendererPrefab;

	//private int w = 250;
	//private int h = 350;

	private int _selectedIndex = -1;

	public int selectedIndex {
		get {
			return _selectedIndex;
		}
		set {
			//Validation:
			if (value < 0) {
				_selectedIndex = cards.Count - 1;
			} else if (value >= cards.Count) {
				_selectedIndex = 0;
			} else {
				_selectedIndex = value;
			}

			//View logic:
			pointer.SetActive(true);
			HorizontalLayoutGroup layout = GetComponent<HorizontalLayoutGroup>();
			//float w = cardRendererPrefab.GetComponent<RectTransform>().rect.width + layout.spacing;
			//w *= _selectedIndex;
			//w += layout.padding.left + cardRendererPrefab.GetComponent<RectTransform>().rect.width / 2;
			//pointer.transform.localPosition = new Vector3(w, 0, 0);
			Vector3 rendererPosition = cardRenderers[_selectedIndex].transform.localPosition;
			pointer.transform.localPosition = new Vector3(
				rendererPosition.x + cardRendererPrefab.GetComponent<RectTransform>().rect.width / 2,
				rendererPosition.y
			);
		}
	}

	public void clearSelection() {
		_selectedIndex = -1;
		pointer.SetActive(false);
	}

	public void hideCard(int index) {
		showCard(index, false);
	}

	public void showCard(int index, bool visible=true) {
		//cardRenderers[index].enabled = enabled;
		//cardRenderers[_selectedIndex].gameObject.SetActive(visible);
		cardRenderers[index].visible = visible;
	}

	private List<CardRenderer> cardRenderers = new List<CardRenderer>();

	private List<Card> cards = new List<Card>();

	public void setCards(List<Card> cards) {
		this.cards = cards;
		updateDisplay();
	}

	private void Awake() {
		clearSelection();
	}

	/// <summary>
	/// Returns -1 if the pos provided is not even inside the display area.
	/// </summary>
	/// <param name="pos">Screen position to test against</param>
	/// <returns>The corresponding card index (if any) under this position</returns>
	public int getCardIndexAtScreenPos(Vector3 pos) {
		RectTransform rectTransform = GetComponent<RectTransform>();

		bool inside = RectTransformUtility.RectangleContainsScreenPoint(rectTransform, pos);
		//Debug.Log("inside? " + inside);
		if (inside) {
			for (int i = 0; i < cardRenderers.Count; i++) {
				if (RectTransformUtility.RectangleContainsScreenPoint(cardRenderers[i].GetComponent<RectTransform>(), pos)) {
					Debug.Log("select: " + i);
					return i;
				}
			}
		}
		return -1;
	}

	private void updateDisplay() {
		HorizontalLayoutGroup layout = GetComponent<HorizontalLayoutGroup>();

		float w = cardRendererPrefab.GetComponent<RectTransform>().rect.width + layout.spacing;
		w *= cards.Count;
		w += layout.padding.left + layout.padding.right - layout.spacing; //take off the final length of spacing.
		float h = cardRendererPrefab.GetComponent<RectTransform>().rect.height + layout.padding.top + layout.padding.bottom;

		RectTransform rectTransform = GetComponent<RectTransform>();
		rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
		rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);

		int n = cards.Count;
		for (int i = 0; i < cards.Count; i++) {
			CardRenderer cardRenderer;
			if (i < cardRenderers.Count) {
				cardRenderer = cardRenderers[i];
				cardRenderer.gameObject.SetActive(true);
				cardRenderer.visible = true;
			} else {
				cardRenderer = Instantiate(cardRendererPrefab) as CardRenderer;
				//cardRenderer.transform.parent = this.transform;
				cardRenderer.transform.SetParent(this.transform, false);
				cardRenderers.Add(cardRenderer);
			}
			cardRenderer.setCard(cards[i]);
		}
		
		while (n < cardRenderers.Count) {
			//Do not destroy excess card renderers, just disable them.
			cardRenderers[n++].gameObject.SetActive(false);
		}
		
		pointer.transform.SetAsLastSibling();
	}

	// Start is called before the first frame update
	void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        
    }
}
