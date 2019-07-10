using UnityEditor;
using UnityEngine;

namespace Platformer
{
    [CustomEditor(typeof(AIController))]
    [CanEditMultipleObjects]
    public class AIEditor : Editor
    {
        private void OnSceneGUI()
        {
            var patrol = (AIController)target;
            if (patrol == null) return;

            var previous = Handles.color;
            Handles.color = Color.red;

            Handles.DrawLine(patrol.transform.position, patrol.LeftPoint);
            Handles.DrawLine(patrol.transform.position, patrol.RightPoint);

            Handles.color = previous;
        }
    }
}
