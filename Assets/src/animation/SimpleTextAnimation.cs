using UnityEngine;

public class SimpleTextAnimation : AnimationManager.BaseAnimation {

	public SimpleTextAnimation(Vector3 pos, string text, float duration = 1, float delay = 1) : base(null, duration, delay) {
		//There's a smarter way to do this.
		
		//Animation target:
		target = new GameObject("Simple Text Animation");
		target.SetActive(false);
		TextMesh textComp = target.AddComponent<TextMesh>();
		textComp.text = text;

		Font font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
		textComp.font = font;
		textComp.fontSize = 20;
		textComp.characterSize = 10;
		textComp.richText = false;
		textComp.anchor = TextAnchor.LowerCenter;
		textComp.color = Color.red;
		//textComp.transform.SetParent(gameObject.transform, false);
		//Adding the TextMesh will automatically add the MeshRenderer
		target.GetComponent<MeshRenderer>().receiveShadows = false;
		target.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		target.transform.position = pos;
		target.transform.forward = Camera.main.transform.forward;
	}

	override public void animate(float completion) {
		target.SetActive(true);
		TextMesh textComp = target.GetComponent<TextMesh>();
		//Have to modify alpha this way.
		Color tempColor = textComp.color;
		tempColor.a = 1 - completion;
		textComp.color = tempColor;
	}

	public override void cleanUp() {
		base.cleanUp();
		Object.Destroy(target);
	}
}
