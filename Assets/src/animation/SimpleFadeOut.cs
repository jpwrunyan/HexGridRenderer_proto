using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleFadeOut : AnimationManager.BaseAnimation {

	private Color startColor;

	public SimpleFadeOut(GameObject target, float duration = 2, float delay = 0) : base(target, duration, delay) {
		startColor = target.GetComponent<Renderer>().material.color;
	}

	override public void animate(float completion) {
		target.GetComponent<Renderer>().material.color = startColor * (1 - completion);
	}

	public override void cleanUp() {
		base.cleanUp();
		target.SetActive(false);
	}
}
