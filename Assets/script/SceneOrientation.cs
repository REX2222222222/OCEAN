using UnityEngine;

public class SceneOrientation : MonoBehaviour
{
    public enum TargetOrientation
    {
        Portrait,
        Landscape
    }

    [Header("這個場景要用什麼方向？")]
    public TargetOrientation sceneOrientation;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ApplyLandscapeOrientation()
    {
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
        Screen.orientation = ScreenOrientation.AutoRotation;
    }

    void Start()
    {
        ApplyLandscapeOrientation();
    }
}
