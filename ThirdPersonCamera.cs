using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField]
    private Transform targetLookAt;
    [SerializeField]
    private Transform targetFollow;

    private ThirdPersonPigeonController pigeonController;
    private ThirdPersonPigeonMotor pigeonMotor;
    private ThirdPersonPigeonAnimator pigeonAnimator;
    
    // Location
    private Vector3 position;
    private Vector3 targetPosition;
    private float currentZoom = 0.5f;
    private float targetZoom = 0.5f;
    private float preOccludedZoom = 0.5f;

    // Min & Max
    private float minY = -35.0f;
    private float maxY = 80.0f;
    private float minZoom = 0.20f;
    private float maxZoom = 2.5f;
    private int maxOcclusionChecks = 10;

    // Sensitivity & Smoothing
    private const float HORIZSENSITIVITY = 5.0f;
    private const float VERTSENSITIVITY = 5.0f;
    private const float ZOOMSENSITIVITY = 1.5f;
    private const float XSMOOTHING = 0.05f;
    private const float YSMOOTHING = 0.05f;
    private const float ZOOMSMOOTHING = 0.25f;
    private const float ZOOMSMOOTHINGRESUME = 0.25f;
    private float currentZoomSmooth = 0.0f;
    private float occlusionDistanceStep = 0.10f;

    // Movement Velocity
    private float xVelocity = 0.0f;
    private float yVelocity = 0.0f;
    private float zVelocity = 0.0f;
    private float zoomVelocity = 0.0f;

    // Following
    private bool hasMoved = false;
    private int maxForwardChecks = 10;
    private int currentForwardChecks = 0;

    // Input
    private float mouseX = 0.0f;
    private float mouseY = 0.5f;

	private void Awake()
    {
        pigeonController = targetLookAt.parent.GetComponent<ThirdPersonPigeonController>();
        pigeonMotor = targetLookAt.parent.GetComponent<ThirdPersonPigeonMotor>();
        pigeonAnimator = targetLookAt.parent.GetComponent<ThirdPersonPigeonAnimator>();

        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
	}

    private void LateUpdate()
    {
        // Do nothing if the camera has no target. 
        if(targetLookAt == null)
        {
            return;
        }

        GetInput();

        
        
        int count = 0;
        do
        {
            CalculateTargetPosition();
            count++;
        }
        while(CheckIfOccluded(count));

        UpdatePosition();
        /*
        if(hasMoved)
        {
            currentForwardChecks = 0;
        }
        else
        {
            currentForwardChecks++;

            if(currentForwardChecks >= maxForwardChecks)
            {
                targetPosition = targetFollow.position;
                //transform.position = targetFollow.position;
                transform.LookAt(targetLookAt);
            }
        }
        */
    }

    private void GetInput()
    {
        hasMoved = false;
        float deadZone = 0.01f;

        // Only move camera X and Y if right clicking. 
        if(Input.GetMouseButton(1))
        {
            mouseX += Input.GetAxis("Mouse X") * HORIZSENSITIVITY;
            mouseY -= Input.GetAxis("Mouse Y") * VERTSENSITIVITY;

            hasMoved = true;
        }
        mouseY = OddJob.ClampAngle(mouseY, minY, maxY);

        float mouseScroll = Input.GetAxis("Mouse ScrollWheel");
        if(mouseScroll > deadZone || mouseScroll < -deadZone)
        {
            targetZoom = Mathf.Clamp((currentZoom - mouseScroll * ZOOMSENSITIVITY), minZoom, maxZoom);
            preOccludedZoom = targetZoom;
            currentZoomSmooth = ZOOMSMOOTHING;

            hasMoved = true;
        }
    }

    private void CalculateTargetPosition()
    {
        ResetTargetZoom();
        currentZoom = Mathf.SmoothDamp(currentZoom, targetZoom, ref zoomVelocity, currentZoomSmooth);

        targetPosition = CalculatePosition(mouseY, mouseX, currentZoom);
    }

    private Vector3 CalculatePosition(float xRot, float yRot, float currentZoom)
    {
        Vector3 direction = new Vector3(0, 0, -currentZoom);
        Quaternion rotation = Quaternion.Euler(xRot, yRot, 0);

        return targetLookAt.position + (rotation * direction);
    }

    private bool CheckIfOccluded(int count)
    {
        bool isOccluded = false;
        float nearDistance = CheckCameraPoints(targetLookAt.position, targetPosition);

        if(nearDistance != -1)
        {
            if(count < maxOcclusionChecks)
            {
                isOccluded = true;
                currentZoom -= occlusionDistanceStep;

                if(currentZoom < 0.05f)
                {
                    currentZoom = 0.05f;
                }
            }
            else
            {
                currentZoom = nearDistance - Camera.main.nearClipPlane;
            }

            targetZoom = currentZoom;
            currentZoomSmooth = ZOOMSMOOTHINGRESUME;
        }

        return isOccluded;
    }

    private float CheckCameraPoints(Vector3 from, Vector3 to)
    {
        float nearDistance = -1.0f;
        RaycastHit hit;
        OddJob.ClipPlanePoints clipPlanePoints = OddJob.ClipPlaneAtNear(to);

        // Draw debug info in editor. 
        Debug.DrawLine(from, to + transform.forward * -Camera.main.nearClipPlane, Color.red);
        Debug.DrawLine(from, clipPlanePoints.UpperLeft);
        Debug.DrawLine(from, clipPlanePoints.UpperRight);
        Debug.DrawLine(from, clipPlanePoints.LowerLeft);
        Debug.DrawLine(from, clipPlanePoints.LowerRight);
        Debug.DrawLine(clipPlanePoints.UpperLeft, clipPlanePoints.UpperRight);
        Debug.DrawLine(clipPlanePoints.LowerLeft, clipPlanePoints.LowerRight);
        Debug.DrawLine(clipPlanePoints.UpperLeft, clipPlanePoints.LowerLeft);
        Debug.DrawLine(clipPlanePoints.UpperRight, clipPlanePoints.LowerRight);

        if(Physics.Linecast(from, clipPlanePoints.UpperLeft, out hit) && hit.collider.tag != "Player")
        {
            nearDistance = hit.distance;
        }
        if(Physics.Linecast(from, clipPlanePoints.UpperRight, out hit) && hit.collider.tag != "Player")
        {
            if(hit.distance < nearDistance || nearDistance == -1)
            {
                nearDistance = hit.distance;
            }
        }
        if(Physics.Linecast(from, clipPlanePoints.LowerLeft, out hit) && hit.collider.tag != "Player")
        {
            if(hit.distance < nearDistance || nearDistance == -1)
            {
                nearDistance = hit.distance;
            }
        }
        if(Physics.Linecast(from, clipPlanePoints.LowerRight, out hit) && hit.collider.tag != "Player")
        {
            if(hit.distance < nearDistance || nearDistance == -1)
            {
                nearDistance = hit.distance;
            }
        }
        if(Physics.Linecast(from, to + transform.forward * -Camera.main.nearClipPlane, out hit) && hit.collider.tag != "Player")
        {
            if(hit.distance < nearDistance || nearDistance == -1)
            {
                nearDistance = hit.distance;
            }
        }

        return nearDistance;
    }

    private void ResetTargetZoom()
    {
        if(targetZoom < preOccludedZoom)
        {
            Vector3 pos = CalculatePosition(mouseY, mouseX, preOccludedZoom);
            float nearDistance = CheckCameraPoints(targetLookAt.position, pos);

            if(nearDistance == -1 || nearDistance > preOccludedZoom)
            {
                targetZoom = preOccludedZoom;
            }
        }
    }

    private void UpdatePosition()
    {
        float xPos = Mathf.SmoothDamp(position.x, targetPosition.x, ref xVelocity, XSMOOTHING);
        float yPos = Mathf.SmoothDamp(position.y, targetPosition.y, ref yVelocity, YSMOOTHING);
        float zPos = Mathf.SmoothDamp(position.z, targetPosition.z, ref zVelocity, XSMOOTHING);
        position = new Vector3(xPos, yPos, zPos);

        transform.position = position;
        transform.LookAt(targetLookAt);
    }

    public void MoveBehindTargetLookAt()
    {
        
    }


    #region Setters & Getters

    public void SetTargetLookAt(Transform newTargetLookAt)
    {
        targetLookAt = newTargetLookAt;
    }

    #endregion

}
