using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Card map is a hierarchy of different level cards.
/// The card level determines the concrete stats/actions on a card and is based off the character holding this card map.
/// The idea is that as a character's stats increase, the level of the card also increases.
/// 
/// No matter what, the first tier will have no requirements. Even if specified, they will be ignored.
/// 
/// Are card maps used to return a card based on the character?
/// 
/// I'm still not sure how I will eventually structure this. For now using static values.
/// </summary>
[Serializable]
public class CardMap {
	public string id = "undefined";

	//----------------------------------------
	// Base card stats/actions
	//----------------------------------------
	public string title = "Undefined";
	public int attack = 0;
	public int move = 0;
	public int minRange = 1;
	public int maxRange = 1;
	public int radius = 0;
	public int initiative = 1;

	public Card getCard(Combatant cardSource) {
		Card card = new Card();
		card.title = title;
		card.attack = attack;
		card.move = move;
		card.minRange = minRange;
		card.maxRange = maxRange;
		card.radius = radius;
		card.initiative = initiative;
		return card;
	}
	/*
	/// <summary>
	/// A card map level. Each card map will have a collection of levels.
	/// Levels will have pre-requisites that determine if a given character has access to that level.
	/// Levels should be organized in increased difficulty of requirements.
	/// </summary>
	[Serializable]
	public class Tier {
		public string title = "Card Title";
		//----------------------------------------
		// Card stats/actions
		//----------------------------------------
		
		//Attack action values:
		public CardStat attack;
		public CardStat minRange;
		public CardStat maxRange;
		public CardStat radius;
		//Move action values:
		public CardStat move;
		public int moveType = 0; //unused

		[Serializable]
		public class CardStat {
			public int baseValue = 0;
			//So far just one modifier per stat.
			//If statBase is empty or null, no modifier is applied.
			public string statBase = "";
			public float statMultiplier = 1;
		}

		[Serializable]
		public class Requirements {
			public int melee = 0;
			public int shooting = 0;
		}
	}
	*/
}
