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
        return ReadFile(filename, ".json", foldername);
    }

    public static string ReadFile(string filename, string extension, string foldername)
    {
        string folderPath = "";
        string fileName = "";

        fileName = filename + extension;

        if (foldername != "")
        {
            folderPath = Application.dataPath + "/../" + foldername + "/";
        }
        else
        {
            folderPath = Application.dataPath + "/../";
        }

        if (File.Exists(folderPath + fileName))
        {
            return File.ReadAllText(folderPath + fileName);
        }
        else
        {
            return "";
        }

        
    }
}
