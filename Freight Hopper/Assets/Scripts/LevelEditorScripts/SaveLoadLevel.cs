using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using HGSLevelEditor;
using System;


//[System.Serializable]
//// Notes for Nina: Seperate level into a seperate file called Level.cs -- will try and do after the basic prototype is finished 
//public class Level
//{
//    public string levelName;
//    public List<LevelObjectInfo> saveObjectInfoList = new List<LevelObjectInfo>();


//    public Level(List<LevelObjectInfo> objects, string name)
//    {
//        saveObjectInfoList = objects;
//        levelName = name;
//    }

//    public List<LevelObjectInfo> GetInfoList() {


//        return saveObjectInfoList;


//    }
//}


// !! !! !! !! !! !! !! !! !! !! !! !! !! !! !! !! !! !! !! 
//Most of this was referenced from Sharp Accent on YouTube -- 
//Video: https://youtu.be/B7fMrbnLJF0
// !! !! !! !! !! !! !! !! !! !! !! !! !! !! !! !! !! !! !! 

[System.Serializable]
public class SaveLoadLevel : MonoBehaviour
{
    //Variables 
    //This is for UI -- Need to be able to access file names for Loading Level Selection
    public List<String> allLevels = new List<string>();

    List<LevelObjectData> saveObjectInfoList = new List<LevelObjectData>();

    public static string saveFolderName = "Level";


    //Will save level + update Load Button UI 
    public void SaveLevelButton(string levelName) {

        SavingLevel(levelName);
        //Refreshes the grid to add new level button
        GridLoadLevelUI.GetInstance().ReloadLevels();
    }

    //Loads the level 
    public void LoadButton(string levelName) {

        LoadingLevel(levelName);
    }

    //Save level -- Saves all objects + their info/data into a file 
    void SavingLevel(string levelName) {
        
        LevelObjectInfo [] obj = FindObjectsOfType<LevelObjectInfo>();

        saveObjectInfoList.Clear();

        foreach (LevelObjectInfo levelObj in obj) {

            saveObjectInfoList.Add(levelObj.GetObject());
        
        }
    
        //Notes for Nina: Going to potentially change to this later 
        //currentLevel = new Level(saveObjectInfoList, levelName);

        SaveLevel levelSave = new SaveLevel();
        levelSave.SaveObject_List = saveObjectInfoList;

        string saveLocation = SavingLocation(levelName);

        //Saving Level
        IFormatter formatter = new BinaryFormatter();
        Stream stream = new FileStream(saveLocation, FileMode.Create, FileAccess.Write, FileShare.None);
        formatter.Serialize(stream, levelSave);
        stream.Close();

        Debug.Log(saveLocation + "works");
    }

    //Loads file + saves files info within 'SaveLevel save'
    bool LoadingLevel(string levelName) {

        bool retrieve = true;

        string fileName = SavingLocation(levelName);
        Debug.Log(fileName);

        //Looks for if file exists under the level name given 
        if (!File.Exists(fileName))
        {
            Debug.Log("Cannot find level!");
            retrieve = false;

        }
        else {

            IFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(fileName, FileMode.Open);

            //Notes for Nina: Need to change this to work with Level class 
            SaveLevel save = (SaveLevel)formatter.Deserialize(stream);
            stream.Close();
            LoadActualLevel(save);
        
        }

        return retrieve; 
    }

    //Assists in saving the file in the 'Streaming Assets' folder 
    static string SavingLocation(string levelName) {

        //Creating new location
        string savingLocation = Application.streamingAssetsPath + "/Levels/";

        if (!Directory.Exists(savingLocation))
        {
            Directory.CreateDirectory(savingLocation);
        }
      
        return savingLocation + levelName;
    }

    //Instantiates game objects + data saved in file 
    void LoadActualLevel(SaveLevel savedLevel) {

        LevelManager.GetInstance().ClearLevel();
    
        for (int i = 0; i < savedLevel.SaveObject_List.Count; i++) {
           
            LevelObjectData s_obj_data = savedLevel.SaveObject_List[i];

            Debug.Log("savedLevel Count: " + savedLevel.SaveObject_List.Count);
            Debug.Log("i: " + i); 

            Vector3 pos; 
            pos.x = s_obj_data.posX;
            pos.y = s_obj_data.posY;
            pos.z = s_obj_data.posZ;


            Debug.Log("POS: " + pos.x + pos.y + pos.z);
            Debug.Log("Rotation: " + s_obj_data.rotX + s_obj_data.rotY + s_obj_data.rotZ);
            

            GameObject load = Instantiate(HGSLevelEditor.ObjectManager.GetInstance().GetObject(s_obj_data.objectID).objPrefab, pos, 
                Quaternion.Euler(s_obj_data.rotX, s_obj_data.rotY, s_obj_data.rotZ));
        }
    }


    [Serializable]
    public class SaveLevel {

        [SerializeField]public List<LevelObjectData> SaveObject_List; 
    }

    //Gives allLevels all the file names within the 'Levels' folder 
    public void LoadAllLevels() { 
    
        DirectoryInfo dirInfo = new DirectoryInfo(Application.streamingAssetsPath + "/Levels");
        FileInfo[] fileInfo = dirInfo.GetFiles();

        foreach (FileInfo file in fileInfo) {

            allLevels.Add(file.Name);
            Debug.Log(file.Name);
        }
    }

    public static SaveLoadLevel instance;
    public static SaveLoadLevel GetInstance() {

        return instance;
    }

    void Awake()
    {
        instance = this;
        LoadAllLevels(); 
    }
}
