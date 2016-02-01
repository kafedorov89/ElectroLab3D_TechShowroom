using UnityEngine;
using System.Collections;

//класс для чтения и записи на диск описания к подсистемам
//(используется для того, чтобы сторонний человек мог редактировать 
//описание к подсистемам при помощи интерфейса данной программы)
[System.Serializable]
public class SubsystemIOText  {

	public string UID; //уникальный идентификатор подсистемы
	public string textAbout; //описание подсистемы

	public SubsystemIOText (string UID, string textAbout)
	{
		this.UID = UID;
		this.textAbout = textAbout;
	}
	public SubsystemIOText (Subsystem subsystem)
	{
		this.UID = subsystem.UID;
		this.textAbout = subsystem.textAbout;
	}
}
