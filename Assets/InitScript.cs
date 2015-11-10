using System;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Vision
{
    public int Range;
    public int FieldOfViewAngle;
    internal readonly HashSet<int> InRange = new HashSet<int>();
}

[System.Serializable]
public struct AudioSense
{
    public int Range;
}

public class InitScript : MonoBehaviour
{
    public Vision Eyes;
    public AudioSense Ears;

    void Start()
    {
        AddEyes();
        AddEars();
    }

    private void AddEars()
    {
        var ears = CreateContainer( "EarsTest" );

        var sphereCollider = ears.AddComponent<SphereCollider>();
        sphereCollider.isTrigger = true;
        sphereCollider.radius = Ears.Range;
        ears.AddComponent( typeof(NonDirectionalSensor) );
    }

    private GameObject CreateContainer(string objectName)
    {
        var ears = new GameObject( objectName );
        ears.transform.parent = gameObject.transform;

        // Place the game object at the same position and rotation as the animal
        ears.transform.position = gameObject.transform.position;
        ears.transform.rotation = gameObject.transform.rotation;
        return ears;
    }

    private void AddEyes()
    {
        var eyes = CreateContainer( "EyesTest" );

        var sphereCollider = eyes.AddComponent<SphereCollider>();
        sphereCollider.isTrigger = true;
        sphereCollider.radius = Eyes.Range;
        eyes.AddComponent( typeof(DirectionalSensor) );
    }
}

public class DirectionalSensor : Sensor
{
    private static readonly Material InRange = new Material( Shader.Find( "Diffuse" ) ) { color = Color.red };

    private static readonly Material CanSee = new Material( Shader.Find( "Diffuse" ) ) { color = Color.black };

    public void OnTriggerStay(Collider other)
    {
        if (!IsAnimalAi(other))
        {
            return;
        }

        var direction = other.transform.position - gameObject.transform.position;
        var angle = Vector3.Angle(direction, transform.forward);
        if ( angle < 0.5 * Eyes.FieldOfViewAngle )
        {
            RaycastHit ray;
            if ( Physics.Raycast( transform.position, direction, out ray ) )
            {
                if ( ray.collider == other )
                {
                    other.GetComponent<Renderer>()
                         .material = CanSee;
                    Eyes.InRange.Add( other.GetInstanceID() );
                    return;
                }
            }
        }
        other.GetComponent<Renderer>().material = InRange;
        Eyes.InRange.Remove( other.GetInstanceID() );
    }
}

public class NonDirectionalSensor : Sensor
{
    private static readonly Material CanOnlyHear = new Material(Shader.Find("Diffuse")) { color = Color.yellow };

    private static readonly Material InRange = new Material(Shader.Find("Diffuse")) { color = Color.red };

    private static readonly Material CanSeeAndHear = new Material( Shader.Find( "Diffuse" ) )
    {
        color = Color.green
    };

    public void OnTriggerStay(Collider other)
    {
        if ( !IsAnimalAi( other ) )
        {
            return;
        }

        var direction = other.transform.position - gameObject.transform.position;
        var angle = Vector3.Angle(direction, transform.forward);
        if (angle < 180)
        {
            other.GetComponent<Renderer>()
                 .material = Eyes.InRange.Contains(other.GetInstanceID()) 
                                ? CanSeeAndHear 
                                : CanOnlyHear;
        }
        else
        {
            other.GetComponent<Renderer>().material = InRange;
        }
    }
}

public class Sensor : MonoBehaviour
{
    private static readonly Type AnimalType = typeof(InitScript);

    protected Vision Eyes
    {
        get
        {
            // ReSharper disable once ConvertPropertyToExpressionBody
            return gameObject.GetComponentInParent<InitScript>()
                             .Eyes;
        }
    }

    protected AudioSense Ears
    {
        get
        {
            // ReSharper disable once ConvertPropertyToExpressionBody
            return gameObject.GetComponentInParent<InitScript>()
                             .Ears;
        }
    }

    protected bool IsAnimalAi(Collider other)
    {
        return other.gameObject.GetComponent( AnimalType ) != null;
    }
}