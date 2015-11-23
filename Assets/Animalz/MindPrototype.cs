using System;
using UnityEngine;

namespace Assets.Animalz
{
    public class MindPrototype : MonoBehaviour
    {
        public VisionPref Eyes;
        public AudioPref Ears;
        public SmellyPref Nose;

        void Start()
        {
            Animal animal = gameObject.AddComponent<Animal>();
            // Awake
//            animal.Mind = CreateMind();
            animal.Init( this );
        }

        private ConcreteMind CreateMind()
        {
            var container = new GameObject("ConcreteMind");
            container.transform.parent = gameObject.transform;

            // Place the game object at the same position and rotation as the animal
            container.transform.position = gameObject.transform.position;
            container.transform.rotation = gameObject.transform.rotation;
            return (ConcreteMind)container.AddComponent(typeof(ConcreteMind));
        }

        private void AddEars()
        {
            var ears = CreateContainer("EarsTest");

            var sphereCollider = ears.AddComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            sphereCollider.radius = Ears.Range;
            ears.AddComponent(typeof(NonDirectionalSensor));
        }

        private GameObject CreateContainer(string objectName)
        {
            var container = new GameObject(objectName);
//            container.transform.parent = ( _mind as Component ).gameObject.transform;
//
//            // Place the game object at the same position and rotation as the animal
//            container.transform.position = (_mind as Component).transform.position;
//            container.transform.rotation = (_mind as Component).transform.rotation;
            return container;
        }

        private void AddEyes()
        {
            var eyes = CreateContainer("EyesTest");

            var sphereCollider = eyes.AddComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            sphereCollider.radius = Eyes.Range;
            Component visionSense = eyes.AddComponent(typeof(VisionSense));
//            ( (VisionSense)visionSense ).CreatedFrom = Eyes;
        }
    }

    [Serializable]
    public class SmellyPref
    {
        public float Range;
    }

    [Serializable]
    public class AudioPref
    {
        public float Range;
    }
    [Serializable]

    public class VisionPref
    {
        public float Range;
        public float FieldOfView;
    }
}