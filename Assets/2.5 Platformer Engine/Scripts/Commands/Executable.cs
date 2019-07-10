using UnityEngine;

namespace Platformer
{
    public class Executable : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        [Tooltip("")]
        public bool AnyCondition;

        /// <summary>
        /// 
        /// </summary>
        [Tooltip("")]
        public ConditionDesc[] Conditions;

        /// <summary>
        /// 
        /// </summary>
        [Tooltip("")]
        public CommandDesc[] Commands;

        public bool IsValid(GameObject target)
        {
            return Validator.Validate(gameObject, target, Conditions, AnyCondition);
        }

        public void Execute(GameObject target)
        {
            if (Validator.Validate(gameObject, target, Conditions, AnyCondition))
                StartCoroutine(Executor.Run(gameObject, target, Commands));
        }
    }
}
