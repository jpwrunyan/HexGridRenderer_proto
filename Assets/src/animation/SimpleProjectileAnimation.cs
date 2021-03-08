using UnityEngine;

public class SimpleProjectileAnimation : AnimationManager.BaseAnimation {

	private Vector3 from;
	private Vector3 to;
	public SimpleProjectileAnimation(GameObject parent, Vector3 from, Vector3 to, float speed = 500, float delay = 0) : base(null, 0, delay) {
		this.from = parent.transform.TransformPoint(from) + (Camera.main.transform.up * 25);
		this.to = parent.transform.TransformPoint(to) + (Camera.main.transform.up * 25);



		GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		sphere.name = "Simple Projectile Animation";
		sphere.transform.parent = parent.transform;
		sphere.transform.localScale = new Vector3(5, 5, 5);
		sphere.transform.position = this.from;

		base.target = sphere;
		base.duration = Vector3.Distance(from, to) / speed;

		tween = AnimationManager.linear;
	}

	override public void animate(float completion) {
		target.transform.position = Vector3.Lerp(from, to, completion);
	}

	public override void cleanUp() {
		base.cleanUp();
		MonoBehaviour.Destroy(target);
	}
}
