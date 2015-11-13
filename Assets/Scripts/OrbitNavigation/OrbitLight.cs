using UnityEngine;
using System.Collections;

public class OrbitLight : MonoBehaviour {

    public GameObject cameraObject;
    public MouseOrbit mouseOrbit;
    
    // Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = mouseOrbit.target.position;
        transform.rotation = cameraObject.transform.rotation;
    }
}
