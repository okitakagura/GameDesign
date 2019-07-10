using System;
using System.Collections;
using UnityEngine;

namespace Platformer
{
    public enum Command
    {
        Destroy,
        Enable,
        Disable,
        Sound,
        Attack,
        CalmDown,
        Zoom,
        Cycle,
        CycleTargetToggle,
        AnimatorBool,
        AnimatorFloat,
        AnimatorInteger,
        AnimatorTrigger,
        AnimatorState,
        AnimatorStart,
        AnimatorStop,
        AddInventory,
        RemoveInventory,
        MoveTo,
        Scale,
        Clone,
        SetParent,
        Health,
        SpeedUp,
        Wait,
        Custom
    }

    [Serializable]
    public struct CommandDesc
    {
        public Command Command;
        public TargetDesc Object;
        public TargetDesc Target;
        public AudioClip Clip;
        public string Name;
        public float Float;
        public int IntegerOrLayer;
        public bool Bool;
        public Vector3 Vector;
        public CycleTarget CycleTarget;
        public float ZoomTarget;
        public float ZoomSpeed;
        public bool ZoomParametersInitialized;
        public InventoryItem Item;
        public float Duration;
    }

    public static class Executor
    {
        public static bool HasObjects(Command command)
        {
            switch (command)
            {
                case Command.Sound:
                case Command.Zoom:
                case Command.Wait:
                    return false;
            }

            return true;
        }

        public static bool HasTargets(Command command)
        {
            switch (command)
            {
                case Command.MoveTo:
                case Command.Attack:
                case Command.SetParent:
                    return true;
            }

            return false;
        }

