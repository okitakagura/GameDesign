using UnityEngine;

namespace Platformer
{
    /// <summary>
    /// Utility functions.
    /// </summary>
    public sealed class Util
    {
        /// <summary>
        /// Delta of a point on AB line closest to the given point.
        /// </summary>
        public static float FindDeltaPath(Vector3 a, Vector3 b, Vector3 point)
        {
            Vector3 ap = point - a;
            Vector3 ab = b - a;
            float ab2 = ab.x * ab.x + ab.y * ab.y + ab.z * ab.z;
            float ap_ab = ap.x * ab.x + ap.y * ab.y + ap.z * ab.z;
            float t = ap_ab / ab2;

            return t;
        }

        /// <summary>
        /// Position of a point on AB line closest to the given point. Clamps the position.
        /// </summary>
        public static Vector3 FindClosestToPath(Vector3 a, Vector3 b, Vector3 point)
        {
            Vector3 ap = point - a;
            Vector3 ab = b - a;
            float ab2 = ab.x * ab.x + ab.y * ab.y + ab.z * ab.z;
            float ap_ab = ap.x * ab.x + ap.y * ab.y + ap.z * ab.z;
            float t = ap_ab / ab2;

            return a + ab * Mathf.Clamp01(t);
        }

        /// <summary>
        /// Position of a point on AB line closest to the given point.
        /// </summary>
        public static Vector3 FindClosestToLine(Vector3 a, Vector3 b, Vector3 point)
        {
            Vector3 ap = point - a;
            Vector3 ab = b - a;
            float ab2 = ab.x * ab.x + ab.y * ab.y + ab.z * ab.z;
            float ap_ab = ap.x * ab.x + ap.y * ab.y + ap.z * ab.z;
            float t = ap_ab / ab2;

            return a + ab * t;
        }
    }
}