using UnityEngine;
using System.Collections;

[AddComponentMenu("Camera-Control/Mouse Orbit with zoom")]
public class MouseOrbit: MonoBehaviour {
	
	//move camera, not object

	public Transform target;

	public float distance;
	public float Distance
	{
		get { return distance; }
		set 
		{ 
			distance = value;
			cameraDistance.transform.localPosition = new Vector3(0.0f, 0.0f, -distance);
		}
	}

	public float xSpeed = 120.0f;
	public float ySpeed = 120.0f;
	
	public float yMinLimit = -20f;
	public float yMaxLimit = 80f;
	
	public float distanceMin = 0.5f;
	public float distanceMax = 15f;
	
	//private Rigidbody rigidbody;
	
	float x = 0.0f;
	float y = 0.0f;

	float dx = 0.0f;
	float dy = 0.0f;

    public float step = 0.16f;
    public float smoothing = 10f;

    private Vector2 mouseOldPosition;
    private Vector2 mousePosition;
    private Vector2 mouseDeltaPosition;
    private bool MouseEvent;

    public GameObject cameraRotation;
    public GameObject cameraDistance;
    private bool PosInited;

    public float startDistance = 10.0f;
	public Vector3 startRotation = new Vector3 (0,0,0);

    void ParallelMoving()
    {
        if (Input.GetMouseButtonDown(2))
        {
            Debug.Log("Start Moving...");
            mousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            mouseOldPosition = mousePosition;
            mouseDeltaPosition = mousePosition - mouseOldPosition;
        }

        if (Input.GetMouseButtonUp(2))
        {
            Debug.Log("Stop Moving...");
            mousePosition = new Vector2();
            mouseOldPosition = new Vector2();
            mouseDeltaPosition = new Vector2();
        }

        if (Input.GetMouseButton(2))
        {
            Debug.Log("Moving...");

            if (mouseDeltaPosition.magnitude >= step)
            {
                // Smoothly interpolate between the camera's current 
                // position and it's target position.
				Vector3 local = transform.localPosition;
                transform.localPosition = Vector3.Lerp(local,
                                                  new Vector3(local.x - 0.005f * mouseDeltaPosition.x, 
				            						local.y - 0.005f * mouseDeltaPosition.y, local.z),
                                                  smoothing * Time.deltaTime);

                Debug.Log("Moving...");
                mouseDeltaPosition = new Vector2();
            }
            else
            {
                mousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                mouseDeltaPosition = mousePosition - mouseOldPosition;
            }
        }
    }

    // Use this for initialization
    void Start () 
	{
        PosInited = false;
        Vector3 angles = transform.eulerAngles;
		x = angles.y;
		y = angles.x;

        Distance = startDistance;
		//cameraRotation.transform.rotation = Quaternion.Euler (startRotation.x, startRotation.y, startRotation.z);
        //cameraDistance.transform.localPosition = new Vector3(0.0f, 0.0f, -distance);
    }
	
    void Update()
    {
        //ParallelMoving();
    }

	public void SetTarget(Transform t)
	{
		target = t;
		cameraRotation.transform.position = target.transform.position;
	}
	void LateUpdate () 
	{
        if (target)
        {
			cameraRotation.transform.position = target.transform.position;

			if (!PosInited)
            {
                PosInited = true;
                //Set camera view point to object's center
                //cameraRotation.transform.localPosition = target.transform.localPosition;
				cameraRotation.transform.position = target.transform.position;
            }

            //Если нажата ЛКМ - обрабатываем вращение объекта
            if (!Input.GetMouseButton(2))
            {
                //Vector3 negDistance = new Vector3();
                Quaternion rotation = new Quaternion();

                //Rotation
				if (Input.GetMouseButton (0)) {
                    
					x = cameraRotation.transform.localRotation.eulerAngles.y;
					y = cameraRotation.transform.localRotation.eulerAngles.x;

					dx = Input.GetAxis ("Mouse X") * xSpeed * 0.02f;
					dy = Input.GetAxis ("Mouse Y") * ySpeed * 0.02f;
					x += dx;
					y -= dy;
					Debug.Log ("Source: " + y);
					y = ClampAngle (y, yMinLimit, yMaxLimit);
					Debug.Log ("Clamp: " + y.ToString());
					rotation = Quaternion.Euler (y, x, 0);
					cameraRotation.transform.localRotation = rotation;

					/*x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
					y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
					y = ClampAngle(y, yMinLimit, yMaxLimit);
					rotation = Quaternion.Euler(y, x, 0);
					cameraRotation.transform.localRotation = rotation;*/

				}

                //Zoom
                if (Input.GetAxis("Mouse ScrollWheel") != 0)
                {
                    //MouseEvent = true;
                    distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * 3, distanceMin, distanceMax);
                    cameraDistance.transform.localPosition = new Vector3(0, 0, -distance);
                }
            }
            else
            {
                //Parallel
                ParallelMoving();
            }


		}
	}

	public static float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360F)
			angle += 360F;
		if (angle > 360F)
			angle -= 360F;
		return Mathf.Clamp(angle, min, max);
		/*if (angle < min)
			angle = min;
		if (angle > max)
			angle = max;
		return angle;*/
	}
	public void SetRotation(Vector3 rot)
	{
		cameraRotation.transform.localRotation = Quaternion.Euler (rot.x, rot.y, rot.z);
		//x = rot.y;
		//y = rot.z;
	}
}
