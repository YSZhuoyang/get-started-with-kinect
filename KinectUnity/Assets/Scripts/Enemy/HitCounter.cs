using UnityEngine;
using System.Collections;

public class HitCounter : MonoBehaviour
{
    private uint score;
    private GUIText billboardText;


    // Use this for initialization
    void Start ()
    {
        billboardText = GameObject.Find("Billboard").GetComponent<GUIText>();
        score = 0;
    }
	
	// Update is called once per frame
	void Update ()
    {
	    
	}

    public void FireBallHit()
    {
        score++;
        billboardText.text = "" + score;
    }

    public void LightningHit()
    {
        score++;
        billboardText.text = "" + score;
    }
}
