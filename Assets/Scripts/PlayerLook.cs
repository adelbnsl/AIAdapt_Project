using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public float sensibilite = 150f;

    public Transform playerBody;

    private float rotationX = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        float sourisX = Input.GetAxis("Mouse X") * sensibilite * Time.deltaTime;

        float sourisY = Input.GetAxis("Mouse Y") * sensibilite * Time.deltaTime;

        rotationX -= sourisY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);

        transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
        playerBody.Rotate(Vector3.up * sourisX);
    }
}