        public static IEnumerator Run(GameObject self, GameObject other, CommandDesc[] commands)
        {
            var audioListenerPosition = Vector3.zero;

            if (AudioListener.FindObjectOfType<AudioListener>())
                audioListenerPosition = AudioListener.FindObjectOfType<AudioListener>().transform.position;

            var actors = new GameObject[] { other };

            foreach (var command in commands)
            {
                var objects = command.Object.Get(self, other, actors);
                var targets = command.Target.Get(self, other, actors);

                PlatformerCamera camera = null;

                if (Camera.main != null)
                    camera = Camera.main.GetComponent<PlatformerCamera>();

                switch (command.Command)
                {
                    case Command.Enable:
                        foreach (var o in objects)
                            o.SetActive(true);
                        break;

                    case Command.Disable:
                        foreach (var o in objects)
                            o.SetActive(false);
                        break;

                    case Command.Sound:
                        if (command.Clip != null)
                        {
                            if (command.Bool)
                                AudioSource.PlayClipAtPoint(command.Clip, audioListenerPosition);
                            else if (self != null)
                                AudioSource.PlayClipAtPoint(command.Clip, self.transform.position);
                        }
                        break;

                    case Command.Attack:
                        if (targets.Length > 0)
                            foreach (var o in objects)
                            {
                                var ai = o.GetComponent<AIController>();

                                if (ai != null)
                                    ai.Attack(targets[0]);
                        }
                        break;

                    case Command.CalmDown:
                        foreach (var o in objects)
                        {
                            var ai = o.GetComponent<AIController>();

                            if (ai != null)
                                ai.CalmDown();
                        }
                        break;

                    case Command.Zoom:
                        if (camera != null)
                            camera.ZoomTo(command.ZoomTarget, command.ZoomSpeed);
                        break;

                    case Command.Cycle:
                        foreach (var o in objects)
                        {
                            var cycle = o.GetComponent<Cycle>();

                            if (cycle != null)
                                cycle.Target = command.CycleTarget;
                        }
                        break;

                    case Command.CycleTargetToggle:
                        foreach (var o in objects)
                        {
                            var cycle = o.GetComponent<Cycle>();

                            if (cycle != null)
                            {
                                if (cycle.Target == CycleTarget.Home)
                                    cycle.Target = CycleTarget.Out;
                                else
                                    cycle.Target = CycleTarget.Home;
                            }
                        }
                        break;

                    case Command.AnimatorBool:
                        foreach (var o in objects)
                        {
                            var animator = o.GetComponent<Animator>();

                            if (animator != null)
                                animator.SetBool(command.Name, command.Bool);
                        }
                        break;

                    case Command.AnimatorFloat:
                        foreach (var o in objects)
                        {
                            var animator = o.GetComponent<Animator>();

                            if (animator != null)
                                animator.SetFloat(command.Name, command.Float);
                        }
                        break;

                    case Command.AnimatorInteger:
                        foreach (var o in objects)
                        {
                            var animator = o.GetComponent<Animator>();

                            if (animator != null)
                                animator.SetFloat(command.Name, command.IntegerOrLayer);
                        }
                        break;

                    case Command.AnimatorTrigger:
                        foreach (var o in objects)
                        {
                            var animator = o.GetComponent<Animator>();

                            if (animator != null)
                                animator.SetTrigger(command.Name);
                        }
                        break;

                    case Command.AnimatorState:
                        foreach (var o in objects)
                        {
                            var animator = o.GetComponent<Animator>();

                            if (animator != null)
                                animator.Play(command.Name, command.IntegerOrLayer, 0);
                        }
                        break;

                    case Command.AnimatorStart:
                        foreach (var o in objects)
                        {
                            var animator = o.GetComponent<Animator>();

                            if (animator != null)
                                animator.StartPlayback();
                        }
                        break;

                    case Command.AnimatorStop:
                        foreach (var o in objects)
                        {
                            var animator = o.GetComponent<Animator>();

                            if (animator != null)
                                animator.StopPlayback();
                        }
                        break;

                    case Command.AddInventory:
                        foreach (var o in objects)
                        {
                            var inventory = o.GetComponent<CharacterInventory>();

                            if (inventory != null)
                                inventory.Add(command.Item);
                        }
                        break;

                    case Command.RemoveInventory:
                        foreach (var o in objects)
                        {
                            var inventory = o.GetComponent<CharacterInventory>();

                            if (inventory != null)
                                inventory.Remove(command.Item);
                        }
                        break;

                    case Command.MoveTo:
                        if (targets.Length > 0)
                            foreach (var o in objects)
                            {
                                if (targets[0].GetComponent<RectTransform>() != null)
                                {
                                    var move = o.GetComponent<MoveTowardsUI>();
                                    if (move == null)
                                    {
                                        move = o.AddComponent<MoveTowardsUI>();
                                        move.Speed = command.Float;
                                    }

                                    move.enabled = true;

                                    move.SetTargetSpeed(command.Float);
                                    move.Target = targets[0];
                                }
                                else
                                {
                                    var move = o.GetComponent<MoveTowards>();
                                    if (move == null)
                                    {
                                        move = o.AddComponent<MoveTowards>();
                                        move.Speed = command.Float;
                                    }

                                    move.enabled = true;

                                    move.SetTargetSpeed(command.Float);
                                    move.Target = targets[0];
                                }
                            }
                        break;

                    case Command.Scale:
                        foreach (var o in objects)
                        {
                            var scale = o.GetComponent<Scale>();
                            if (scale == null)
                                scale = o.AddComponent<Scale>();
                            scale.enabled = true;

                            scale.Target = command.Vector;
                            scale.Speed = command.Float;
                        }
                        break;

                    case Command.Destroy:
                        foreach (var o in objects)
                            GameObject.Destroy(o, command.Float);
                        break;

                    case Command.Clone:
                        actors = new GameObject[objects.Length];

                        for (int i = 0; i < objects.Length; i++)
                        {
                            actors[i] = GameObject.Instantiate(objects[i]);
                            actors[i].transform.parent = objects[i].transform.parent;
                            actors[i].transform.position = objects[i].transform.position;
                            actors[i].transform.eulerAngles = objects[i].transform.eulerAngles;
                            actors[i].transform.localScale = objects[i].transform.localScale;
                        }

                        break;

                    case Command.SetParent:
                        foreach (var obj in objects)
                            obj.transform.parent = targets.Length == 0 ? null : targets[0].transform;

                        break;

                    case Command.Health:
                        foreach (var obj in objects)
                        {
                            var health = obj.GetComponent<CharacterHealth>();

                            if (health != null)
                                health.Deal(-command.Float);
                        }
                        break;

                    case Command.SpeedUp:
                        foreach (var obj in objects)
                        {
                            var motor = obj.GetComponent<CharacterMotor>();

                            if (motor != null)
                                motor.SpeedUp(command.Float, command.Duration);
                        }
                        break;

                    case Command.Custom:
                        foreach (var o in objects)
                            o.SendMessage(command.Name, SendMessageOptions.RequireReceiver);
                        break;

                    case Command.Wait:
                        yield return new WaitForSeconds(command.Float);
                        break;

                    default:
                        foreach (var o in objects)
                            o.SendMessage(command.Command.ToString(), SendMessageOptions.RequireReceiver);
                        break;
                }
            }
        }
    }
}