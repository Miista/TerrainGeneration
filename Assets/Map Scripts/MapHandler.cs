using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using RestSharp;


[System.Serializable]
public class BingMapsLocation
{
	public string locality = "trento";
	public string adminDistrict = "";
	public string addressLine = "";
	public string ISOCountryRegion = "IT";
	public int zoom = 12;
	public double latitude;
	public double longitude;
}

public class MapHandler : MonoBehaviour {
	public enum ImagerySet {
		Aerial,
		AerialWithLabels,
		Birdseye,
		BirdseyeWithLabels,
		Road
	}
	public bool loadOnStart = true;
	public ImagerySet imSet;
	public BingMapsLocation centerLocation;
	public bool heights = true;
	public bool allowLoad = true;
	public 	int smoothings = 3; //times I do the smoothing
	public int neighbors = 12; //neighbors I want to use for smoothing height values

	private Texture2D texture;
	private string key = "Aqb8ECpcSTeT8RxLKH-r7SiS5NI7JK2hVF5FZFKap30lls9Nc7fQWH_-OKjYButM";
	private int size = 512; 
	private List<double> centerCoord = new List<double>(2);
	private List<double> firstBbox = new List<double>(4);
	private double realGPSSize;
	private Vector3 firstSpawnPoint;
	private bool firstRequest = true;
	private bool keepGoing = false;

	protected Dictionary<Vector3, float[,]> cachedHeightmaps = new Dictionary<Vector3, float[,] >();
	
	public IEnumerator saveCachedHeighmaps( Vector3 cellSize, Vector3 center ){
		for (int i = 0; i < 9; i++){
				cachedHeightmaps.Add( GetCachedHMIndex(center, cellSize, i) , new float[size, size]);
			}
		Debug.Log ("started");
		string url;
		if (firstRequest)
		{
			url = GetUrlMetadata();
		}
		else {
			url = GetBaseImageryUrl();
			url += imSet + "?";
			url += "mapArea=" + GetBBoxString(cellSize, center);
			url += "&mapSize=" + size + "," + size;
			url += "&mapMetadata=1&key=" + key;
		}
		WWW metaReq = new WWW ( url );
		yield return metaReq;


		MetadataObject metadata = JsonConvert.DeserializeObject<MetadataObject>( metaReq.text );

		if (firstRequest) {
			firstRequest = false;
			firstSpawnPoint = new Vector3(center.x + cellSize.x, center.y, center.z + cellSize.z);
			firstBbox = metadata.resourceSets[0].resources[0].bbox;
			realGPSSize = firstBbox[3] - firstBbox[1]; //east - west
		}

		List<double> bbox = metadata.resourceSets[0].resources[0].bbox;

		url = "http://dev.virtualearth.net/REST/v1/Elevation/Bounds?bounds=";
		url += bbox[0] + "," + bbox[1] + "," +  bbox[2] + "," +  bbox[3];
		int rows = 32;
		url += "&rows=" + rows + "&cols=" + rows + "&heights=sealevel&key=" + key;
		//max height point retrievable = 1024 = 32 * 32

		WWW elevReq = new WWW (url);
		yield return elevReq;

		ElevDataObject elevData = JsonConvert.DeserializeObject<ElevDataObject> (elevReq.text);
		List<int> elevations = elevData.resourceSets[0].resources[0].elevations;  //elevations at sea level in meters
		
		//TerrainData terrain = GetComponent<Terrain> ().terrainData;
		int width = 512 * 3 ;
		float[,] heightmapData = new float[width, width]; //terrain.GetHeights(0, 0, width, width);

		if (width % rows == 0)
		{	
			heightmapData = ApplyElevationsToHeightmap (elevations, heightmapData, width);
			heightmapData = Smooth(heightmapData, smoothings, neighbors, width);

			for (int i = 0; i < 9; i++){
				cachedHeightmaps.Add( GetCachedHMIndex(center, cellSize, i) , GetHeightmapForChunk(heightmapData, i));
			}
		}
		else {
			Debug.Log ("Something went wrong: size of terrain is not processable for heightmap generation");
		}
		keepGoing = false;
		Debug.Log ("finished");
		Debug.Log( cachedHeightmaps.Count );
	}

