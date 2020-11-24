using System.Collections.Generic;
using UnityEngine;

public class HexPathRenderer : MonoBehaviour {
	public List<Vector2Int> pathPos = new List<Vector2Int>();

	public Shader shader;

	private MeshFilter meshFilter;
	
	private void Awake() {
		meshFilter = gameObject.AddComponent<MeshFilter>();
		MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
		Material material = meshRenderer.material;
		material.shader = shader;
		Debug.Log("material: " + material.name + " shader: " + material.shader);
	}
	// Start is called before the first frame update
	void Start() {
		if (pathPos.Count == 0) {
			Debug.Log("NO PATH TO DRAW");
		} else {
			meshFilter.mesh = createPathMesh(true);
		}
	}

    // Update is called once per frame
    void Update() {
        
    }


	/// <summary>
	/// TODO: remove this method and either make it live in Update() or on demand via getter/setter
	/// </summary>
	public void updateOnDemand() {
		if (pathPos.Count == 0) {
			meshFilter.mesh.Clear();
		} else {
			meshFilter.mesh = createPathMesh(true);
		}
	}
	
	private void OnDrawGizmos() {
		Gizmos.color = Color.red;
		if (pathPos.Count == 0) {

		} else {
			Gizmos.DrawWireMesh(createPathMesh());
		}
	}

	/// <summary>
	/// This is a temporary place holder for drawing paths until I get one I like.
	/// </summary>
	/// <param name="useRaw"></param>
	/// <returns></returns>
	private Mesh createPathMesh(bool useRaw = false) {

		List<Vector3> vertices = new List<Vector3>();
		List<int> triangles = new List<int>();
		List<Vector2> uv = new List<Vector2>();
		// create new colors array where the colors will be created.
		//Color[] colors = new Color[vertices.Count];
		List<Color> colors = new List<Color>();

		Vector2Int[] hexPos = pathPos.ToArray();
		
		int prevX = hexPos[0].x;
		int prevY = hexPos[0].y;
		Vector3 prevXYZPos = HexGridRenderer.getXYZPos(prevX, prevY);

		for (int i = 1; i < hexPos.Length; i++) {
			int x = hexPos[i].x;
			int y = hexPos[i].y;
			Vector3 xyzPos = HexGridRenderer.getXYZPos(x, y);
			
			Vector2 mXYZ = getPointOnVector(prevXYZPos, xyzPos, HexGridRenderer.CELL_WIDTH / 2);

			Vector2 p1;
			Vector2 p2;
			Vector2 p3;
			Vector2 p4;

			var p2a = getPointOnVector(prevXYZPos, xyzPos, HexGridRenderer.CELL_WIDTH * 0.1f);
			var p2b = getPointOnVector(prevXYZPos, xyzPos, HexGridRenderer.CELL_WIDTH * 0.9f);
			var dist = HexGridRenderer.CELL_WIDTH * 0.8f;
			//standard form
			//Ax + By = C
			var x1 = p2a.x;
			var y1 = p2a.y;
			var x2 = p2b.x;
			var y2 = p2b.y;

			var dx = x1 - x2;
			var dy = y1 - y2;
			//Don't need to calc dist, it is always constant.
			//var dist = Mathf.Sqrt(dx * dx + dy * dy);
			dx /= dist;
			dy /= dist;
			//Debug.Log("dx: " + dx + " dy: " + dy);
			
			var x3 = x1 + (2) * dy;
			var y3 = y1 - (2) * dx;
			var x4 = x1 - (2) * dy;
			var y4 = y1 + (2) * dx;

			p1 = new Vector2(x3, y3);
			p2 = new Vector2(x4, y4);

			var x5 = x2 + (2) * dy;
			var y5 = y2 - (2) * dx;
			var x6 = x2 - (2) * dy;
			var y6 = y2 + (2) * dx;

			p3 = new Vector2(x5, y5);
			p4 = new Vector2(x6, y6);

			if (useRaw) {
				//vertices.AddRange(getHexagonVertices(xyzPos));
				vertices.Add(p1);
				vertices.Add(p2);
				vertices.Add(p3);
				vertices.Add(p4);
			} else {
				//Vertices have to be in world space unfortunately!
				//vertices.AddRange(transformVectors(getHexagonVertices(xyzPos), transform));
				vertices.Add(transform.TransformPoint(p1));
				vertices.Add(transform.TransformPoint(p2));
				vertices.Add(transform.TransformPoint(p3));
				vertices.Add(transform.TransformPoint(p4));
			}
			//Gizmos.DrawSphere(transform.TransformPoint(p1), 1);
			//Gizmos.DrawSphere(transform.TransformPoint(p2), 2);
			int n = vertices.Count;
			
			//TR
			triangles.Add(n - 4);
			triangles.Add(n - 2);
			triangles.Add(n - 1);

			triangles.Add(n - 1);
			triangles.Add(n - 3);
			triangles.Add(n - 4);

			prevX = x;
			prevY = y;
			prevXYZPos = xyzPos;

			colors.AddRange(new Color[] { Color.red, Color.red, Color.gray, Color.gray });
		}


		Mesh mesh = new Mesh();
		mesh.name = "Placeholder Mesh";
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.uv = uv.ToArray();

		mesh.colors = colors.ToArray();

		mesh.RecalculateNormals();

		return mesh;
	}

