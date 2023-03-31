using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public Transform camOffset;

    public Transform cam;

    private Vector3 offset;

    public float mul = 1;

    public InputActionReference input_reset;
    private CharacterController characterController;
    private Vector3 camLastPos;
    private void Awake()
    {
        offset = camOffset.localPosition;
        characterController = GetComponent<CharacterController>();
    }

    private void Start()
    {
        input_reset.action.performed += (v) =>
        {
            if (v.ReadValueAsButton())
            {
                SceneManager.LoadScene(0);
            }
        };
    }
    private void Update()
    {
        camOffset.localPosition = offset - cam.localPosition;

        Vector3 camMove = cam.localPosition - camLastPos;

        characterController.Move(camOffset.TransformDirection(camMove)* mul);

        camLastPos = cam.localPosition;

        if (!characterController.isGrounded)
        {
            characterController.Move(Vector3.down * Time.deltaTime*5);
        }
    }
}
