using System.Collections.Generic;
using UnityEngine;

namespace Platformer
{
    /// <summary>
    /// Description of a character in the game world.
    /// </summary>
    public struct Character
    {
        /// <summary>
        /// Link to the game object of the character.
        /// </summary>
        public GameObject Object;

        /// <summary>
        /// Link to the character motor of the character.
        /// </summary>
        public CharacterMotor Motor;
        
        /// <summary>
        /// Link to the possible AI component of the object.
        /// </summary>
        public AIController AI;

        /// <summary>
        /// Side/team id of the character. The value is only meaningful if HasSide is true.
        /// </summary>
        public int Side;

        /// <summary>
        /// Does the character have a defined side.
        /// </summary>
        public bool HasSide;

        private static RaycastHit[] _hits = new RaycastHit[16];

        /// <summary>
        /// Returns true if the character top is in camera's view.
        /// </summary>
        public bool IsInSight(float height, float delta)
        {
            if (Object == null)
                return false;

            return CanSeePosition(PositionAtHeight(height), delta);
        }

        /// <summary>
        /// Helper function to see if the position is not occluded.
        /// </summary>
        public bool CanSeePosition(Vector3 position, float delta)
        {
            var camera = Camera.main;

            if (camera == null)
                return false;

            var wdelta = delta * (float)Screen.height / (float)Screen.width;

            var viewportPosition = ViewportPoint(position);
            if (viewportPosition.x >= -wdelta && viewportPosition.y >= -delta && viewportPosition.x <= 1 + wdelta && viewportPosition.y <= 1 + delta && viewportPosition.z > 0)
            {
                var vector = position - camera.transform.position;
                var distance = vector.magnitude;
                vector.Normalize();

                for (int i = 0; i < Physics.RaycastNonAlloc(camera.transform.position, vector, _hits); i++)
                {
                    var hit = _hits[i];

                    if (!hit.collider.isTrigger && hit.collider.gameObject.layer == 0 && hit.distance < distance - 0.5f)
                        return false;
                }

                return true;
            }

            return false;
        }

        public Vector3 PositionAtHeight(float height)
        {
            if (Object == null)
                return Vector3.zero;

            if (height > float.Epsilon || height < -float.Epsilon)
            {
                var capsule = Object.GetComponent<CapsuleCollider>();

                if (capsule != null)
                    height *= Object.transform.TransformVector(Vector3.up * capsule.height).y;
            }

            return Object.transform.position + Vector3.up * height;
        }

        /// <summary>
        /// Helper function to get viewport point of the character at given height.
        /// </summary>
        public Vector3 ViewportPoint(float height = 0)
        {
            return ViewportPoint(PositionAtHeight(height));
        }

        /// <summary>
        /// Helper function to get viewport point of the given position.
        /// </summary>
        public Vector3 ViewportPoint(Vector3 position)
        {
            if (Camera.main == null)
                return Vector2.zero;

            return Camera.main.WorldToViewportPoint(position);
        }

        /// <summary>
        /// Returns true if both characters are on the same team/side.
        /// </summary>
        public bool IsSameSide(Character other)
        {
            if (HasSide && other.HasSide)
                return Side == other.Side;
            else
                return (AI != null) == (other.AI != null);
        }
    }

    public static class Characters
    {
        /// <summary>
        /// All alive characters during the last update.
        /// </summary>
        public static IEnumerable<Character> All
        {
            get
            {
                foreach (var character in list)
                    if (character.Motor.IsAlive)
                        yield return character;
            }
        }

        public static Character MainPlayer;

        private static Dictionary<GameObject, Character> dictionary = new Dictionary<GameObject, Character>();
        private static List<Character> list = new List<Character>();

        public static void Register(CharacterMotor motor)
        {
            if (motor == null)
                return;

            var build = Build(motor);
            dictionary[motor.gameObject] = build;

            if (MainPlayer.Object == null)
                if (motor.GetComponent<PlayerController>())
                    MainPlayer = build;

            var isContained = false;

            for (int i = 0; i < list.Count; i++)
                if (list[i].Motor == motor)
                {
                    list[i] = Build(motor);
                    isContained = true;
                }

            if (!isContained)
                list.Add(build);
        }

        public static void Unregister(CharacterMotor motor)
        {
            if (motor != null && dictionary.ContainsKey(motor.gameObject))
                dictionary.Remove(motor.gameObject);

            for (int i = 0; i < list.Count; i++)
                if (list[i].Motor == motor)
                {
                    list.RemoveAt(i);
                    break;
                }
        }

        /// <summary>
        /// Returns cached character description for the given object.
        /// </summary>
        public static Character Get(GameObject gameObject)
        {
            if (!dictionary.ContainsKey(gameObject))
                dictionary[gameObject] = Build(gameObject.GetComponent<CharacterMotor>());

            return dictionary[gameObject];
        }

        /// <summary>
        /// Creates and returns character description for the given object.
        /// </summary>
        public static Character Build(CharacterMotor motor)
        {
            Character character;
            character.AI = motor.GetComponent<AIController>();
            character.Motor = motor;
            character.Object = motor.gameObject;

            var side = motor.GetComponent<CharacterSide>();
            character.Side = side == null ? 0 : side.Side;
            character.HasSide = side != null;

            return character;
        }
    }
}
