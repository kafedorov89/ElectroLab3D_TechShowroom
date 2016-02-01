using UnityEngine;
using System.Collections;
using System.IO;

public static class FileReader{

    public static string ReadJSONfromRoot(string filename)
    {
        return ReadFile(filename, ".json", "");
    }

	public static string ReadJSONfromFolder(string filename, string foldername)
    {
		return ReadFile (filename, ".json", foldername);
    }

	public static string ReadFile(string filename, string extension, string foldername)
    {
        string folderPath = "";

		if (foldername.CompareTo("") != 0)
		{
			folderPath = Application.dataPath + "/" + foldername + "/";
		}
		else
		{
			folderPath = Application.dataPath + "/";
		}

		string fileName = filename + extension; //имя файла с расширением
		string fullPath = folderPath + fileName; //полный путь к файлу

		if (File.Exists(fullPath))
        {
			Debug.Log ("Read from file: " + fullPath);
			return File.ReadAllText(fullPath);
        }
        else
        {
            return "";
        }
    }
}
