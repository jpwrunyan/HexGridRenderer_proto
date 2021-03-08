using UnityEngine;

public class BumpAnimation : AnimationManager.BaseAnimation {

	private float originX;
	private float originY;
	private float originZ;
	private float dx;
	private float dy;
	private float dz;

	public BumpAnimation(GameObject target, Vector3 to, float factor = 0.5f, float duration = .5f, float delay = 0) : base(target, duration, delay) {
		Vector3 from = target.transform.localPosition;
		originX = from.x;
		originY = from.y;
		originZ = from.z;
		dx = (to.x - from.x) * factor;
		dy = (to.y - from.y) * factor;
		dz = (to.z - from.z) * factor;
	}

	override public void animate(float completion) {
		completion *= 2;
		if (completion <= 1) {
			target.transform.localPosition = new Vector3(
				originX + dx * completion, 
				originY + dy * completion, 
				originZ + dz * completion
			);
		} else {
			completion -= 1;
			target.transform.localPosition = new Vector3(
				originX + dx - (dx * completion), 
				originY + dy - (dy * completion), 
				originZ + dz - (dz * completion)
			);
		}
	}
}
