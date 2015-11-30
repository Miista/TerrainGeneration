using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Animalz
{

    //Circular Sense
    //Range based
    //Not Implemented yet
    //General ideas would be to take wind direction and height in consideration and detect based on those factors
    public class ScentSense : Sense
    {
        protected override void loadVars()
        {
            Range = Prototype.ScentSensePreferences.Range;
        }
    }

    //Circular Sense
    //range based
    public class AudiatorySense : Sense
    {
        internal AudioPref CreatedFrom = new AudioPref();

        protected override void loadVars()
        {
            Range = Prototype.AudioSensePreferences.Range;
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

            ReportDetection(other);
        }

    }

    //Forward directional sense
    //Forward-vector is the direction
    //FieldOfViewAngle Angle depicts the viewsection
    public class VisionSense : Sense
    {
        public float FieldOfViewAngle = 0f;

        protected override void loadVars()
        {
            Range = Prototype.VisonSensePreferences.Range;
            FieldOfViewAngle = Prototype.VisonSensePreferences.FieldOfView;
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
            if (angle < 0.5 * FieldOfViewAngle)
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
            if (angle < 0.5 * FieldOfViewAngle)
            {
                RaycastHit ray;
                if (Physics.Raycast(transform.position, direction, out ray))
                {
                    var otherAnimal = other.GetComponentInParent<AiRig>();
                    var collidedAnimal = ray.collider.GetComponentInParent<AiRig>();
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

    //General Sense
    public class Sense : MonoBehaviour
    {
        protected AiPrototype Prototype;

        //Use for Parent lookups
        private static readonly Type AiBase = typeof(AiRig);

        //General range variable
        public float Range = 0f;

        //Detection set - contains detected entities
        internal HashSet<Collider> Detected = new HashSet<Collider>();

        //Trigger component for detection events
        internal SphereCollider Collider { get; set; }

        //Reference to Parent mind for callbacks
        internal AbstractMind Mind { get; set; }

        public void Start()
        {
            Collider.isTrigger = true;
            Prototype = AiPrototype.ExtractPrototypeInfo(this);
            loadVars();
        }

        protected virtual void loadVars()
        {
            Range = 10;
        }

        public void Update()
        {
            loadVars();
            if (Collider.radius != Range)
            {
                Collider.radius = Range;
            }
        }

        protected bool IsAnimalAi(Collider other)
        {
            return other.gameObject.GetComponent(AiBase) != null;
        }

        protected bool IsSelf(Collider other)
        {
            var components = GetComponentInParent(AiBase);
            var otherAnimal = other.GetComponentInParent(AiBase);
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
}