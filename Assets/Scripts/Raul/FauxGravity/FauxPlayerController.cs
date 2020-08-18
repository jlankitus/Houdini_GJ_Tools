using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FauxPlayerController : MonoBehaviour
{

    public float moveSpeed = 15;
    public Transform mainCameraTransform;
    public Transform GFXTransform;

    private Vector3 moveDir;
    private Rigidbody rigidbody;

    // Local rotation related
    public float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
    }

    Vector3 a = Vector3.zero;
    Vector3 b = Vector3.zero;
    Vector3 c = Vector3.zero;
    private void FixedUpdate()
    {


        //Vector3 moveDirWithCam = new 
        /*
        Vector3 moveDirWithCamera = (mainCameraTransform.position - transform.position).normalized + moveDir;
        moveDirWithCamera = moveDirWithCamera.normalized;
        */
        //Vector3 moveDirWithCam = Vector3.RotateTowards(moveDir, Vector3.Cross(mainCameraTransform.localEulerAngles, Vector3.forward).normalized, 1, 20);
        //// Debug.Log(mainCameraTransform.eulerAngles.y);
        //Vector3 diff = mainCameraTransform.position - transform.position;
        //Debug.Log(diff.normalized);
        //moveDirWithCam = Quaternion.LookRotation(diff.normalized) * moveDir;

        // moveDirWithCam -=
        // Vector3 moveDirWithCam = 

        //a = transform.TransformDirection(newPosition);
        //b = transform.TransformDirection(moveDirWithCam);
        //c = transform.TransformDirection(newPosition);


        if (moveDir.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg + mainCameraTransform.localEulerAngles.y; //- transform.eulerAngles.y;

            Vector3 yRotation = Vector3.up * targetAngle * 50f * Time.fixedDeltaTime;
            Quaternion deltaRotation = Quaternion.Euler(yRotation);
            Quaternion targetRotation = rigidbody.rotation * deltaRotation;
            // Working as expected. Movement is off, though
            //GFXTransform.GetComponent<Rigidbody>().MoveRotation(Quaternion.Slerp(rigidbody.rotation, targetRotation, 50f * Time.deltaTime));
            GFXTransform.GetComponent<Rigidbody>().MoveRotation(Quaternion.Slerp(rigidbody.rotation, targetRotation, 50f * Time.deltaTime));

            Vector3 newPosition = rigidbody.position + GFXTransform.forward * moveSpeed * Time.deltaTime;
            rigidbody.MovePosition(newPosition);



            /*
            Vector3 bodyForward = GFXTransform.forward;
            Quaternion targetRotation = Quaternion.FromToRotation(bodyForward, moveDir);
            targetRotation *= GFXTransform.localRotation;
            GFXTransform.transform.localRotation = Quaternion.Slerp(GFXTransform.localRotation, targetRotation, 50 * Time.deltaTime);

            float targetAngle = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg + mainCameraTransform.localEulerAngles.y;
            float smoothedAngle = Mathf.SmoothDampAngle(GFXTransform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            GFXTransform.localEulerAngles = new Vector3(0, targetAngle, 0);
            */
            //float targetAngle = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg + mainCameraTransform.eulerAngles.y;
            //float smoothedAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            //// transform.RotateAroundLocal(Vector3.up, smoothedAngle);
            //transform.Rotate(Vector3.up, smoothedAngle, Space.Self);
        }
    }

    

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, a);

        if (b.magnitude != 0)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, b);
        }

        if (c.magnitude != 0)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, c);
        }
    }
}