	public static Vector2 getMidPoint(Vector2 p1, Vector2 p2) {
		return new Vector2(
			(p1.x + p2.x) / 2, (p1.y + p2.y) / 2
		);
	}

	public static Vector2 getPointOnVector(Vector2 p1, Vector2 p2, float t) {
		var dx = p2.x - p1.x;
		var dy = p2.y - p1.y;
		var d = Mathf.Sqrt(dx * dx + dy * dy);
		var dt = t / d;
		return new Vector2(p1.x + dx * dt, p1.y + dy * dt);
	}

	private Mesh createPathMeshOld(bool useRaw = false) {
		List<Vector3> vertices = new List<Vector3>();
		List<int> triangles = new List<int>();
		List<Vector2> uv = new List<Vector2>();
		// create new colors array where the colors will be created.
		//Color[] colors = new Color[vertices.Count];
		List<Color> colors = new List<Color>();
		Vector2Int[] hexPos = pathPos.ToArray();
		int x = hexPos[0].x;
		int y = hexPos[0].y;
		Vector3 xyzPos = HexGridRenderer.getXYZPos(x, y);

		int prevIndex = -1;

		Vector3 prevP1 = new Vector3();
		Vector3 prevP2 = new Vector3();

		for (int i = 1; i < hexPos.Length; i++) {
			int nextX = hexPos[i].x;
			int nextY = hexPos[i].y;

			//Look at the next pos to figure out an angle which tells us where the verts angle at THIS point.
			float angle = HexGrid.getAngle(x, y, nextX, nextY);

			Debug.Log("x: " + x + " y: " + y + " nextX: " + nextX + " nextY: " + nextY + " angle: " + angle);


			int index = (int)HexGrid.getAngle(x, y, nextX, nextY) / 60;

			Debug.Log("index: " + index);


			Vector3 p1;
			Vector3 p2;
			if (prevIndex == -1) {
				Vector3 hexPoint = HexGridRenderer.hexPoints[index] * 0.25f;
				p1 = xyzPos + hexPoint;
				//p1.x += 1; all I have to do... but use the vectors...
				Gizmos.DrawSphere(transform.TransformPoint(p1), 2);

				p2 = xyzPos - hexPoint;
				Gizmos.DrawSphere(transform.TransformPoint(p2), 2);
			} else {
				Vector3 hexPoint = HexGridRenderer.hexPoints[prevIndex] * 0.25f;
				Vector3 hexPoint2 = HexGridRenderer.hexPoints[index] * 0.25f;


				//So p1 and p2 actually give us the line to get the intersect for line on vector prevP1 and prevP2

				p1 = xyzPos + (hexPoint + hexPoint2) / 2;
				Gizmos.DrawSphere(transform.TransformPoint(p1), 2);

				p2 = xyzPos - (hexPoint + hexPoint2) / 2;
				Gizmos.DrawSphere(transform.TransformPoint(p2), 2);

				//p1 = xyzPos + hexPoint2.normalized * HexGridRenderer.CELL_WIDTH; 
				//p2 = xyzPos + hexPoint2.normalized * HexGridRenderer.CELL_WIDTH;

				Gizmos.DrawLine(transform.TransformPoint(prevP1), transform.TransformPoint(p1));
				Gizmos.DrawLine(transform.TransformPoint(prevP2), transform.TransformPoint(p2));

			}

			prevP1 = p1;
			prevP2 = p2;
			prevIndex = index;



			//Vector3 hexPoint = HexGridRenderer.hexPoints[index];
			//Vector3 p1 = xyzPos + hexPoint * .5f;
			//Gizmos.DrawSphere(p1, 2);

			x = nextX;
			y = nextY;


			Vector3 nextXYZPos = HexGridRenderer.getXYZPos(nextX, nextY);

			

			Gizmos.DrawLine(transform.TransformPoint(xyzPos), transform.TransformPoint(nextXYZPos));
			xyzPos = nextXYZPos;
			
			//Something odd happens with OnDrawGizmos where un-transformed vertices render correctly.
			//But when in play-mode, vertices need to be transformed.
			if (useRaw) {
				//vertices.AddRange(getHexagonVertices(xyzPos));
			} else {
				//Vertices have to be in world space unfortunately!
				//vertices.AddRange(transformVectors(getHexagonVertices(xyzPos), transform));
			}
			//uv.AddRange(hexUV);

			//Two vectors are equal if their X and Y elements are equal.
			//Do this until we switch to use Vector2Int[] for all positions.
			//Vector2Int pos = new Vector2Int(x, y);
			
			//colors.AddRange(new Color[] { Color.white, Color.red, Color.red, Color.red, Color.red, Color.red, Color.red });
			

			/*
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
			*/
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
}