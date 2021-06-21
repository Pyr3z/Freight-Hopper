using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class LevelController : MonoBehaviour
{
    public LevelName CurrentLevelName => levelName;
    [SerializeField, ReadOnly] private LevelName levelName;
    [SerializeField] private LevelData levelData;

    public static event Action PlayerRespawned;

    public static event Action LevelLoadedIn;

    public static LevelController Instance => instance;
    private static LevelController instance;

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        GizmosExtensions.DrawGizmosArrow(levelData.spawnPosition, Vector3.forward);
    }

#endif

    public string GetNextLevel()
    {
        if (levelData.nextLevelStatus == LevelData.NextLevelStatus.NextLevel)
        {
            return levelName.NextLevel();
        }
        if (levelData.nextLevelStatus == LevelData.NextLevelStatus.NextWorld)
        {
            return levelName.NextWorld();
        }
        return levelName.CurrentLevel();
    }

    private void Awake()
    {
        if (levelData != null)
        {
            Scene levelScene = SceneManager.GetActiveScene();

#if UNITY_EDITOR
            string playerSceneName = "DefaultScene";
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (!playerSceneName.Equals(SceneManager.GetSceneAt(i).name))
                {
                    levelScene = SceneManager.GetSceneAt(i);
                }
            }
#endif
            levelName = new LevelName(levelScene.name);
        }
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
        Player.PlayerLoadedIn += ResetPlayerPosition;
        Player.PlayerLoadedIn += UnlockAbilities;

        LevelLoadedIn?.Invoke();
    }

    private void OnDestroy()
    {
        Player.PlayerLoadedIn -= UnlockAbilities;
    }

    public void UnlockAbilities()
    {
        Player.Instance.GetComponent<PlayerAbilities>().SetActiveAbilities(levelData.activeAbilities);
    }

    private void ResetPlayerPosition()
    {
        GameObject player = Player.Instance.gameObject;
        if (player == null)
        {
            Debug.LogWarning("Can't find player");
        }
        player.transform.position = levelData.spawnPosition;
        player.GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

    public void Respawn()
    {
        LevelLoader.LoadLevel(levelName.CurrentLevel());
        PlayerRespawned?.Invoke();
        //player.transform.Rotate(Vector3.up, levelData.rotationAngle); Doesn't work due to camera
    }

    public void Respawn(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        Respawn();
    }
}