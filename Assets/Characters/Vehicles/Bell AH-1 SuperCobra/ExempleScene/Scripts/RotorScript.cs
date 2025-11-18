using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RotorScript : MonoBehaviour {

    
    [Header("Rotation Settings")]
    [SerializeField]
    [Tooltip("Rotation axis of the rotor in local Space")]
    private RotationAxis rotationAxis;
    [SerializeField]
    [Tooltip("Rotation speed the rotor will rise")]
    private float rotorSpeed = 200;
    [SerializeField]
    [Tooltip("Rotation speed factor")]
    private float rotorSpeedRise = 5;
    private Vector3 rotationVector;

    [Header("Animation Settings")]
    [SerializeField]
    private Animator anim;

    [Tooltip("Invert the rotation")]
    [SerializeField]
    private bool isInvert = false;
    [SerializeField]
    private bool rotorStarted;
    [SerializeField]
    [Tooltip("Check this bos if the rotor is Rigged")]
    private bool hasBendingAnim;
    [SerializeField]
    [Range(0, 1)]
    [Tooltip("How much the rotor can bend")]
    float bendLimit = 0.5f;
    [SerializeField]
    [Tooltip("Check this box if you want the rotor to slightly bend")]
    private bool isWindAnim;
    [SerializeField]
    [Range(1, 10)]
    [Tooltip("Check isWindAnim's box to use this feature")]
    private float frequenceWind;
    [SerializeField]
    [Range(0, 1)]
    [Tooltip("Check isWindAnim's box to use this feature")]
    private float bendAmount;
    [SerializeField]
    private bool fakeMotion;
  

    [Header("GameObjects Settings")]
    [SerializeField]
    private GameObject rotorGameObject; //GFX Rotor

    [SerializeField]
    private GameObject motionRotorGameObject; //GFX en mouvement

    [SerializeField]
    private GameObject[] bladeGameObject;//GFX rotor Principal

    



    [Header("Others Settings")]
    [SerializeField]
    private bool debugMode = false;



    private enum RotationAxis { X, Y, Z };
    private float bendLimitSet;
    private float speedCurrent = 0;
    private int factInvert = 1;

    private delegate void RotorEvent();
    private event RotorEvent rotorEventHandler;


    private void Awake()
    {
        switch (rotationAxis)
        {
            case RotationAxis.X:
                rotationVector = Vector3.right;
                break;
            case RotationAxis.Y:
                rotationVector = Vector3.up;
                break;
            case RotationAxis.Z:
                rotationVector = Vector3.forward;
                break;
        }
        if (isInvert)
        {
            factInvert = -1;
        }
        bendLimitSet = bendLimit;
        if (rotorStarted)
        {
            speedCurrent = factInvert *rotorSpeed;
            StartRotor();
        }
    }


    void Update()
    {
        if (debugMode)
        {
            DebugAnimation();
        }
        
        if (rotorEventHandler != null)
        {
            rotorEventHandler();
        }else
        {
            RotorWind();
        }
        if (fakeMotion)
        {
            OnFakeMotion();
        }
        
    }
    private void DebugAnimation()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            StopRotor();
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            StartRotor();
        }
        if (Input.GetAxis("Vertical") != 0)
        {
            SetCollective(Input.GetAxis("Vertical"));
        }
        if (Input.GetAxis("Horizontal") != 0)
        {
            SetTail(Input.GetAxis("Horizontal"));
        }
    }
    private void RotorWind()
    {
        if (hasBendingAnim && isWindAnim)
        {
            float sin = Mathf.Clamp(((Mathf.Sin(Time.time*frequenceWind)+1)*bendAmount)/2,0, bendLimit);
            anim.SetFloat("BendFloat", sin);
           
        }
    }
    private void SetBend()
    {
        if (!hasBendingAnim)
            return;
        anim.SetFloat("BendFloat", Mathf.Clamp(factInvert*speedCurrent / rotorSpeed*1.1f,0,bendLimitSet));
    }
    public void SetCollective(float f)
    {
        anim.SetFloat("CollectiveFloat", Mathf.Clamp01(f));
        bendLimitSet = 1-(1-bendLimit - Mathf.Clamp01(f) * bendLimit);
    }
    public void SetTail(float f)
    {
        anim.SetFloat("TailFloat", Mathf.Clamp01(f));
    }
    #region RotorEvent

    private void OnRotorStart()
    {
        if(factInvert*speedCurrent < rotorSpeed)
        {
            speedCurrent += factInvert * rotorSpeedRise * Time.deltaTime;
        }
        SetBend();
        rotorGameObject.transform.Rotate(rotationVector*speedCurrent);
    }
    private void OnRotorStop()
    {
        if (factInvert*speedCurrent > 0)
        {
            speedCurrent += factInvert * -rotorSpeedRise * Time.deltaTime;
        }
        else
        {
            speedCurrent = 0;
            rotorEventHandler = null;
        }
        SetBend();
        rotorGameObject.transform.Rotate(rotationVector * speedCurrent);
    }
    private void OnFakeMotion()
    {
        if(factInvert * speedCurrent > rotorSpeed / 2)
        {
            motionRotorGameObject.SetActive(true);
            SetActiveArray(bladeGameObject, false);
        }
        else
        {
            motionRotorGameObject.SetActive(false);
            SetActiveArray(bladeGameObject,true);
        }
       
    }
    #endregion

    //Démarrer le rotor
    public void StartRotor()
    {
        rotorEventHandler = OnRotorStart;
    }
    public void StopRotor()
    {
        rotorEventHandler = OnRotorStop;
    }
    private void SetActiveArray(GameObject[] gos,bool value)
    {
        foreach (GameObject go in gos)
        {
            go.SetActive(value);
        }
    }
}
