using UnityEngine;
using System.Collections;

public class CanvasRenderer : MonoBehaviour
{
    private GameObject sourceManager;
    private SourceManager sourceManagerScript;

	// Use this for initialization
	void Start ()
    {
        sourceManager = GameObject.Find("SourceManager");
    }

    // Update is called once per frame
    void Update ()
    {
        if (sourceManager == null)
        {
            return;
        }

        sourceManagerScript = sourceManager.GetComponent<SourceManager>();

        if (sourceManagerScript == null)
        {
            return;
        }
        
        GetComponent<Renderer>().material.mainTexture = sourceManagerScript.GetColorFrameTex();
    }
}