	public string GetBBoxString(Vector3 cellSize, Vector3 center){
		string ret = "";
		int xOffset = (int) (( firstSpawnPoint.x - center.x ) / cellSize.x);
		int zOffset = (int) (( firstSpawnPoint.z - center.z ) / cellSize.z);
		ret += firstBbox[0] + (realGPSSize / 3 * zOffset) + ","; //south
		ret += firstBbox[1] + (realGPSSize / 3 * xOffset) + ","; //west
		ret += firstBbox[2] + (realGPSSize / 3 * zOffset) + ","; //north
		ret += firstBbox[3] + (realGPSSize / 3 * xOffset); //east
		return ret;
	}

	public IEnumerator StartMap(Vector3 cellSize, Vector3 center){
		if ( !cachedHeightmaps.ContainsKey(center))
		{
			keepGoing = true;
			yield return StartCoroutine( saveCachedHeighmaps(cellSize, center) );
		}
	}

	public IEnumerator StopAndWait(){
		while( keepGoing ){
			Debug.Log("waiting..");
			yield return new WaitForSeconds(0.1f);
		}
		Debug.Log("finished waiting");
	}

	//public void Start() {
		//if(loadOnStart) Refresh();	
	//}
	
	public void Refresh() {
		StartCoroutine(_Refresh());
	}

	public Texture2D GetTexture(){
		return texture;
	}
	
	IEnumerator _Refresh (){
		WWW reqImage = new WWW ( GetUrlImage() );
		if (heights == true ){
			if (allowLoad && GameControl.control.LoadCurrent() 
				&& GameControl.control.currentCenter[0] == centerCoord[0]
				&& GameControl.control.currentCenter[1] == centerCoord[1]
				&& GameControl.control.currentZoom == centerLocation.zoom) {
				Debug.Log("heightmap loaded");
			} 
			else {
				WWW reqMeta = new WWW ( GetUrlMetadata() );
				yield return reqMeta;
				StartCoroutine(ApplyBingHeightmapV2(reqMeta, key));
				GameControl.control.LoadCurrent();
				Debug.Log("heightmap created");
			}
		}

		yield return reqImage;
		texture = reqImage.texture;
	}
	
	public string GetUrlMetadata(){
		return GetBaseImageryUrl() + GetGeneralRequest() + "&mapMetadata=1";
	}

	public string GetUrlImage(){
		return GetBaseImageryUrl() + GetGeneralRequest() + "&mapMetadata=0";
	}

	public string GetBaseLocationUrl(){
		return "http://dev.virtualearth.net/REST/v1/Locations?";
	}

	public string GetBaseImageryUrl(){
		return "http://dev.virtualearth.net/REST/v1/Imagery/Map/";
	}

	public string GetGeneralRequest(){
		string url = GetBaseLocationUrl();
		string qs = "";
		qs +=  (centerLocation.locality!= "") ? "locality=" + centerLocation.locality : "";
		qs +=  (centerLocation.adminDistrict!= "") ? "&adminDistrict=" + centerLocation.adminDistrict : "";
		qs +=  (centerLocation.addressLine!= "") ? "&addressLine=" + centerLocation.addressLine : "";
		qs +=  (centerLocation.ISOCountryRegion!= "") ? "&countryRegion=" + centerLocation.ISOCountryRegion : "";

		if (qs != "")
		{
			//StartCoroutine(Geolocate( url, qs ));
			Geolocate2( url, qs);
		}
		else {
			if (centerLocation.latitude != 0 && centerLocation.longitude != 0)
			{
				centerCoord[0] = centerLocation.latitude;
				centerCoord[1] = centerLocation.longitude;				
			}
			else {
				Debug.Log("Something went wrong: no valid coordinates found");
			}
		}
		//set up the request
		url = GetBaseImageryUrl();
		qs = "";
		qs += imSet + "/";
		qs += centerCoord[0] + "," + centerCoord[1] + "/";
		qs += centerLocation.zoom + "?";
		qs += "mapSize=" + size + "," + size;
		qs += "&key=" + key;
		return qs;
	}
	public void Geolocate2( string url, string qs){
		RestClient _client = new RestClient();
		_client.BaseUrl = new System.Uri(url);
		_client.AddDefaultParameter("key", key, ParameterType.GetOrPost);
		var request = new RestRequest();
		request.Resource = qs + "&maxResults=1";
		request.RequestFormat = DataFormat.Json;
        request.JsonSerializer = new RestSharpJsonNetSerializer();
        var response = _client.Get<GeocodedObject>(request);
        if (response.Data.statusDescription == "OK")
		{
			centerCoord = response.Data.resourceSets[0].resources[0].point.coordinates;
		}
		else {
			Debug.Log("Something went wrong: no location retrieved from geolocation");
		}
	}

