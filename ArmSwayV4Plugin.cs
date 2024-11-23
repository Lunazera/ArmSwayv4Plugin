using System;
using UnityEngine;
using VNyanInterface;
using ArmSwayV4;
using System.Collections.Generic;

namespace ArmSwayV4Plugin
{
    public class ArmSwayV4Plugin : MonoBehaviour
    {
        // VNyan-Unity UI Settings
        [Header("ArmSway Active")]
        private string parameterNameArmSwayActive = "ArmSwayActive";
        public float parameterArmSwayActive = 0f;

        [Header("Track Arms")]
        private string parameterNameArmSwayLeft = "LeftSwayActive";
        public float parameterArmSwayLeft = 0f;
        private string parameterNameArmSwayRight = "RightSwayActive";
        public float parameterArmSwayRight = 0f;
        private string parameterNameFingerLeft = "LeftFingerActive";
        public float parameterFingerLeft = 0f;
        private string parameterNameFingerRight = "RightFingerActive";
        public float parameterFingerRight = 0f;

        [Header("Track Legs")]
        private string parameterNameArmSwayLeftLeg = "LeftLegSwayActive";
        public float parameterArmSwayLeftLeg = 0f;
        private string parameterNameArmSwayRightLeg = "RightLegSwayActive";
        public float parameterArmSwayRightLeg = 0f;

        private string parameterNameLeftMotionDetect = "LeftMotionDetect";
        private float parameterLeftMotionDetect = 0f;
        private string parameterNameRightMotionDetect = "RightMotionDetect";
        private float parameterRightMotionDetect = 0f;

        private string parameterNameMirrorTracking = "MirrorTracking";
        private float parameterMirrorTracking = 0f;

        private GameObject avatar;
        private Animator animator;


        // Set up pose layer according to our class
        IPoseLayer ArmSwayV4 = new ArmSwayV4.ArmSwayv4();

        public void initBoneParameters()
        {
            // Initialize Bone Parameters
            foreach (int i in ArmSwayV4Settings.bonesToRead)
            {
                string boneName = Enum.GetNames(typeof(UnityEngine.HumanBodyBones))[i];

                // Apply rotation method from VNyan parameters and overwrite bone in rotation dictionary
                // we have an if-else case for slightly different bone name scheme
                if (boneName.Contains("Thumb"))
                {
                    VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(boneName + "X", 0);
                    VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(boneName + "Curl", 0);
                    VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(boneName + "Z", 0);
                }
                else if (boneName.Contains("Index") || boneName.Contains("Middle") || boneName.Contains("Ring") || boneName.Contains("Little"))
                {
                    VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(boneName + "X", 0);
                    VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(boneName + "Y", 0);
                    VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(boneName + "Curl", 0);
                }
                else
                {
                    VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(boneName + "X", 0);
                    VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(boneName + "Y", 0);
                    VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(boneName + "Z", 0);
                }
            }
        }

