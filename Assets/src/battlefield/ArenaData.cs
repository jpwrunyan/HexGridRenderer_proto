using System;
using System.Collections.Generic;

[Serializable]
public struct ArenaData {

	//Making this a struct because it should be short-lived.
	//This class/struct will be a simple data structure, so make the conversion to hex grid part of the
	//hex grid class's constructor.
	public string testVar;
	public HexPositions hexes;
	//public List<List<int>> startPositions;
	public List<HexPositions> startPositions;
	/*
	private string _testVar;
	public string testVar {
		get {
			return this._testVar;
		}
		set {
			this._testVar = value;
		}
	}
	*/
	public List<ImageLibrary.Image> images;
	public List<Terrain> terrain;

	[Serializable]
	public class HexPositions {
		public int[] xy;
	}

	[Serializable]
	public struct Terrain {
		public string name;
		public int x;
		public int y;
		public int movementModifier;
		public bool blocksMovement;
		public bool blocksVision;
		public string image;
	}
}