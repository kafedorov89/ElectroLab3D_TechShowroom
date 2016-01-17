using UnityEngine;
using System.Collections;

namespace ElectroLab3D{
public class AddLOD : MonoBehaviour {
    public float PercentForHide;
    
    public AddLOD(){
        PercentForHide = 0.02f;
    }
    
    public void Add(Transform objTrans){
        
        
        MeshFilter meshFilter = objTrans.gameObject.GetComponent<MeshFilter>();
        Renderer renderer = objTrans.gameObject.GetComponent<MeshRenderer>() as Renderer;
        LODGroup lodGroup = objTrans.gameObject.GetComponent<LODGroup>();
        if(meshFilter != null)
        {
            if(lodGroup == null)
            {
                objTrans.gameObject.AddComponent<LODGroup>();
                Debug.Log("LODGroup was created");
            }/*else
            {
                DestroyImmediate(lodGroup);
                objTrans.gameObject.AddComponent<LODGroup>();
                Debug.Log("LODGroup was created again");
            }*/
            
            lodGroup = objTrans.gameObject.GetComponent<LODGroup>();
            LOD[] lodarray = lodGroup.GetLODs();
            Debug.Log("lodarray size =" + lodarray.Length);
            lodarray[2].screenRelativeTransitionHeight = PercentForHide;
            lodarray[0].renderers = new Renderer[1];
            lodarray[1].renderers = new Renderer[1];
            lodarray[2].renderers = new Renderer[1];
            lodarray[0].renderers[0] = renderer;
            lodarray[1].renderers[0] = renderer;
            lodarray[2].renderers[0] = renderer;
            
            //lodGroup.s
            lodGroup.SetLODs(lodarray);
            lodGroup.RecalculateBounds();
            
        }
        else{
            if(lodGroup != null)
            {
                DestroyImmediate(lodGroup);
            }
        }
        
        foreach (Transform childTrans in objTrans)
        {
            Add(childTrans);
            Debug.Log ("Go to recursive");
        }
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
}
