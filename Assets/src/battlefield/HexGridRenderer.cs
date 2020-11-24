using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HexGridRenderer : MonoBehaviour {
	public const float ROOT3 = 1.7320508075f; //this gets truncated to 1.732051
	public const float TAN30 = ROOT3 / 2;
	public const float CELL_HEIGHT = 60;
	public const float CELL_WIDTH = CELL_HEIGHT * TAN30;

	//Not sure what I need this for yet...
	public const float PIDIV6 = Mathf.PI / 6;
	
	public static Vector3[] hexPoints = {
		new Vector3(0f, CELL_HEIGHT / 2),
		new Vector3(CELL_WIDTH / 2, CELL_HEIGHT / 4),
		new Vector3(CELL_WIDTH / 2, -CELL_HEIGHT / 4),
		new Vector3(0f, -CELL_HEIGHT / 2),
		new Vector3(-CELL_WIDTH / 2, -CELL_HEIGHT / 4),
		new Vector3(-CELL_WIDTH / 2, CELL_HEIGHT / 4)
	};

	private static Vector2[] hexUV = {
		new Vector2(0.5f, 0.5f),
		new Vector2(0.5f, 1),
		new Vector2(1, 1),
		new Vector2(1, 0),
		new Vector2(0.5f, 0),
		new Vector2(0, 0),
		new Vector2(0, 1)
	};

	public HexGrid hexGrid;
	//public List<ArenaData.Terrain> terrain; //later to be replaced by hex entities.
	public List<BattlefieldEntity> battlefieldEntities;

	public ImageLibrary imageLibarary;
	
	private MeshFilter meshFilter;
	private MeshCollider meshCollider;

	//A list of sprite rendering entities on the field.
	//To be used later.
	//private List<EntityRenderer> entityRenderers;
	private Dictionary<string, EntityRenderer> entityRenderers;

	//Temporary for testing terrain blocking.
	//public List<Vector2Int> blockedHexes = new List<Vector2Int>();

	private Dictionary<string, Sprite> spriteCache; //possibly replace imageLibrary with this.
	
	private List<Vector2Int> hilightPos = new List<Vector2Int>();

	private List<Vector2Int> hilight2Pos = new List<Vector2Int>();

	public static Vector3 getXYZPos(Vector2Int xyPos) {
		return getXYZPos(xyPos.x, xyPos.y);
	}

	public static Vector3 getXYZPos(int x, int y) {
		//float xPos = CELL_WIDTH / 2;
		//float yPos = CELL_HEIGHT / 2;
		float xOffset = y * CELL_WIDTH / 2;
		float xPos = x * CELL_WIDTH + xOffset;// + CELL_WIDTH / 2;
		float yPos = y * CELL_HEIGHT * 3 / 4;// + CELL_HEIGHT / 2;
		return new Vector3(xPos, yPos);
	}

	/// <summary>
	/// Not sure if we need this override method...
	/// </summary>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <returns></returns>
	public static Vector3 getXYZPos(float x, float y) {
		//float xPos = CELL_WIDTH / 2;
		//float yPos = CELL_HEIGHT / 2;
		float xOffset = y * CELL_WIDTH / 2;
		float xPos = x * CELL_WIDTH + xOffset;// + CELL_WIDTH / 2;
		float yPos = y * CELL_HEIGHT * 3 / 4;// + CELL_HEIGHT / 2;
		return new Vector3(xPos, yPos);
	}

	public Vector2Int[] getHexPos() {
		return hexGrid.hexPos;
	}

	/// <summary>
	/// Takes a world point position on the hex grid renderer and returns the hex position that it corresponds to.
	/// </summary>
	/// <param name="point">Point in world space that lies on the hex renderer</param>
	/// <returns>Hex position</returns>
	public Vector2Int getHexPosFromWorldPoint(Vector3 point) {
		return getGridXY(transform.InverseTransformPoint(point));
	}

	public void clearHilight() {
		hilightPos = new List<Vector2Int>();
	}

	public void setHilight(Vector2Int[] posArray) {
		foreach (Vector2Int pos in posArray) {
			setHilight(pos);
		}
	}

	public void setHilight(Vector2Int pos) {
		setHilight(pos, true);
	}

	public void setHilight(Vector2Int pos, bool isHilight) {
		if (isHilight) {
			if (!hilightPos.Contains(pos)) {
				//Don't add multiple entries.
				hilightPos.Add(pos);
			}
		} else {
			hilightPos.Remove(pos);
		}
	}

	public void clearHilight2() {
		hilight2Pos = new List<Vector2Int>();
	}

	public void setHilight2(Vector2Int[] posArray) {
		foreach (Vector2Int pos in posArray) {
			setHilight2(pos);
		}
	}

	public void setHilight2(Vector2Int pos) {
		setHilight2(pos, true);
	}

	public void setHilight2(Vector2Int pos, bool isHilight) {
		if (isHilight) {
			if (!hilight2Pos.Contains(pos)) {
				//Don't add multiple entries.
				hilight2Pos.Add(pos);
			}
		} else {
			hilight2Pos.Remove(pos);
		}
	}

	public bool isHilighted2(Vector2Int pos, bool isHilight=true) {
		return hilight2Pos.Contains(pos) == isHilight;
	}

	private void Awake() {
		meshFilter = gameObject.AddComponent<MeshFilter>();
		//MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
		meshCollider = gameObject.AddComponent<MeshCollider>();
		spriteCache = new Dictionary<string, Sprite>();
		entityRenderers = new Dictionary<string, EntityRenderer>();
	}

	// Start is called before the first frame update
	//This is not the place to setup display based on properties loaded asychronously.
	void Start() {
		
	}

	public void updateDisplay() {
		meshFilter.mesh = meshCollider.sharedMesh = createGridMesh(hexGrid.hexPos, true);
		if (battlefieldEntities != null) {
			foreach (BattlefieldEntity entity in battlefieldEntities) {
				createEntityRenderer(entity);
			}
		} else {
			Debug.Log("battlefieldEntities not set");
		}
		if (!positionLabelsCreated) {
			createPositionLabels();
			positionLabelsCreated = true;
		}
	}

	private bool positionLabelsCreated = false;

	private void createPositionLabels() {
		//Text test
		RectTransform rectTransform;
		Canvas textCanvas = GetComponentInChildren<Canvas>();

		foreach (Vector2Int hex in getHexPos()) {
			GameObject newText = new GameObject("Hex " + hex.ToString(), typeof(RectTransform));

			Text newTextComp = newText.AddComponent<Text>();
			newTextComp.text = hex.ToString();

			Font font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
			newTextComp.font = font;
			newTextComp.alignment = TextAnchor.MiddleCenter;
			newTextComp.fontSize = 12;

			newText.transform.SetParent(textCanvas.transform, false);
			Vector3 pos = getXYZPos(hex.x, hex.y);

			newText.transform.localPosition = pos;

			rectTransform = newText.GetComponent<RectTransform>();

			//To work with the label as a centered pic (h/w don't really matter)
			//Anchor is relative the the text canvas' sizeDelta
			rectTransform.anchorMin = new Vector2(.5f, .5f);
			rectTransform.anchorMax = new Vector2(.5f, .5f);
			//Pivot is relative the the text components sizeDelta
			rectTransform.pivot = new Vector2(.5f, .5f);
			rectTransform.sizeDelta = new Vector2(40, 24);
		}
	}

	private void createEntityRenderer(BattlefieldEntity entity) {
		//Debug.Log("create renderer: " + entity.name + " image: " + entity.image);
		EntityRenderer spriteHolder;
		if (!entityRenderers.ContainsKey(entity.name)) {
			if (entity.image == null) {
				//For now terrain with no image is aborted.
				return;
			}

			//Not sure we should recycle/share sprites like this.
			Sprite entitySprite;
			if (spriteCache.ContainsKey(entity.image)) {
				entitySprite = spriteCache[entity.image];
			} else {
				byte[] pngBytes = imageLibarary.getImageBytesById(entity.image);
				Texture2D tex = new Texture2D(2, 2);
				tex.LoadImage(pngBytes);
				//entitySprite = spriteCache[entity.image] = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.0f), 5.0f);
				int maxPixelSize = Mathf.Max(tex.width, tex.height);
				float pixelsPerUnit = maxPixelSize / 36f;
				entitySprite = spriteCache[entity.image] = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.0f), pixelsPerUnit);
				//180 pixels = 5 pixels per unit
				//36 pixels units



				//Debug.Log("pixs per unit: " + entitySprite.pixelsPerUnit);
			}
			spriteHolder = EntityRenderer.instantiate(entity, entitySprite);
			spriteHolder.name = entity.name;
			spriteHolder.transform.parent = gameObject.transform;
			//spriteRenderer = spriteHolder.gameObject.AddComponent<SpriteRenderer>() as SpriteRenderer;
			entityRenderers.Add(entity.name, spriteHolder);
		} else {
			spriteHolder = entityRenderers[entity.name];
		}

		

		Vector3 pos = getXYZPos(entity.pos);
		//pos.y = pos.y - CELL_HEIGHT / 4;
		spriteHolder.transform.localPosition = pos;

		//In the future possibly look at camera:
		// MyTransform.forward = MyCameraTransform.forward;
		//or myTransform.forward = -myCameraTransform.forward;
		//This gets the Main Camera from the Scene
		Camera mainCamera = Camera.main;

		//spriteHolder.transform.eulerAngles = new Vector3(90, 0, 0);
		spriteHolder.transform.forward = mainCamera.transform.forward;
		
		//Temporary to help judge positioning.
		GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		//sphere.transform.position = transform.TransformPoint(pos);
		sphere.transform.parent = gameObject.transform;
		sphere.transform.localPosition = pos;
		sphere.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
	}

	public EntityRenderer getEntityRendererByName(string name) {
		if (entityRenderers.ContainsKey(name)) {
			return entityRenderers[name];
		} else {
			return null;
		}
	}
	
	// Update is called once per frame
	void Update() {
		
	}

	/// <summary>
	/// Touch a cell and highlight it from a raycast Vector3 position.
	/// </summary>
	/// <returns>
	/// The hexPos (Vector2Int x, y) that was touched
	/// </returns>
	public Vector2Int touch(Vector3 point) {
		//Implicity convert to Vector2
		Vector2 localPoint = transform.InverseTransformPoint(point);

		//Vector3 localPoint = transform.InverseTransformPoint(point);
		//GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		//Debug.Log("localPoint: " + localPoint + " " + Time.frameCount);
		//So I think I want to use local point for determining hex mesh position, but to actually display we need to transform it again.
		//sphere.transform.position = transform.TransformPoint(localPoint);
		Vector2Int p = getGridXY(localPoint);

		Vector3 xyzPos = getXYZPos(p.x, p.y);
		//sphere.transform.position = transform.TransformPoint(xyzPos);
		//Debug.Log("localPoint: " + localPoint + " - p: " + p);

		return p;
	}

	

	public void reDrawColor() {
		Color pink = new Color(1, 0.5f, 0.5f, 1);
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		Vector2Int[] hexPos = hexGrid.hexPos;
		List<Color> colors = new List<Color>();
		for (int i = 0; i < hexPos.Length; i++) {
			int x = hexPos[i].x;
			int y = hexPos[i].y;

			if (hilightPos.Contains(new Vector2Int(x, y))) {
				colors.AddRange(new Color[] { Color.white, Color.red, Color.red, Color.red, Color.red, Color.red, Color.red });
			} else if (hilight2Pos.Contains(new Vector2Int(x, y))) {
				colors.AddRange(new Color[] { Color.white, pink, pink, pink, pink, pink, pink });
			} else {
				//if (x == p.x && y == p.y) {
				//	colors.AddRange(new Color[] { Color.yellow, Color.yellow, Color.yellow, Color.yellow, Color.yellow, Color.yellow, Color.yellow });
				//} else {
					colors.AddRange(new Color[] { Color.white, Color.white, Color.white, Color.white, Color.white, Color.white, Color.white });
				//}
			}
		}
		mesh.colors = colors.ToArray();
	}
	public Vector2Int getGridXYFromWorldPos(Vector3 worldPos) {
		//Implicity convert to Vector2
		Vector2 localPoint = transform.InverseTransformPoint(worldPos);
		return getGridXY(localPoint);
	}
	Vector2Int getGridXY(Vector2 localXY) {
		float size = CELL_HEIGHT / 2;

		float x = ((localXY.x * ROOT3 / 3) - localXY.y / 3) / size;
		float y = localXY.y * 2 / 3 / size;
		float z = -x - y;
		
		//Complicated logic for rounding to the nearest hex x/y successfully.
		int rx = (int) Mathf.Round(x);
		int ry = (int) Mathf.Round(y);
		int rz = (int) Mathf.Round(z);

		float dx = Mathf.Abs(rx - x);
		float dy = Mathf.Abs(ry - y);
		float dz = Mathf.Abs(rz - z);

		if ((dx > dy) && (dx > dz)) {
			rx = -ry - rz;
		} else if (dy > dz) {
			ry = -rx - rz;
		} else {
			//Not currently used.
			rz = -rx - ry;
		}
		
		return new Vector2Int(rx, ry);
	}
	
	/**
	 * Iterate over an array of x/y pairs and return false if any duplicate is found.
	 * @return bool true if this array contains only unique pairs.
	 */
	private bool containsUniquePairs(int[] xys) {
		for (int i = 0; i < xys.Length - 2; i += 2) {
			int x = xys[i];
			int y = xys[i + 1];
			for (int j = i + 2; j < xys.Length; j += 2) {
				if (x == xys[j] && y == xys[j + 1]) {
					return false;
				}
			}
		}
		return true;
	}

	private void OnDrawGizmos() {
		Gizmos.color = Color.white;
		Gizmos.DrawWireMesh(createGridMesh(previewPos.ToArray()));
	}

	public List<Vector2Int> previewPos = new List<Vector2Int>();

	private Mesh createGridMesh(Vector2Int[] hexPos, bool useRaw=false) {
		List<Vector3> vertices = new List<Vector3>();
		List<int> triangles = new List<int>();
		List<Vector2> uv = new List<Vector2>();
		// create new colors array where the colors will be created.
		//Color[] colors = new Color[vertices.Count];
		List<Color> colors = new List<Color>();
		//Vector2Int[] hexPos = useRaw ? hexGrid.hexPos : previewPos.ToArray();
		//Vector2Int[] hexPos = hexGrid.hexPos;
		for (int i = 0; i < hexPos.Length; i++) {
			int x = hexPos[i].x;
			int y = hexPos[i].y;
			Vector3 xyzPos = getXYZPos(x, y);
			//Something odd happens with OnDrawGizmos where un-transformed vertices render correctly.
			//But when in play-mode, vertices need to be transformed.
			if (useRaw) {
				vertices.AddRange(getHexagonVertices(xyzPos));
			} else {
				//Vertices have to be in world space unfortunately!
				vertices.AddRange(transformVectors(getHexagonVertices(xyzPos), transform));
			}
			uv.AddRange(hexUV);

			//Two vectors are equal if their X and Y elements are equal.
			//Do this until we switch to use Vector2Int[] for all positions.
			Vector2Int pos = new Vector2Int(x, y);
			//Debug.Log("hilights: " + hilightPos);
			if (hilightPos.Contains(pos)) {
				colors.AddRange(new Color[] { Color.white, Color.red, Color.red, Color.red, Color.red, Color.red, Color.red });
			} else {
				colors.AddRange(new Color[] { Color.white, Color.white, Color.white, Color.white, Color.white, Color.white, Color.white });
			}

			int n = vertices.Count;
			//TR
			triangles.Add(n - 7);
			triangles.Add(n - 6);
			triangles.Add(n - 5);
			//R
			triangles.Add(n - 7);
			triangles.Add(n - 5);
			triangles.Add(n - 4);
			//BR
			triangles.Add(n - 7);
			triangles.Add(n - 4);
			triangles.Add(n - 3);
			//BL
			triangles.Add(n - 7);
			triangles.Add(n - 3);
			triangles.Add(n - 2);
			//L
			triangles.Add(n - 7);
			triangles.Add(n - 2);
			triangles.Add(n - 1);
			//TL
			triangles.Add(n - 7);
			triangles.Add(n - 1);
			triangles.Add(n - 6);
		}

		Mesh mesh = new Mesh();
		mesh.name = "Gizmo Mesh";
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.uv = uv.ToArray();
		
		mesh.colors = colors.ToArray();

		mesh.RecalculateNormals();

		return mesh;
	}

	/**
	 * Given a local coordinate point, return a fully transformed set of indices for a hexagon mesh.
	 */
	private Vector3[] getHexagonVertices(Vector3 center) {
		return new Vector3[7] {
			center,
			center + hexPoints[0],
			center + hexPoints[1],
			center + hexPoints[2],
			center + hexPoints[3],
			center + hexPoints[4],
			center + hexPoints[5]
		};
	}

	private Vector3[] transformVectors(Vector3[] vectors, Transform transform) {
		for (int i = 0; i < vectors.Length; i++) {
			vectors[i] = transform.TransformPoint(vectors[i]);
		}
		return vectors;
	}
}