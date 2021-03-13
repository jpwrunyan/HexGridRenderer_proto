using System;
using UnityEngine.SceneManagement;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;
using System.Globalization;

public class StartupLoader : MonoBehaviour {
	private void Awake() {

		GameState test = GameState.getInstance();
		test.test();
		_ = init();

	}
	private async Task init() {
		string arenaFilename = "testBattlefield.json";

		try {
			Dictionary<string, CardMap> cardMaps = new Dictionary<string, CardMap>();
			foreach (CardMap cardMap in JsonHelper.FromJsonArray<CardMap>(await loadJSONFile("cards/baseCards.json"))) {
				cardMaps[cardMap.id] = cardMap;
			}

			//Because it's a struct, arenaData cannot be null
			ArenaData arenaData = JsonUtility.FromJson<ArenaData>(await loadJSONFile(arenaFilename));
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

			//Create terrain entities.
			//This is ad-hoc at the moment.
			List<BattlefieldEntity> battlefieldEntities = createTerrainEntities(arenaData);
			battlefieldEntities.AddRange(createExtraEntities());
			battleState.battlefieldEntities = battlefieldEntities;

			List<Card> cards;
			Card card;

			//Character 1
			Combatant character = new Combatant();
			character.name = "Gunslinger Male";
			character.pos = new Vector2Int(4, 3);
			character.movementModifier = 0;
			character.blocksMovement = true;
			character.movementModifier = 100;
			character.blocksVision = true;
			character.health = 4;
			character.image = "gunslinger_male";
			character.deck = new Deck(getCardsFromCardMaps(character, cardMaps, "walk", "run", "shoot", "dash", "lunge"), "Character 1");

			battleState.addActiveCombatant(character);

			//Character 2
			character = new Combatant();
			character.name = "Medic Female";
			character.pos = new Vector2Int(4, 2);
			character.movementModifier = 0;
			character.blocksMovement = true;
			character.movementModifier = 100;
			character.blocksVision = true;
			character.health = 4;
			character.image = "medic_female";
			character.isAI = true;
			character.deck = new Deck(getCardsFromCardMaps(character, cardMaps, "step", "walk", "run"), "Medic", 3);

			battleState.addActiveCombatant(character);

			//Character 3
			character = new Combatant();
			character.name = "Gunslinger Female";
			character.pos = new Vector2Int(2, 3);
			character.blocksMovement = true;
			character.movementModifier = 100;
			character.blocksVision = true;
			character.health = 4;
			character.image = "gunslinger_female";
			character.isAI = false;
			character.deck = new Deck(getCardsFromCardMaps(character, cardMaps, "walk", "run", "shoot", "dash", "lunge"), "Gunslinger Female", 4);

			battleState.addActiveCombatant(character);

			//Character 4
			character = new Combatant();
			character.name = "Merc Female";
			character.pos = new Vector2Int(2, 4);
			character.blocksMovement = true;
			character.movementModifier = 100;
			character.blocksVision = true;
			character.health = 4;
			character.image = "merc_female";
			character.isAI = false;
			character.deck = new Deck(getCardsFromCardMaps(character, cardMaps, "walk", "run", "run", "shoot", "grenade"), "Merc Female", 3);

			battleState.addActiveCombatant(character);

			//Finish with characters
			battleState.setAllies(
				battleState.getCombatantsInTurnOrder()[0] as Combatant,
				battleState.getCombatantsInTurnOrder()[1] as Combatant
			);

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
			terrainEntity.blocksMovement = entity.blocksMovement;
			if (terrainEntity.blocksMovement) {
				terrainEntity.movementModifier = 100;
			} else {
				terrainEntity.movementModifier = entity.movementModifier;
			}
			terrainEntity.blocksVision = entity.blocksVision;
			terrainEntity.image = entity.image;
			if (entity.color != null) {
				terrainEntity.color = uint.Parse(entity.color, NumberStyles.HexNumber);
			}
			terrainEntity.erect = entity.erect;
			battlefieldEntities.Add(terrainEntity);
		}
		return battlefieldEntities;
	}

	private List<BattlefieldEntity> createExtraEntities() {
		List<BattlefieldEntity> battlefieldEntities = new List<BattlefieldEntity>();
		
		BattlefieldEntity testEntity = new BattlefieldEntity();
		testEntity.name = "door";
		testEntity.pos = new Vector2Int(5, 3);
		testEntity.movementModifier = 1;
		testEntity.blocksMovement = false;
		testEntity.blocksVision = true;
		testEntity.image = "door";
		testEntity.erect = true;
		
		battlefieldEntities.Add(testEntity);
		
		return battlefieldEntities;
	}

	private static List<Card> getCardsFromCardMaps(Combatant cardSource, Dictionary<string, CardMap> cardMaps, params string[] cardMapIds) {
		List<Card> cards = new List<Card>();
		foreach (string cardMapId in cardMapIds) {
			if (cardMaps.ContainsKey(cardMapId)) {
				cards.Add(cardMaps[cardMapId].getCard(cardSource));
			} else {
				cards.Add(new Card());
			}
		}
		return cards;
	}

	private static async Task<string> loadJSONFile(string filename) {
		Debug.Log("load file: " + filename);
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

/// <summary>
/// For de/serialization of JSON arrays.
/// </summary>
public static class JsonHelper {
	public static T[] FromJson<T>(string json) {
		Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
		return wrapper.Items;
	}

	public static T[] FromJsonArray<T>(string json) {
		return FromJson<T>("{\"Items\":" + json + "}");
	}

	public static string ToJson<T>(T[] array, bool prettyPrint=false) {
		Wrapper<T> wrapper = new Wrapper<T>();
		wrapper.Items = array;
		return JsonUtility.ToJson(wrapper, prettyPrint);
	}

	[Serializable]
	private class Wrapper<T> {
		public T[] Items;
	}
}
