using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SplashSceneManager : MonoBehaviour
{
    public float waitTime = 10.0f;
    void Start()
    {
        StartCoroutine(WaitAndLoadNextLevel());
    }
    private void Update()
    {
        if (Input.anyKeyDown ||
        Input.GetMouseButtonDown(0) ||
        Input.GetMouseButtonDown(1) ||
        Input.GetMouseButtonDown(2))
        {
            LoadNextLevel();
        }
    }
    IEnumerator WaitAndLoadNextLevel()
    {
        yield return new WaitForSeconds(waitTime);
        LoadNextLevel();
    }
    private void LoadNextLevel()
    {
        SceneManager.LoadScene("02MainMenu");
    }
}
