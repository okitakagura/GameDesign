using UnityEditor;
using UnityEngine;

namespace Platformer
{
    [CustomEditor(typeof(OnEnter))]
    public class OnEnterEditor : Editor
    {
        private void OnSceneGUI()
        {
            CommandEditor.Show(this);
        }

        public override void OnInspectorGUI()
        {
            CommandEditor.Edit(this);
        }
    }

    [CustomEditor(typeof(OnLeave))]
    public class OnLeaveEditor : Editor
    {
        private void OnSceneGUI()
        {
            CommandEditor.Show(this);
        }

        public override void OnInspectorGUI()
        {
            CommandEditor.Edit(this);
        }
    }

    [CustomEditor(typeof(OnArrive))]
    public class OnArriveEditor : Editor
    {
        private void OnSceneGUI()
        {
            CommandEditor.Show(this);
        }

        public override void OnInspectorGUI()
        {
            CommandEditor.Edit(this);
        }
    }

    [CustomEditor(typeof(OnDeath))]
    public class OnDeathEditor : Editor
    {
        private void OnSceneGUI()
        {
            CommandEditor.Show(this);
        }

        public override void OnInspectorGUI()
        {
            CommandEditor.Edit(this);
        }
    }

    [CustomEditor(typeof(OnAction))]
    public class OnActionEditor : Editor
    {
        private void OnSceneGUI()
        {
            CommandEditor.Show(this);
        }

        public override void OnInspectorGUI()
        {
            CommandEditor.Edit(this);
        }
    }

    public static class CommandEditor
    {
        public static void Show(Editor editor)
        {
            var e = (Executable)editor.target;
            if (e.Commands == null || e.Commands.Length == 0)
                return;

            var previous = Handles.color;

            foreach (var command in e.Commands)
            {
                var objects = command.Object.Get(e.gameObject, null, null);
                var targets = command.Target.Get(e.gameObject, null, null);

                if (Executor.HasObjects(command.Command) && objects != null)
                {
                    Handles.color = Color.green;

                    foreach (var obj in objects)
                        Handles.DrawLine(e.transform.position, obj.transform.position);
                }

                if (Executor.HasTargets(command.Command) && targets != null)
                {
                    Handles.color = Color.red;

                    foreach (var obj in targets)
                        Handles.DrawLine(e.transform.position, obj.transform.position);
                }
            }

            Handles.color = previous;
        }