        public void setBonesFromVNyan(float mirrorSetting, float armswayFlag, float leftArmFlag, float leftFingerFlag, float rightArmFlag, float rightFingerFlag, float leftLegFlag, float rightLegFlag)
        {
            // Check VNyan Parameters to change settings of the graph
            // Then, trigger bones to be read
            ArmSwayV4Settings.setLayerOnOff(VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(parameterNameArmSwayActive));

            // Check for motion detected in right and left arm parameters, switching based on mirror setting
            if (mirrorSetting == 0f)
            {
                parameterLeftMotionDetect = VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(parameterNameLeftMotionDetect);
                parameterRightMotionDetect = VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(parameterNameRightMotionDetect);
            } else
            {
                parameterLeftMotionDetect = VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(parameterNameRightMotionDetect);
                parameterRightMotionDetect = VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(parameterNameLeftMotionDetect);
            }

            // Check left arm
            if (parameterLeftMotionDetect == 0f && leftArmFlag == 1f)
            {
                ArmSwayV4Settings.setLeftArm(VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(parameterNameArmSwayLeft));
            } 
            else
            {
                ArmSwayV4Settings.setLeftArm(0f);
            }

            // Check left finger
            if (parameterLeftMotionDetect == 0f && leftFingerFlag == 1f)
            {
                ArmSwayV4Settings.setLeftFingers(VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(parameterNameArmSwayLeft));
            }
            else
            {
                ArmSwayV4Settings.setLeftFingers(0f);
            }

            // Check right arm
            if (parameterRightMotionDetect == 0f && rightArmFlag == 1f)
            {
                ArmSwayV4Settings.setRightArm(VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(parameterNameArmSwayRight));
            } 
            else
            {
                ArmSwayV4Settings.setRightArm(0f);
            }

            // Check Right Fingers
            if (parameterRightMotionDetect == 0f && rightFingerFlag == 1f)
            {
                ArmSwayV4Settings.setRightFingers(VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(parameterNameArmSwayRight));
            }
            else
            {
                ArmSwayV4Settings.setRightFingers(0f);
            }

            // Check legs
            if (leftLegFlag == 1f)
            {
                ArmSwayV4Settings.setLeftLeg(VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(parameterNameArmSwayLeftLeg));
            }
            else
            {
                ArmSwayV4Settings.setLeftLeg(0f);
            }
            if (rightLegFlag == 1f)
            {
                ArmSwayV4Settings.setRightLeg(VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(parameterNameArmSwayRightLeg));
            }
            else
            {
                ArmSwayV4Settings.setRightLeg(0f);
            }

            // Set bones
            ArmSwayV4Settings.setBonesToRead();
        }

        public void Start()
        {
            // Register Pose Layer for VNyan to listen to
            VNyanInterface.VNyanInterface.VNyanAvatar.registerPoseLayer(ArmSwayV4);

            // Set base parameters to use.
            VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(parameterNameArmSwayActive, parameterArmSwayActive);

            VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(parameterNameArmSwayLeft, parameterArmSwayLeft);
            VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(parameterNameArmSwayRight, parameterArmSwayRight);

            VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(parameterNameArmSwayLeftLeg, parameterArmSwayLeftLeg);
            VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(parameterNameArmSwayRightLeg, parameterArmSwayRightLeg);

            VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(parameterNameFingerLeft, parameterFingerLeft);
            VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(parameterNameFingerRight, parameterFingerRight);

            VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(parameterNameLeftMotionDetect, parameterLeftMotionDetect);
            VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(parameterNameRightMotionDetect, parameterRightMotionDetect);
            VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(parameterNameMirrorTracking, parameterMirrorTracking);

            // Set Default pose
            VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterString("currentPose", "DefaultPose");

            // Initialize Bone Parameters
            initBoneParameters();


        }

        public void Update()
        {
            // Get ArmSway Flags and Parameters
            parameterMirrorTracking = VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(parameterNameMirrorTracking);

            VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(parameterNameArmSwayActive);

            VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(parameterNameArmSwayLeft);
            VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(parameterNameArmSwayRight);

            VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(parameterNameArmSwayLeftLeg);
            VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(parameterNameArmSwayRightLeg);

            VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(parameterNameFingerLeft);
            VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(parameterNameFingerRight);

            VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(parameterNameLeftMotionDetect);
            VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(parameterNameRightMotionDetect);
            VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(parameterNameMirrorTracking);

            // Get avatar object
            avatar = VNyanInterface.VNyanInterface.VNyanAvatar.getAvatarObject() as GameObject;
            animator = avatar.GetComponent<Animator>();

            // Set current chest bone transform
            var avatarChest = animator.GetBoneTransform(HumanBodyBones.Chest);

            // Manage Pendulum Chains
            ArmSwayV4Settings.setSwayChains(avatarChest.position.x, avatarChest.position.y, 50f);

            // Set bone parameters
            setBonesFromVNyan(parameterMirrorTracking, parameterArmSwayActive, parameterArmSwayLeft, parameterFingerLeft, parameterArmSwayRight, parameterFingerRight, parameterArmSwayLeftLeg, parameterArmSwayRightLeg);
        }
    }
}