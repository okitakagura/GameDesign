using System.Collections.Generic;
using UnityEngine;

namespace Platformer
{
    /// <summary>
    /// Manages a trail mesh and visibility. Usually used for weapons.
    /// </summary>
    public class Trail : MonoBehaviour
    {
        /// <summary>
        /// Height of the trail.
        /// </summary>
        [Tooltip("Height of the trail.")]
        public float Height = 1.0f;

        /// <summary>
        /// Time in seconds to cut off old trail points.
        /// </summary>
        [Tooltip("Time in seconds to cut off old trail points.")]
        public float MaxTime = 0.2f;

        /// <summary>
        /// Speed of the appearance and dissapearance of the trai.
        /// </summary>
        [Tooltip("Speed of the appearance and dissapearance of the trai.")]
        public float FadeSpeed = 10;

        private List<TrailPoint> _points = new List<TrailPoint>();
        private Mesh _mesh;
        private float _fade = 0.0f;
        private float _fadeTarget = 0.0f;
        private Material _defaultMaterial;

        /// <summary>
        /// Overrides the material with the given value. Resets to default material if given null.
        /// </summary>
        /// <param name="value"></param>
        public void SetMaterialOverride(Material value)
        {
            var renderer = GetComponent<Renderer>();

            if (renderer != null)
            {
                if (_defaultMaterial == null)
                    _defaultMaterial = renderer.material;

                if (value == null)
                    renderer.material = _defaultMaterial;
                else
                    renderer.material = value;
            }
        }

        /// <summary>
        /// Sets the trail to appear.
        /// </summary>
        public void Show()
        {
            _fadeTarget = 1.0f;
        }

        /// <summary>
        /// Sets the trail to dissapear.
        /// </summary>
        public void Hide()
        {
            _fadeTarget = 0.0f;
        }

        /// <summary>
        /// Updates the trail mesh and visibility.
        /// </summary>
        private void Update()
        {
            var time = Time.realtimeSinceStartup;
            var position = transform.position;

            if (_fadeTarget > _fade)
                _fade = Mathf.Clamp(_fade + Time.deltaTime * FadeSpeed, _fade, _fadeTarget);
            else
                _fade = Mathf.Clamp(_fade - Time.deltaTime * FadeSpeed, _fadeTarget, _fade);

            if (_fade > float.Epsilon)
                _points.Insert(0, new TrailPoint(position, transform.up, time));

            for (int i = _points.Count - 1; i >= 0; i--)
                if (time - _points[i].Time > MaxTime)
                    _points.RemoveAt(i);

            if (_mesh == null)
                _mesh = GetComponent<MeshFilter>().mesh;

            if (_mesh != null)
            {
                _mesh.Clear();

                if (_points.Count > 2)
                {
                    const int interpolation = 3;
                    var vertexCount = (_points.Count - 1) * interpolation * 2;

                    var vertices = new Vector3[vertexCount];
                    var colors = new Color[vertexCount];
                    var uv = new Vector2[vertexCount];
                    var triangles = new int[(vertexCount - 2) * 3];

                    var length = time - _points[_points.Count - 1].Time;
                    var matrix = transform.worldToLocalMatrix;

                    int point = 0;
                    var data = new TrailPoint[4];
                    var v = 0;

                    while (point < _points.Count - 1)
                    {
                        int count;

                        var left = _points.Count - point;
                        if (left == 4 || left > 5)
                            count = 4;
                        else
                            count = 3;

                        for (int i = 0; i < count; i++)
                            data[i] = _points[point + i];

                        for (int s = 0; s < (count - 1) * interpolation; s++)
                        {
                            var step = (float)s / (float)((count - 1) * interpolation);

                            Vector3 pos;
                            Vector3 up;
                            float t;

                            if (count == 4)
                            {
                                var n = 1.0f - step;
                                var w1 = n * n * n;
                                var w2 = 3 * n * n * step;
                                var w3 = 3 * n * step * step;
                                var w4 = step * step * step;

                                pos = w1 * data[0].Position + w2 * data[1].Position + w3 * data[2].Position + w4 * data[3].Position;
                                up = (w1 * data[0].Up + w2 * data[1].Up + w3 * data[2].Up + w4 * data[3].Up).normalized;
                                t = w1 * data[0].Time + w2 * data[1].Time + w3 * data[2].Time + w4 * data[3].Time;
                            }
                            else if (count == 3)
                            {
                                var n = 1.0f - step;
                                var w1 = n * n;
                                var w2 = 2 * n * step;
                                var w3 = step * step;

                                pos = w1 * data[0].Position + w2 * data[1].Position + w3 * data[2].Position;
                                up = (w1 * data[0].Up + w2 * data[1].Up + w3 * data[2].Up).normalized;
                                t = w1 * data[0].Time + w2 * data[1].Time + w3 * data[2].Time;
                            }
                            else
                            {
                                pos = Vector3.zero;
                                up = Vector3.zero;
                                t = 0;
                                Debug.Assert(false);
                            }

                            t = time - t;
                            var u = Mathf.Clamp01(t / length);
                            var f = Mathf.Clamp01(1.0f - t / length);

                            var color = Color.Lerp(new Color(1, 1, 1, _fade), new Color(1, 1, 1, 0), f * _fade);

                            vertices[v * 2 + 0] = matrix.MultiplyPoint(pos);
                            vertices[v * 2 + 1] = matrix.MultiplyPoint(pos + up * Height);

                            uv[v * 2 + 0] = new Vector2(u, 0);
                            uv[v * 2 + 1] = new Vector2(u, 1);

                            colors[v * 2 + 0] = color;
                            colors[v * 2 + 1] = color;

                            v += 1;
                        }

                        point += count - 1;
                    }

                    for (int i = 0; i < triangles.Length / 6; i++)
                    {
                        triangles[i * 6 + 0] = i * 2;
                        triangles[i * 6 + 1] = i * 2 + 1;
                        triangles[i * 6 + 2] = i * 2 + 2;

                        triangles[i * 6 + 3] = i * 2 + 2;
                        triangles[i * 6 + 4] = i * 2 + 1;
                        triangles[i * 6 + 5] = i * 2 + 3;
                    }

                    _mesh.vertices = vertices;
                    _mesh.colors = colors;
                    _mesh.uv = uv;
                    _mesh.triangles = triangles;
                }
            }
        }
    }

    /// <summary>
    /// Info about a point in the trail.
    /// </summary>
    public struct TrailPoint
    {
        /// <summary>
        /// Position of the point.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Orientation of the point.
        /// </summary>
        public Vector3 Up;

        /// <summary>
        /// Time in seconds since the start of the game when this point appeared.
        /// </summary>
        public float Time;

        public TrailPoint(Vector3 position, Vector3 up, float time)
        {
            Position = position;
            Up = up;
            Time = time;
        }
    }
}