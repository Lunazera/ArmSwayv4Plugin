using System;
using System.Collections.Generic;
using System.Text;
using VNyanInterface;
using VNyanExtra;
using UnityEngine;

// Plugin adaptation of Hebi's Arm Sway v4 graph.
// by lunazera
//
//
// I had adapted in a number of features into the graph version, including having everything driven by parameters.
// This plugin will just take the place of the graph, reading parameters and applying the rotations.

// TODO
// - Have transition when part is being turned on and off
// - Work alongside leap motion settings


namespace ArmSwayV4
{
    public class ArmSwayV4Settings
    {
        // This allows for some methods to be accessible by the unity loop, if we wanted to change settings by a UI or by parameter settings.

        // Layer On/Off Setting
        public static bool layerActive = true; // flag for layer being active or not (when inactive, vnyan will stop reading from the rotations entirely)
        public static void setLayerOnOff(float val) => layerActive = (val == 1f) ? true : false;
        
        // Pendulum Chains
        public static IPendulumRoot LZArmSwayChain = VNyanInterface.VNyanInterface.VNyanPendulum.createPendulumChain(4, 1f, .07f, 0f, 0f);
        public static IPendulumRoot LZWobbleChain = VNyanInterface.VNyanInterface.VNyanPendulum.createPendulumChain(6, 0.065f, 0.02f, 0.01f, 0.6f);

        // Bones List
        // logic: a bonesSettings dictionary will have list out the main body parts (L/R fingers, arms, and legs). each one will have a setting, 1 or 0. 
        // The dictionary will be read to populate a list of bones. this list of bones will be what is iterated through to apply the rotations.
        // to turn off body parts, you'll only need to update the dictionary.
        public static Dictionary<string, int> bonesSettings = new Dictionary<string, int>
        {
            { "leftarm", 0 },
            { "rightarm", 0 },
            { "leftfingers", 0 },
            { "rightfingers", 0 },
            { "leftleg", 0 },
            { "rightleg", 0 },
        };

        // Dictionary settings constructor methods
        // if these get set to 1, then it set's the bone to be read by loading "1" in that dictionary. otherwise it sets 0
        // these are all kept as floats to keep in line with VNyan Parameters.
        public static void setLeftArm(float val) => bonesSettings["leftarm"] = (val == 1f) ? 1 : 0;
        public static void setRightArm(float val) => bonesSettings["rightarm"] = (val == 1f) ? 1 : 0;
        public static void setLeftFingers(float val) => bonesSettings["leftfingers"] = (val == 1f) ? 1 : 0;
        public static void setRightFingers(float val) => bonesSettings["rightfingers"] = (val == 1f) ? 1 : 0;
        public static void setLeftLeg(float val) => bonesSettings["leftleg"] = (val == 1f) ? 1 : 0;
        public static void setRightLeg(float val) => bonesSettings["rightleg"] = (val == 1f) ? 1 : 0;


        public static List<int> bonesToRead = new List<int> { }; // List of bones to read, this is populated by setBonesToRead()
        public static void setBonesToRead()
        {
            bonesToRead = new List<int> { };

            // Goes through the dictionary, populates bonesToRead based on dictionary settings
            if (bonesSettings["leftarm"] == 1) { bonesToRead.AddRange(VNyanExtra.BoneLists.bonesLeftArm); }
            if (bonesSettings["rightarm"] == 1) { bonesToRead.AddRange(VNyanExtra.BoneLists.bonesRightArm); }
            if (bonesSettings["leftfingers"] == 1) { bonesToRead.AddRange(VNyanExtra.BoneLists.bonesLeftFingers); }
            if (bonesSettings["rightfingers"] == 1) { bonesToRead.AddRange(VNyanExtra.BoneLists.bonesRightFingers); }
            if (bonesSettings["leftleg"] == 1) { bonesToRead.AddRange(VNyanExtra.BoneLists.bonesLeftLeg); }
            if (bonesSettings["rightleg"] == 1) { bonesToRead.AddRange(VNyanExtra.BoneLists.bonesRightLeg); }
        }

        public static void setSwayChains(float chestX, float chestY, float multiplier)
        {
            LZArmSwayChain.setPositionValue(chestY*multiplier - chestX);
            LZWobbleChain.setPositionValue(-ArmSwayV4Settings.LZArmSwayChain.getChains()[3].getValue() * 2);
        }
    }
    public class ArmSwayv4 : IPoseLayer
    {
        // Set up our frame-by-frame information
        public PoseLayerFrame ArmSwayV4Frame = new PoseLayerFrame();

        // Create containers to load pose data each frame
        public Dictionary<int, VNyanQuaternion> BoneRotations;
        public Dictionary<int, VNyanVector3> BonePositions;
        public Dictionary<int, VNyanVector3> BoneScales;
        public VNyanVector3 RootPos;
        public VNyanQuaternion RootRot;

