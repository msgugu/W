using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Example : MonoBehaviour
{
    public GameObject Bodys;  //Scene���� ���콺�� �巡�� �Ͽ� �����Ѵ�.

    SkinnedMeshRenderer bodys_skinnedMeshRenderer;
    Mesh bodys_skinnedMesh;
    int blendSpeed = 2;
    int blendShapeCount = 0;
    int dir = 1;
    int current_motion = 0;
    int current_weight = 0;
    Dictionary<string, int> blendDict = new Dictionary<string, int>();

    void Awake()
    {
        bodys_skinnedMeshRenderer = Bodys.GetComponent<SkinnedMeshRenderer>();
        bodys_skinnedMesh = Bodys.GetComponent<SkinnedMeshRenderer>().sharedMesh;
    }

    void Start()
    {
        blendShapeCount = bodys_skinnedMesh.blendShapeCount;
        Debug.Log("blendShapeCount=" + blendShapeCount);
        for (int i = 0; i < blendShapeCount; i++)
        {
            string expression = bodys_skinnedMesh.GetBlendShapeName(i);
            Debug.Log(expression);
            blendDict.Add(expression, i);
        }
        //print dictionary
        foreach (KeyValuePair<string, int> pair in blendDict)
        {
            Debug.Log(pair);
        }
    }

    // Update is called once per frame
    void Update()
    {
        current_weight = current_weight + blendSpeed * dir;
        if (current_weight >= 100.0f)
        {
            dir = -1;
        }
        else if (current_weight < 0)
        {
            dir = 1;
            current_motion = current_motion + 1;
            current_weight = 0;
            if (current_motion >= blendShapeCount) current_motion = 0;
        }
        // ���� �߿� Blend Shape Weight���� �ٲپ� �ش�. 
        bodys_skinnedMeshRenderer.SetBlendShapeWeight(current_motion, current_weight);
    }
}
