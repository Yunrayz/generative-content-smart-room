using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class GuideLine : MonoBehaviour
{
    public Transform target;

    public Transform toGuide; //Object to be guided (Line Start)
    public bool killOnPointLost = false; //If true, the line will be destroyed when the start or toGuide is lost

    // Start is called before the first frame update
    void Start()
    {
        EventManager.StartListening("DeleteGuideLine", DeleteGuideLine);

    }
    // Start is called before the first frame update
    private void OnDestroy()
    {


        EventManager.StopListening("DeleteGuideLine", DeleteGuideLine);

    }
    void Init(Transform start, Transform end)
    {

    }
    // Update is called once per frame
    void Update()
    {
        if (target != null && toGuide != null)
        {
            LineRenderer line = GetComponent<LineRenderer>();
            line.SetPosition(0, target.position);
            line.SetPosition(1, toGuide.position);
        }
        else if (killOnPointLost)
        {
            Destroy(gameObject);
        }

    }
    void DeleteGuideLine(EventDict dict)
    {
        GameObject sender = dict["sender"] as GameObject;
        if (sender.transform == target)
        {
            Destroy(gameObject);
        }
    }
    void DeleteGuideLine((Transform start, Transform end) data) //Doesn't work
    {
        if (data.start == target && data.end == toGuide)
        {
            Destroy(gameObject);
            return;
        }
        else if (data.start == target && data.end == null)
        {
            Destroy(gameObject);
            return;
        }
        else if (data.start == null && data.end == toGuide)
        {
            Destroy(gameObject);
            return;
        }
    }


    public void SetTarget(Transform t)
    {
        target = t;
    }
    public void SetToGuide(Transform t)
    {
        toGuide = t;
    }

    public void test(string a)
    {
        Debug.Log(a);
    }
    public void test(int a)
    {
        Debug.Log(a.GetType());
    }
}
