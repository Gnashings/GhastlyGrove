using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartBeat : MonoBehaviour
{
    public float dist;
    public float minDist = 1.5F;
    public float maxDist = 4.5F;
    public float closePitch = 1.5F;
    public float farPitch = 1F;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float x = Mathf.Clamp(dist, 1.5f, 4.5f);
        float pitch = (farPitch - closePitch) * (x - minDist) / (maxDist - minDist) + closePitch;
        gameObject.GetComponent<AudioSource>().pitch = pitch;
    }
}
