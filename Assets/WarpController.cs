using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using UnityEngine.Rendering.PostProcessing;
using DG.Tweening;
using System;
using EzySlice;

public class WarpController : MonoBehaviour
{

    PlayerController input;

    private Animator anim;

    public bool isLocked;

    public CinemachineFreeLook cameraFreelock;
    private CinemachineImpulseSource impulse;
    private PostProcessVolume postVolume;
    private PostProcessProfile postProfile;

    [Space]

    public List<Transform> screenTargets = new List<Transform>();
    public Transform target;
    public float warpDuration = 0.5f;

    [Space]

    public Transform sword;
    public Transform swordHand;
    private Vector3 swordOrigRot;
    private Vector3 swordOrigPos;
    private MeshRenderer swordMesh;

    [Space]
    public Material glowMaterial;

    [Space]

    [Header("Particles")]
    public ParticleSystem blueTrail;
    public ParticleSystem whiteTrail;
    public ParticleSystem swordParticle;

    [Space]
    [Header("Prefabs")]
    public GameObject hitParticle;

    [Space]
    [Header("Canvas")]
    public Image aim;
    public Image lockAim;
    public Vector2 uiOffset;


    [Space]
    [Header("Slice Layer")]
    public LayerMask layerMask;
    public Material sliceMaterial;
    Vector3 warpPos;
    public Transform cutplane;

    private void Start()
    {
        //if (screenTargets.Count == 0)
        //{
        //    Target[] targets = FindObjectsOfType<Target>();
        //    foreach(var t in targets)
        //    {
        //        screenTargets.Add(t.transform);
        //    }
        //}
        
        input = GetComponent<PlayerController>();
        Cursor.visible = false;
       
        anim = GetComponentInChildren<Animator>();

        impulse = cameraFreelock.GetComponent<CinemachineImpulseSource>();

        postVolume = Camera.main.GetComponent<PostProcessVolume>();
        postProfile = postVolume.profile;

        swordOrigPos = sword.localPosition;
        swordOrigRot = sword.localEulerAngles;
        swordMesh = sword.GetComponentInChildren<MeshRenderer>();
        swordMesh.enabled = false;

        cutplane.gameObject.SetActive(false);
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.visible = false;
            Application.Quit();
        }
        //anim.SetFloat("Blend", input.Speed);
        UserInterface();