	public IEnumerator Geolocate( string url, string qs ){
		WWW geocode = new WWW (url + qs + "&maxResults=1&key=" + key);
		yield return geocode;
		GeocodedObject locationJson = JsonConvert.DeserializeObject<GeocodedObject>( geocode.text );
		if (locationJson.statusDescription == "OK")
		{
			centerCoord = locationJson.resourceSets[0].resources[0].point.coordinates;
		}
		else {
			Debug.Log("Something went wrong: no location retrieved from geolocation");
		}
	}

	public void ApplyMapTexture (){
		//need this to add the texture to the terrain so it looks like a map
		List<SplatPrototype> splatList = new List<SplatPrototype>();
		SplatPrototype newSplat = new SplatPrototype();
		newSplat.texture = texture;
		float width = GetComponent<Terrain> ().terrainData.size.x;
		newSplat.tileSize = new Vector2( width, width );
		newSplat.tileOffset = Vector2.zero;
		splatList.Add (newSplat);
		GetComponent<Terrain> ().terrainData.splatPrototypes = splatList.ToArray();
	}

	public int FindChunk (int row, int column){
		return (row * 3) + column ;
	}

	public float[,] GetHeightmapForChunk ( float[,] heightmapData , int chunk ){
		int width = 512 ;
		int gridWidth = 3;
		//int chunk = FindChunk (row, column);

		Vector3 index = GetIndex(chunk , gridWidth, width * 3);

		float[,] heightmap = new float[width, width];
		int m = 0, n = 0;
		//take only the section needed from the big heightmap
		for (int x = (int) index.x ; x < index.x + index.z ; x++){
			for (int y = (int) index.y; y < index.y + index.z ; y++ ){
				heightmap[m,n] = heightmapData[x,y];
				n++;
			}
			n = 0;
			m++;
		}
		return heightmap;
	}


	public Vector3 GetIndex(int i, int gridWidth, int width ){
		// chunks are numbered 0..8
		if ( width % gridWidth == 0)
		{
			//x and y are the offsets in the heightmapData for the required chunk, z is the width in pixels of each chunk
			Vector3 position = new Vector3(0,0,0);
			int index = width / gridWidth;
			switch(i){
				case 0:
				position = new Vector3(0,0, index);
				break;
				case 3://1
				position = new Vector3(index,0, index);
				break;
				case 6: //2
				position = new Vector3(2*index,0, index);
				break;
				case 1: //3
				position = new Vector3(0,index, index);
				break;
				case 4:
				position = new Vector3(index,index, index);
				break;
				case 7: //5
				position = new Vector3(2*index,index, index);
				break;
				case 2: //6
				position = new Vector3(0,2*index, index);
				break;
				case 5: //7
				position = new Vector3(index,2*index, index);
				break;
				case 8:
				position = new Vector3(2*index,2*index, index);
				break;
			}
			return position;
		}
		else {
			Debug.Log("Something went wrong: resolution not valid for this number of chunks");
			Debug.Log(width + "%" + gridWidth + " = " + width%gridWidth);
			return new Vector3();
		}
	}

	public Vector3 GetPosition(int i, int cellWidth ){
		Vector3 position = new Vector3();
		switch(i){
			case 1:
			position = new Vector3(0,0,0);
			break;
			case 2:
			position = new Vector3(cellWidth,0,0);
			break;
			case 3:
			position = new Vector3(2*cellWidth,0,0);
			break;
			case 4:
			position = new Vector3(0,0,cellWidth);
			break;
			case 5:
			position = new Vector3(cellWidth,0,cellWidth);
			break;
			case 6:
			position = new Vector3(2*cellWidth,0,cellWidth);
			break;
			case 7:
			position = new Vector3(0,0,2*cellWidth);
			break;
			case 8:
			position = new Vector3(cellWidth,0,2*cellWidth);
			break;
			case 9:
			position = new Vector3(2*cellWidth,0,2*cellWidth);
			break;
		}
		return position;
	}