        // VNyan Get Methods, VNyan uses these to get the pose after doUpdate()
        VNyanVector3 IPoseLayer.getBonePosition(int i)
        {
            return BonePositions[i];
        }
        VNyanQuaternion IPoseLayer.getBoneRotation(int i)
        {
            return BoneRotations[i];
        }
        VNyanVector3 IPoseLayer.getBoneScaleMultiplier(int i)
        {
            return BoneScales[i];
        }
        VNyanVector3 IPoseLayer.getRootPosition()
        {
            return RootPos;
        }
        VNyanQuaternion IPoseLayer.getRootRotation()
        {
            return RootRot;
        }

        // Pose Toggle Method, can be used to activate
        bool IPoseLayer.isActive()
        {
            return ArmSwayV4Settings.layerActive;
        }

        // doUpdate is how we get all current bone values and where we lay out the calculation/work each frame
        public void doUpdate(in PoseLayerFrame ArmSwayV4Frame)
        {
            // Get all current Bone and Root values up to this point from our Layer Frame, and load them in our holdover values.
            BoneRotations = ArmSwayV4Frame.BoneRotation;
            BonePositions = ArmSwayV4Frame.BonePosition;
            BoneScales = ArmSwayV4Frame.BoneScaleMultiplier;
            RootPos = ArmSwayV4Frame.RootPosition;
            RootRot = ArmSwayV4Frame.RootRotation;

            // Get Pendulum Chain Values
            float SwayMultiplier_R = ArmSwayV4Settings.LZArmSwayChain.getChains()[3].getValue();
            float SwayMultiplier_L = -SwayMultiplier_R;

            float SwayFeedback = ArmSwayV4Settings.LZWobbleChain.getChains()[3].getValue() * 1;
            float SwayFeedback2 = ArmSwayV4Settings.LZWobbleChain.getChains()[4].getValue() * 3;
            float SwayFeedback3 = ArmSwayV4Settings.LZWobbleChain.getChains()[3].getValue() * 2;
            float SwayFeedback4 = ArmSwayV4Settings.LZWobbleChain.getChains()[5].getValue() * 3;

            // Sway Pause (can scale down and stop sway effect)
            //SwayMultiplier_R *= VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat("SwayPauseMult");
            //SwayMultiplier_L *= VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat("SwayPauseMult");

            // Set VNyan Sway Values
            VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat("SwayMultiplier_L", SwayMultiplier_L);
            VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat("SwayMultiplier_R", SwayMultiplier_R);

            VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat("SwayFeedback", SwayFeedback);
            VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat("SwayFeedback2", SwayFeedback2);
            VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat("SwayFeedback3", SwayFeedback3);
            VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat("SwayFeedback4", SwayFeedback4);

            // Calculate and set Sway Variables from different mults
            VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat("LeftShoulderSway", SwayMultiplier_L * VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat("LeftShoulderMult"));
            VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat("LeftUpSway", SwayMultiplier_L * VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat("LeftUpMult"));
            VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat("LeftDownSway", SwayMultiplier_L * VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat("LeftDownMult"));
            VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat("LeftHandSway", SwayMultiplier_L * VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat("LeftHandMult"));
            VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat("LeftFingerSway", SwayMultiplier_L * VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat("LeftFingerMult"));

            VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat("RightShoulderSway", SwayMultiplier_R * VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat("RightShoulderMult"));
            VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat("RightUpSway", SwayMultiplier_R * VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat("RightUpMult"));
            VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat("RightDownSway", SwayMultiplier_R * VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat("RightDownMult"));
            VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat("RightHandSway", SwayMultiplier_R * VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat("RightHandMult"));
            VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat("RightFingerSway", SwayMultiplier_R * VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat("RightFingerMult"));

            // Set ArmSway Values from Triggers
            VNyanInterface.VNyanInterface.VNyanTrigger.callTrigger("ArmSwayAddon", 0, 0, 0, "", "", "");
            VNyanInterface.VNyanInterface.VNyanTrigger.callTrigger("LeftAddon", 0, 0, 0, "", "", "");
            VNyanInterface.VNyanInterface.VNyanTrigger.callTrigger("RightAddon", 0, 0, 0, "", "", "");

            // Apply rotations
            foreach (int i in ArmSwayV4Settings.bonesToRead)
            {
                string boneName = Enum.GetNames(typeof(UnityEngine.HumanBodyBones))[i];

                // Apply rotation method from VNyan parameters and overwrite bone in rotation dictionary
                // we have an if-else case for slightly different bone name scheme
                if (boneName.Contains("Thumb"))
                {
                    BoneRotations[i] = VNyanExtra.QuaternionMethods.rotateByEulerUnity(BoneRotations[i],
                        VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(boneName + "X"),
                        VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(boneName + "Curl"),
                        VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(boneName + "Z"));
                }
                else if (boneName.Contains("Index") || boneName.Contains("Middle") || boneName.Contains("Ring") || boneName.Contains("Little"))
                {
                    BoneRotations[i] = VNyanExtra.QuaternionMethods.rotateByEulerUnity(BoneRotations[i],
                        VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(boneName + "X"),
                        VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(boneName + "Y"),
                        VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(boneName + "Curl"));
                }
                else
                {
                    BoneRotations[i] = VNyanExtra.QuaternionMethods.rotateByEulerUnity(BoneRotations[i],
                        VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(boneName + "X"),
                        VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(boneName + "Y"),
                        VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(boneName + "Z"));
                }
            }
        }
    }
}
