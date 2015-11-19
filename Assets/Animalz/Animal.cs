using System;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Animalz
{
    internal class Animal : MonoBehaviour
    {
        public AudiatorySense Ears;
        public VisionSense Eyes;
        public SmellySense Nose;

        internal ConcreteMind Mind { get; set; }
        int Size { get; set; }
        AnimalType Type { get; set; }

        public void Init(MindPrototype prototype)
        {
            GameObject vision = CreateContainer( "VisionTest" );
            Eyes = vision.AddComponent<VisionSense>();
            Eyes.CreatedFrom = prototype.Eyes;

            GameObject audio = CreateContainer("AudioTest");
            Ears = audio.AddComponent<AudiatorySense>();
            Ears.CreatedFrom = prototype.Ears;

            GameObject smell = CreateContainer("SmellyTest");
            Nose = smell.AddComponent<SmellySense>();
            Nose.CreatedFrom = prototype.Nose;
        }

        private GameObject CreateContainer(string objectName)
        {
            var container = new GameObject(objectName);
            container.transform.parent = gameObject.transform;

            // Place the game object at the same position and rotation as the animal
            container.transform.position = transform.position;
            container.transform.rotation = transform.rotation;
            return container;
        }
    }

    enum AnimalType
    {
        Predator,
        Prey
    }

    internal interface IConcreteMind
    {
        AudiatorySense Ears { get; }
        VisionSense Eyes { get; }
        SmellySense Nose { get; }
    }

    internal class ConcreteMind : MonoBehaviour //, IConcreteMind
    {
//        public AudiatorySense Ears;
//        public VisionSense Eyes;
//        public SmellySense Nose;
//
//        public ConcreteMind()
//        {
//            Ears = new AudiatorySense();
//            Nose = new SmellySense();
//            Eyes = new VisionSense();
//        }
    }

    [Serializable]
    internal class AudiatorySense : MonoBehaviour
    {
        internal AudioPref CreatedFrom { get; set; }
    }

    [Serializable]
    internal class VisionSense : MonoBehaviour
    {
        internal VisionPref CreatedFrom { get; set; }

//        private readonly SphereCollider collider;
//
//        public VisionSense( GameObject go )
//        {
//            var eyes = CreateContainer(go, "EyesTest");
//
//            collider = eyes.AddComponent<SphereCollider>();
//            collider.isTrigger = true;
//            collider.radius = 0f;
//            eyes.AddComponent(typeof(DirectionalSensor));
//        }
//
//        private GameObject CreateContainer(GameObject go, string objectName)
//        {
//            var container = new GameObject(objectName);
//            container.transform.parent = go.transform;
//
//            // Place the game object at the same position and rotation as the animal
//            container.transform.position = go.transform.position;
//            container.transform.rotation = go.transform.rotation;
//            return container;
//        }


        public float FieldOfView
        {
            // ReSharper disable once ConvertPropertyToExpressionBody
            get { return CreatedFrom.FieldOfView; }
        }

        public void Update()
        {
            print(FieldOfView);
        }
    }

    [Serializable]
    internal class SmellySense : MonoBehaviour
    {
        internal SmellyPref CreatedFrom { get; set; }
    }
}
