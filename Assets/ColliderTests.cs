using System;
using UnityEngine;

public class Coll : Collider
{

}

public class ColliderTests : MonoBehaviour
{
    private SphereCollider _eyes;

    // Use this for initialization
    private void Start()
    {
        gameObject.GetComponent<Renderer>()
          .material = new Material(Shader.Find("Diffuse"))
          {
              color = Color.cyan
          };
        _eyes = gameObject.AddComponent<SphereCollider>();
        _eyes.isTrigger = true;
        _eyes.radius = 5;
    }

    public void OnCollisionEnter(Collision collision)
    {
        print( collision.collider.name );
    }

    public void OnCollisionStay(Collision collision)
    {
        print( collision.collider.name );

    }

    public void OnCollisionExit(Collision collision)
    {
        print( collision.collider.name );

    }

    public void OnTriggerStay(Collider other)
    {
//        print("Eyes: " + _eyes.bounds.Intersects(other.bounds));

        var direction = other.transform.position - _eyes.transform.position;
        var angle = Vector3.Angle(direction, transform.forward);
        if (angle < 45)
        {
            RaycastHit ray;
            if (Physics.Raycast(transform.position, direction, out ray))
            {
//                print("Eyes: " + _eyes.bounds.Intersects(other.bounds));
                other.GetComponent<Renderer>()
                     .material = new Material(Shader.Find("Diffuse"))
                     {
                         color = Color.black
                     };
            }
            else
            {
                other.GetComponent<Renderer>()
                     .material = new Material(Shader.Find("Diffuse"))
                     {
                         color = Color.red
                     };
            }
        }
        else
        {
            other.GetComponent<Renderer>().material = new Material(Shader.Find("Diffuse")) { color = Color.red };
        }
    }

    public void OnTriggerExit(Collider other)
    {
        other.GetComponent<Renderer>().material = new Material(Shader.Find("Diffuse")) { color = Color.white };
    }
}
