using UnityEngine;

namespace Assets.Animalz
{

    //Write behaviour using the Sensor feedback or use AiRig hook to access senses in update()
    public abstract class AbstractMind : MonoBehaviour
    {
        internal AiRig AiRig;

        internal abstract void ReportDetectOf(Sense origin, Collider other);
        internal abstract void ReportUndectOf(Sense origin, Collider other);
    }
}