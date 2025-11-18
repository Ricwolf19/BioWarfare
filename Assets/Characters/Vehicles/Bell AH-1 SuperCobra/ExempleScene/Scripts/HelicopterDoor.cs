using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelicopterDoor : MonoBehaviour {

    [Header("Animation Settings")]
    [SerializeField]
    private Animator anim;
    [SerializeField]
    private float speed = 5;
    [SerializeField]
    private bool pilotDoorOpen;
    [SerializeField]
    private bool gunnerDoorOpen;
    [Header("Others Settings")]
    [SerializeField]
    private bool debugMode;

    private bool pilotDoor;
    private float pilotDoorFloat;
    private bool gunnerDoor;
    private float gunnerDoorFloat;

    void Start()
    {
        if (pilotDoorOpen)
        {
            Debug.Log("PILOT");
            pilotDoorFloat = 1;
            anim.SetFloat("PilotDoorFloat", 0.9f);
            PilotDoor();
        }
        if (gunnerDoorOpen)
        {
         
            gunnerDoorFloat = 1;
            anim.SetFloat("GunnerDoorFloat", 0.9f);
            GunnerDoor();
        }
    }

    private void Update()
    {
        if (debugMode)
        {
            DebugAnimation();

        }

        OnPilotDoor();
        OnGunnerDoor();
    }

    private void OnPilotDoor()
    {
        if (pilotDoor)
        {
            pilotDoorFloat += speed * Time.deltaTime;
        }
        else
        {
            pilotDoorFloat -= speed * Time.deltaTime;
        }
        pilotDoorFloat = Mathf.Clamp(pilotDoorFloat,0,0.9f);
        anim.SetFloat("PilotDoorFloat", pilotDoorFloat);
    }
    private void OnGunnerDoor()
    {
        if (gunnerDoor)
        {
            gunnerDoorFloat += speed * Time.deltaTime;
        }
        else
        {
            gunnerDoorFloat -= speed * Time.deltaTime;
        }
        gunnerDoorFloat = Mathf.Clamp(gunnerDoorFloat,0,0.9f);
        anim.SetFloat("GunnerDoorFloat", gunnerDoorFloat);
    }

    private void DebugAnimation()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            PilotDoor();
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            GunnerDoor();
        }

    }

    public void PilotDoor()
    {
        if (!pilotDoor)
        {
            anim.SetBool("PilotDoor", true); 
            pilotDoor = true;
        }else
        {
            pilotDoor = false;
        }
       


    }
    public void GunnerDoor()
    {
        if (!gunnerDoor)
        {
            anim.SetBool("GunnerDoor", true);
            gunnerDoor = true;
        }
        else
        {
            gunnerDoor = false;
        }
    }
}
