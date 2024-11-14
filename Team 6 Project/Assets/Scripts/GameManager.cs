// GameManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    // Level settings
    // Used for next level button. Takes the index of the current scene and compares it
    // to the total amount of levels we have. Level one has to be index one, level two
    // index two, etc. Update this value whenever a new level is added.
    public int totalLevelCount;
    private int currentLevel;
    //UI Stuff
    [SerializeField] GameObject menuActive, menuPause, menuWin, menuLose;
    [SerializeField] Button nextLevelButton;
    [SerializeField] TMP_Text enemyCountText, waveCountText, cropCountText;
    [SerializeField] GameObject effectBlind;
    [SerializeField] GameObject effectShield;
    public bool isPaused;
    int enemyCount;
    int cropCount;
    int waveCount;
    //Player Stuff
    public PlayerController playerScript;
    public GameObject player;
    public GameObject playerDamageScreen;
    public Image playerHPBar;
    float timeScaleOrig;

    public GameObject[] cropsArray;

    // Wave settings
    [SerializeField] List<GameObject> molePrefabs;
    [SerializeField] List<Transform> spawnPoints;
    [SerializeField] int numOfWaves = 1;
    [SerializeField] int enemiesPerWave = 5;
    [SerializeField] float timeBetweenSpawns = 1f, waveCooldown = 50F; 

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1;
        timeScaleOrig = Time.timeScale;

        currentLevel = SceneManager.GetActiveScene().buildIndex;

        cropsArray = GameObject.FindGameObjectsWithTag("Crop");
        //cropCount = cropsArray.Length;
        cropCountText.text = cropCount.ToString("F0");

        enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
        waveCount = 0;

        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<PlayerController>();

        StartNextWave();
    }

    public void UnregisterCrop(GameObject crop)
    {
        for (int i = 0; i < cropsArray.Length; i++)
        {
            if (cropsArray[i] == crop)
            {
                cropsArray[i] = null;
                break;
            }
        }
        UpdateCrop(-1);
    }

    public GameObject GetNearestCrop(Vector3 position)
    {
        GameObject nearestCrop = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject crop in cropsArray)
        {
            if (crop != null)
            {
                float distance = Vector3.Distance(position, crop.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestCrop = crop;
                }
            }
        }

        return nearestCrop;
    }

    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            if (menuActive == null)
            {
                StatePause();
                menuActive = menuPause;
                menuActive.SetActive(isPaused);
            }
            else if (menuActive == menuPause)
            {
                StateUnpause();
            }
        }
    }

    //Game States
    public void StatePause()
    {
        isPaused = true;
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }
    public void StateUnpause()
    {
        isPaused = false;
        Time.timeScale = timeScaleOrig;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        if (menuActive != null)
        {
            menuActive.SetActive(false);
            menuActive = null;
        }
    }
    public void YouLose()
    {
        StatePause();
        menuActive = menuLose;
        menuActive.SetActive(true);
    }
    public void GameGoal(int amount)
    {
        enemyCount += amount;
        enemyCountText.text = enemyCount.ToString("F0");

        if (enemyCount <= 0)
        {
            if (waveCount < numOfWaves)
            {
                // Start the next wave if there are waves left
                StartCoroutine(NextWaveWithCooldown());
            }
            else
            {
                // End game with win condition if all waves are completed
                StatePause();
                menuActive = menuWin;
                menuActive.SetActive(true);

                if (currentLevel + 1 > totalLevelCount)
                {
                    nextLevelButton.interactable = false;
                }
                else
                {
                    nextLevelButton.interactable = true;
                }
            }
        }
    }

    public void UpdateCrop(int amount)
    {
        cropCount += amount;

        cropCountText.text = cropCount.ToString("F0");

        if (cropCount <= 0)
        {
            YouLose();
        }
    }

    public IEnumerator ApplyBlindEffect()
    {
        if (effectBlind != null)
        {
            effectBlind.SetActive(true);
            yield return new WaitForSeconds(2f);
            effectBlind.SetActive(false);
        }
    }

    public IEnumerator ApplyShieldEffect(float duration)
    {
        if (effectShield != null)
        {
            effectShield.SetActive(true);
            yield return new WaitForSeconds(duration);
            effectShield.SetActive(false);
        }
    }


    public void StartNextWave()
    {
        if (waveCount < numOfWaves)
        {
            waveCount++;
            UpdateWaveCountUI();
            StartCoroutine(SpawnWave());
        }
        else
        {
            Debug.Log("All waves completed.");
        }
    }

    IEnumerator NextWaveWithCooldown()
    {
        yield return new WaitForSeconds(waveCooldown);  // Wait for cooldown before starting next wave
        StartNextWave();
    }

    IEnumerator SpawnWave()
    {
        for (int i = 0; i < enemiesPerWave; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(timeBetweenSpawns);
        }

        yield return new WaitForSeconds(waveCooldown);

        if (cropCount > 0)
        {
            StartNextWave();
        }
    }

    private void SpawnEnemy()
    {
        if (spawnPoints.Count > 0 && molePrefabs.Count > 0)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
            GameObject enemyPrefab = molePrefabs[Random.Range(0, molePrefabs.Count)];
            Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        }
    }

    void UpdateEnemyCountUI()
    {
        enemyCountText.text = enemyCount.ToString("F0");
    }

    void UpdateWaveCountUI()
    {
        waveCountText.text = waveCount + " / " + numOfWaves;
    }

    public int GetCurrentLevelIndex()
    {
        return currentLevel;
    }
}
