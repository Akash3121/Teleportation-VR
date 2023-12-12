using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserVisibility : MonoBehaviour
{
    public Material laserMaterial;
    public Material teleportationMaterial;
    private LineRenderer lineRenderer;
    public float thumbstickThreshold = 0.9f;

    public GameObject ovrRig;
    public GameObject centerOVRcam;
    private bool thumbstickPushed = false;
    private Vector3 teleportPosition = new Vector3(10.0f, 1.0f, 10.0f);
    private GameObject teleportObject = null;

    public GameObject arrowGO;

    private Vector3 rayDirection = Vector3.zero;

    private Quaternion ArrowRotation;
    private bool isArrowMoved = false;

    private Quaternion lastRotation = Quaternion.Euler(0,90,0);

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.material = laserMaterial;
        arrowGO.SetActive(false);
        // ArrowRotation = Quaternion.LookRotation(rayDirection);
    }

    // Update is called once per frame
    void Update()
    {

        float thumbstickInput = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick).y;

        float horizontalInput = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick).x;
        float verticalInput = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick).y;
        float LInputThreshold = 0.7f;

        if (Mathf.Abs(horizontalInput) > LInputThreshold || Mathf.Abs(verticalInput) > LInputThreshold)
        {
            isArrowMoved = true;
        }

        if (thumbstickInput > thumbstickThreshold && !thumbstickPushed)
        {
            thumbstickPushed = true;
            lineRenderer.enabled = true;
        }

        if (thumbstickInput < thumbstickThreshold && thumbstickPushed) // RThumbstick released
        {
            thumbstickPushed = false;
            lineRenderer.enabled = false;
            arrowGO.SetActive(false);
            isArrowMoved = false; // check this - do we need to add in the below if statement or here wold be fine

            if (teleportObject != null)
            {
                // offset from camera and OVRRig
                Vector3 offset = ovrRig.transform.position - centerOVRcam.transform.position;

                // remove y component for this
                offset.y = 0f;

                // teleport to teleportPosition
                // ovrRig.transform.position = teleportPosition;
                ovrRig.transform.position = teleportPosition + offset;


                ovrRig.transform.rotation = ArrowRotation;
            }

        }

        Vector3 rayEnd = transform.position + transform.forward * 10.0f;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit))
        {
            rayEnd = hit.point;

            if (hit.collider.CompareTag("teleportation"))
            {
                teleportObject = hit.collider.gameObject;
                teleportPosition = new Vector3(hit.point.x, hit.point.y + 1.0f, hit.point.z);
            }

            else
            {
                teleportObject = null;
            }
        }

        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, rayEnd);

        if (thumbstickPushed)// RThumbstick puched and holded
        {
            // change color if pointing at teleportation target
            if (teleportObject != null)
            {
                // Set material to teleport mat
                lineRenderer.material = teleportationMaterial;
                arrowGO.SetActive(true);
                arrowGO.transform.position = teleportPosition;
                // rayDirection = rayEnd - transform.position;
                rayDirection = transform.position - rayEnd;

                if (!isArrowMoved) //isArrowMoved = false
                {
                    ArrowRotation = Quaternion.LookRotation(rayDirection);
                }

                if (isArrowMoved && (Mathf.Abs(horizontalInput) > LInputThreshold || Mathf.Abs(verticalInput) > LInputThreshold)) //isArrowMoved = true
                {
                    Vector3 thumbstickDirection = new Vector3(horizontalInput, 0.0f, verticalInput);
                    ArrowRotation = Quaternion.LookRotation(thumbstickDirection.normalized, Vector3.up);
                }

                ArrowRotation.eulerAngles = new Vector3(0, ArrowRotation.eulerAngles.y, 0);

                // Store the last calculated rotation
                lastRotation = ArrowRotation;

                // Apply the rotation to the arrowGO
                arrowGO.transform.rotation = ArrowRotation;

            }
            else
            {
                // set material to default mat
                lineRenderer.material = laserMaterial;
                arrowGO.SetActive(false);
                isArrowMoved = false;
            }
        }
    }
}
