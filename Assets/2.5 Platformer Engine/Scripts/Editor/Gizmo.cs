using UnityEditor;
using UnityEngine;

namespace Platformer
{
    public class Gizmo : Editor
    {
        [DrawGizmo(GizmoType.NotInSelectionHierarchy | GizmoType.Pickable | GizmoType.Selected)]
        static void ExampleGizmo(Turn turn, GizmoType type)
        {
            var isSelected = (type & GizmoType.Selected) != 0;
            var alpha = isSelected ? 0.8f : 0.4f;

            var collider = turn.GetComponent<BoxCollider>();
            if (collider != null)
            {
                var bounds = collider.bounds;

                Gizmos.color = new Color(0, 0.7f, 1, alpha);
                Gizmos.DrawCube(bounds.center, bounds.extents * 2);
            }

            Gizmos.color = new Color(1, 0, 0, alpha);
            Gizmos.DrawLine(turn.transform.position, turn.transform.position + turn.EnterDirection * 4);

            Gizmos.color = new Color(0, 1, 0, alpha);
            Gizmos.DrawLine(turn.transform.position, turn.transform.position + turn.ExitDirection * 4);
        }

        [DrawGizmo(GizmoType.NotInSelectionHierarchy | GizmoType.Pickable | GizmoType.Selected)]
        static void ExampleGizmo(Edge edge, GizmoType type)
        {
            var isSelected = (type & GizmoType.Selected) != 0;
            var alpha = isSelected ? 0.8f : 0.4f;

            var collider = edge.GetComponent<BoxCollider>();
            if (collider != null)
            {
                var bounds = collider.bounds;

                Gizmos.color = new Color(0, 1, 0.6f, alpha);
                Gizmos.DrawCube(bounds.center, bounds.extents * 2);
            }
        }

        [DrawGizmo(GizmoType.NotInSelectionHierarchy | GizmoType.Pickable | GizmoType.Selected)]
        static void ExampleGizmo(Executable executable, GizmoType type)
        {
            var isSelected = (type & GizmoType.Selected) != 0;
            var alpha = isSelected ? 0.4f : 0.15f;

            var collider = executable.GetComponent<BoxCollider>();
            if (collider != null)
            {
                var bounds = collider.bounds;

                Gizmos.color = new Color(1, 0.5f, 0, alpha);
                Gizmos.DrawCube(bounds.center, bounds.extents * 2);
            }
        }
    }
}
