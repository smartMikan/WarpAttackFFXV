using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    public Vector3 centerOffset;
    public Vector2 uiOffset;
    WarpController warp;
    // Start is called before the first frame update
    void Awake()
    {
        warp = FindObjectOfType<WarpController>();
    }

    private void OnBecameVisible()
    {
        if (!warp.screenTargets.Contains(transform))
        {
            warp.screenTargets.Add(this.transform);
        }
    }

    private void OnBecameInvisible()
    {
        //Debug.Log("lose Target");
        if (warp.screenTargets.Contains(transform))
        {
            warp.screenTargets.Remove(transform);
        }
    }

    private void OnDestroy()
    {
        if (warp.screenTargets.Contains(transform))
        {
            warp.screenTargets.Remove(transform);
        }
    }
}
