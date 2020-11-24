using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SpriteRenderer))]
public class EntityRenderer : MonoBehaviour {

	public static EntityRenderer instantiate(BattlefieldEntity entity, Sprite sprite) {
		GameObject gameObject = new GameObject(entity.name);
		//Must instantiate and add the SpriteRenderer to the base GameObject before attaching the EntityRenderer script due to RequireComponent.

		SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>() as SpriteRenderer;
		spriteRenderer.sprite = sprite;
		
		EntityRenderer entityRenderer = gameObject.AddComponent<EntityRenderer>();
		entityRenderer.spriteRenderer = spriteRenderer;

		entityRenderer.spriteRenderer.transform.localPosition = new Vector3(0, HexGridRenderer.CELL_HEIGHT / 4);

		entityRenderer.createText(entity);

		return entityRenderer;
	}

	private void createText(BattlefieldEntity entity) {
		GameObject canvasHolder = new GameObject("Canvas", typeof(RectTransform));

		//Strangely, unless I make the canvas a child object, it just converts transform to recttransform...
		//GameObject canvasHolder = new GameObject("Canvas", typeof(RectTransform));
		canvas = canvasHolder.AddComponent<Canvas>();
		//Show text above the default 0 (Sprite)
		//Note: this renders on top of ALL Sprites.
		canvas.sortingOrder = 1;
		canvas.transform.SetParent(gameObject.transform);

		CanvasScaler canvasScalar = canvasHolder.AddComponent<CanvasScaler>();
		canvasScalar.dynamicPixelsPerUnit = 10;
		
		GameObject textHolder = new GameObject("Text", typeof(RectTransform));
		
		Text textComp = textHolder.AddComponent<Text>();
		textComp.text = entity.name;

		Font font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
		textComp.font = font;
		textComp.alignment = TextAnchor.UpperCenter;
		textComp.fontSize = 12;
		textComp.color = Color.red;
		textComp.transform.SetParent(canvas.transform, false);
		Vector3 pos = new Vector3(0, 0, 0);

		textHolder.transform.localPosition = pos;

		RectTransform rectTransform = textHolder.GetComponent<RectTransform>();

		//To work with the label as a centered pic (h/w don't really matter)
		//Anchor is relative the the text canvas' sizeDelta
		rectTransform.anchorMin = new Vector2(.5f, .5f);
		rectTransform.anchorMax = new Vector2(.5f, .5f);
		//Pivot is relative the the text components sizeDelta
		rectTransform.pivot = new Vector2(.5f, .5f);
		rectTransform.sizeDelta = new Vector2(40, 24);
	}

	private SpriteRenderer spriteRenderer;
	private Canvas canvas;

	public string test = "test";

    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        
    }
}
