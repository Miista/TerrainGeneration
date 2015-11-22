using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IPath
{
    List<Vector3> FindPath(Vector3 startPosition, Vector3 endPosition);

}
