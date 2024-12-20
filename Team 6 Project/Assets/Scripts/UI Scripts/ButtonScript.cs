using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonScript : MonoBehaviour
{
    GameObject[] quitButtons;
    GameObject mainMenuPlay;
    GameObject mainMenuSettings;
    Vector3 webGLPlayPos;

    private void Start()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            quitButtons = GameObject.FindGameObjectsWithTag("Quit Button");
            foreach (GameObject button in quitButtons)
            {
                button.SetActive(false);
            }
            mainMenuPlay = GameObject.FindGameObjectWithTag("MMPlayButton");
            mainMenuSettings = GameObject.FindGameObjectWithTag("MMSettingsButton");
            webGLPlayPos = mainMenuPlay.transform.position;
            webGLPlayPos.x = mainMenuSettings.transform.position.x;
            mainMenuPlay.transform.position = webGLPlayPos;
        }
    }
    private void Update()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            quitButtons = GameObject.FindGameObjectsWithTag("Quit Button");
            foreach (GameObject button in quitButtons)
            {
                button.SetActive(false);
            }
        }
    }
    public void Resume()
    {
        GameManager.Instance.StateUnpause();
    }
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        GameManager.Instance.StateUnpause();
    }
    public void MainMenu()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StateUnpause();
        }
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene("MainMenu");
    }
    public void Quit()
    {

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif

    }

    public void NextLevel()
    {
        int nextSceneIndex = GameManager.Instance.GetCurrentLevelIndex() + 1;
        if (nextSceneIndex > GameManager.Instance.totalLevelCount)
        {
            return;
        }

        SceneManager.LoadScene(nextSceneIndex);
        GameManager.Instance.StateUnpause();
    }

    public void Credits()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StateUnpause();
        }
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene("Credits");
    }

    public void DeleteSaveData()
    {
        PlayerPrefs.DeleteAll();
    }
}
