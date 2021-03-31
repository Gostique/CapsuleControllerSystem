using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CapsuleControllerSystem;
using Tools;

public class TestController : MonoBehaviour
{

    #region Fields
    [SerializeField] private CapsuleController _controller;
    #endregion

    #region Unity API
    private void Update()
    {
        Vector3 input = new Vector3(
            Input.GetAxisRaw("Horizontal"),
            0f,
            Input.GetAxisRaw("Vertical")
            );
        _controller.MoveLocal(input);

        if (Input.GetButtonDown("Jump"))
        {
            _controller.Jump();
        }

        _controller.Rotate(Input.GetAxis("Mouse X"));

    }
    #endregion

}
