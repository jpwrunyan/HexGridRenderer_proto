using System;
using UnityEngine.SceneManagement;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;

public class StartupLoader : MonoBehaviour {
	private void Awake() {

		GameState test = GameState.getInstance();
		test.test();
		_ = init();

	}
	private async Task init() {
		string arenaFilename = "testBattlefield.json";

		try {
			//Because it's a struct, arenaData cannot be null
			ArenaData arenaData = JsonUtility.FromJson<ArenaData>(await loadArenaData(arenaFilename));
			GameState.PRNG.seed = 10;

			ImageLibrary imageLibrary = new ImageLibrary();

			bool success = await imageLibrary.load(arenaData.images);
			if (success) {
				//Debug.Log("loaded terrain images");
			}
			List<ImageLibrary.Image> additionalImages = new List<ImageLibrary.Image>();
			ImageLibrary.Image im = new ImageLibrary.Image();
			im.id = "gunslinger_male";
			im.file = "images/characters/gunslinger_male.png";
			additionalImages.Add(im);

			im = new ImageLibrary.Image();
			im.id = "medic_female";
			im.file = "images/characters/medic_female.png";
			additionalImages.Add(im);

			im = new ImageLibrary.Image();
			im.id = "gunslinger_female";
			im.file = "images/characters/gunslinger_female.png";
			additionalImages.Add(im);

			im = new ImageLibrary.Image();
			im.id = "merc_female";
			im.file = "images/characters/merc_female.png";
			additionalImages.Add(im);

			im = new ImageLibrary.Image();
			im.id = "door";
			im.file = "door_biohazard.png";
			additionalImages.Add(im);

			success = await imageLibrary.load(additionalImages);
			if (success) {
				//Debug.Log("loaded additional images");
			}
			BattleState battleState = new BattleState();
			battleState.hexGrid = new HexGrid(arenaData);

			//Create BattlefieldEntity entities
			List<BattlefieldEntity> battlefieldEntities = createTerrainEntities(arenaData);
			battlefieldEntities.AddRange(createExtraEntities());
			battleState.battlefieldEntities = battlefieldEntities;

			battleState.combatantIdTurnOrder = new List<int>();
			battleState.combatantIdDecks = new Dictionary<int, Deck>();

			//Character 1
			BattlefieldEntity character = new BattlefieldEntity();
			character.name = "Gunslinger Male";
			character.pos = new Vector2Int(4, 3);
			character.movementModifier = 0;
			character.blocksMovement = true;
			character.blocksVision = true;
			character.health = 4;
			character.image = "gunslinger_male";

			battlefieldEntities.Add(character);
			Deck deck;
			List<Card> cards = new List<Card>();

			Card card;
			card = new Card();
			card.title = "Character 1\nMove and Attack";
			card.move = 1;
			card.attack = 2;
			card.minRange = 1;
			card.maxRange = 1;
			cards.Add(card);

			card = new Card();
			card.title = "Character 1\nMove 2";
			card.move = 2;
			cards.Add(card);

			card = new Card();
			card.title = "Character 1\nMove 4";
			card.move = 4;
			cards.Add(card);

			card = new Card();
			card.title = "Character 1\nAttack 4";
			card.attack = 4;
			card.minRange = 1;
			card.maxRange = 5;
			card.radius = 0;

			cards.Add(card);

			battleState.combatantIdTurnOrder.Add(
				battlefieldEntities.IndexOf(character)
			);
			deck = new Deck(cards);
			battleState.combatantIdDecks.Add(
				battlefieldEntities.IndexOf(character),
				deck
			);
			deck.drawHand();
			
			//Character 2
			character = new BattlefieldEntity();
			character.name = "Medic Female";
			character.pos = new Vector2Int(3, 3);
			character.movementModifier = 0;
			character.blocksMovement = true;
			character.blocksVision = true;
			character.health = 4;
			character.image = "medic_female";
			character.isAI = true;

			battlefieldEntities.Add(character);

			cards = new List<Card>();

			card = new Card();
			card.title = "Medic Move 1";
			card.move = 1;
			cards.Add(card);

			card = new Card();
			card.title = "Medic Move 2";
			card.move = 2;
			cards.Add(card);

			card = new Card();
			card.title = "Medic Move 3";
			card.move = 3;
			cards.Add(card);

			battleState.combatantIdTurnOrder.Add(
				battlefieldEntities.IndexOf(character)
			);

			deck = new Deck(cards);
			battleState.combatantIdDecks.Add(
				battlefieldEntities.IndexOf(character), 
				deck
			);
			deck.handSize = 2;
			deck.drawHand();

			//Character 3
			character = new BattlefieldEntity();
			character.name = "Gunslinger Female";
			character.pos = new Vector2Int(2, 3);
			character.movementModifier = 0;
			character.blocksMovement = true;
			character.blocksVision = true;
			character.health = 4;
			character.image = "gunslinger_female";

			battlefieldEntities.Add(character);

			cards = new List<Card>();

			card = new Card();
			card.title = "Gunslinger\nMove and Attack";
			card.move = 1;
			card.attack = 2;
			card.minRange = 1;
			card.maxRange = 1;
			cards.Add(card);

			card = new Card();
			card.title = "Gunslinger Move 2";
			card.move = 2;
			cards.Add(card);

			card = new Card();
			card.title = "Gunslinger Move 3";
			card.move = 3;
			cards.Add(card);

			battleState.combatantIdTurnOrder.Add(
				battlefieldEntities.IndexOf(character)
			);

			deck = new Deck(cards);
			battleState.combatantIdDecks.Add(
				battlefieldEntities.IndexOf(character),
				deck
			);
			deck.drawHand();

			//Character 4
			character = new BattlefieldEntity();
			character.name = "Merc Female";
			character.pos = new Vector2Int(2, 4);
			character.movementModifier = 0;
			character.blocksMovement = true;
			character.blocksVision = true;
			character.health = 4;
			character.image = "merc_female";
			character.isAI = true;

			battlefieldEntities.Add(character);

			cards = new List<Card>();

			card = new Card();
			card.title = "Merc Move 1";
			card.move = 1;
			cards.Add(card);

			card = new Card();
			card.title = "Throw Grenade 4";
			card.attack = 4;
			card.minRange = 1;
			card.maxRange = 4;
			card.radius = 1;
			cards.Add(card);

			battleState.combatantIdTurnOrder.Add(
				battlefieldEntities.IndexOf(character)
			);

			deck = new Deck(cards);
			battleState.combatantIdDecks.Add(
				battlefieldEntities.IndexOf(character),
				deck
			);
			deck.drawHand();
			
			//Finish with characters


			GameState.getInstance().imageLibrary = imageLibrary;
			GameState.getInstance().arenaData = arenaData;
			GameState.getInstance().battleState = battleState;

			SceneManager.LoadScene("MenuTest");
		} catch (Exception e) {
			Debug.Log("Exception in init() procedure: " + e.Message);
		}


	}

