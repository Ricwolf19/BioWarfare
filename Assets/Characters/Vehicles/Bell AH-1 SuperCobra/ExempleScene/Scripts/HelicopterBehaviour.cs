using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class HelicopterBehaviour : MonoBehaviour {
    public static HelicopterBehaviour InstancePlayer;

    [HideInInspector]
    public Rigidbody rb;

    [HideInInspector]
    public float altitudeSet; //Altitude assignée


    [SerializeField]
    private Vector4 smooth; //smooth mouvement actif
    [SerializeField]
    private Vector4 smoothBack; //smooth mouvement inactif
    [SerializeField]
    private Vector4 _factor; //facteur de transformation du Rigidbody
    [SerializeField]
    private Vector4 _animFactor; //facteur d'intensité des animations du gameObject
    private Vector4 _movSmooth; // Calcule le smooth actuel

   
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate ()
    {

        Vector4 _mov;
        #region Torque
        _mov.w = Input.GetAxis("Horizontal");
        if (_mov.w == 0)
        {
            _movSmooth.w = Mathf.Lerp(_movSmooth.w, _mov.w, smoothBack.w * Time.fixedDeltaTime);
        }
        else
        {
            _movSmooth.w = Mathf.Lerp(_movSmooth.w, _mov.w, smooth.w * Time.fixedDeltaTime);
        }
        Vector3 _movTorque = new Vector3(0, _movSmooth.w, 0) * _factor.w;
        #endregion
        #region Collectif Up/Down
        _mov.z = 0;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            _mov.z = _factor.z;
        }else if(Input.GetKey(KeyCode.LeftControl))
        {
            _mov.z = -_factor.z;
        }
        if (_mov.z == 0)
        {
            _movSmooth.z = Mathf.Lerp(_movSmooth.z, _mov.z, smoothBack.z * Time.fixedDeltaTime);
        }
        else
        {
            _movSmooth.z = Mathf.Lerp(_movSmooth.z, _mov.z, smooth.z * Time.fixedDeltaTime);
        }

        Vector3 _movCollective = transform.up * _movSmooth.z * _factor.z;
       
        #endregion

        #region Cyclic Left/Right

        _mov.x = Input.GetAxis("Horizontal");

        if (_mov.x == 0)
        {
            _movSmooth.x = Mathf.Lerp(_movSmooth.x, _mov.x, smoothBack.x * Time.fixedDeltaTime);
        }
        else
        {
            _movSmooth.x = Mathf.Lerp(_movSmooth.x, _mov.x, smooth.x * Time.fixedDeltaTime);
        }

        //Vector3 actualRot = Camera.main.transform.localEulerAngles;
        //Camera.main.transform.localEulerAngles = new Vector3(actualRot.x, actualRot.y, -_movSmooth.x * _animFactor.x);
        Vector3 _movHorizontal = transform.right * _movSmooth.x * _factor.x;
        #endregion

        #region Cyclic Up/Down

        _mov.y = Input.GetAxis("Vertical");
        
        if (_mov.y == 0)
        {
            _movSmooth.y = Mathf.Lerp(_movSmooth.y, _mov.y, smoothBack.y * Time.fixedDeltaTime);
        }
        else
        {
            _movSmooth.y = Mathf.Lerp(_movSmooth.y, _mov.y, smooth.y * Time.fixedDeltaTime);
        }
        Vector3 _movVertical = transform.forward * _movSmooth.y * _factor.y;
        #endregion
       
        Vector3 _velocity = (_movHorizontal + _movVertical + _movCollective);

            Quaternion desiredRotation = Quaternion.Euler(new Vector3(_movSmooth.y * _animFactor.y, transform.localEulerAngles.y, -_movSmooth.x * _animFactor.x));
            Quaternion smoothedRotation = Quaternion.Lerp(transform.rotation, desiredRotation, 5 * Time.fixedDeltaTime);

            transform.rotation = smoothedRotation;
            rb.linearVelocity = (_velocity * Time.fixedDeltaTime);
            rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, 50);
            rb.angularVelocity = (transform.up * _movTorque.y) * Time.fixedDeltaTime;
        }

       
    


}