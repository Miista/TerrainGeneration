using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Animalz
{
    internal class Animal : MonoBehaviour
    {
        public AudiatorySense Ears;
        public VisionSense Eyes;
        public SmellySense Nose;
        internal Movement _movement;

        internal ConcreteMind Mind { get; set; }
        int Size { get; set; }
        AnimalType Type { get; set; }

        public void Awake()
        {
            Mind = gameObject.AddComponent<ConcreteMind>();
            Mind.MyAnimal = this;

            GameObject vision = CreateContainer("VisionTest");
            var visionCollider = vision.AddComponent<SphereCollider>();
            vision.AddComponent<Rigidbody>()
                  .isKinematic = true;
            Eyes = vision.AddComponent<VisionSense>();
            Eyes.Collider = visionCollider;
            Eyes.Mind = Mind;
            visionCollider.isTrigger = true;
            visionCollider.radius = Eyes.Range;

            GameObject audio = CreateContainer("AudioTest");
            var audioCollider = audio.AddComponent<SphereCollider>();
            audio.AddComponent<Rigidbody>()
                 .isKinematic = true;
            Ears = audio.AddComponent<AudiatorySense>();
            Ears.Collider = audioCollider;
            Ears.Mind = Mind;
            audioCollider.isTrigger = true;
            audioCollider.radius = Ears.Range;

            GameObject smell = CreateContainer("SmellyTest");
            Nose = smell.AddComponent<SmellySense>();

            _movement = this.gameObject.AddComponent<Movement>();
        }

        public void Init(MindPrototype prototype)
        {
            Eyes.CreatedFrom = prototype.Eyes;
            Ears.CreatedFrom = prototype.Ears;
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

    internal class ConcreteMind : MonoBehaviour
    {
        private static readonly Material CanSeeAndHear = new Material(Shader.Find("Diffuse")) { color = Color.green };

        private static readonly Material CanSee = new Material(Shader.Find("Diffuse")) { color = Color.black };

        private static readonly Material CanOnlyHear = new Material(Shader.Find("Diffuse")) { color = Color.yellow };

        internal Animal MyAnimal;

        public void Update()
        {
            if ( !MyAnimal._movement.TakeMoveStep() )
            {
                Collider animal = MyAnimal.Eyes.Detected.FirstOrDefault();
                if (animal != null)
                {
                    print( "SHOULD MOVE!!!!!" );
                    Vector3 positionOfOther = animal.transform.position;
                    MyAnimal._movement.updateTarget( positionOfOther );
                }
            }
        }

        public void ReportDetectOf(Sense origin, Collider other)
        {
            Material m = null;
            if (origin is VisionSense)
            {
                m = MyAnimal.Ears.Detected.Contains(other)
                    ? CanSeeAndHear
                    : CanSee;
            }
            else if (origin is AudiatorySense)
            {
                m = MyAnimal.Eyes.Detected.Contains(other)
                    ? CanSeeAndHear
                    : CanOnlyHear;
            }
            other.GetComponent<Renderer>()
                 .material = m;
        }

        internal void ReportUndectOf(Sense origin, Collider other)
        {
            other.GetComponent<Renderer>()
                    .material = new Material(Shader.Find("Diffuse"))
                    {
                        color = Color.white
                    };
        }
    }

    internal class AudiatorySense : Sense
    {
        internal AudioPref CreatedFrom = new AudioPref();

        public float Range
        {
            // ReSharper disable once ConvertPropertyToExpressionBody
            get { return CreatedFrom.Range; }
        }

        public void FixedUpdate()
        {
            if (Collider.radius != Range)
            {
                Collider.radius = Range;
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (!IsAnimalAi(other))
            {
                return;
            }

            if (IsSelf(other))
            {
                return;
            }

            if (Detected.Contains(other))
            {
                ReportUndetection(other);
            }
        }

        public void OnTriggerEnter(Collider other)
        {
            if (!IsAnimalAi(other))
            {
                return;
            }

            if (IsSelf(other))
            {
                return;
            }

            var direction = other.transform.position - gameObject.transform.position;
            var angle = Vector3.Angle(direction, transform.forward);
            if (angle < 180)
            {
                ReportDetection(other);
            }
        }

        //        public void OnTriggerStay(Collider other)
        //        {
        //            if (!IsAnimalAi(other))
        //            {
        //                return;
        //            }
        //
        //            if (IsSelf(other))
        //            {
        //                return;
        //            }
        //
        //            var direction = other.transform.position - gameObject.transform.position;
        //            var angle = Vector3.Angle(direction, transform.forward);
        //            if (angle < 180)
        //            {
        //                ReportDetection(other);
        //            }
        //            else
        //            {
        //                ReportUndetection(other);
        //            }
        //        }
    }

    internal class Sense : MonoBehaviour
    {
        private static readonly Type AnimalType = typeof(Animal);

        internal HashSet<Collider> Detected = new HashSet<Collider>();

        internal SphereCollider Collider { get; set; }
        internal ConcreteMind Mind { get; set; }

        protected bool IsAnimalAi(Collider other)
        {
            return other.gameObject.GetComponent(AnimalType) != null;
        }

        protected bool IsSelf(Collider other)
        {
            var components = GetComponentInParent<Animal>();
            var otherAnimal = other.GetComponentInParent<Animal>();
            return components == otherAnimal;
        }

        protected void ReportUndetection(Collider other)
        {
            Detected.Remove(other);
            Mind.ReportUndectOf(this, other);
        }

        protected void ReportDetection(Collider other)
        {
            Detected.Add(other);
            Mind.ReportDetectOf(this, other);
        }
    }

    internal class VisionSense : Sense
    {
        internal VisionPref CreatedFrom = new VisionPref();

        public float FieldOfView
        {
            // ReSharper disable once ConvertPropertyToExpressionBody
            get { return CreatedFrom.FieldOfView; }
        }

        public float Range
        {
            // ReSharper disable once ConvertPropertyToExpressionBody
            get { return CreatedFrom.Range; }
        }

        public void FixedUpdate()
        {
            if (Collider.radius != Range)
            {
                Collider.radius = Range;
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (!IsAnimalAi(other))
            {
                return;
            }

            if (IsSelf(other))
            {
                return;
            }

            if (Detected.Contains(other))
            {
                ReportUndetection(other);
            }
        }

        public void OnTriggerEnter(Collider other)
        {
            if (!IsAnimalAi(other))
            {
                return;
            }

            if (IsSelf(other))
            {
                return;
            }

            var direction = other.transform.position - gameObject.transform.position;
            var angle = Vector3.Angle(direction, transform.forward);
            if (angle < 0.5 * FieldOfView)
            {
                RaycastHit ray;
                if (Physics.Raycast(transform.position, direction, out ray))
                {
                    if (ray.collider == other)
                    {
                        ReportDetection(other);
                    }
                }
            }
        }

        public void OnTriggerStay(Collider other)
        {
            if (!IsAnimalAi(other))
            {
                return;
            }

            if (IsSelf(other))
            {
                return;
            }

            var direction = other.transform.position - gameObject.transform.position;
            var angle = Vector3.Angle(direction, transform.forward);
            if (angle < 0.5 * FieldOfView)
            {
                RaycastHit ray;
                if (Physics.Raycast(transform.position, direction, out ray))
                {
                    var otherAnimal = other.GetComponentInParent<Animal>();
                    var collidedAnimal = ray.collider.GetComponentInParent<Animal>();
                    if (collidedAnimal == otherAnimal)
                    {
                        ReportDetection(other);
                        return;
                    }
                }
            }
            if (Detected.Contains(other))
            {
                ReportUndetection(other);
            }
        }
    }

    internal class SmellySense : Sense
    {
        internal SmellyPref CreatedFrom = new SmellyPref();
    }
}
