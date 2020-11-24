using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class ImageLibrary {

	private Dictionary<string, byte[]> rawImageBytes;

	public void clearImageCache() {
		rawImageBytes = new Dictionary<string, byte[]>();
	}

	public async Task<bool> load(List<Image> images) {
		try {
			if (rawImageBytes == null) {
				clearImageCache();
			}
			int n = rawImageBytes.Keys.Count + images.Count;
			List<Task> tasks = new List<Task>();
			foreach (Image image in images) {
				//load image
				tasks.Add(loadToCache(image.id, image.file, rawImageBytes));
			}
			await Task.WhenAll(tasks);
			return rawImageBytes.Keys.Count == n;
		} catch (Exception e) {
			Debug.Log("error on load: " + e.Message);
			return false;
		}
	}

	public byte[] getImageBytesById(string id) {
		if (rawImageBytes.ContainsKey(id)) {
			return rawImageBytes[id];
		} else {
			return null;
		}
	}

	private static async Task<Boolean> loadToCache(string id, string filename, Dictionary<string, byte[]> cache) {
		byte[] imageBytes = await loadImageBytes(filename);
		if (imageBytes != null) {
			cache.Add(id, imageBytes);
			return true;
		} else {
			Debug.Log("fail on: " + filename);
			return false;
		}
	}

	private static async Task<byte[]> loadImageBytes(string filename) {
		//Debug.Log("load image bytes: " + filename);
		string path = Application.streamingAssetsPath + Path.DirectorySeparatorChar + filename;
		if (File.Exists(path)) {
			using (FileStream SourceStream = File.Open(path, FileMode.Open)) {
				byte[] result = new byte[SourceStream.Length];
				await SourceStream.ReadAsync(result, 0, (int)SourceStream.Length);
				return result;
			}
			//tex.LoadImage(pngBytes);
		} else {
			Debug.Log("image not found: " + filename);
			return null;
		}
	}

	[Serializable]
	public struct Image {
		public string id;
		public string file;
	}
}
