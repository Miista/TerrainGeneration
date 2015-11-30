using UnityEngine;
using System.Collections;
using System;
using System.Runtime.Serialization.Formatters.Binary; //encription to binary file
using System.IO;

//I use this class for storing some data about the heightmap in a file -martino
public class GameControl : MonoBehaviour {

	public static GameControl control;

	public float[,] currentHeightmap;
	public int currentZoom;
	public float[] currentCenter;

	void Awake () 
	{
		if (control == null) {
			DontDestroyOnLoad(gameObject);
			control = this;
		} else if ( control != this){
			Destroy(gameObject);
		}
		
		/* 
		DontDestroyOnLoad(gameObject); 
		works if I attach this script to a game object, 
		let's say I will give it to an empty game object that I can call gameController and it's fine
		the object wont be destroyed
		*/
	}

	public void SetCurrentHeightmapData ( float[,] _heightmap, int _zoom, float[] _centerCoords) {
		currentHeightmap = _heightmap;
		currentZoom = _zoom;
		currentCenter = _centerCoords;
	}


	public void SaveAsCurrent()
	{
		//usage: GameControl.control.SaveAsCurrent()
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Create(Application.persistentDataPath + "/currentHeightsInfo.dat");
		
		HeightmapData data = new HeightmapData();
		data.heightmap = currentHeightmap;
		data.zoomLevel = currentZoom;
		data.centerCoords = currentCenter;

		bf.Serialize(file, data);
		file.Close();
		Debug.Log("file created: " + Application.persistentDataPath + "/currentHeightsInfo.dat");
	}

	public bool LoadCurrent()
	{
		if (File.Exists(Application.persistentDataPath + "/currentHeightsInfo.dat")){
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Open(Application.persistentDataPath + "/currentHeightsInfo.dat", FileMode.Open);
			HeightmapData data = (HeightmapData) bf.Deserialize(file);
			file.Close();
			currentHeightmap = data.heightmap;
			currentZoom = data.zoomLevel;
			currentCenter = data.centerCoords;
			return true;
		} else {
			Debug.Log("ALERT: no current heightmap found");
		}
		return false;
	}

	public bool checkCurrent() {
		return File.Exists(Application.persistentDataPath + "/currentHeightsInfo.dat");
	}

}

[Serializable]
class HeightmapData	
{
	public float[,] heightmap;
	public int zoomLevel;
	public float[] centerCoords;
}