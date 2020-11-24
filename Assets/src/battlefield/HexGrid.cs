using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGrid {

	public static Vector2Int[] HEX_ROTATIONS = {
		new Vector2Int(0, -1), //SW
		new Vector2Int(-1, 0), //W
		new Vector2Int(-1, 1), //NW
		new Vector2Int(0, 1), //NE
		new Vector2Int(1, 0), //E
		new Vector2Int(1, -1) //SE
	};

	private Vector2Int[] _hexPos;

	public Vector2Int[] hexPos {
		get {
			return _hexPos;
		}
	}

	/// <summary>
	/// Take an angle between 0-360 and return the clockwise direction.
	/// </summary>
	/// <param name="angle">The Euler angle to get a direction from</param>
	/// <returns>A direction from the "center" that this angle represents</returns>
	public static Vector2Int getHexDirectionAtAngle(float angle) {
		return HEX_ROTATIONS[Mathf.FloorToInt(angle / 60)];
	}

	/// <summary>
	/// Returns the Manhattan distance of two Hexes. (Should always be an int. Using Number for now)
	/// </summary>
	/// <param name="x1">start pos x</param>
	/// <param name="y1">start pos y</param>
	/// <param name="x2">destination x</param>
	/// <param name="y2">destination y</param>
	/// <returns>Manhattan distance between the hexes.</returns>
	public static int distance(int x1, int y1, int x2, int y2) {
		int z1 = -x1 - y1;
		int z2 = -x2 - y2;
		return (Mathf.Abs(x1 - x2) + Mathf.Abs(y1 - y2) + Mathf.Abs(z1 - z2)) / 2;
	}

	/// <summary>
	/// This should return more exact values for angles between hexes than using Math.atan2()
	/// </summary>
	/// <param name="x1">start pos x</param>
	/// <param name="y1">start pos y</param>
	/// <param name="x2">destination x</param>
	/// <param name="y2">destination y</param>
	/// <returns>Angle between hex positions with 0/360 equivalent to 3 o'clock</returns>
	public static float getAngle(int x1, int y1, int x2, int y2) {
		int dist = distance(x1, y1, x2, y2);
		
		int xPos = x1 + dist; //start at 3'oclock, ie origin.x + radius, origin.y
		int yPos = y1;

		int index = 0;
		for (int i = 0; i < 6; i++) {
			for (int j = 0; j < dist; j++) {
				if (xPos == x2 && yPos == y2) {
					index = ((i * dist) + j);
					goto done;
				}
				//This moves along the parameter starting SW direction.
				xPos += HEX_ROTATIONS[i].x;
				yPos += HEX_ROTATIONS[i].y;
			}
		}
		done:
		return index * (360f / (dist * 6));
	}

	public static Vector2Int[] getXYsWithinRadius(int x, int y, int radius, int innerRadius=0) {
		int hexesInRadius(int r) => (r < 1) ? 1 : r * 6 + hexesInRadius(r - 1);
		Vector2Int[] hexXYs;
		if (innerRadius > 0) {
			hexXYs = new Vector2Int[hexesInRadius(radius) - hexesInRadius(innerRadius)];
		} else {
			hexXYs = new Vector2Int[hexesInRadius(radius)];
			hexXYs[0] = new Vector2Int(x, y);
		}
		
		int l = 1;
		for (int r = 1; r <= radius; r++) {
			Array.Copy(getXYsAtRadius(x, y, r), 0, hexXYs, l, r * 6);
			l += r * 6;
		}
		return hexXYs;
	}

	/// <summary>
	/// Get x/y values corresponding to positions around a parimeter centered at x and y, starting at 3 o'clock.
	/// If radius is 0, should return an empty array.
	/// If radius is less than 0, should throw an exception.
	/// </summary>
	/// <param name="x">center x position</param>
	/// <param name="y">center y position</param>
	/// <param name="radius">radius from center position to get perimeter of</param>
	/// <returns>Perimeter hex positions of x, y at radius</returns>
	public static Vector2Int[] getXYsAtRadius(int x, int y, int radius) {
		int l = radius * 6;
		Vector2Int[] hexXYs = new Vector2Int[l];
		//clockwise progression around the paremeter starting at HEX_ROTATIONS[0] position
		//xPos and yPos must be integers because it is possible for a negative value to be calculated if either x or y is 0.
		int xPos = x + radius; //start at 3'oclock, ie origin.x + radius, origin.y
		int yPos = y;
		int index = 1;
		for (int i = 0; i < 6; i++) {
			for (int j = 0; j < radius; j++) {
				//This moves along the parameter starting SW direction.
				xPos += HEX_ROTATIONS[i].x;
				yPos += HEX_ROTATIONS[i].y;
				hexXYs[index++] = new Vector2Int(xPos, yPos);
				if (index == l) {
					index = 0;
				}
			}
		}
		return hexXYs;
	}

	public static Vector2Int[] getVisibleHexes(Vector2Int pov, Vector2Int[] blockedPos, int minRadius, int maxRadius) {
		return getVisibleHexes(pov, blockedPos, minRadius, maxRadius, new float[2] { 0, 360 });

	}

	public static Vector2Int[] getVisibleHexes(Vector2Int pov, Vector2Int[] blockedPos, int minRadius, int maxRadius, float[] visionArcs) {
		//Debug.Log("pov: " + pov.ToString() + " blockedPos: " + String.Join(",", blockedPos) + " minRadius: " + minRadius + " maxRadius: " + maxRadius + " visionArcs: " + String.Join(",", visionArcs));

		//These values need to be mutable during calculation.
		List<Vector2Int> visibleHexes = new List<Vector2Int>();
		List<Vector2Int> blockedHexes = new List<Vector2Int>(blockedPos);
		//This list will be updated as obstructions cut off the field of vision outward.
		List<float> visionArcList = new List<float>(visionArcs);

		if (minRadius == 0) {
			//Add the pov as visible if it's in radius.
			visibleHexes.Add(pov);
			if (blockedHexes.Contains(pov)) {
				//Short-circuit the logic below. No need to check. Nothing is visible beyond the pov.
				maxRadius = 0;
			}
			minRadius++;
		}

		int x = pov.x;
		int y = pov.y;

		for (int r = minRadius; r <= maxRadius; r++) {
			Vector2Int[] checkHexes = getXYsAtRadius(x, y, r);
			int m = checkHexes.Length;
			//Making 360 a float is incredibly important here...
			float angleIncrement = 360f / m;
			int visionArcIndex = 0;
			for (int j = 0; j < m; j++) {
				Vector2Int hex = checkHexes[j];
				float angle = j * angleIncrement;
				int l = visionArcList.Count;
				while (visionArcIndex < l) {
					if (angle >= visionArcList[visionArcIndex]) {
						if (angle <= visionArcList[visionArcIndex + 1]) {
							visibleHexes.Add(hex);
							int foundIndex = blockedHexes.IndexOf(hex);
							//Now check and update vision arcs.
							if (foundIndex != -1) {
								blockedHexes.RemoveAt(foundIndex);
								updateVisionArcs(visionArcList, visionArcIndex, angle, angleIncrement);
							}
							break;
						} else {
							//Not in this arc; check the next one.
							visionArcIndex += 2;
						}
					} else {
						//also not visible.
						break;
					}
				}
				/*
				int foundIndex = blockedHexes.IndexOf(hex);
				//Now check and update vision arcs.
				if (foundIndex != -1) {
					Debug.Log("hex that is blocked: " + hex.ToString());
					blockedHexes.RemoveAt(foundIndex);
					updateVisionArcs(visionArcList, visionArcIndex, angle, angleIncrement);
				}
				*/
			}
		}
		return visibleHexes.ToArray();
	}

	private static void updateVisionArcs(List<float> visionArcList, int visionArcIndex, float angle, float angleIncrement) {
		//Debug.Log("updateVisionArcs: " + String.Join(",", visionArcList) + " visionArcIndex: " + visionArcIndex + " angle: " + angle + " angleIncrement: " + angleIncrement);
		float currentMinAngle = visionArcList[visionArcIndex];
		float currentMaxAngle = visionArcList[visionArcIndex + 1];
		float shadowArcStart = angle - angleIncrement / 2;
		float shadowArcEnd = shadowArcStart + angleIncrement;
		if (angle == 0) {
			int last = visionArcList.Count - 1;
			shadowArcStart = 360 + shadowArcStart; //In this case shadowArcStart will be negative.
			visionArcList[last] = Mathf.Min(visionArcList[last], shadowArcStart);
			visionArcList[0] = Mathf.Max(visionArcList[0], shadowArcEnd);
		} else {
			if (shadowArcStart <= currentMinAngle) {
				if (shadowArcEnd > currentMaxAngle) {
					visionArcList.RemoveRange(visionArcIndex, 2);
					if (visionArcIndex > 0) {
						visionArcIndex -= 2;
					}
				} else if (shadowArcEnd > currentMinAngle) {
					visionArcList[visionArcIndex] = shadowArcEnd;
				}
			} else {
				if (visionArcIndex + 1 < visionArcList.Count) {
					visionArcList[visionArcIndex + 1] = shadowArcStart;
					if (shadowArcEnd < currentMaxAngle) {
						visionArcList.InsertRange(visionArcIndex + 2, new float[2] { shadowArcEnd, currentMaxAngle });
					}
				} else {
					visionArcIndex -= 2;
				}
			}
			if (visionArcList[visionArcIndex] == visionArcList[visionArcIndex + 1]) {
				visionArcList.RemoveRange(visionArcIndex, 2);
			}
		}
	}

	/**
	 * Iterate over an array of x/y pairs and return false if any duplicate is found.
	 * @return bool true if this array contains only unique pairs.
	 */
	private static bool containsUniquePairs(int[] xys) {
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

	public HexGrid(ArenaData data) {
		int[] xys = data.hexes.xy;
		if (containsUniquePairs(xys)) {
			_hexPos = new Vector2Int[xys.Length / 2];
			for (int i = 0; i < xys.Length; i += 2) {
				_hexPos[i / 2] = new Vector2Int(xys[i], xys[i + 1]);
			}
		} else {
			throw new System.ArgumentException("Value must contain only unique x/y pairs.");
		}
	}

	public int indexOfPos (Vector2Int pos) {
		return Array.IndexOf(_hexPos, pos);	
	}
}
