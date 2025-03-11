using System;
using UnityEngine;

public class CameraParameters
{
    public class Intrinsics
    {
        public float focalLength;
        public Vector2 sensorSize;
        public Vector2 lensShift;

        public Intrinsics(float i_FocalLength, Vector2 i_SensorSize, Vector2 i_LensShift)
        {
            focalLength = i_FocalLength;
            sensorSize = i_SensorSize;
            lensShift = i_LensShift;
        }
    }

    public class Extrinsics
    {
        public Vector3 position;
        public Vector3 rotation;

        public Extrinsics(Vector3 i_Position, Vector3 i_Rotation)
        {
            position = i_Position;
            rotation = i_Rotation;
        }
    }

    // Mirror Y-axis transformation
    private static readonly float[,] Sy = new float[,]
    {
        { 1,  0,  0 },
        { 0, -1,  0 },
        { 0,  0,  1 },
    };

    // Invert Y and Z axes transformation
    private static readonly float[,] YZ = new float[,]
    {
        { 1, 0, 0 },
        { 0, 0, 1 },
        { 0, 1, 0 },
    };


    public static Intrinsics ComputeIntrinsics(int i_ImageWidth, int i_ImageHeight,
            Vector2 i_FocalLengthPx, Vector2 i_PrincipalPoint, float i_SensorX = 35f)
    {
        float focalLength = i_FocalLengthPx.x * (i_SensorX / i_ImageWidth);
        Vector2 sensorSize = new Vector2(
            i_SensorX,
            focalLength * (i_ImageHeight / i_FocalLengthPx.y)
        );
        Vector2 lensShift = new Vector2(
            -(i_PrincipalPoint.x - (i_ImageWidth / 2)) / i_ImageWidth,
            (i_PrincipalPoint.y - (i_ImageHeight / 2)) / i_ImageHeight
        );

        return new Intrinsics(focalLength, sensorSize, lensShift);
    }

    public static Extrinsics ComputeExtrinsics(float[,] i_RotationMatrix, Vector3 i_Translation)
    {
        Vector3 unityPosition = MultiplyMatrixVector(YZ,
            -MultiplyMatrixVector(
                Transpose(MultiplyMatrices(Sy, i_RotationMatrix)),
                MultiplyMatrixVector(Sy, i_Translation)
            )
        );

        // Compute the rotation matrix in Unity's reference frame (camera -> world)
        float[,] unityRotation = MultiplyMatrices(YZ,
            Transpose(MultiplyMatrices(Sy, i_RotationMatrix))
        );

        // Convert to Unity Euler angles
        float[,] R_eun = new float[,]
        {
            { unityRotation[2, 2], unityRotation[2, 0], unityRotation[2, 1] },
            { unityRotation[0, 2], unityRotation[0, 0], unityRotation[0, 1] },
            { unityRotation[1, 2], unityRotation[1, 0], unityRotation[1, 1] },
        };
        Vector3 eulerAngles = Ieul(R_eun) * Mathf.Rad2Deg;
        Vector3 unityEulerRotation = new Vector3(eulerAngles.y, eulerAngles.z, eulerAngles.x);

        return new Extrinsics(unityPosition, unityEulerRotation);
    }


    // Transposes a 3x3 matrix
    private static float[,] Transpose(float[,] M)
    {
        return new float[,]
        {
            { M[0, 0], M[1, 0], M[2, 0] },
            { M[0, 1], M[1, 1], M[2, 1] },
            { M[0, 2], M[1, 2], M[2, 2] },
        };
    }

    // Multiplies two 3x3 matrices
    private static float[,] MultiplyMatrices(float[,] A, float[,] B)
    {
        float[,] result = new float[3, 3];
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                result[i, j] = A[i, 0] * B[0, j] + A[i, 1] * B[1, j] + A[i, 2] * B[2, j];
            }
        }
        return result;
    }

    // Multiplies a 3x3 matrix with a 3x1 vector
    private static Vector3 MultiplyMatrixVector(float[,] M, Vector3 v)
    {
        return new Vector3(
                M[0, 0] * v.x + M[0, 1] * v.y + M[0, 2] * v.z,
                M[1, 0] * v.x + M[1, 1] * v.y + M[1, 2] * v.z,
                M[2, 0] * v.x + M[2, 1] * v.y + M[2, 2] * v.z);
    }

    // Euler angles from a rotation matrix
    private static Vector3 Ieul(float[,] R)
    {
        return new Vector3(
                Mathf.Atan2(R[2, 1], R[2, 2]),
                Mathf.Atan2(-R[2, 0], Mathf.Sqrt(R[2, 1] * R[2, 1] + R[2, 2] * R[2, 2])),
                Mathf.Atan2(R[1, 0], R[0, 0])
        );
    }
}
