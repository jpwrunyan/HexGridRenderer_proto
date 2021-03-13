using UnityEngine;

public class BumpAnimation : AnimationManager.BaseAnimation {

	private float originX;
	private float originY;
	private float originZ;
	
	private Vector3 d;

	public BumpAnimation(GameObject target, Vector3 to, float length = 10f, float duration = .5f, float delay = 0) : base(target, duration, delay) {
		Vector3 from = target.transform.localPosition;
		originX = from.x;
		originY = from.y;
		originZ = from.z;
		d = Vector3.Normalize(to - from) * length;
	}

	override public void animate(float completion) {
		completion *= 2;
		if (completion <= 1) {
			target.transform.localPosition = new Vector3(
				originX + d.x * completion, 
				originY + d.y * completion, 
				originZ + d.z * completion
			);
		} else {
			completion -= 1;
			target.transform.localPosition = new Vector3(
				originX + d.x - (d.x * completion), 
				originY + d.y - (d.y * completion), 
				originZ + d.z - (d.z * completion)
			);
		}
	}
}
