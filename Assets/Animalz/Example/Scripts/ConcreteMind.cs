using UnityEngine;

namespace Assets.Animalz
{
    [System.Serializable]
    public class ConcreteMind : AbstractMind
    {
        float DistanceToWander = 10f;
        private BaseMovement _move;
        private Vector3 lastTarget,
        directionVector3 ;
        private static readonly Material CanSeeAndHear = new Material(Shader.Find("Diffuse")) { color = Color.green };
        private static readonly Material CanSee = new Material(Shader.Find("Diffuse")) { color = Color.black };
        private static readonly Material CanOnlyHear = new Material(Shader.Find("Diffuse")) { color = Color.yellow };

        internal override void ReportDetectOf(Sense origin, Collider other)
        {
            Material m = null;
            if (origin is VisionSense)
            {
                m = AiRig.AudioSensor.Detected.Contains(other)
                    ? CanSeeAndHear
                    : CanSee;
            }
            else if (origin is AudiatorySense)
            {
                m = AiRig.VisionSensor.Detected.Contains(other)
                    ? CanSeeAndHear
                    : CanOnlyHear;
            }
            other.GetComponent<Renderer>()
                .material = m;
        }

        internal override void ReportUndectOf(Sense origin, Collider other)
        {
            other.GetComponent<Renderer>()
                .material = new Material(Shader.Find("Diffuse"))
                {
                    color = Color.white
                };
        }

        public void Start()
        {
            lastTarget = transform.position;
            _move = AiRig.Movement;
        }

        public void Update()
        {
            var targetReached = Vector3.Distance(transform.position, lastTarget) < 3f;
            if (targetReached)
            {
                Wander();
            }

            _move.Move(directionVector3 * Time.deltaTime);
        }

        //Updates behaviour to wander
        void Wander()
        {
            //generate
            //Set Random Direction
            var randomVector2 = Random.insideUnitCircle;

            directionVector3 = new Vector3 { x = randomVector2.x * DistanceToWander, y = 0f, z = randomVector2.y * DistanceToWander };

            //copy of transform
            var t = transform.position;

            //Set Position to Movement to relative to Current Position
            t += directionVector3;

            lastTarget = t;
        }
    }
}