	public Vector3 GetCachedHMIndex(Vector3 center, Vector3 cellSize, int i){
		//wrong! start from 0 not 4
		switch(i){
			case 0:
			return new Vector3(center.x - cellSize.x, center.y, center.z - cellSize.z);
			break;
			case 1:
			return new Vector3(center.x, center.y, center.z - cellSize.z);
			break;
			case 2:
			return new Vector3(center.x + cellSize.x, center.y, center.z - cellSize.z);
			break;
			case 3:
			return new Vector3(center.x - cellSize.x, center.y, center.z);
			break;
			case 4:
			return new Vector3(center.x, center.y, center.z);
			break;
			case 5: 
			return new Vector3(center.x + cellSize.x, center.y, center.z);
			break;
			case 6: 
			return new Vector3(center.x - cellSize.x, center.y, center.z  + cellSize.y);
			break;
			case 7: 
			return new Vector3(center.x , center.y, center.z + cellSize.y);
			break;
			case 8:
			return new Vector3(center.x + cellSize.x, center.y, center.z + cellSize.y);
			break;
			default: 
			return new Vector3();
			Debug.Log("Something went wrong: impossible chunk");
		}
	}

	public IEnumerator ApplyBingHeightmapV2(WWW metaReq, string key ){

		MetadataObject metadata = JsonConvert.DeserializeObject<MetadataObject>( metaReq.text );
		List<double> bbox = metadata.resourceSets[0].resources[0].bbox;
		firstBbox = bbox;

		string url = "http://dev.virtualearth.net/REST/v1/Elevation/Bounds?bounds=";
		url += bbox[0] + "," + bbox[1] + "," +  bbox[2] + "," +  bbox[3];
		int rows = 32;
		url += "&rows=" + rows + "&cols=" + rows + "&heights=sealevel&key=" + key;
		//max height point retrievable = 1024 = 32 * 32

		WWW elevReq = new WWW (url);
		yield return elevReq;

		ElevDataObject elevData = JsonConvert.DeserializeObject<ElevDataObject> (elevReq.text);
		List<int> elevations = elevData.resourceSets[0].resources[0].elevations;  //elevations at sea level in meters
		

		//TerrainData terrain = GetComponent<Terrain> ().terrainData;
		int width = 512 * 3 ;
		float[,] heightmapData = new float[width, width]; //terrain.GetHeights(0, 0, width, width);

		if (width % rows == 0)
		{	
			heightmapData = ApplyElevationsToHeightmap (elevations, heightmapData, width);
			heightmapData = Smooth(heightmapData, smoothings, neighbors, width);

			float[] coordinates = new float[2];
			coordinates[0] = float.Parse(metadata.resourceSets[0].resources[0].mapCenter.coordinates[0]);
			coordinates[1] = float.Parse(metadata.resourceSets[0].resources[0].mapCenter.coordinates[1]);
			GameControl.control.SetCurrentHeightmapData(heightmapData, centerLocation.zoom, coordinates);
			GameControl.control.SaveAsCurrent();
		}
		else {
			Debug.Log ("Something went wrong: size of terrain is not processable for heightmap generation");
		}
		
	}



	public float[,] ApplyElevationsToHeightmap( List<int> elevations, float[,] heightmapData, int width ){
		float minELev = Mathf.Min(elevations.ToArray());//meters
		float maxElev = Mathf.Max(elevations.ToArray());//meters
		float maxReach = 8850; //meters -> m. everest
		int index = 0;
		int rows = 32;
		float zoomScale = (float) ((centerLocation.zoom - 1.0)/(21.0 - 1.0)) * (maxElev - minELev)/minELev; //21 is max zoom value
		
		for (int y = 0; y < width; y+=width/rows) {
			for (int x = 0; x < width; x+=width/rows) {
				if (y % (width/rows) == 0 && x % (width/rows) == 0) 
				{
					index = (y*rows + x )/(width/rows);
					heightmapData[y, x] = ((elevations[index] - minELev)/(maxReach - minELev)) * zoomScale; 
				}
				else {
					heightmapData[y, x] = minELev; //initialize empty with min elevation
				}
			}
		}
		float distance = 0;
		// (x, y) is a peak, adjust the other values (weighted avg according to distance to peak)
		for (int y=0; y < width; y+=width/rows){
			for (int x=0; x < width; x+=width/rows){
				// each peak works on his square [x+n, y+m]
				for (int n = 0; n < width/rows; n ++){
					for (int m = 0; m < width/rows; m++){
						if ((y+m) % (width/rows) != 0 || (x+n) % (width/rows) != 0) {
							distance = Mathf.Sqrt((m*m) + (n*n));
							heightmapData[y+m, x+n] = heightmapData[y,x] * (1 - (distance/((Mathf.Sqrt(2) * width/rows))));
						}
					}
				}
			}
		}
		return heightmapData;
	}

