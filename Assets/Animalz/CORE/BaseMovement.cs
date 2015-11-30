using UnityEngine;

namespace Assets.Animalz
{
    //Controller based movement
    public class BaseMovement : MonoBehaviour
    {
        public float MovementSpeed = 5f, RunSpeed = 10f;

        //BaseMovement using character
        protected CharacterController CharController;

        public Vector3 Velocity()
        {
            return CharController.velocity;
        }

        public virtual void Move(Vector3 direction)
        {
            CharController.SimpleMove(direction * RunSpeed);
        }

        public virtual void MoveTo(Vector3 target)
        {
            CharController.Move(target);
        }

        public virtual void Awake()
        {
            if (CharController == null) CharController = gameObject.AddComponent<CharacterController>();
        }
    }
}