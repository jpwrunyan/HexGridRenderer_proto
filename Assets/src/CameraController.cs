using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	private const float speed = 5f;

	//private bool mouseDown = false;
	//private Camera camera;

	// Start is called before the first frame update
	void Start() {
		//camera = GetComponent<Camera>();
	}

	// Update is called once per frame
	void Update() {
		/*
		if (Input.GetMouseButtonDown(0)) {
			mouseDown = true;

			Debug.Log("mouse down " + Time.frameCount);
			//transform.Rotate(0, 0, -(Input.GetAxis("Mouse X")) * Time.deltaTime * speed);
			//mouse picking
			Vector3 point = Input.mousePosition;
			Ray ray = camera.ScreenPointToRay(point);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit)) {
				//StartCoroutine(showHit(hit.point));
				HexGridRenderer hexGridRenderer = hit.transform.gameObject.GetComponent<HexGridRenderer>();
				if (hexGridRenderer != null) {
					hexGridRenderer.clearHilight();
					Vector2Int hexPos = hexGridRenderer.touch(hit.point);
					hexGridRenderer.setHilight(
						//HexGrid.getXYsAtRadius(hexPos.x, hexPos.y, 1)
						//HexGrid.getXYsWithinRadius(hexPos.x, hexPos.y, 3)
						HexGrid.getVisibleHexes(
							hexPos,
							hexGridRenderer.blockedHexes.ToArray(),
							0,
							10
						)
						
					);
					//Debug.Log(String.Join(", ", HexGrid.getXYsAtRadius(hexPos.x, hexPos.y, 2)));
					hexGridRenderer.reDrawColor();
				}
			} else {
				Debug.Log("no interaction");
			}

		} else if (Input.GetMouseButtonUp(0)) {
			mouseDown = false;
		}

		if (mouseDown && (Input.GetAxis("Mouse X") < 0 || (Input.GetAxis("Mouse X") > 0))) {
			
		}
		*/
	}

	private IEnumerator showHit(Vector3 pos) {
		GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		sphere.transform.position = pos;
		sphere.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
		yield return new WaitForSeconds(1);
		sphere.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
		yield return new WaitForSeconds(1);
		Destroy(sphere);
	}

	void LateUpdate() {
		/*
		float horInput = Input.GetAxis("Horizontal");
		float vertInput = Input.GetAxis("Vertical");

		float xPos = 0f;
		float zPos = 0f;
		
		if (horInput != 0) {
			xPos = horInput * speed;
		}
		if (vertInput != 0) {
			zPos = vertInput * speed;
		}
		Vector3 pos = new Vector3(xPos, 0, zPos);


		//The offset point is rotated around its own 0, 0 origin.
		//Then it is applied to the global position.
		transform.position += pos;
		*/
	}
}