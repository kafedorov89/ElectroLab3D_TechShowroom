using UnityEngine;
using System.Collections;
using System.IO;

public static class FileWriter{

    public static void OverwriteJSONtoRoot(string filename, string content)
    {
        WriteFile(filename, ".json", "", content, true);
    }

    public static void OverwriteJSONtoFolder(string filename, string foldername, string content)
    {
        WriteFile(filename, ".json", foldername, content, true);
    }

    public static void WriteFile(string filename, string extension, string foldername, string content, bool overwrite)
    {
        string folderPath = "";

        if (filename != "")
        {
			folderPath = Application.dataPath + "/" + foldername + "/";
        }
        else
        {
			folderPath = Application.dataPath + "/";
        }

        string fileName = filename + extension; //имя файла с расширением
		string fullPath = folderPath + fileName; //полный путь к файлу

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        if (File.Exists(fullPath))
        {
            if (overwrite)
            {
				File.Delete(fullPath);
				File.WriteAllText(fullPath, content);
				Debug.Log ("Write to file: " + fullPath);
            }
        }
        else
        {
            File.WriteAllText(folderPath + fileName, content);
			Debug.Log ("Write to file: " + fullPath);
        }
    }
}
