using UnityEngine;
using System.Collections;

[System.Serializable]
public class Subsystem {

	public string name;
	public GameObject gameObject;
	[TextArea(3,10)]
	public string textAbout;

	//TODO: сделать свойства, как полагается
	/*public string Name
	{
		get { return name; }
		set { name = value;}
	}*/

	public Subsystem()
	{
		name = "some subsystem";
		gameObject = null;
		textAbout = "some description";
	}
}