	public float[,] Smooth( float[,] heightmapData , int smoothings, int neighbors, int width ) {
		float sum =0;
		int count = 0;
		//smooth everything
		for (int i=0; i < smoothings; i++){
			for (int y=0; y < width; y++){
				for (int x=0; x < width; x++){
					for (int n = 0; n < neighbors; n++){
						sum += (y-n <= 0 ? 0 : heightmapData[y-n, x]);
						sum += (x-n <= 0 ? 0 : heightmapData[y, x-n]);
						sum += ((x-n<=0 || y-n<=0) ? 0 : heightmapData[y-n, x-n]);
						count += (y-n <= 0 ? 0 : 1);
						count += (x-n <= 0 ? 0 : 1);
						count += ((x-n<=0 || y-n<=0) ? 0 : 1);
					}
					heightmapData[y,x] = (heightmapData[y,x] + sum )/ (count+1);
					sum = 0;
					count = 0;
				}
			}
		}
		return heightmapData;
	}
}



// -------- CLASSES FOR JSON PARSING --------
//for geolocation

class Point
{
	public string type { get; set; }
	public List<double> coordinates { get; set; }
}

class Address
{
	public string addressLine { get; set; }
	public string adminDistrict { get; set; }
	public string adminDistrict2 { get; set; }
	public string countryRegion { get; set; }
	public string formattedAddress { get; set; }
	public string locality { get; set; }
	public string postalCode { get; set; }
}

class GeocodePoint
{
	public string type { get; set; }
	public List<double> coordinates { get; set; }
	public string calculationMethod { get; set; }
	public List<string> usageTypes { get; set; }
}

class Resource
{
	public string __type { get; set; }
	public List<double> bbox { get; set; }
	public string name { get; set; }
	public Point point { get; set; }
	public Address address { get; set; }
	public string confidence { get; set; }
	public string entityType { get; set; }
	public List<GeocodePoint> geocodePoints { get; set; }
	public List<string> matchCodes { get; set; }
}

class ResourceSet
{
	public int estimatedTotal { get; set; }
	public List<Resource> resources { get; set; }
}

class GeocodedObject
{
	public string authenticationResultCode { get; set; }
	public string brandLogoUri { get; set; }
	public string copyright { get; set; }
	public List<ResourceSet> resourceSets { get; set; }
	public int statusCode { get; set; }
	public string statusDescription { get; set; }
	public string traceId { get; set; }
}

// for map metadata

class MapCenter
{
	public string type { get; set; }
	public List<string> coordinates { get; set; }
}

class ResourceMeta
{
	public string __type { get; set; }
	public List<double> bbox { get; set; }
	public string imageHeight { get; set; }
	public string imageWidth { get; set; }
	public MapCenter mapCenter { get; set; }
	public List<object> pushpins { get; set; }
	public string zoom { get; set; }
}

class ResourceMetaSet
{
	public int estimatedTotal { get; set; }
	public List<ResourceMeta> resources { get; set; }
}

class MetadataObject
{
	public string authenticationResultCode { get; set; }
	public string brandLogoUri { get; set; }
	public string copyright { get; set; }
	public List<ResourceMetaSet> resourceSets { get; set; }
	public int statusCode { get; set; }
	public string statusDescription { get; set; }
	public string traceId { get; set; }
}

//for elevation data

class ResourceElev
{
	public string __type { get; set; }
	public List<int> elevations { get; set; }
	public int zoomLevel { get; set; }
}

class ResourceElevSet
{
	public int estimatedTotal { get; set; }
	public List<ResourceElev> resources { get; set; }
}

class ElevDataObject
{
	public string authenticationResultCode { get; set; }
	public string brandLogoUri { get; set; }
	public string copyright { get; set; }
	public List<ResourceElevSet> resourceSets { get; set; }
	public int statusCode { get; set; }
	public string statusDescription { get; set; }
	public string traceId { get; set; }
}
