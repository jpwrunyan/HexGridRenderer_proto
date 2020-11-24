using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Displays a card.
[RequireComponent(typeof(CanvasGroup))]
public class CardRenderer : MonoBehaviour {

	[SerializeField]
	private Text titleText;

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
	}

    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
