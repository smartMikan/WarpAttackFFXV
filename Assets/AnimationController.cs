using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum AnimState
{
    Idle,
    Run
}


public class AnimationController : MonoBehaviour
{
    public static AnimationController Instance;

    Animator anim;
    public AnimState state;

    // Start is called before the first frame update
    void Start()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        Instance = this;
        anim = GetComponent<Animator>();
    }

    public void SetAnim(AnimState animState)
    {
        switch (state)
        {
            case AnimState.Idle:
                anim.SetTrigger("Idle");
                break;
            case AnimState.Run:
                anim.SetTrigger("Run");
                break;
            default:
                break;
        }
    }

    public Animator getAnim()
    {
        return anim;
    }
}
