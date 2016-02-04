using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class SolidTrigger : MonoBehaviour {

     //The list of colliders currently inside the trigger
     public List<GameObject> TriggerList;
 
     //called when something enters the trigger
     void  OnTriggerEnter(Collider other)
     {
         //if the object is not already in the list
         if(!TriggerList.Contains(other.gameObject))
         {
             //add the object to the list
             TriggerList.Add(other.gameObject);
         }
     }
 
     //called when something exits the trigger
     void OnTriggerExit(Collider other)
     {
         //if the object is in the list
         if (TriggerList.Contains(other.gameObject))
         {
             //remove it from the list
             TriggerList.Remove(other.gameObject);
         }
     }
    
    
    // Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
