using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Animalz;
using Pathfinding;
using Pathfinding.Serialization;
using Animal = Assets.Animalz.Animal;

public class BeastMind : ConcreteMind //MonoBehaviour
{
    public enum STATE
    {
        WANDER, FLEE, CHASE
    }

    private class MyPath : AIPath
    {
        public override void OnTargetReached()
        {
            
        }
    }

    public float DistanceToWander = 5f,
        MovementSpeed = 2f,
        RunSpeed = 5f;

    private STATE _state = STATE.WANDER;

    //LastTarget chased - for logic
    private Collider _chaseTarget;

    //Movement and pathing
    private CharacterController _charController;
    private MyPath _pathfinder;

    //Lists
    private HashSet<Collider> threats = new HashSet<Collider>();
    private HashSet<Collider> prey = new HashSet<Collider>();

    public void Awake()
    {
        var mAnimal = this.GetComponentInParent<Assets.Animalz.Animal>();
        _charController = this.GetComponentInParent<CharacterController>();
        //Make sure a characterController exists
        if (_charController == null)
        {
            _charController = mAnimal.gameObject.AddComponent<CharacterController>();
            _charController.transform.parent = mAnimal.transform;
        }

        //make sure our pathfinder instance is init
        if (_pathfinder == null)
        {
            _pathfinder = mAnimal.gameObject.AddComponent<MyPath>();
            _pathfinder.transform.parent = mAnimal.transform;
        }
        
        //initiate a wander() state
        Wander();
    }

    internal override void ReportDetectOf(Sense origin, Collider other)
    {
        if (origin is VisionSense)
        {
            var otherAnimal = other.GetComponentInParent<Assets.Animalz.Animal>();
            var isDangerous = this.MyAnimal.Rank < otherAnimal.Rank;
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
                if (_chaseTarget == other)
                {
                    //convert to 'last known position'
                    Chase( _chaseTarget.transform.position );
                }
            }
            Debug.Log("LostSight " + other.name);
        }
        else if (origin is AudiatorySense)
        {
            //Shouldnt react to not hearing anything
        }
    }

    override
    public void Update()
    {

        if (!_pathfinder.TargetReached)
        {
            if (threats.Count > 0) FleeFrom(threats);
            else
            {
                if (_state == STATE.WANDER && prey.Count > 0)
                {
                    Chase(prey.First().transform);
                }
            }
        }
        else
        {
            UpdateState();
        }

    }

    void UpdateState()
    {
            switch (_state)
            {
                case STATE.WANDER:
                    Wander();
                    break;
                case STATE.CHASE:
                    Debug.Log("Chase");
                    Chase(_chaseTarget.transform);
                    break;
                case STATE.FLEE:
                    Debug.Log("Flee");
                    FleeFrom(threats);
                    break;
            }        
    }

    //Use the pathfinder to recompute a new path
    //with transform
    void RecomputePath(Transform target)
    {
        _pathfinder.chaseTarget = target;
        _pathfinder.TrySearchPath();
    }
    //with vector
    void RecomputePath(Vector3 target)
    {
        _chaseTarget = null;
        _pathfinder.target = target;
        _pathfinder.TrySearchPath();
    }


    //Updates the movement to Chase behaviour
    void Chase(Transform target)
    {
        RecomputePath(target);
    }
    void Chase(Vector3 target)
    {
        RecomputePath(target);
    }

    //Updates behaviour to flee from the threats
    void FleeFrom(ICollection<Collider> others)
    {
        if (others == null || others.Count == 0) return; //if wrong input do nothing
        //sort by increasing order
        var ordered = (others.ToArray()).OrderBy(a => a.transform.position.x).ToArray();
        ordered = ordered.OrderBy(a => a.transform.position.z).ToArray();

        //do pairwise vector additions, until we end up with 1 vector
        var count = ordered.Count();
        var last = ordered.Select(a => a.transform.position).ToArray();
        while (count > 1)
        {
            var tmp = new Vector3[(last.Count() + 1) / 2];
            for (var i = 0; i < last.Count(); i = i + 2)
            {
                var a = ordered[i].transform.position.normalized;

                var b = ordered[i + 1].transform.position.normalized;
                tmp[(i + 1) / 2] = a + b;
            }
            count = tmp.Length;
            last = tmp;
        }

        //and reverse that vector
        RecomputePath(-last[0]);
    }

    //Updates behaviour to wander
    void Wander()
    {
        //generate
        //Set Random Direction
        var randomVector2 = Random.insideUnitCircle;

        var directionVector3 = new Vector3 { x = randomVector2.x * DistanceToWander, y = 0f, z = randomVector2.y * DistanceToWander };

        //copy of transform
        var t = transform.position;

        //Set Position to Move to relative to Current Position
        t += directionVector3;

        RecomputePath(t);
    }

}
