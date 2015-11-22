using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Movement : Pathfinding
{
    //Variables
    public float DistanceToWander = 6f;
    public float MovementSpeed = 1f;

    private Vector3 sourcePosition;
    private Vector3 targetPosition;

    internal void updateTarget(Vector3 target)
    {
        sourcePosition = transform.position;
        targetPosition = target;
        FindPath(sourcePosition, targetPosition);
    }

    public void Flee(List<Vector3> enemyPositions)
    {        
        updateTarget( FleePosition( enemyPositions.ToArray() ) );
    }

    public void Flee(Vector3[] enemyPositions)
    {
        updateTarget( FleePosition(enemyPositions) );
    }

    internal Vector3 FleePosition(Vector3[] enemyPositions)
    {
        if (enemyPositions == null || enemyPositions.Count() == 0) return transform.position; //if wrong input return own position

        //sort by increasing order
        var ordered = enemyPositions.OrderBy(a => a.x).ToArray();
        ordered = ordered.OrderBy(a => a.z).ToArray();

        //do pairwise vector additions, until we end up with 1 vector
        var count = ordered.Length;
        var last = ordered;
        while (count > 1)
        {
            var tmp = new Vector3[(last.Length + 1)/2];
            for (var i = 0; i < last.Length; i = i + 2)
            {
                var a = ordered[i].normalized;
                var b = ordered[i + 1].normalized;
                tmp[(i + 1)/2] = a + b;
            }
            count = tmp.Length;
            last = tmp;
        }

        //and reverse that vector
        return -last[0];
    }

    public void Wander()
    {
        //generate
        //Set Random Direction
        Vector2 newVector = Random.insideUnitCircle;

//        Vector2 newVector3 = new Vector2 {x = newVector.x, y = newVector.y};

        //Set Length of Vector
        newVector = newVector*DistanceToWander;

        //Set Position to Move to relative to Current Position
        Vector3 vector = transform.position + new Vector3
                                                {
                                                    x = newVector.x,
                                                    y = 5000f,
                                                    z = newVector.y
                                                };

        //check vector is within MIN/MAX

        updateTarget(vector);
    }

    public bool TakeMoveStep()
    {
        if (Path.Count > 0)
        {
            transform.position = Vector3.MoveTowards(transform.position, Path[0], Time.deltaTime*720F);
            if (Vector3.Distance(transform.position, Path[0]) < 0.1F)
            {
                Path.RemoveAt(0);
            }
            return true;
        }
        return false;
    }

    // Update is called once per frame
    void Update()
    {
        if (TakeMoveStep()) //true if step is taken false if it has reached end of path
        {
            //do some sensing - maybe update the movement type to recompute path to flee, chase or wander
        }
        else
        {
                Wander();
        }
    }

}