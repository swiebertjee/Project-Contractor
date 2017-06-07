﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading;

public class AdditiveSceneManager : MonoBehaviour {

    // Use this for initialization
    [SerializeField]
    private GameObject[] _doors;
    [SerializeField]
    private string[] _sceneNames;
    [SerializeField]
    private int _buildIndex = 0;
    private List<string> names = new List<string>();
    private List<GameObject> _rootObjs = new List<GameObject>();
    private int _counter = 0;
    private int _numberOfObjects = 0;
    private bool _load = false;
    private bool _unload = false;
    GameObject[] gameObjs;


    void Start () {
		for(int i = 0; i < _doors.Length; i++)
        {
            names.Add(_doors[i].name);
        }
        for(int i = 0; i < _sceneNames.Length; i++)
        {
            LoadSceneToMain(_sceneNames[i]);
        }
        


    }

    //private void Cut()
    //{
    //    CutScene(gameObjs);
    //    for (int i = 0; i < 3; i++)
    //    {
    //        CutWhatWasCut(gameObjs);
    //    }
    //}
	
	// Update is called once per frame
	void Update () {

        if (_load){
            
            _numberOfObjects = gameObjs.Length;
            for (int i = 0; i < 3; i++)
            {
                gameObjs[_counter].SetActive(true);
                if (_counter >= _numberOfObjects - 1)
                {
                    _load = false;
                    _counter = 0;
                    break;
                }
                else
                {
                    _counter++;
                }

            }
        }
        if (_unload)
        {
            _numberOfObjects = gameObjs.Length;
            for(int i = 0; i < 4; i++)
            {
                gameObjs[_counter].SetActive(false);
                if (_counter >= _numberOfObjects - 1)
                {
                    _unload = false;
                    _counter = 0;
                    break;
                }
                else
                {
                    _counter++;
                }

            }
        }
    }

    public void LoadScene(int buildindex)
    {
        gameObjs = GetRootObjectsOfScene(_sceneNames[buildindex]);
        GameObject door = FindDoorWithName(name);
        if(door != null) { door.SetActive(false); }
        _load = true;
    }
    public void UnloadScene(int buildindex)
    {
        gameObjs = GetRootObjectsOfScene(_sceneNames[buildindex]);
        GameObject door = FindDoorWithName(name);
        if (door != null) { door.SetActive(true); }
        _unload = true;
    }
    
    

    private GameObject FindDoorWithName(string name)
    {

        for (int i = 0; i < _doors.Length; i++)
        {
            if (names.Contains(name))
            {
                return _doors[i];
            }
        }
        return null;
    }
    private GameObject[] GetRootObjectsOfScene(string name)
    {
        
        Scene scene = SceneManager.GetSceneByName(name);
        GameObject[] sceneObj = scene.GetRootGameObjects();
        return sceneObj;
    }

    public void LoadSceneToMain(string name)
    {
        SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);
    }
    public void RemoveSceneFromMain(string name)
    {
        GameObject door = FindDoorWithName(name);
        if (door != null) { door.SetActive(true); }
        Scene scene = SceneManager.GetSceneByName(name);
        GameObject sceneObj = scene.GetRootGameObjects()[0];
        _rootObjs.Remove(sceneObj);
        SceneManager.UnloadSceneAsync(name);
    }

    private void CutScene(GameObject[] objs)
    {
        
        foreach(GameObject obj in objs)
        {
            if (obj.GetComponent<CutScene>())
            {
                obj.GetComponent<CutScene>().CutSceneInTwo();
            }
        }
    }
    private void CutWhatWasCut(GameObject[] objs)
    {
        
        foreach (GameObject obj in objs)
        {
            if (obj.GetComponent<CutScene>())
            {
                obj.GetComponent<CutScene>().CutWhatWasCutInTwo();
            }
        }
    }













}
