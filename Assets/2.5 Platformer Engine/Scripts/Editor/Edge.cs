using UnityEditor;
using UnityEngine;

namespace Platformer
{
    [CustomEditor(typeof(Edge))]
    public class EdgeEditor : Editor
    {
        private void OnSceneGUI()
        {
            var edge = (Edge)target;
            if (edge == null) return;

            var previous = Handles.color;

            Handles.color = Color.red;
            Handles.DrawLine(edge.transform.position - edge.transform.right * 1.5f, edge.transform.position);

            Handles.color = previous;
        }
    }
}