	private List<BattlefieldEntity> createTerrainEntities(ArenaData arenaData) {
		List<BattlefieldEntity> battlefieldEntities = new List<BattlefieldEntity>();

		foreach (ArenaData.Terrain entity in arenaData.terrain) {
			BattlefieldEntity terrainEntity = new BattlefieldEntity();
			terrainEntity.name = entity.name;
			terrainEntity.pos = new Vector2Int(entity.x, entity.y);
			terrainEntity.movementModifier = entity.movementModifier;
			terrainEntity.blocksMovement = entity.blocksMovement;
			terrainEntity.blocksVision = entity.blocksVision;
			terrainEntity.image = entity.image;

			battlefieldEntities.Add(terrainEntity);
		}
		return battlefieldEntities;
	}

	private List<BattlefieldEntity> createExtraEntities() {
		List<BattlefieldEntity> battlefieldEntities = new List<BattlefieldEntity>();

		BattlefieldEntity testEntity = new BattlefieldEntity();
		testEntity.name = "door";
		testEntity.pos = new Vector2Int(5, 3);
		testEntity.movementModifier = 0;
		testEntity.blocksMovement = true;
		testEntity.blocksVision = true;
		testEntity.image = "door";

		battlefieldEntities.Add(testEntity);

		return battlefieldEntities;
	}
	private static async Task<string> loadArenaData(string filename) {
		string path = Application.streamingAssetsPath + Path.DirectorySeparatorChar + filename;
		if (File.Exists(path)) {
			using (StreamReader sr = new StreamReader(path)) {
				return Regex.Replace(await sr.ReadToEndAsync(), @"\/\*[^*]*\*+([^/*][^*]*\*+)*\/", string.Empty, RegexOptions.Singleline);
			}
		} else {
			Debug.Log("CANT FIND FILE");
			return null;
		}
	}
}
