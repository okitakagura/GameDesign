using System;
using UnityEngine;

namespace Platformer
{
    public enum Condition
    {
        IsEnabled,
        IsDisabled,
        IsObject,
        HasTag,
        HasInventory,
        IsInCycleState
    }

    [Serializable]
    public struct ConditionDesc
    {
        public Condition Condition;
        public TargetDesc Target;
        public string Tag;
        public InventoryItem Item;
        public CycleTarget CycleState;
        public bool Not;
    }

    public static class Validator
    {
        public static bool Validate(GameObject self, GameObject other, ConditionDesc[] conditions, bool any)
        {
            var anySuccess = conditions.Length == 0;

            foreach (var condition in conditions)
            {
                var result = false;
                var targets = condition.Target.Get(self, other, new GameObject[] { other });
                var hasAnyTrue = false;
                var hasSomeFalse = false;

                switch (condition.Condition)
                {
                    case Condition.IsEnabled:
                        foreach (var o in targets)
                            if (o.activeSelf != condition.Not)
                                hasAnyTrue = true;
                            else
                                hasSomeFalse = true;

                        result = hasAnyTrue && (anySuccess || !hasSomeFalse);
                        break;

                    case Condition.IsDisabled:
                        foreach (var o in targets)
                            if (o.activeSelf == condition.Not)
                                hasAnyTrue = true;
                            else
                                hasSomeFalse = true;

                        result = hasAnyTrue && (anySuccess || !hasSomeFalse);
                        break;

                    case Condition.HasInventory:
                        foreach (var o in targets)
                        {
                            var inventory = o.GetComponent<CharacterInventory>();

                            if (inventory != null)
                            {
                                if (inventory.Contains(condition.Item) == !condition.Not)
                                    hasAnyTrue = true;
                                else
                                    hasSomeFalse = true;
                            }
                            else if (condition.Not)
                                hasAnyTrue = true;
                            else
                                hasSomeFalse = true;

                            result = hasAnyTrue && (anySuccess || !hasSomeFalse);
                        }
                        break;

                    case Condition.IsObject: result = (other == condition.Target.Object) != condition.Not; break;
                    case Condition.HasTag: result = other.CompareTag(condition.Tag) != condition.Not; break;

                    case Condition.IsInCycleState:
                        foreach (var o in targets)
                        {
                            var cycle = o.GetComponent<Cycle>();

                            if (cycle != null)
                            {
                                if (cycle.Target == condition.CycleState)
                                    hasAnyTrue = true;
                                else
                                    hasSomeFalse = true;
                            }
                            else if (condition.Not)
                                hasAnyTrue = true;
                            else
                                hasSomeFalse = true;
                        }

                        result = hasAnyTrue && (anySuccess || !hasSomeFalse);
                        break;
                }

                if (result)
                    anySuccess = true;
                else if (!any)
                    return false;
            }

            return anySuccess;
        }
    }
}
