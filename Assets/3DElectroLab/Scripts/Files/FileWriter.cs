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
			folderPath = Application.dataPath + "/../" + foldername + "/";
        }
        else
        {
            folderPath = Application.dataPath + "/../";
        }

        string fileName = filename + extension;

        if (!Directory.Exists(folderPath))
        {
            //Debug.Log("folder doesn't exist");
            Directory.CreateDirectory(folderPath);
            //Debug.Log("folder was created");
        }
        else
        {
            //Debug.Log("folder alredy exist");
        }

        if (File.Exists(folderPath + fileName))
        {
            //Debug.Log("File " + fileName + " already exists.");
            if (overwrite)
            {
                File.Delete(folderPath + fileName);
                //Debug.Log("File " + fileName + " was deleted");
                File.WriteAllText(folderPath + fileName, content);
            }
        }
        else
        {
            File.WriteAllText(folderPath + fileName, content);
        }
    }
}
