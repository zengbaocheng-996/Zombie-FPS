using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedController : MonoBehaviour
{
    // Public Variables
    public float speed = 0.0f;
    // Private
    private Animator _controller = null;
    void Start()
    {
        _controller = GetComponent<Animator>();
    }
    void Update()
    {
        _controller.SetFloat("Speed", speed);
    }
}
