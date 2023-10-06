using UnityEditor;
using UnityEngine;

public class Turret : MonoBehaviour
{
    private Camera MainCamera => SceneView.lastActiveSceneView.camera;
    [SerializeField] private GameObject platform;
    [SerializeField] private GameObject trigger;
    [SerializeField] private GameObject turretHead;
    [SerializeField] private float angle = 30;
    [SerializeField] private float maxRadius = 3.0f;
    [SerializeField] private float minRadius = 1.0f;
    [SerializeField] private float height = 3.0f;
    [SerializeField] private float headRotationSpeed = 1.0f;
    [SerializeField] private float maxFireRange = 10.0f;
    private Vector3 downPosition => transform.position;
    private Vector3 upPosition => transform.position + transform.up * height;
    private Quaternion defaultOrientationOfHead => Quaternion.LookRotation(transform.forward, transform.up);
    [HideInInspector] public bool fire = false; 
    private void OnDrawGizmosSelected() {
        Physics.Raycast(MainCamera.transform.position, MainCamera.transform.forward, out RaycastHit hitInfo, 100f);

        Gizmos.DrawSphere(hitInfo.point, 0.1f);
        
        Gizmos.color = Color.green;
        Gizmos.DrawRay(hitInfo.point, hitInfo.normal);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(hitInfo.point, Vector3.Cross(MainCamera.transform.right, hitInfo.normal));
    }

    private void OnDrawGizmos() {
        var rightRay = transform.forward * Mathf.Sin((90 - angle / 2.0f) * Mathf.Deg2Rad ) + transform.right * Mathf.Cos((90 - angle / 2.0f) * Mathf.Deg2Rad );
        var leftRay = transform.forward * Mathf.Sin((90 - angle / 2.0f) * Mathf.Deg2Rad ) - transform.right * Mathf.Cos((90 - angle / 2.0f) * Mathf.Deg2Rad );

        var rightDown = downPosition + rightRay * maxRadius;
        var rightUp = upPosition + rightRay * maxRadius;
        var leftDown = downPosition + leftRay * maxRadius;
        var leftUp = upPosition + leftRay * maxRadius;

        bool isTargetInside = TriggerCheck(trigger.transform.position);

        Gizmos.color = isTargetInside ? Color.red : Color.green;
        Handles.color = isTargetInside ? Color.red : Color.green;

        Gizmos.DrawLine(downPosition + rightRay * minRadius, rightDown);
        Gizmos.DrawLine(downPosition + leftRay * minRadius, leftDown);

        Gizmos.DrawLine(upPosition + rightRay * minRadius, rightUp);
        Gizmos.DrawLine(upPosition + leftRay * minRadius, leftUp);

        Gizmos.DrawLine(rightDown, rightUp);
        Gizmos.DrawLine(leftDown, leftUp);

        Gizmos.DrawLine(downPosition + rightRay * minRadius, upPosition + rightRay * minRadius);
        Gizmos.DrawLine(downPosition + leftRay * minRadius, upPosition + leftRay * minRadius);

        Handles.DrawWireArc(upPosition, transform.up, leftRay, angle, maxRadius);
        Handles.DrawWireArc(downPosition, transform.up, leftRay, angle, maxRadius);

        Handles.DrawWireArc(upPosition, transform.up, leftRay * minRadius, angle, minRadius);
        Handles.DrawWireArc(downPosition, transform.up, leftRay * minRadius, angle, minRadius);

        // Turret Head Rotation Section (DO NOT FORGET "ALWAYS REFRESH" IN SCENE VIEW)------------------------------------
        var fromRotation = turretHead.transform.rotation;
        var toRotation = isTargetInside ? Quaternion.LookRotation(trigger.transform.position - turretHead.transform.position, transform.up) : defaultOrientationOfHead;
        var deltaRotation = Quaternion.Angle(fromRotation, toRotation);

        turretHead.transform.rotation = Quaternion.Slerp(fromRotation, toRotation, headRotationSpeed / deltaRotation);
        // Turret Head Rotation Section ----------------------------------------------------------------------------------

        Gizmos.color = Color.yellow;

        if(fire)
            MathUtils.BounceLaser(maxFireRange, turretHead.transform.position, turretHead.transform.forward);
    }

    public void PlaceTurret() {
        bool raycastSucceeded = Physics.Raycast(MainCamera.transform.position, MainCamera.transform.forward, out RaycastHit hitInfo, 100f);

        if(raycastSucceeded)
        {
            transform.position = hitInfo.point;
            transform.rotation = Quaternion.LookRotation(Vector3.Cross(MainCamera.transform.right, hitInfo.normal), hitInfo.normal);
        }
    }

    public void Fire() {
        fire = !fire;
    }

    public bool TriggerCheck(Vector3 triggerPosition) {
        var dirToTargetWorld = triggerPosition - downPosition;
        var dirToTargetLocal = transform.InverseTransformVector(dirToTargetWorld);

        // height check
        if(dirToTargetLocal.y < 0 || dirToTargetLocal.y > height)
            return false;

        // cylindirical radius check
        var ignoreYAxis = new Vector3(dirToTargetLocal.x, 0, dirToTargetLocal.z);

        if(ignoreYAxis.magnitude > maxRadius || ignoreYAxis.magnitude < minRadius)
            return false;

        // angle check
        if(Vector3.Angle(transform.TransformVector(ignoreYAxis.normalized), transform.forward) > angle / 2.0f)
            return false;

        return true;
    }
}
