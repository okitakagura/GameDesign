using UnityEditor;
using UnityEngine;

namespace Platformer
{
    [CustomEditor(typeof(Cycle))]
    public class CycleEditor : Editor
    {
        private void OnSceneGUI()
        {
            var cycle = (Cycle)target;
            if (cycle == null) return;

            var previous = Handles.color;
            Handles.color = Color.green;

            Handles.DrawLine(cycle.transform.position, cycle.transform.position + cycle.Shift * Cycle.Smooth(1 - cycle.Travel));
            Handles.DrawLine(cycle.transform.position, cycle.transform.position - cycle.Shift * Cycle.Smooth(cycle.Travel));

            Handles.color = previous;
        }
    }
}
