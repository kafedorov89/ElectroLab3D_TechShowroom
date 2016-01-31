using UnityEngine;
using System.Collections;
using System.IO;

public static class FileReader{

    public static string ReadJSONfromRoot(string filename)
    {
        return ReadFile(filename, ".json", "");
    }

	public static string ReadJSONfromFolder(string filename, string foldername)//, bool fullpath)
    {
		return ReadFile (filename, ".json", foldername);//, fullpath);
    }

	public static string ReadFile(string filename, string extension, string foldername)//, bool fullpath)
    {
        string folderPath = "";
        string fileName = "";

        fileName = filename + extension;

		//if (!fullpath) {
			if (foldername != "") {
				folderPath = Application.dataPath + "/../" + foldername + "/";
			} else {
				folderPath = Application.dataPath + "/../";
			}
		//} else
		//	folderPath = foldername;

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
