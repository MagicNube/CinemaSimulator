using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuSceneManager : MonoBehaviour
{

    public Button defaultButton;

    void Start()
    {
        EventSystem.current.SetSelectedGameObject(defaultButton.gameObject);
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Javi");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }


    public void OnEnterName(string playerName)
    {
        Debug.Log("PlayerName: " + playerName);
    }

    public void OnSFXVolume(float volume)
    {
        Debug.Log("SFX Volume: " + volume);
    }

    public void OnMusicVolume(float volume)
    {
        Debug.Log("Music Volume: " + volume);
    }
}
