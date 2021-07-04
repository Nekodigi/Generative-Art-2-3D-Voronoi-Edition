using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    public string name;

    public void Load()
    {
        SceneManager.LoadScene(name, LoadSceneMode.Single);
    }
}
