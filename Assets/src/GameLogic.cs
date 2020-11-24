using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

public class GameLogic : MonoBehaviour {
	
	public HexGridRenderer hexGridRendererPrefab;

	public HexPathRenderer hexPathRenderer;

	private void Awake() {

		GameState test = GameState.getInstance();
		test.test();
		_ = init();
		
    }

	private async Task init() {
		HexGridRenderer hexGridRenderer = null;
		string arenaFilename = "testBattlefield.json";
		
		try {
			//Because it's a struct, arenaData cannot be null
			ArenaData arenaData = JsonUtility.FromJson<ArenaData>(await loadArenaData(arenaFilename));

			//Debug.Log(string.Join(", ", arenaData.hexes.xy));
			
			ImageLibrary imageLibrary = new ImageLibrary();
			
			bool success = await imageLibrary.load(arenaData.images);

			//Don't instantiate until all properties are loaded.
			hexGridRenderer = Instantiate<HexGridRenderer>(hexGridRendererPrefab);
			hexGridRenderer.name = "Hex Grid Renderer";
			hexGridRenderer.hexGrid = new HexGrid(arenaData);
			//hexGridRenderer.terrain = arenaData.terrain;
			hexGridRenderer.imageLibarary = imageLibrary;
			/*
			HexNodePathMap pathMap = new HexNodePathMap(hexGridRenderer.hexGrid, hexGridRenderer.terrain);
			pathMap.setOrigin(new Vector2Int(3, 3));
			HexNode dest = (HexNode) pathMap.getHexNodeAt(new Vector2Int(6, 4));
			List<Vector2Int> path = new List<Vector2Int>();
			//Debug.Log("Dest: " + dest.hexPos + " travel cost: " + dest.travelCost + " prevNode: " + dest.prevNode);
			while (dest != null) {
				//Debug.Log("Move to: " + ((HexNode) dest.prevNode).hexPos);
				path.Add(dest.hexPos);
				dest = (HexNode) dest.prevNode;
			}
			hexPathRenderer.pathPos = path;
			hexPathRenderer.updateOnDemand();
			*/
			//hexGridRenderer Start() is called
			Debug.Log("renderer property set");
		} catch (Exception e) {
			Debug.Log("Exception in init() procedure: " + e.Message);
		}
		
		
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
