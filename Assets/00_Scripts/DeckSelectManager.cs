using UnityEngine;
using UnityEngine.SceneManagement;

public class DeckSelectManager : MonoBehaviour
{
    public void MoveToScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
