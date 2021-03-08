using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Animation manager component
/// </summary>
public class AnimationManager : MonoBehaviour {
	//----------------------------------------
	// Animation manager
	//----------------------------------------

	public System.Action onComplete;
	//or Action<BaseAnimation> ? So we can check the completion based on specific animations that trigger it?
	
	public bool isPlaying {
		get => animationQueue.Count > 0;
	}

	private List<BaseAnimation> animationQueue = new List<BaseAnimation>();

	public BaseAnimation queueAnimation(BaseAnimation animation) {
		animationQueue.Add(animation);
		return animation;
	}

	private void Update() {
		for (int i = 0; i < animationQueue.Count; i++) {
			animateTarget(animationQueue[i]);
		}
		if (animationQueue.Count == 0) {
			//owner.removeEventListener(Event.ENTER_FRAME, enterFrameHandler);
			//owner.dispatchEvent(new Event(ALL_ANIMATION_COMPLETE));
		}
	}

	private void animateTarget(BaseAnimation animation) {
		if (animation.delay > 0) {
			Debug.Log("there is a delay: " + animation.delay + " " + animation.GetType().Name);
			animation.delay -= Time.deltaTime;
			return;
		}
		animation.t += Time.deltaTime; //Delta time is in seconds.
		if (animation.t == 0) {
			//owner.dispatchEvent(new AnimationEvent(AnimationEvent.START, animation));
		} else if (animation.t <= animation.duration) {
			float completion = animation.tween(animation.t, 0, 1, animation.duration);
			animation.animate(completion);
		} else {
			//Dispatch complete on the frame AFTER the animation is at 100%
			animation.cleanUp();
			animationQueue.Remove(animation);
			//animationQueue.splice(animationQueue.indexOf(animation), 1);
			//owner.dispatchEvent(new AnimationEvent(AnimationEvent.COMPLETE, animation));

			//If onComplete is not null, invoke it.
			//Note: This may not be the right place for this because it loses track of what actual BaseAnimation was called.
			//Might be better to only call if the animation queue is empty???
			onComplete?.Invoke();
		}
	}

	public abstract class BaseAnimation {
		public GameObject target;
		//public var tween:Function = Tween.easeInOutSine;
		public Func<float, float, float, float, float> tween = AnimationManager.easeInOutSine;
		/*
		public float tween(float t, float t2, float t3, float t4) {
			return -1f;
		}
		*/

		internal float duration;
		internal float delay = 0;
		internal float t = 0;

		/// <summary>
		/// Instantiate a base animation
		/// </summary>
		/// <param name="target">GameObject that will be modified</param>
		/// <param name="duration">Animation duration in seconds</param>
		/// <param name="delay">Delay before starting animation in seconds</param>
		public BaseAnimation(GameObject target, float duration = 10, float delay = 0) {
			this.target = target;
			this.duration = duration;
			this.delay = delay;
			t = 0;
		}

		public float getDuration() {
			return duration;
		}

		/// <summary>
		/// Animation logic. Must be overridden.
		/// </summary>
		/// <param name="completion"></param>
		abstract public void animate(float completion);

		virtual public void cleanUp() {
			//To be overridden if necessary.
		}
	}

	public class MoveAnimation : BaseAnimation {

		private float originX;
		private float originY;
		private float originZ;
		private float dx;
		private float dy;
		private float dz;
		public MoveAnimation(GameObject target, Vector3 from, Vector3 to, float duration = 10, float delay = 0) : base(target, duration, delay) {
			originX = from.x;
			originY = from.y;
			originZ = from.z;
			dx = to.x - from.x;
			dy = to.y - from.y;
			dz = to.z - from.z;
		}

		override public void animate(float completion) {
			target.transform.localPosition = new Vector3(originX + dx * completion, originY + dy * completion, originZ + dz * completion);
		}
	}

	public static float linear(float t, float b, float c, float d) {
		return c * t / d + b;
	}

	public static float easeInOutSine(float t, float b, float c, float d) {
		return -c / 2 * (Mathf.Cos(Mathf.PI * t / d) - 1) + b;
	}
}
