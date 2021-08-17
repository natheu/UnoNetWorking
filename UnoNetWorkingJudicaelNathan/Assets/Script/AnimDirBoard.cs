using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimDirBoard : MonoBehaviour
{
    [SerializeField]
    GameObject PrefabDirection;
    [SerializeField]
    float Dist = 2f;
    [SerializeField]
    float Speed = 2f;

    Transform FirstArrow;
    Transform SecondArrow;

    int Dir;
    float currentPos;

    // Start is called before the first frame update
    void Start()
    {
        FirstArrow = Instantiate(PrefabDirection, transform).transform;
        FirstArrow.localPosition = new Vector3(Mathf.Cos(Mathf.PI) * Dist, 0f, Mathf.Sin(Mathf.PI) * Dist);
        SecondArrow = Instantiate(PrefabDirection, transform).transform;
        SecondArrow.localPosition = new Vector3(Mathf.Cos(0f) * Dist, 0f, Mathf.Sin(0f) * Dist);
        Dir = -1;

        currentPos = Mathf.PI / 2f;
    }

    public void UpdateDirection(int newDir)
    {
        Dir = newDir;
    }

    // Update is called once per frame
    void Update()
    {
        currentPos += Time.deltaTime * Speed * Dir;

        Vector3 fNextPos = new Vector3(Mathf.Cos(currentPos) * Dist, 0f, Mathf.Sin(currentPos) * Dist);
        FirstArrow.forward = (fNextPos - FirstArrow.localPosition).normalized;
        FirstArrow.localPosition = fNextPos;

        Vector3 sNextPos = new Vector3(Mathf.Cos(currentPos - (Mathf.PI)) * Dist, 0f, Mathf.Sin(currentPos - (Mathf.PI)) * Dist);
        SecondArrow.forward = (sNextPos - SecondArrow.localPosition).normalized;
        SecondArrow.localPosition = sNextPos;
    }
}
