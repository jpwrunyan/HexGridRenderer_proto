using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(HorizontalLayoutGroup))]
[RequireComponent(typeof(RectTransform))]
public class TurnOrderDisplay : MonoBehaviour {

	public CombatantRenderer combatantRendererPrefab;

	//private List<CombatantRenderer> combatantRenderers = new List<CombatantRenderer>();

	private Dictionary<Combatant, CombatantRenderer> combatantRenderers = new Dictionary<Combatant, CombatantRenderer>();

	public void updateDisplay(BattleState battleState, ImageLibrary imageLibrary) {
		//Hide any existing renderers.
		foreach (CombatantRenderer combatantRenderer in combatantRenderers.Values) {
			combatantRenderer.gameObject.SetActive(false);
		}

		List<Combatant> combatantsInTurnOrder = battleState.getCombatantsInTurnOrder();
		int n = combatantsInTurnOrder.Count;

		for (int i = 0; i < n; i++) {
			Combatant combatant = combatantsInTurnOrder[i];
			CombatantRenderer combatantRenderer;
			if (combatantRenderers.ContainsKey(combatant)) {
				combatantRenderer = combatantRenderers[combatant];
				combatantRenderer.gameObject.SetActive(true);
				//combatantRenderer.visible = true;
			} else {
				combatantRenderer = Instantiate(combatantRendererPrefab) as CombatantRenderer;
				Sprite entitySprite;
				byte[] pngBytes = imageLibrary.getImageBytesById(combatantsInTurnOrder[i].image);
				Texture2D tex = new Texture2D(2, 2);
				tex.LoadImage(pngBytes);
				//entitySprite = spriteCache[entity.image] = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.0f), 5.0f);
				int maxPixelSize = Mathf.Max(tex.width, tex.height);
				float pixelsPerUnit = maxPixelSize / 36f;
				entitySprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.0f), pixelsPerUnit);
				entitySprite.name = combatantsInTurnOrder[i].name;
				combatantRenderer.GetComponent<Image>().sprite = entitySprite;
				combatantRenderer.GetComponent<Image>().preserveAspect = true;
				combatantRenderer.gameObject.name = combatantsInTurnOrder[i].name + " Portrait Renderer";
				combatantRenderer.setText(combatantsInTurnOrder[i].name);

				combatantRenderer.transform.SetParent(this.transform, false);
				combatantRenderers[combatant] = combatantRenderer;
			}
			combatantRenderer.transform.SetSiblingIndex(i);
		}


		/*
		CombatantRenderer testRenderer = Instantiate(combatantRendererPrefab) as CombatantRenderer;
		testRenderer.transform.SetParent(this.transform, false);
		Sprite entitySprite;
		byte[] pngBytes = imageLibrary.getImageBytesById(battleState.getCurrentCombatant().image);
		Texture2D tex = new Texture2D(2, 2);
		tex.LoadImage(pngBytes);
		//entitySprite = spriteCache[entity.image] = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.0f), 5.0f);
		int maxPixelSize = Mathf.Max(tex.width, tex.height);
		float pixelsPerUnit = maxPixelSize / 36f;
		entitySprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.0f), pixelsPerUnit);
		entitySprite.name = battleState.getCurrentCombatant().name;
		testRenderer.GetComponent<Image>().sprite = entitySprite;
		testRenderer.GetComponent<Image>().preserveAspect = true;
		testRenderer.gameObject.name = battleState.getCurrentCombatant().name + " Portrait Renderer";
		testRenderer.setText(battleState.getCurrentCombatant().name);



		testRenderer.gameObject.transform.SetSiblingIndex(0);
		//testRenderer.GetComponent<TextMesh>().text = "SHOW ME SOMETHING!";
		*/
	}

	// Start is called before the first frame update
	void Start() {
		//
    }

    // Update is called once per frame
    void Update() {
        //
    }
}
