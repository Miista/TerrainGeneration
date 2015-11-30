using UnityEngine;

namespace Assets.Animalz
{
    public class AiRig : MonoBehaviour
    {
        protected AiPrototype Prototype;

        public AudiatorySense AudioSensor;
        public VisionSense VisionSensor;
        public ScentSense ScentSensor;
        public Ranking RankingSystem;
        public BaseMovement Movement;
        internal AbstractMind Mind { get; set; }

        public void Awake()
        {
            Prototype = AiPrototype.ExtractPrototypeInfo(this);

            Mind = MindImpl();
            Mind.AiRig = this;
            
            //Movement
            Movement = MoveImpl();


            //Vision Setup
            GameObject vision = CreateContainer("Vision");
            var visionCollider = vision.AddComponent<SphereCollider>();
            vision.AddComponent<Rigidbody>()
                  .isKinematic = true;
            VisionSensor = vision.AddComponent<VisionSense>();
            VisionSensor.Collider = visionCollider;
            VisionSensor.Mind = Mind;

            //Audio Setup
            GameObject audio = CreateContainer("Audio");
            var audioCollider = audio.AddComponent<SphereCollider>();
            audio.AddComponent<Rigidbody>()
                 .isKinematic = true;
            AudioSensor = audio.AddComponent<AudiatorySense>();
            AudioSensor.Collider = audioCollider;
            AudioSensor.Mind = Mind;

            //Scent Setup
            GameObject smell = CreateContainer("Scent");
            var scentCollider = smell.AddComponent<SphereCollider>();
            smell.AddComponent<Rigidbody>()
                  .isKinematic = true;
            ScentSensor = smell.AddComponent<ScentSense>();
            ScentSensor.Collider = scentCollider;
            ScentSensor.Mind = Mind;

            //Ranking between AI's
            RankingSystem = RankingImpl();


        }

        //Overwrite with your specific implementation
        protected virtual AbstractMind MindImpl()
        {
            return gameObject.AddComponent<ConcreteMind>();
        }

        protected virtual BaseMovement MoveImpl()
        {
            return gameObject.AddComponent<BaseMovement>();
        }

        protected virtual Ranking RankingImpl()
        {
            return new Ranking(this) { Rank = Prototype.RankingPreferences.Rank };
        }

        //Use to create subcomponents for senses
        protected GameObject CreateContainer(string objectName)
        {
            var container = new GameObject(objectName);
            container.transform.parent = gameObject.transform;

            // Place the game object at the same position and rotation as the AiRig
            container.transform.position = transform.position;
            container.transform.rotation = transform.rotation;
            return container;
        }

        //Should only be created as part of our base framework
        public static AiRig ExtractRigInfo(Component c)
        {
            var rig = c.GetComponent<AiRig>();
            if (rig == null)
                c.GetComponentInParent<AiRig>();
            return rig;
        }
    }
}