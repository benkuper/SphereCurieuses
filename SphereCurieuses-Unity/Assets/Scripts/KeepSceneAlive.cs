using UnityEngine;
using System.Collections;


public class KeepSceneAlive : MonoBehaviour
{
    public bool KeepSceneViewActive;

    void Start()
    {
        if (this.KeepSceneViewActive && Application.isEditor)
        {
#if UNITY_EDITOR
            UnityEditor.SceneView.FocusWindowIfItsOpen(typeof(UnityEditor.SceneView));
#endif
        }
    }
}