using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HeightmapProvider : MapHandler, ITerrainProvider
{
    private readonly Comparer _vectorComparer = new Comparer();

    public GameObject CreateTerrain(Vector3 cellSize, Vector3 center )
    {
        int width = 512;
        TerrainData terrainData = new TerrainData();
        terrainData.heightmapResolution = width;
        
        /*
        PrefetchAroundPoint(cellSize, center);
        while (!cachedHeightmaps.Keys.Contains(center, _vectorComparer)){
            Debug.Log(cachedHeightmaps.Keys.Count);
        }

        float[,] heightmap = cachedHeightmaps[center];
        terrainData.SetHeights(0, 0, heightmap);
        */

        GameObject newTerrainGameObject = Terrain.CreateTerrainGameObject( terrainData );
        Terrain t = newTerrainGameObject.GetComponent<Terrain>();
        t.terrainData.size = new Vector3(cellSize.x, cellSize.y, cellSize.z);
        newTerrainGameObject.transform.position = center;
        t.Flush();
        return newTerrainGameObject;
    }

    public void PrefetchAroundPoint (Vector3 cellSize, Vector3 spawnPoint){
        //StartCoroutine( StartMap(cellSize, spawnPoint) );
        //StartCoroutine( StopAndWait() );
    }

}