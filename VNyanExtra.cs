using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using UnityEngine;
using VNyanInterface;

namespace VNyanExtra
{
    public class BoneLists
    {
        public static List<int> bonesLeftArm = new List<int> { 11, 13, 15, 17 };
        public static List<int> bonesRightArm = new List<int> { 12, 14, 16, 18 };

        public static List<int> bonesLeftFingers = new List<int> { 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38 };
        public static List<int> bonesRightFingers = new List<int> { 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53 };

        public static List<int> bonesLeftLeg = new List<int> { 1, 3, 5 };
        public static List<int> bonesRightLeg = new List<int> { 2, 4, 6 };
    }
    public class QuaternionMethods
    {
        public static VNyanQuaternion vnyanQuatProd(VNyanQuaternion q, VNyanQuaternion b)
        {
            VNyanQuaternion B = new VNyanQuaternion();
            B.W = q.W * b.W - q.X * b.X - q.Y * b.Y - q.Z * b.Z;
            B.X = q.W * b.X + q.X * b.W + q.Y * b.Z - q.Z * b.Y;
            B.Y = q.W * b.Y + q.Y * b.W + q.Z * b.X - q.X * b.Z;
            B.Z = q.W * b.Z + q.Z * b.W + q.X * b.Y - q.Y * b.X;

            return B;
        }

        public static VNyanQuaternion vnyanQuatSum(VNyanQuaternion q, VNyanQuaternion b)
        {
            VNyanQuaternion B = new VNyanQuaternion();
            B.W = q.W + b.W;
            B.X = q.X + b.X;
            B.Y = q.Y + b.Y;
            B.Z = q.Z + b.Z;

            return B;
        }

        public static VNyanQuaternion vnyanQuatInverse(VNyanQuaternion q)
        {
            VNyanQuaternion B = new VNyanQuaternion();
            float d = q.W * q.W + q.X * q.X + q.Y * q.Y + q.Z * q.Z;

            B.W = q.W / d;
            B.X = -q.X / d;
            B.Y = -q.Y / d;
            B.Z = -q.Z / d;

            return B;
        }

        public static double vnyanQuatNorm(VNyanQuaternion q)
        {
            return Math.Sqrt(q.W * q.W + q.X * q.X + q.Y * q.Y + q.Z * q.Z);
        }

        public static VNyanQuaternion setFromAxisAngle(double x, double y, double z, double angle)
        {
            // Another approach to create a Quaternion with a rotation along a single axis.
            // This one we set the "amount" for x/y/z and then just follow through

            VNyanQuaternion B = new VNyanQuaternion();
            double s = Math.Sin(angle / 2);

            B.X = (float)(x * s);
            B.Y = (float)(y * s);
            B.Z = (float)(z * s);
            B.W = (float)Math.Cos(angle / 2);

            return B;
        }

        public static VNyanQuaternion setFromEuler(float x, float y, float z)
        {
            // This is taken from https://en.wikipedia.org/wiki/Conversion_between_quaternions_and_Euler_angles
            // applying the 3-2-1 sequence for conversion
            VNyanQuaternion B = new VNyanQuaternion();


            // We take the Cosine and Sine of x, y, and z (corresponding with pitch, roll, and yaw)
            float cx = Mathf.Cos(x * Mathf.Deg2Rad / 2);
            float sx = Mathf.Sin(x * Mathf.Deg2Rad / 2);
            float cy = Mathf.Cos(y * Mathf.Deg2Rad / 2);
            float sy = Mathf.Sin(y * Mathf.Deg2Rad / 2);
            float cz = Mathf.Cos(z * Mathf.Deg2Rad / 2);
            float sz = Mathf.Sin(z * Mathf.Deg2Rad / 2);

            B.W = (cx * cy * cz + sx * sy * sz);
            B.X = (sx * cy * cz - cx * sy * sz);
            B.Y = (cx * sy * cz + sx * cy * sz);
            B.Z = (cx * cy * sz - sx * sy * cz);

            return B;
        }

        public static VNyanQuaternion rotateByEuler(VNyanQuaternion q, float x, float y, float z)
        {
            // first create our rotation quaternion. Takes in degrees and converts to radians.
            VNyanQuaternion p = setFromEuler(x, y, z);

            // Then we take the product to rotate
            VNyanQuaternion B = vnyanQuatProd(q, p);

            return B;

        }

        public static VNyanQuaternion rotateByEulerUnity(VNyanQuaternion q, float x, float y, float z)
        {
            // could ignore mixing for now and just replace.
            // So we do Quaternion.Euler(X,Y,Z)
            // and then transplant the quarternion into the VNyanQuarternion
            Quaternion p = Quaternion.Euler(x, y, z);

            Quaternion unityQ = new Quaternion(q.X, q.Y, q.Z, q.W);

            Quaternion rotatedQ = unityQ * p;

            q.X = rotatedQ.x;
            q.Y = rotatedQ.y;
            q.Z = rotatedQ.z;
            q.W = rotatedQ.w;

            return q;
        }


        public static Dictionary<int, VNyanQuaternion> loopThroughBones(Dictionary<int, VNyanQuaternion> BoneRotations)
        {
            Dictionary<int, VNyanQuaternion> newBoneRotations = BoneRotations;


            int i = 0;
            foreach (string boneName in Enum.GetNames(typeof(HumanBodyBones)))
            {
                i++;

                // Get the desired rotations from VNyanParameters
                float boneX = VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(boneName + "X");
                float boneY = VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(boneName + "Y");
                float boneZ = VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(boneName + "Z");

                // Apply rotation to this bone and save over the dictionary value
                newBoneRotations[i] = rotateByEulerUnity(BoneRotations[i], boneX, boneY, boneZ);
            }

            return newBoneRotations;
        }
    }
}
