using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//public struct MeshPack
//{
//	public MeshRenderer[] meshes;
//}

public class VisTransparent : VisClass {

	public float alphaInitial = 1.0f; //when start 
	public float alphaMain = 0.5f; //when start animation
	public GameObject[] targets;
	public List<MeshPack> meshPacks;

	public override void StartVis()
	{
		base.StartVis();
		SetAlphaForTargets (alphaMain);
	}
	
	public override void StopVis()
	{
		//Debug.Log ("Stop");
		base.StopVis();
		SetAlphaForTargets (alphaInitial);
	}
	
	public override void Start()
	{
		//Debug.Log ("Start");
		base.Start();
		meshPacks = new List<MeshPack> (0);
		BuildMeshes ();
		SetAlphaForTargets (alphaInitial);
	}

	public void BuildMeshes()
	{
		meshPacks.Clear ();
		foreach (GameObject t in targets)
		{
			MeshPack pack;
			pack.meshes = t.GetComponentsInChildren<MeshRenderer>();
			meshPacks.Add(pack);
		}
	}

	public void SetAlphaForTargets(float a)
	{
		foreach (MeshPack pack in meshPacks)
			SetAlphaForMeshes (pack.meshes, a);
	}

	void PrepareForAlphaBlending(MeshRenderer[] meshes)
	{
		if (meshes == null)
			return;
		foreach (MeshRenderer rend in meshes)
		{
			foreach (Material material in rend.materials)
			{
				material.SetFloat ("_Mode", 2.0f); //fade or transparent
				material.SetInt ("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
				material.SetInt ("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				//material.SetInt ("_ZWrite", 0);
				material.DisableKeyword ("_ALPHATEST_ON");
				material.EnableKeyword ("_ALPHABLEND_ON");
				material.DisableKeyword ("_ALPHAPREMULTIPLY_ON");
				material.renderQueue = 3000;
			}
		}
	}

	public void SetAlphaForMeshes(MeshRenderer[] meshes, float a)
	{
		foreach (MeshRenderer rend in meshes)
		{
			foreach (Material material in rend.materials)
			{
				if (material.HasProperty("_node_op"))
					material.SetFloat("_node_op", a);
				SetColorMaterialAlpha(material, "_Color", a);
			}
		}
	}
	public void SetColorMaterialAlpha(Material material, string colorName, float a)
	{
		if (material.HasProperty (colorName))
		{
			Color clr = material.GetColor(colorName);
			material.SetColor (colorName, new Color (clr.r, clr.g, clr.b, a));
		}
	}
}
