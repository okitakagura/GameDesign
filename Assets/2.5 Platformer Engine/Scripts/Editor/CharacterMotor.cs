using UnityEditor;
using UnityEngine;

namespace Platformer
{
    [CustomEditor(typeof(CharacterMotor))]
    [CanEditMultipleObjects]
    public class CharacterMotorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Auto IK Setup"))
            {
                Undo.RecordObjects(targets, "IK Setup");

                foreach (var object_ in targets)
                {
                    var motor = (CharacterMotor)object_;
                    var animator = motor.GetComponent<Animator>();
                    var settings = motor.IK;

                    settings.LeftArmChain.Bones = new IKBone[3];
                    settings.LeftArmChain.Bones[0] = new IKBone(animator.GetBoneTransform(HumanBodyBones.LeftShoulder), 0.5f);
                    settings.LeftArmChain.Bones[1] = new IKBone(animator.GetBoneTransform(HumanBodyBones.LeftUpperArm), 0.5f);
                    settings.LeftArmChain.Bones[2] = new IKBone(animator.GetBoneTransform(HumanBodyBones.LeftLowerArm), 0.8f);

                    settings.RightArmChain.Bones = new IKBone[3];
                    settings.RightArmChain.Bones[0] = new IKBone(animator.GetBoneTransform(HumanBodyBones.RightShoulder), 0.5f);
                    settings.RightArmChain.Bones[1] = new IKBone(animator.GetBoneTransform(HumanBodyBones.RightUpperArm), 0.5f);
                    settings.RightArmChain.Bones[2] = new IKBone(animator.GetBoneTransform(HumanBodyBones.RightLowerArm), 0.8f);

                    settings.UpperBodyChain.Bones = new IKBone[2];
                    settings.UpperBodyChain.Bones[0] = new IKBone(animator.GetBoneTransform(HumanBodyBones.Hips), 0.5f);
                    settings.UpperBodyChain.Bones[1] = new IKBone(animator.GetBoneTransform(HumanBodyBones.Spine), 0.8f);

                    settings.LeftHand = animator.GetBoneTransform(HumanBodyBones.LeftIndexProximal);
                    settings.RightHand = animator.GetBoneTransform(HumanBodyBones.RightIndexProximal);
                    settings.Head = animator.GetBoneTransform(HumanBodyBones.Head);

                    motor.IK = settings;
                }
            }
        }
    }
}
