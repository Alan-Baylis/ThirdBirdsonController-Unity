using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class OddJob
{
    public struct ClipPlanePoints
    {
        public Vector3 UpperLeft;
        public Vector3 UpperRight;
        public Vector3 LowerLeft;
        public Vector3 LowerRight;
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        while(angle < -360 || angle > 360)
        {
            if(angle < -360)
            {
                angle += 360;
            }

            if(angle > 360)
            {
                angle -= 360;
            }
        }

        return Mathf.Clamp(angle, min, max);
    }

    public static ClipPlanePoints ClipPlaneAtNear(Vector3 position)
    {
        ClipPlanePoints clipPlanePoints = new ClipPlanePoints();

        // Do nothing if the camera does not exist. 
        if(Camera.main == null)
        {
            return clipPlanePoints;
        }

        Transform cameraTransform = Camera.main.transform;
        float halfFOV = (Camera.main.fieldOfView / 2) * Mathf.Deg2Rad;
        float aspect = Camera.main.aspect;
        float distance = Camera.main.nearClipPlane;
        float height = distance * Mathf.Tan(halfFOV);
        float width = height * aspect;

        clipPlanePoints.UpperLeft = position - cameraTransform.right * width;
        clipPlanePoints.UpperLeft += cameraTransform.up * height;
        clipPlanePoints.UpperLeft += cameraTransform.forward * distance;

        clipPlanePoints.UpperRight = position + cameraTransform.right * width;
        clipPlanePoints.UpperRight += cameraTransform.up * height;
        clipPlanePoints.UpperRight += cameraTransform.forward * distance;

        clipPlanePoints.LowerLeft = position - cameraTransform.right * width;
        clipPlanePoints.LowerLeft -= cameraTransform.up * height;
        clipPlanePoints.LowerLeft += cameraTransform.forward * distance;

        clipPlanePoints.LowerRight = position + cameraTransform.right * width;
        clipPlanePoints.LowerRight -= cameraTransform.up * height;
        clipPlanePoints.LowerRight += cameraTransform.forward * distance;

        return clipPlanePoints;
    }
}
