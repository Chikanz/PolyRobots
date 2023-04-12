using System.Collections;
using System.Collections.Generic;
using dook.tools.animatey;
using UnityEngine;

public class ScaleUp : MonoBehaviour
{
    public Animatey anim;

    public float animationDelay;
    // Start is called before the first frame update
    void Start()
    {
        transform.localScale = Vector3.zero;
        
        anim.Action = (val, args) =>
        {
            transform.localScale = Vector3.one * val;
        };
        Invoke(nameof(StartAnim), animationDelay);
    }

    void StartAnim()
    {
        anim.Play(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
