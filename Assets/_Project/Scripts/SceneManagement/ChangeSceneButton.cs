using UnityEngine;

public class ChangeSceneButton : MonoBehaviour
{
    public void ChangeScene(int sceneIndex) {
        AsyncLevelLoader.Instance.LoadScene(sceneIndex);
    }
}