        if (!input.canMove) return;
        if (screenTargets.Count < 1)
        {
            target = null;
            return;
        }

        
        if (!isLocked)
        {
            target = screenTargets[targetIndex()];
        }
        if (Input.GetMouseButtonDown(1))
        {
            if (target == null) return;
            LockInterFace(true);
            isLocked = true;
            //camreaFreelock.LookAt = target;
        }
        if (Input.GetMouseButtonUp(1) && input.canMove)
        {
            if (!isLocked) return;
            LockInterFace(false);
            isLocked = false;
            //camreaFreelock.LookAt = transform;
        }

        
        if (!isLocked)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (target == null) return;
            input.RotateTowards(target);
            input.canMove = false;
            swordParticle.Play();
            swordMesh.enabled = true;
            anim.SetTrigger("Slash");
        }

        
        
    }

    private void LockInterFace(bool state)
    {
        float size = state ? 1 : 2;
        float fade = state ? 1 : 0;
        lockAim.DOFade(fade, 0.15f);
        lockAim.transform.DOScale(size, 0.15f).SetEase(Ease.OutBack);
        lockAim.transform.DORotate(Vector3.forward * 180, 0.15f, RotateMode.FastBeyond360).From();
        aim.transform.DORotate(Vector3.forward * 90, 0.15f, RotateMode.LocalAxisAdd);
    }


    public void Warp()
    {
        GameObject clone = Instantiate(gameObject, transform.position, transform.rotation);
        Destroy(clone.GetComponent<WarpController>().sword.gameObject);
        Destroy(clone.GetComponentInChildren<Animator>());
        Destroy(clone.GetComponent<WarpController>());
        Destroy(clone.GetComponent<PlayerController>());
        Destroy(clone.GetComponent<CharacterController>());

        warpPos = transform.position;

        SkinnedMeshRenderer[] skinMeshList = clone.GetComponentsInChildren<SkinnedMeshRenderer>();

        foreach (SkinnedMeshRenderer mr in skinMeshList)
        {
            mr.material = glowMaterial;
            mr.material.DOFloat(1, "_AlphaThreshold",1f).OnComplete(() => Destroy(mr));
        }

        Destroy(clone, 1.5f);

        ShowBody(false);
        anim.speed = 0;



        sword.parent = null;
        sword.DOMove(target.position + (Vector3)target.GetComponentInChildren<Target>().uiOffset, warpDuration / 1.2f);
        sword.DOLookAt(target.position + (Vector3)target.GetComponentInChildren<Target>().uiOffset, 0.2f, AxisConstraint.None);
        transform.DOMove(new Vector3(target.position.x, target.position.y + target.GetComponentInChildren<Target>().centerOffset.y, target.position.z), warpDuration).SetEase(Ease.InExpo).OnComplete(() => FinishWarp());


        //sword.parent = null;
        //sword.DOMove(target.position, warpDuration / 1.2f);
        //sword.DOLookAt(target.position, 0.2f, AxisConstraint.None);

        //transform.DOMove(target.position, warpDuration).SetEase(Ease.InExpo).OnComplete(() => FinishWarp());



        blueTrail.Play();
        whiteTrail.Play();

        DOVirtual.Float(0, -80, 0.2f, DistortionAmount);
        DOVirtual.Float(1, 2f, .2f, ScaleAmount);
    }

    private void FinishWarp()
    {
        ShowBody(true);

        cutplane.gameObject.SetActive(true);
        cutplane.position = target.position;
        cutplane.transform.right = warpPos - transform.position;
        Slice(cutplane);
        cutplane.gameObject.SetActive(false);

        sword.parent = swordHand;
        sword.localPosition = swordOrigPos;
        Debug.Log(swordOrigPos + " " + sword.localPosition);
        sword.localEulerAngles = swordOrigRot;
        Debug.Log(swordOrigRot + " " + sword.localEulerAngles);

        SkinnedMeshRenderer[] skinMeshList = GetComponentsInChildren<SkinnedMeshRenderer>();

        foreach (SkinnedMeshRenderer mr in skinMeshList)
        {
            GlowAmount(30);
            DOVirtual.Float(30, 0, 0.5f, GlowAmount);
        }

        Instantiate(hitParticle, sword.position, Quaternion.identity);

        //target.GetComponentInParent<Animator>().SetTrigger("hit");

        StartCoroutine(HideSword());
        StartCoroutine(PlayerAnimation());
        StartCoroutine(StopParticles());

        isLocked = false;
        LockInterFace(false);
        aim.color = Color.clear;

        impulse.GenerateImpulse(Vector3.right * 2);


        DOVirtual.Float(-80, 0, 0.2f, DistortionAmount);
        DOVirtual.Float(2f, 1, 0.1f, ScaleAmount);

    }

    IEnumerator PlayerAnimation()
    {
        yield return new WaitForSeconds(0.2f);
        anim.speed = 1;
    }

    IEnumerator StopParticles()
    {
        yield return new WaitForSeconds(0.2f);
        blueTrail.Stop();
        whiteTrail.Stop();

    }

    IEnumerator HideSword()
    {
        yield return new WaitForSeconds(0.8f);
        swordParticle.Play();

        GameObject swordClone = Instantiate(sword.gameObject, sword.position, sword.rotation);

        swordMesh.enabled = false;

        MeshRenderer swordMR = swordClone.GetComponentInChildren<MeshRenderer>();
        Material[] materials = swordMR.materials;

        for(int i = 0; i< materials.Length; i++)
        {
            Material m = glowMaterial;
            materials[i] = m;
        }

        swordMR.materials = materials;

        

        for (int i = 0; i < swordMR.materials.Length; i++)
        {
            swordMR.materials[i].DOFloat(1, "_AlphaThreshold", 0.3f).OnComplete(() => Destroy(swordClone));
        }

       input.canMove = true;
    }

    void ShowBody(bool state)
    {
        SkinnedMeshRenderer[] skinMeshList = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer mr in skinMeshList)
        {
            mr.enabled = state;
        }
    }

    void GlowAmount(float x)
    {
        SkinnedMeshRenderer[] skinMeshList = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer mr in skinMeshList)
        {
            mr.material.SetVector("_FresnelAmount", new Vector4(x, x, x, x));
        }
    }

    void DistortionAmount(float x)
    {
        postProfile.GetSetting<LensDistortion>().scale.value = x;
    }
    void ScaleAmount(float x)
    {
        postProfile.GetSetting<LensDistortion>().scale.value = x;
    }





    private int targetIndex()
    {
        float[] distances = new float[screenTargets.Count];

        for (int i = 0; i < screenTargets.Count; i++)
        {
            distances[i] = Vector2.Distance(Camera.main.WorldToScreenPoint(screenTargets[i].position), new Vector2(Screen.width / 2, Screen.height / 2));
        }

        float minDistance = Mathf.Min(distances);
        int index = 0;
        for (int i = 0; i < distances.Length; i++)
        {
            if (minDistance == distances[i]) index = i;
        }
        return index;
    }

    private void UserInterface()
    {
        if (target != null)
        {
            aim.transform.position = Camera.main.WorldToScreenPoint(target.position + (Vector3)target.GetComponentInChildren<Target>().uiOffset);

            if (input.canMove)
                return;
        }
       
        Color c = screenTargets.Count < 1 ? Color.clear : Color.white;
        aim.color = c;
    }


    void Slice(Transform tr)
    {
        Collider[] hits = Physics.OverlapBox(tr.position, new Vector3(1f, 0.1f, 3f), tr.rotation, layerMask);
        if (hits.Length <= 0)
        {
            return;
        }

        for (int i = 0; i < hits.Length; i++)
        {
            SlicedHull hull = SliceObject(hits[i].gameObject, tr, sliceMaterial);
            if (hull != null)
            {
                GameObject lower = hull.CreateLowerHull(hits[i].gameObject, sliceMaterial);
                GameObject upper = hull.CreateUpperHull(hits[i].gameObject, sliceMaterial);
                AddHullCompents(lower);
                AddHullCompents(upper);
                Destroy(hits[i].gameObject);
            }
        }
    }

    public void AddHullCompents(GameObject go)
    {
        go.layer = 10;
        Rigidbody rb = go.AddComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        MeshCollider collider = go.AddComponent<MeshCollider>();
        collider.convex = true;

        rb.AddExplosionForce(100, go.transform.position, 20);
        //rb.AddForce(go.transform.position - warpPos);
    }

    public SlicedHull SliceObject(GameObject go, Transform tr, Material SliceMaterial = null)
    {
        if(go.GetComponent<MeshFilter>() == null)
        {
            return null;
        }
        return go.Slice(tr.position, tr.up, SliceMaterial);
    }

}
