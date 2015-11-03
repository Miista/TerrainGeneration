using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TerrainGenerationController : MonoBehaviour
{
    /// <summary>
    ///     The single dimension that makes of the quadratic map that will be generated.
    /// </summary>
    public float Size;

    /// <summary>
    ///     A percentage indicating how far the player has to cross into
    ///     an adjacent cell before new cells are loaded.
    /// </summary>
    public double Threshold;

    /// <summary>
    ///     The player.
    ///     This should be the top-most container of the player.
    /// </summary>
    public GameObject Player;

    public ITerrainProvider TerrainProvider;

    private double _threshold
    {
        get { return _cellSize.x * (Threshold * 0.01); }
    }

    private readonly Dictionary<Vector3, GameObject> _grid = new Dictionary<Vector3, GameObject>();

    private double BoundaryX
    {
        get
        {
            return _cellSize.x / 2 + _threshold;
        }
    }

    private double BoundaryY
    {
        get
        {
            return _cellSize.z / 2 + _threshold;
        }
    }

    private Vector3 _spawnPoint;

    private Vector3 _cellSize;

    // Use this for initialization
	void Start ()
	{
        TerrainProvider = new ColorProvider();
	    _spawnPoint = new Vector3
	    {
	        x = Size / 2,
	        y = Player.transform.position.y,
	        z = Size / 2
	    };
	    Player.transform.position = _spawnPoint;
	    _cellSize = new Vector3
	    {
	        x = Size / 3,
            y = 1,
	        z = Size / 3
	    };
        GenerateFromSpawnPoint();
	}

    // Update is called once per frame
	void Update ()
	{
	    var direction = GetDirection();
	    switch (direction)
	    {
            case Direction.None:
	            return;
	        case Direction.NorthEast:
	            _spawnPoint = new Vector3
	            {
	                x = _spawnPoint.x + _cellSize.x,
	                y = _spawnPoint.y,
	                z = _spawnPoint.z + _cellSize.z
	            };
	            break;
	        case Direction.NorthWest:
	            _spawnPoint = new Vector3
	            {
	                x = _spawnPoint.x - _cellSize.x,
	                y = _spawnPoint.y,
	                z = _spawnPoint.z + _cellSize.z
	            };
	            break;
	        case Direction.SouthEast:
	            _spawnPoint = new Vector3
	            {
	                x = _spawnPoint.x + _cellSize.x,
	                y = _spawnPoint.y,
	                z = _spawnPoint.z - _cellSize.z
	            };
	            break;
	        case Direction.SouthWest:
	            _spawnPoint = new Vector3
	            {
	                x = _spawnPoint.x - _cellSize.x,
	                y = _spawnPoint.y,
	                z = _spawnPoint.z - _cellSize.z
	            };
	            break;
	        case Direction.North:
	            _spawnPoint = new Vector3
	            {
	                x = _spawnPoint.x,
	                y = _spawnPoint.y,
	                z = _spawnPoint.z + _cellSize.z
	            };
	            break;
	        case Direction.East:
	            _spawnPoint = new Vector3
	            {
	                x = _spawnPoint.x + _cellSize.x,
	                y = _spawnPoint.y,
	                z = _spawnPoint.z
	            };
	            break;
	        case Direction.South:
	            _spawnPoint = new Vector3
	            {
	                x = _spawnPoint.x,
	                y = _spawnPoint.y,
	                z = _spawnPoint.z - _cellSize.z
	            };
	            break;
	        case Direction.West:
	            _spawnPoint = new Vector3
	            {
	                x = _spawnPoint.x - _cellSize.x,
	                y = _spawnPoint.y,
	                z = _spawnPoint.z
	            };
	            break;
	        default:
	            print( "Do the mambo dance!!" );
	            break;
	    }
        GenerateFromSpawnPoint();
//	    print( string.Format( "({0},{1},{2})", _spawnPoint.x, _spawnPoint.y, _spawnPoint.z ) );
	}

    private Direction GetDirection()
    {
        var delta = new Vector3
        {
            x = Player.transform.position.x - _spawnPoint.x,
            z = Player.transform.position.z - _spawnPoint.z
        };

        if ( Math.Abs( delta.x ) > BoundaryX )
        {
            if ( Math.Abs( delta.z ) > BoundaryY )
            {
                if (delta.x > 0)
                {
                    return delta.z > 0 
                        ? Direction.NorthEast 
                        : Direction.SouthEast;
                }
                if (delta.x < 0)
                {
                    return delta.z > 0 
                        ? Direction.NorthWest 
                        : Direction.SouthWest;
                }
            }
            else
            {
                return delta.x > 0 
                    ? Direction.East 
                    : Direction.West;
            }
        }
        else if ( Math.Abs( delta.z ) > BoundaryY )
        {
            return delta.z > 0 
                ? Direction.North 
                : Direction.South;
        }

        return Direction.None;
    }

    private enum Direction
    {
        NorthWest = 0,
        North = 1,
        NorthEast = 2,
        West = 3,
        None = 4,
        East = 5,
        SouthWest = 6,
        South = 7,
        SouthEast = 8,
    }

    private void GenerateFromSpawnPoint()
    {
        var zOffset = _spawnPoint.z - _cellSize.z;
        var xOffset = _spawnPoint.x - _cellSize.x;
        for (var row = 0; row < 3; row++)
        {

            for (var column = 0; column < 3; column++)
            {
                var spawnPoint = new Vector3
                {
                    x = xOffset + ( column * _cellSize.x ),
                    y = 0,
                    z = zOffset + ( row * _cellSize.z )
                };
                if ( _grid.ContainsKey( spawnPoint ) )
                {
                    continue;
                }
                var primitive = TerrainProvider.CreateTerrain( _cellSize, spawnPoint );
                _grid[spawnPoint] = primitive;
            }

        }

        RemoveObsoleteCells();
    }

    /// <summary>
    ///     Remove the cells that the player can (should) no longer see.
    /// </summary>
    private void RemoveObsoleteCells()
    {
        var upperRow = _spawnPoint + new Vector3( 0, 0, _cellSize.z );
        var e1 = _grid.Keys.Where( v => v.z > upperRow.z );

        var lowerRow = _spawnPoint - new Vector3( 0, 0, _cellSize.z );
        var e2 = _grid.Keys.Where( v => v.z < lowerRow.z );

        var leftColumn = _spawnPoint - new Vector3( _cellSize.x, 0, 0 );
        var e3 = _grid.Keys.Where( v => v.x < leftColumn.x );

        var rightColumn = _spawnPoint + new Vector3( _cellSize.x, 0, 0 );
        var e4 = _grid.Keys.Where( v => v.x > rightColumn.x );

        var obsoleteCells = e1.Union( e2 )
                              .Union( e3 )
                              .Union( e4 );
        foreach (var v in obsoleteCells.ToList())
        {
            Destroy( _grid[v] );
            _grid.Remove( v );
        }
    }
}

public interface ITerrainProvider
{
    GameObject CreateTerrain(Vector3 cellSize, Vector3 center);
}

class ColorProvider : ITerrainProvider
{
    private List<Color> colors = new List<Color>
    {
        Color.black,
        Color.cyan,
        Color.blue,
        Color.magenta,
        Color.red,
        Color.yellow,
        Color.green,
        Color.white,
        Color.clear
    };

    private int n = 0;

    public GameObject CreateTerrain(Vector3 cellSize, Vector3 center)
    {
        var primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
        primitive.GetComponent<Renderer>()
                 .material = new Material(Shader.Find("Diffuse"))
                 {
                     color = colors[n % 9]
                 };
        primitive.transform.transform.localScale = new Vector3(cellSize.x, cellSize.y, cellSize.z);
        primitive.transform.transform.position = center;
        n++;
        return primitive;
    }
}
