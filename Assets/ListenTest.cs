using UnityEngine;
using System.Collections;



public class ListenTest : MonoBehaviour
{
    private SphereCollider _ears;

    // Use this for initialization
    void Start()
    {
        _ears = gameObject.AddComponent<SphereCollider>();
        _ears.isTrigger = true;
        _ears.radius = 10;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnTriggerEnter(Collider other)
    {
//        print("Listen: " + _ears.bounds.Intersects(other.bounds));
    }

    public void OnTriggerStay(Collider other)
    {
//        print("Listen: " + _ears.bounds.Intersects(other.bounds));
    }

    public void OnTriggerExit(Collider other)
    {
//        print("Listen: " + _ears.bounds.Intersects(other.bounds));
    }
}