        public static void Edit(Editor editor)
        {
            var e = (Executable)editor.target;
            Undo.RecordObject(e, "Command editor");

            {
                if (e.Conditions != null)
                {
                    int toDelete = -1;

                    EditorGUILayout.LabelField("Conditions", EditorStyles.boldLabel);

                    EditorGUI.indentLevel++;
                    e.AnyCondition = EditorGUILayout.Toggle("Any", e.AnyCondition);
                    EditorGUI.indentLevel--;

                    for (int i = 0; i < e.Conditions.Length; i++)
                    {
                        EditorGUILayout.Space();

                        var rect = EditorGUILayout.BeginVertical();
                        GUI.Box(rect, GUIContent.none);

                        GUILayout.BeginHorizontal();

                        var condition = e.Conditions[i];
                        condition.Condition = (Condition)EditorGUILayout.EnumPopup(condition.Condition);

                        var old = GUI.backgroundColor;
                        GUI.backgroundColor = Color.red;
                        if (GUILayout.Button("X", GUILayout.Width(20)))
                            toDelete = i;
                        GUI.backgroundColor = old;

                        GUILayout.EndHorizontal();

                        EditorGUI.indentLevel++;
                        condition.Not = EditorGUILayout.Toggle("Not", condition.Not);
                        EditorGUI.indentLevel--;
                        EditorGUILayout.Space();

                        switch (e.Conditions[i].Condition)
                        {
                            case Condition.IsEnabled:
                                EditObject(ref condition.Target);
                                break;

                            case Condition.IsDisabled:
                                EditObject(ref condition.Target);
                                break;

                            case Condition.HasInventory:
                                EditObject(ref condition.Target);
                                EditItem(ref condition.Item);
                                break;

                            case Condition.IsObject:
                                condition.Target.Target = Target.Specific;
                                condition.Target.Object = (GameObject)EditorGUILayout.ObjectField("Object", condition.Target.Object, typeof(GameObject), true);
                                break;

                            case Condition.HasTag:
                                condition.Tag = EditorGUILayout.TagField("Tag", condition.Tag);
                                break;

                            case Condition.IsInCycleState:
                                EditObject(ref condition.Target);
                                condition.CycleState = (CycleTarget)EditorGUILayout.EnumPopup("State", condition.CycleState);
                                break;
                        }

                        e.Conditions[i] = condition;

                        EditorGUILayout.EndVertical();
                    }

                    if (toDelete >= 0)
                    {
                        var old = e.Conditions;
                        e.Conditions = new ConditionDesc[old.Length - 1];

                        for (int i = 0; i < toDelete; i++)
                            e.Conditions[i] = old[i];

                        for (int i = toDelete + 1; i < old.Length; i++)
                            e.Conditions[i - 1] = old[i];
                    }
                }

                EditorGUILayout.Space();

                if (GUILayout.Button("Add Condition"))
                {
                    if (e.Conditions == null)
                        e.Conditions = new ConditionDesc[1];
                    else
                    {
                        var old = e.Conditions;
                        e.Conditions = new ConditionDesc[old.Length + 1];

                        for (int i = 0; i < old.Length; i++)
                            e.Conditions[i] = old[i];
                    }

                    e.Conditions[e.Conditions.Length - 1].Item.Count = 1;
                }
            }

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            {
                if (e.Commands != null)
                {
                    int toDelete = -1;

                    EditorGUILayout.LabelField("Commands", EditorStyles.boldLabel);

                    for (int i = 0; i < e.Commands.Length; i++)
                    {
                        EditorGUILayout.Space();

                        var rect = EditorGUILayout.BeginVertical();
                        GUI.Box(rect, GUIContent.none);

                        GUILayout.BeginHorizontal();

                        var command = e.Commands[i];
                        command.Command = (Command)EditorGUILayout.EnumPopup(command.Command);

                        var old = GUI.backgroundColor;
                        GUI.backgroundColor = Color.red;
                        if (GUILayout.Button("X", GUILayout.Width(20)))
                            toDelete = i;
                        GUI.backgroundColor = old;

                        GUILayout.EndHorizontal();
                        EditorGUILayout.Space();

                        switch (e.Commands[i].Command)
                        {
                            case Command.Destroy:
                                EditObject(ref command.Object);
                                command.Float = EditorGUILayout.FloatField("Delay", command.Float);
                                break;

                            case Command.Enable:
                                EditObject(ref command.Object);
                                break;

                            case Command.Disable:
                                EditObject(ref command.Object);
                                break;

                            case Command.Sound:
                                command.Clip = (AudioClip)EditorGUILayout.ObjectField("Clip", command.Clip, typeof(AudioClip), false);
                                command.Bool = EditorGUILayout.Toggle("Play at listener", command.Bool);
                                break;

                            case Command.Attack:
                                EditObject(ref command.Object);
                                EditObject(ref command.Target, "Target");
                                break;

                            case Command.CalmDown:
                                EditObject(ref command.Object);
                                break;

                            case Command.Zoom:
                                if (!command.ZoomParametersInitialized)
                                {
                                    command.ZoomTarget = 1.0f;
                                    command.ZoomSpeed = 1.0f;
                                    command.ZoomParametersInitialized = true;
                                }

                                command.ZoomTarget = EditorGUILayout.FloatField("Target", command.ZoomTarget);
                                command.ZoomSpeed = EditorGUILayout.FloatField("Speed", command.ZoomSpeed);
                                break;

                            case Command.Cycle:
                                EditObject(ref command.Object);
                                command.CycleTarget = (CycleTarget)EditorGUILayout.EnumPopup("Target", command.CycleTarget);
                                break;

                            case Command.CycleTargetToggle:
                                EditObject(ref command.Object);
                                break;

                            case Command.AnimatorBool:
                                EditObject(ref command.Object);
                                command.Name = EditorGUILayout.TextField("Name", command.Name);
                                command.Bool = EditorGUILayout.Toggle("Value", command.Bool);
                                break;

                            case Command.AnimatorFloat:
                                EditObject(ref command.Object);
                                command.Name = EditorGUILayout.TextField("Name", command.Name);
                                command.Float = EditorGUILayout.FloatField("Value", command.Float);
                                break;

                            case Command.AnimatorInteger:
                                EditObject(ref command.Object);
                                command.Name = EditorGUILayout.TextField("Name", command.Name);
                                command.IntegerOrLayer = EditorGUILayout.IntField("Value", command.IntegerOrLayer);
                                break;

                            case Command.AnimatorState:
                                EditObject(ref command.Object);
                                command.Name = EditorGUILayout.TextField("Name", command.Name);
                                command.IntegerOrLayer = EditorGUILayout.IntField("Layer", command.IntegerOrLayer);
                                break;

                            case Command.AnimatorTrigger:
                                EditObject(ref command.Object);
                                command.Name = EditorGUILayout.TextField("Name", command.Name);
                                break;

                            case Command.AnimatorStart:
                                EditObject(ref command.Object);
                                break;

                            case Command.AnimatorStop:
                                EditObject(ref command.Object);
                                break;

                            case Command.AddInventory:
                            case Command.RemoveInventory:
                                EditObject(ref command.Object);
                                EditItem(ref command.Item);
                                break;

                            case Command.MoveTo:
                                EditObject(ref command.Object, "Object");
                                EditObject(ref command.Target, "Target");
                                command.Float = EditorGUILayout.FloatField("Speed", command.Float);
                                break;

                            case Command.Scale:
                                EditObject(ref command.Object, "Object");
                                command.Vector = EditorGUILayout.Vector3Field("Target", command.Vector);
                                command.Float = EditorGUILayout.FloatField("Speed", command.Float);
                                break;

                            case Command.Clone:
                                EditObject(ref command.Object, "Object");
                                break;

                            case Command.SetParent:
                                EditObject(ref command.Object, "Object");
                                EditObject(ref command.Target, "Parent");
                                break;

                            case Command.Health:
                                EditObject(ref command.Object, "Object");
                                command.Float = EditorGUILayout.FloatField("Health", command.Float);
                                break;

                            case Command.SpeedUp:
                                EditObject(ref command.Object, "Object");
                                command.Float = EditorGUILayout.FloatField("Multiplier", command.Float);
                                command.Duration = EditorGUILayout.FloatField("Duration", command.Duration);
                                break;

                            case Command.Wait:
                                command.Float = EditorGUILayout.FloatField("Duration", command.Float);
                                break;

                            case Command.Custom:
                                EditObject(ref command.Object);
                                command.Name = EditorGUILayout.TextField("Name", command.Name);
                                break;
                        }

                        e.Commands[i] = command;

                        EditorGUILayout.EndVertical();
                    }

                    if (toDelete >= 0)
                    {
                        var old = e.Commands;
                        e.Commands = new CommandDesc[old.Length - 1];

                        for (int i = 0; i < toDelete; i++)
                            e.Commands[i] = old[i];

                        for (int i = toDelete + 1; i < old.Length; i++)
                            e.Commands[i - 1] = old[i];
                    }
                }

                EditorGUILayout.Space();

                if (GUILayout.Button("Add Command"))
                {
                    if (e.Commands == null)
                        e.Commands = new CommandDesc[1];
                    else
                    {
                        var old = e.Commands;
                        e.Commands = new CommandDesc[old.Length + 1];

                        for (int i = 0; i < old.Length; i++)
                            e.Commands[i] = old[i];
                    }

                    e.Commands[e.Commands.Length - 1].Item.Count = 1;
                    e.Commands[e.Commands.Length - 1].Vector = Vector3.one;
                    e.Commands[e.Commands.Length - 1].Float = 1;
                    e.Commands[e.Commands.Length - 1].Duration = 1;
                }
            }
        }

