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

    private Mind _mind;

    void Start()
    {
        _mind = CreateMind();
        AddEyes();
        AddEars();
    }

    private Mind CreateMind()
    {
        var container = new GameObject( "Mind" );
        container.transform.parent = gameObject.transform;

        // Place the game object at the same position and rotation as the animal
        container.transform.position = gameObject.transform.position;
        container.transform.rotation = gameObject.transform.rotation;
        return (Mind)container.AddComponent( typeof(Mind) );
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
        var container = new GameObject( objectName );
        container.transform.parent = _mind.gameObject.transform;

        // Place the game object at the same position and rotation as the animal
        container.transform.position = _mind.transform.position;
        container.transform.rotation = _mind.transform.rotation;
        return container;
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

class DirectionalSensor : Sensor
{
    public void OnTriggerStay(Collider other)
    {
        if (!IsAnimalAi(other))
        {
            return;
        }

        var direction = other.transform.position - gameObject.transform.position;
        var angle = Vector3.Angle(direction, transform.forward);
        if ( angle < 0.5 * EyeSettings.FieldOfViewAngle )
        {
            RaycastHit ray;
            if ( Physics.Raycast( transform.position, direction, out ray ) )
            {
                if ( ray.collider == other )
                {
                    InnerMind.ReportSightOf( other );
                    return;
                }
            }
        }
        InnerMind.LostSightOf( other );
    }
}

class NonDirectionalSensor : Sensor
{
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
            InnerMind.ReportSoundOf( other );
        }
        else
        {
            InnerMind.ReportSoundMissing( other );
        }
    }
}

public class Sensor : MonoBehaviour
{
    internal Mind InnerMind
    {
        get
        {
            // ReSharper disable once ConvertPropertyToExpressionBody
            return gameObject.GetComponentInParent<Mind>();
        }
    }

    private static readonly Type AnimalType = typeof(Animal);

    protected Vision EyeSettings
    {
        get
        {
            // ReSharper disable once ConvertPropertyToExpressionBody
            return gameObject.GetComponentInParent<InitScript>()
                             .Eyes;
        }
    }

    protected AudioSense EarSettings
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

public class Mind : MonoBehaviour
{
    public Mind()
    {
        InHearingDistance = new HashSet<Collider>();
        InViewingDistance = new HashSet<Collider>();
    }

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

    private HashSet<Collider> InHearingDistance { get; set; }
    private HashSet<Collider> InViewingDistance { get; set; }

    private static readonly Material CanOnlyHear = new Material( Shader.Find( "Diffuse" ) )
    {
        color = Color.yellow
    };

    private static readonly Material InRange = new Material( Shader.Find( "Diffuse" ) )
    {
        color = Color.red
    };

    private static readonly Material CanSeeAndHear = new Material( Shader.Find( "Diffuse" ) )
    {
        color = Color.green
    };

    public void ReportSoundOf(Collider other)
    {
        InHearingDistance.Add( other );
        other.GetComponent<Renderer>()
             .material = InViewingDistance.Contains( other )
                 ? CanSeeAndHear
                 : CanOnlyHear;
    }

    private static readonly Material CanSee = new Material( Shader.Find( "Diffuse" ) )
    {
        color = Color.black
    };

    public void ReportSightOf(Collider other)
    {
        InViewingDistance.Add( other );
        other.GetComponent<Renderer>()
             .material = InHearingDistance.Contains( other )
                 ? CanSeeAndHear
                 : CanSee;
    }

    internal void LostSightOf(Collider other)
    {
        if ( InViewingDistance.Remove( other ) )
        {
            other.GetComponent<Renderer>()
                 .material = new Material( Shader.Find( "Diffuse" ) )
                 {
                     color = Color.white
                 };
        }
    }

    public void ReportSoundMissing(Collider other)
    {
        InHearingDistance.Remove( other );
    }
}

