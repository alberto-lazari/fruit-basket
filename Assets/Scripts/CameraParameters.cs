using System;
using UnityEngine;

public class CameraParameters
{
    public class Intrinsics
    {
        public float focalLength;
        public Vector2 sensorSize;
        public Vector2 lensShift;

        public Intrinsics(float focalLength, Vector2 sensorSize, Vector2 lensShift)
        {
            this.focalLength = focalLength;
            this.sensorSize = sensorSize;
            this.lensShift = lensShift;
        }
    }

    public class Extrinsics
    {
        public Vector3 position;
        public Vector3 rotation;

        public Extrinsics(Vector3 position, Vector3 rotation)
        {
            this.position = position;
            this.rotation = rotation;
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


    public static Intrinsics ComputeIntrinsics(int imageWidth, int imageHeight,
            Vector2 focalLengthPx, Vector2 principalPoint, float? sensorX)
    {
        float defaultSensorX = 35f;
        float focalLength = focalLengthPx.x * (sensorX ?? defaultSensorX / imageWidth);
        Vector2 sensorSize = new Vector2(
            sensorX ?? defaultSensorX,
            focalLength * (imageHeight / focalLengthPx.y)
        );
        Vector2 lensShift = new Vector2(
            -(principalPoint.x - (imageWidth / 2)) / imageWidth,
            (principalPoint.y - (imageHeight / 2)) / imageHeight
        );

        return new Intrinsics(focalLength, sensorSize, lensShift);
    }

    public static Extrinsics ComputeExtrinsics(float[,] rotationMatrix, Vector3 translation)
    {
        Vector3 unityPosition = MultiplyMatrixVector(YZ,
            -MultiplyMatrixVector(
                Transpose(MultiplyMatrices(Sy, rotationMatrix)),
                MultiplyMatrixVector(Sy, translation)
            )
        );

        // Compute the rotation matrix in Unity's reference frame (camera -> world)
        float[,] unityRotation = MultiplyMatrices(YZ,
            Transpose(MultiplyMatrices(Sy, rotationMatrix))
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
