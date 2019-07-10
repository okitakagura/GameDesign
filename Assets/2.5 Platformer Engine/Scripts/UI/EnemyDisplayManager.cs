using System.Collections.Generic;
using UnityEngine;

namespace Platformer
{
    /// <summary>
    /// Manages and instantiates health bars and arrows for enemies.
    /// </summary>
    public class EnemyDisplayManager : MonoBehaviour
    {
        /// <summary>
        /// Prototype of a health bar to be shown on any visible enemy.
        /// </summary>
        [Tooltip("Prototype of a health bar to be shown on any visible enemy.")]
        public RectTransform HealthPrototype;

        /// <summary>
        /// Player that is used to determine who is an enemy.
        /// </summary>
        [Tooltip("Player that is used to determine who is an enemy.")]
        public GameObject Player;

        /// <summary>
        /// Offset of the health bar relative to the screen height.
        /// </summary>
        [Tooltip("Offset of the health bar relative to the screen height.")]
        public Vector2 Offset = new Vector2(0, 0.2f);

        private Dictionary<GameObject, GameObject> _bars = new Dictionary<GameObject, GameObject>();
        private List<GameObject> _keep = new List<GameObject>();

        /// <summary>
        /// Updates positions of health bars and creates and destroys bars when needed.
        /// </summary>
        private void LateUpdate()
        {
            /// Manage health bars
            {
                _keep.Clear();

                if (HealthPrototype != null)
                {
                    foreach (var character in Characters.All)
                        if (character.Object != Player)
                        {
                            var position = character.ViewportPoint(1);

                            if (character.IsInSight(0.5f, -0.01f))
                            {
                                _keep.Add(character.Object);

                                if (!_bars.ContainsKey(character.Object))
                                {
                                    var clone = GameObject.Instantiate(HealthPrototype.gameObject);
                                    clone.transform.SetParent(transform, false);
                                    clone.GetComponent<HealthBar>().Target = character.Object;
                                    _bars.Add(character.Object, clone);
                                }

                                var t = _bars[character.Object].GetComponent<RectTransform>();
                                t.gameObject.SetActive(true);
                                t.position = new Vector3(position.x * Screen.width + Offset.x * Screen.height, position.y * Screen.height + Offset.y * Screen.height, t.position.z);
                            }
                        }
                }

                for (int i = 0; i < _bars.Count - _keep.Count; i++)
                {
                    foreach (var key in _bars.Keys)
                        if (!_keep.Contains(key))
                        {
                            GameObject.Destroy(_bars[key]);
                            _bars.Remove(key);
                            break;
                        }
                }
            }
        }
    }
}
