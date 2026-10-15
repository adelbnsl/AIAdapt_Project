
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float vitesse = 3f;

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 direction = transform.right * horizontal
                          + transform.forward * vertical;

        controller.Move(direction * vitesse * Time.deltaTime);
    }
}
