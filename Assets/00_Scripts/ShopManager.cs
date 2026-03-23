using UnityEngine;
using UnityEngine.SceneManagement;

public class ShopManager : MonoBehaviour
{


    public void MoveToScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
