using UnityEngine;

namespace Platformer
{
    public enum CycleTarget
    {
        Loop,
        Home,
        Out
    }

    /// <summary>
    /// Moves an object in a cycle.
    /// </summary>
    public class Cycle : MonoBehaviour
    {
        /// <summary>
        /// Position shift for the object to travel.
        /// </summary>
        [Tooltip("Position shift for the object to travel.")]
        public Vector3 Shift = new Vector3(1, 0, 0);

        /// <summary>
        /// Rotation from home to out positions.
        /// </summary>
        [Tooltip("Rotation from home to out positions.")]
        public Vector3 Rotation = new Vector3(0, 0, 0);

        /// <summary>
        /// Target state. If set to home or out the object will stay at that position without looping.
        /// </summary>
        [Tooltip("Target state. If set to home or out the object will stay at that position without looping.")]
        public CycleTarget Target = CycleTarget.Home;

        /// <summary>
        /// Duration in seconds for the object to go from the home state of the out state.
        /// </summary>
        [Tooltip("Duration in seconds for the object to go from the home state of the out state.")]
        public float ForwardDuration = 1.0f;

        /// <summary>
        /// Duration in seconds for the object to go from the out state to the home state.
        /// </summary>
        [Tooltip("Duration in seconds for the object to go from the out state to the home state.")]
        public float BackwardDuration = 1.0f;

        /// <summary>
        /// Duration in seconds for the object to wait at home before going to the out state.
        /// </summary>
        [Tooltip("Duration in seconds for the object to wait at home before going to the out state.")]
        public float HomePauseDuration = 0.0f;

        /// <summary>
        /// Duration in seconds for the object to wait at the out position before going to the home state.
        /// </summary>
        [Tooltip("Duration in seconds for the object to wait at the out position before going to the home state.")]
        public float OutPauseDuration = 0.0f;

        /// <summary>
        /// Current fraction of the distance between the home and out states.
        /// </summary>
        [Tooltip("Current fraction of the distance between the home and out states.")]
        [Range(0, 1)]
        public float Travel;

        private enum Step
        {
            forward,
            outPause,
            back,
            homePause
        }

        private Step _step;
        private float _pause;

        /// <summary>
        /// Manages the object position and cycle states.
        /// </summary>
        private void Update()
        {
            Travel = Mathf.Clamp01(Travel);
            var previousTravel = Travel;

            var elapsed = Time.deltaTime;

            if (_step == Step.forward && Target == CycleTarget.Home)
                _step = Step.back;

            if (_step == Step.back && Target == CycleTarget.Out)
                _step = Step.forward;

            while (elapsed > float.Epsilon)
                switch (_step)
                {
                    case Step.forward:
                    {
                        var left = (1 - Travel) * ForwardDuration;

                        if (left <= Time.deltaTime)
                        {
                            Travel = 1;
                            _pause = 0;
                            _step = Step.outPause;

                            if (left > 0)
                            elapsed -= left;
                        }
                        else
                        {
                            Travel += Time.deltaTime / ForwardDuration;
                            elapsed = 0;
                        }
                    } break;

                    case Step.back:
                    {
                        var left = Travel * BackwardDuration;

                        if (left <= Time.deltaTime)
                        {
                            Travel = 0;
                            _pause = 0;
                            _step = Step.homePause;

                            if (left > 0)
                                elapsed -= left;
                        }
                        else
                        {
                            Travel -= Time.deltaTime / BackwardDuration;
                            elapsed = 0;
                        }
                    } break;

                    case Step.homePause:
                    {
                        var left = (1 - _pause) * HomePauseDuration;

                        if (left <= Time.deltaTime && Target != CycleTarget.Home)
                        {
                            Travel = 0;
                            _pause = 0;
                            _step = Step.forward;
                            
                            if (left > 0)
                                elapsed -= left;
                        }
                        else
                        {
                            _pause += Time.deltaTime / HomePauseDuration;
                            elapsed = 0;

                            if (_pause > 1)
                                _pause = 1;
                        }
                    } break;

                    case Step.outPause:
                    {
                        var left = (1 - _pause) * OutPauseDuration;

                        if (left <= Time.deltaTime && Target != CycleTarget.Out)
                        {
                            Travel = 1;
                            _pause = 0;
                           _step = Step.back;

                            if (left > 0)
                                elapsed -= left;
                        }
                        else
                        {
                            _pause += Time.deltaTime / OutPauseDuration;
                            elapsed = 0;

                            if (_pause > 1)
                                _pause = 1;
                        }
                    } break;
                }

            var offset = Smooth(Travel) - Smooth(previousTravel);
            transform.position = transform.position + Shift * offset;
            transform.eulerAngles = transform.eulerAngles + Rotation * offset;
        }

        /// <summary>
        /// Smoothens the travel value.
        /// </summary>
        public static float Smooth(float t)
        {
            return t * t * (3.0f - 2.0f * t);
        }
    }
}
