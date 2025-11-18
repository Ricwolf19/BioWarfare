using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadarScript : MonoBehaviour {
    [Header("Rotation Settings")]
    [SerializeField]
    private float speedRadar;
    [SerializeField]
    private float speedTurret;
    [SerializeField]
    private float speedCanon;

    [Header("GameObjects Settings")]
    [SerializeField]
    private GameObject radar;
    [SerializeField]
    private GameObject turret;
    [SerializeField]
    private GameObject canon;

    private Transform target;
    [SerializeField]
    private Transform debugTarget;

    private void Update()
    {
        //Camera.main.transform.LookAt(radar.transform);
        if (debugTarget)
        {
            DebugMode();
        }
        if(target == null)
        {
            Quaternion defaultrotationCanon = Quaternion.Euler(-10, 0, 0);
            Quaternion defaultrotation = Quaternion.Euler(0, 0, 0);
            radar.transform.localRotation = Quaternion.Slerp(radar.transform.localRotation, defaultrotation, Time.deltaTime * speedRadar);
            turret.transform.localRotation = Quaternion.Slerp(turret.transform.localRotation, defaultrotation, Time.deltaTime * speedTurret);
            canon.transform.localRotation = Quaternion.Slerp(canon.transform.localRotation, defaultrotationCanon, Time.deltaTime * speedCanon);
            return;
        }
       
        //ROtation needs to be in localSpace and must be clamped
        Vector3 lookPos = target.position - canon.transform.position;

        Quaternion rotationTurret = Quaternion.LookRotation(lookPos);

        radar.transform.rotation = Quaternion.Slerp(radar.transform.rotation, rotationTurret, Time.deltaTime*speedRadar);

        turret.transform.rotation = Quaternion.Slerp(turret.transform.rotation, rotationTurret, Time.deltaTime*speedTurret);

        canon.transform.rotation = Quaternion.Slerp(canon.transform.rotation, rotationTurret, Time.deltaTime * speedCanon);

    }
    private void DebugMode()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            SetTarget(debugTarget);
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            UnsetTarget();
        }
    }
    public void SetTarget(Transform targetTransform)
    {
        target = targetTransform;
    }
    public void UnsetTarget()
    {
        target = null;
    }
}
