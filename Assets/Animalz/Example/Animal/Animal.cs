using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Animalz
{

    //Example script of how to use the Framework
    public class Animal : AiPrototype
    {
        protected override AiRig AiRigImpl()
        {
            return gameObject.AddComponent<AnimalRig>();
        }
    }

    [System.Serializable]
    public class AnimalRig : AiRig
    {
        protected override AbstractMind MindImpl()
        {
            return gameObject.AddComponent<AnimalMind>();
        }

        protected override BaseMovement MoveImpl()
        {
            return gameObject.AddComponent<AnimalMove>();
        }
    }

    [System.Serializable]
    public class AnimalMove : BaseMovement
    {
        //Adding pathfinding rather than direct movement
        private AIPath _pathfinder;

        public override void Awake()
        {
            base.Awake();
            if (_pathfinder == null)
            {
                _pathfinder = gameObject.AddComponent<AIPath>();
                _pathfinder.transform.parent = transform;
            }
        }           

        public override void Move(Vector3 direction)
        {
            _pathfinder.Target = direction;
            _pathfinder.TrySearchPath();
        }
    }

    //Example Mind
    [System.Serializable]
    public class AnimalMind : AbstractMind
    {
        protected BaseMovement Move;
        protected float DistanceToWander = 5f;
        public enum STATE
        {
            WANDER, FLEE, CHASE
        }

        //LastTarget chased - for logic
        private Vector3 _lastTarget;
        private Transform _chaseTarget;

        private STATE _state = STATE.WANDER;

        //Lists
        private HashSet<Collider> threats = new HashSet<Collider>();
        private HashSet<Collider> prey = new HashSet<Collider>();

        public void Start()
        {
            var rig = AiRig.ExtractRigInfo(this);
            Move = rig.Movement;
            
            //initiate a wander() state
            Wander();
        }

        internal override void ReportDetectOf(Sense origin, Collider other)
        {
            if (origin is VisionSense)
            {
                var otherAiRig = AiRig.ExtractRigInfo(other);
                var isDangerous = this.AiRig.RankingSystem.GreaterThan(otherAiRig.RankingSystem);
                if (isDangerous) threats.Add(other);
                //else if (isDangerous) threats.Add(other);
                else prey.Add(other);
            }
            else if (origin is AudiatorySense)
            {
                //turn towards the sound
                //var dir = transform.position - other.transform.position;
                //_charController.transform.rotation.SetLookRotation(dir);
            }
        }

        internal override void ReportUndectOf(Sense origin, Collider other)
        {
            if (origin is VisionSense)
            {
                if (prey.Contains(other))
                {
                    prey.Remove(other);
                }
            }
            else if (origin is AudiatorySense)
            {
                //Shouldnt react to not hearing anything
            }
        }

        public void Update()
        {
                //Check is any threats have been detected
            if (threats.Count > 0)
            {
                var visibleThreats = threats.Any(  a => AiRig.VisionSensor.Detected.Contains(a) );
                if (visibleThreats)
                {
                    FleeFromThreats();
                    return;
                }
            }

                    //check if we are wandering - and check if prey is nearby
                    if (_state == STATE.WANDER && prey.Count > 0)
                    {
                        Chase(prey.First().transform);
                        return;
                    }

                    //default to wander behaviour
                Wander();
        }

        //Use the pathfinder to recompute a new path
        //with vector
        void RecomputePath(Vector3 target)
        {
            _lastTarget = target;
            Move.Move(target);
        }


        //Updates the movement to Chase behaviour
        void Chase(Transform target)
        {
            var desired_velocity = Vector3.Normalize(target.position - transform.position)*3;
            var steering = desired_velocity - Move.Velocity();
            _chaseTarget = target;
            RecomputePath(steering);
            _state = STATE.CHASE;
        }

        //Updates behaviour to flee from the threats
        void FleeFromThreats()
        {
            var currPos = transform.position;

            if (threats == null || threats.Count == 0) return; //if wrong input do nothing

            //sort by increasing order (ascending) and taking the first
            var nearestEnemy = threats.OrderBy(a => Vector3.Distance(a.transform.position, currPos) ).First().transform.position;

            var dir = Vector3.Normalize(currPos - nearestEnemy)*3;
            var steering = dir - Move.Velocity();

            //and reverse that vector
            RecomputePath(steering);
            _state = STATE.FLEE;
        }

        //Updates behaviour to wander
        void Wander()
        {
            //generate
            //Set Random Direction
            var randomVector2 = Random.insideUnitCircle;

            var directionVector3 = new Vector3 { x = randomVector2.x * DistanceToWander, y = 0f, z = randomVector2.y * DistanceToWander };

            RecomputePath(directionVector3 * 3);
            _state = STATE.WANDER;
        }

    }
}
