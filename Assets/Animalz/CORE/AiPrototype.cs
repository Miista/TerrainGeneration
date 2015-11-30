using System;
using UnityEngine;

namespace Assets.Animalz
{
    public class AiPrototype : MonoBehaviour
    {
        protected AiRig ThisAiImpl;

        //Sensor Variables
        public VisionPref VisonSensePreferences;
        public AudioPref AudioSensePreferences;
        public ScentPref ScentSensePreferences;

        //RankingPreferences variables
        public RankingPref RankingPreferences;

        void Start()
        {
            //Actual AiRig Implementation
            ThisAiImpl = AiRigImpl();
        }

        //overwrite with specific implementation
        protected virtual AiRig AiRigImpl()
        {
            return gameObject.AddComponent<AiRig>();           
        }

        //Should only be created as part of our base framework and hence would always be a 'parent'
        public static AiPrototype ExtractPrototypeInfo(Component c)
        {
            return c.GetComponentInParent<AiPrototype>();
        }
    }

    [Serializable]
    public class ScentPref
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

    [Serializable]
    public class RankingPref
    {
        public int Rank;
    }

    [Serializable]
    public class MovePref
    {
        public int MoveSpeed = 1, RunSpeed = 2;
    }
}