        private static void EditObject(ref TargetDesc target, string name = "Object")
        {
            target.Target = (Target)EditorGUILayout.EnumPopup(name, target.Target);

            switch (target.Target)
            {
                case Target.Specific:
                    target.Object = (GameObject)EditorGUILayout.ObjectField(" ", target.Object, typeof(GameObject), true);
                    break;

                case Target.Tag:
                    target.Tag = EditorGUILayout.TagField(" ", target.Tag);
                    break;

                case Target.Find:
                    EditorGUI.indentLevel++;
                    target.FindTarget = (FindTarget)EditorGUILayout.EnumPopup("Inside", target.FindTarget);

                    EditorGUI.indentLevel++;

                    switch (target.FindTarget)
                    {
                        case FindTarget.Specific:
                            target.Object = (GameObject)EditorGUILayout.ObjectField(" ", target.Object, typeof(GameObject), true);
                            break;

                        case FindTarget.Tag:
                            target.Tag = EditorGUILayout.TagField(" ", target.Tag);
                            break;
                    }

                    EditorGUI.indentLevel--;

                    target.Name = EditorGUILayout.TextField("Name", target.Name);
                    EditorGUI.indentLevel--;
                    break;
            } 
        }

        private static void EditItem(ref InventoryItem item, string name = "Item")
        {
            item.Name = EditorGUILayout.TextField(name + " Name", item.Name);
            item.Count = EditorGUILayout.IntField(name + " Count", item.Count);
        }
    }
}
