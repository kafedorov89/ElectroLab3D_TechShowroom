using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

public class SubsystemList : MonoBehaviour {

	public string systemName; //name of main system
	public string dataFileName; //name of file for disk input-output
	public List<Subsystem> list;

	public void Start()
	{


	}
	public void OnEnable()
	{

	}	

	public void LoadFromFile(string foldername)
	{
		int N = 0, M = 0;
		Debug.Log ("Load subsystems for " + systemName + " (" + gameObject.name + ")");
		List<string> errors = new List<string>();
		string json = FileReader.ReadJSONfromFolder (dataFileName, foldername);
		List<SubsystemIO> data = JsonConvert.DeserializeObject<List<SubsystemIO>> (json, 
			new JsonSerializerSettings
			{
				Error = delegate(object sender, ErrorEventArgs args)
				{
					string message = args.ErrorContext.Error.Message;
					errors.Add(message);
					args.ErrorContext.Handled = true;
					Debug.LogError(message);
				}
			});

		//данные мы получили, теперь нужно раскидать текст по подсистемам в соответствии с их
		//уникальными идентификаторами (UID)
		if (data != null) {
			N = list.Count;
			for (int i = 0; i < list.Count; ++i) {
				for (int j = 0; j < data.Count; ++j) {
					if (list [i].UID.CompareTo (data [j].UID) == 0) {
						//ключевой момент
						if (data [j].textAbout.Length > 0) {
							list [i].textAbout = data [j].textAbout;
							++M;
						}
					}
				}	
			}
		}
		Debug.Log (M + "/" +  N + " loaded");
	}

	public void WriteToFile (string foldername)
	{
		Debug.Log ("Write subsystems for " + systemName + " (" + gameObject.name + ")");
		List<SubsystemIO> subsIO = new List<SubsystemIO> ();
		foreach (Subsystem subsystem in list) {
			subsIO.Add (new SubsystemIO (subsystem));
		}
		string json = JsonConvert.SerializeObject (subsIO);
		FileWriter.OverwriteJSONtoFolder (dataFileName, foldername, json);
	}
}
