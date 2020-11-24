using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Global game state. Could potentially later separate the Singleton from its container... maybe.
/// Game state container would still be its own singleton MonoBehavior.
/// </summary>
public class GameState : MonoBehaviour {

	static GameState mInstance;

	public string id;

	public ImageLibrary imageLibrary;
	public ArenaData arenaData;

	public BattleState battleState;

	//public static SingletonMonoBehavior Instance => mInstance ? (mInstance = new GameObject("MyClassContainer").AddComponent<SingletonMonoBehavior>()) : mInstance;
	public static GameState getInstance() {
		if (mInstance == null) {
			mInstance = (new GameObject("GameStateContainer")).AddComponent<GameState>();
			System.Guid myGUID = System.Guid.NewGuid();
			mInstance.id = myGUID.ToString();
			Debug.Log("created with id: " + mInstance.id);
			//Do I need: DontDestroyOnLoad(this.gameObject); or mInstance.gameObject?
			DontDestroyOnLoad(mInstance.gameObject);

		}
		return mInstance;
	}

	int i = 0;
	/*
	public List<Card> cards;

	public List<Card> getCards() {
		return cards;
	}
	*/
	private void Start() {
		//Debug.Log("minstance start");
	}

	private void Update() {
		i++;
		if (i < 5) {
			//Debug.Log("I'm updating");
		}
	}

	public void test() {
		//Debug.Log("tested fine!");
	}

	/// <summary>
	/// Stop fucking with this. It works.
	/// </summary>
	public static class PRNG {

		//m = int.MaxValue (ie 2,147,483,647)
		//internal const uint a = 1_103_515_245;
		//internal const uint c = 12_345;

		/**
		 * set seed with a 31 bit unsigned integer
		 * between 1 and 0X7FFFFFFE inclusive. don't use 0!
		 */
		public static int seed = 0x7FFFFFFE;

		/**
		* provides the next pseudorandom number
		* as an unsigned integer (31 bits)
		*/
		public static int randomInt() {
			//Convert the possible 0 - 4_294_967_295 range to min int - max int range.
			return gen();
		}

		//Return a random number from 0 up to but not including 1.
		public static float random() {

			//eg you can have x = 0..9
			// y = 10
			//how do you get a real number between 0 and 1?

			//0 / 10 = 0
			//9 / 10 = .9

			return gen() / (float) int.MaxValue;
		}

		/**
		 * generator:
		 * new-value = (old-value * 16807) mod (2^31 - 1)
		 * 
		 * X0 , the starting value; X0 ≥ 0 .
		 * a, the multiplier; a ≥ 0.
		 * c, the increment; c ≥ 0.
		 * m, the modulus; m > X0, m > a, m > c.
		 * x[n+1] = ax[n] + c
		 */
		private static int gen() {
			//https://rosettacode.org/wiki/Linear_congruential_generator#Java
			//2,147,483,647
			seed = (seed * 1_103_515_245 + 12_345) & int.MaxValue;
			return seed;
		}
	}
}