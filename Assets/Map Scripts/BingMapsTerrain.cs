using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;


[System.Serializable]
public class BingMapsLocation
{
	public string locality = "innsbrueck";
	public string adminDistrict = "";
	public string addressLine = "";
	public string ISOCountryRegion = "";
	public int zoom = 12;
	public double latitude;
	public double longitude;
}

public class BingMapsTerrain : MonoBehaviour {
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
	public 	int smoothings = 4; //times I do the smoothing
	public int neighbors = 6; //neighbors I want to use for smoothing height values

	
	void Start() {
		if(loadOnStart) Refresh();	
	}
	
	public void Refresh() {
		StartCoroutine(_Refresh());
		//TestRefresh();
	}
	
	IEnumerator _Refresh (){
		string key = "Aqb8ECpcSTeT8RxLKH-r7SiS5NI7JK2hVF5FZFKap30lls9Nc7fQWH_-OKjYButM";
		int size = 512; 
		GetComponent<Terrain> ().terrainData.size = new Vector3 (25, 10, 25);
		GetComponent<Terrain> ().terrainData.heightmapResolution = size;

		List<double> centerCoord = new List<double>(2);
		string url = "http://dev.virtualearth.net/REST/v1/Locations?"; 
		string qs = "";
		qs +=  (centerLocation.locality!= "") ? "locality=" + centerLocation.locality : "";
		qs +=  (centerLocation.adminDistrict!= "") ? "&adminDistrict=" + centerLocation.adminDistrict : "";
		qs +=  (centerLocation.addressLine!= "") ? "&addressLine=" + centerLocation.addressLine : "";
		qs +=  (centerLocation.ISOCountryRegion!= "") ? "&countryRegion=" + centerLocation.ISOCountryRegion : "";

		if (qs != "")
		{
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
		url = "http://dev.virtualearth.net/REST/v1/Imagery/Map/";
		qs = "";
		qs += imSet + "/";
		qs += centerCoord[0] + "," + centerCoord[1] + "/";
		qs += centerLocation.zoom + "?";
		qs += "mapSize=" + size + "," + size;
		qs += "&key=" + key;

		string qs2 = qs;
		qs += "&mapMetadata=0"; //for having the image
		qs2 += "&mapMetadata=1"; //for map metadata

		WWW reqImage = new WWW (url + qs);
		if (heights == true){
			WWW reqMeta = new WWW (url + qs2);
			yield return reqMeta;
			StartCoroutine(ApplyBingHeightmap (reqMeta, key));
		}

		yield return reqImage;

		//need this to add the texture to the terrain so it looks like a map
		List<SplatPrototype> splatList = new List<SplatPrototype>();
		SplatPrototype newSplat = new SplatPrototype();
		newSplat.texture = reqImage.texture; //satellite: req, heightmap: req2 (use req normally)
		float width = GetComponent<Terrain> ().terrainData.size.x;
		newSplat.tileSize = new Vector2( width, width );
		newSplat.tileOffset = Vector2.zero;
		splatList.Add (newSplat);
		GetComponent<Terrain> ().terrainData.splatPrototypes = splatList.ToArray();
	}

	IEnumerator ApplyBingHeightmap(WWW metaReq, string key ){

		MetadataObject metadata = JsonConvert.DeserializeObject<MetadataObject>( metaReq.text );
		List<double> bbox = metadata.resourceSets[0].resources[0].bbox;

		string url = "http://dev.virtualearth.net/REST/v1/Elevation/Bounds?bounds=";
		url += bbox[0] + "," + bbox[1] + "," +  bbox[2] + "," +  bbox[3];
		int rows = 32;
		url += "&rows=" + rows + "&cols=" + rows + "&heights=sealevel&key=" + key;
		//max height point retrievable = 1024 = 32 * 32

		WWW elevReq = new WWW (url);
		yield return elevReq;

		ElevDataObject elevData = JsonConvert.DeserializeObject<ElevDataObject> (elevReq.text);
		List<int> elevations = elevData.resourceSets[0].resources[0].elevations;  //elevations at sea level in meters
		float minELev = Mathf.Min(elevations.ToArray());
		float maxElev = 8848;//Mathf.Max(elevations.ToArray());

		TerrainData terrain = GetComponent<Terrain> ().terrainData;
		int width = terrain.heightmapWidth-1;
		float[,] heightmapData = terrain.GetHeights(0, 0, width, width);
		int index = 0;

		if (width % rows == 0)
		{
			for (int y = 0; y < width; y+=width/rows) {
				for (int x = 0; x < width; x+=width/rows) {
					if (y % (width/rows) == 0 && x % (width/rows) == 0) {
						index = (y*rows + x )/(width/rows);
						heightmapData[y, x] = ((elevations[index] - minELev)/(maxElev - minELev)) * terrain.size.y; // normalize meters to terrain height
						} else {
						heightmapData[y, x] = minELev; //initialize empty with min elevation
					}

				}
			}
			float distance = 0;
			// (x, y) is a peak
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


			float sum =0;
			int count = 0;
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

			terrain.SetHeights(0, 0, heightmapData);
		}
		else {
			Debug.Log ("Something went wrong: size of terrain is not processable for heightmap generation");
		}
	}
}

/*List<int> elevations = new List<int> 
	{ 	464, 481, 459, 428,
		693, 601, 650, 619,
		908, 890, 950, 967,
		1507, 1595, 1522, 1622};*/

//classes for Json parsing
//for geolocation

public class Point
{
	public string type { get; set; }
	public List<double> coordinates { get; set; }
}

public class Address
{
	public string addressLine { get; set; }
	public string adminDistrict { get; set; }
	public string adminDistrict2 { get; set; }
	public string countryRegion { get; set; }
	public string formattedAddress { get; set; }
	public string locality { get; set; }
	public string postalCode { get; set; }
}

public class GeocodePoint
{
	public string type { get; set; }
	public List<double> coordinates { get; set; }
	public string calculationMethod { get; set; }
	public List<string> usageTypes { get; set; }
}

public class Resource
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

public class ResourceSet
{
	public int estimatedTotal { get; set; }
	public List<Resource> resources { get; set; }
}

public class GeocodedObject
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

public class MapCenter
{
	public string type { get; set; }
	public List<string> coordinates { get; set; }
}

public class ResourceMeta
{
	public string __type { get; set; }
	public List<double> bbox { get; set; }
	public string imageHeight { get; set; }
	public string imageWidth { get; set; }
	public MapCenter mapCenter { get; set; }
	public List<object> pushpins { get; set; }
	public string zoom { get; set; }
}

public class ResourceMetaSet
{
	public int estimatedTotal { get; set; }
	public List<ResourceMeta> resources { get; set; }
}

public class MetadataObject
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

public class ResourceElev
{
	public string __type { get; set; }
	public List<int> elevations { get; set; }
	public int zoomLevel { get; set; }
}

public class ResourceElevSet
{
	public int estimatedTotal { get; set; }
	public List<ResourceElev> resources { get; set; }
}

public class ElevDataObject
{
	public string authenticationResultCode { get; set; }
	public string brandLogoUri { get; set; }
	public string copyright { get; set; }
	public List<ResourceElevSet> resourceSets { get; set; }
	public int statusCode { get; set; }
	public string statusDescription { get; set; }
	public string traceId { get; set; }
}


