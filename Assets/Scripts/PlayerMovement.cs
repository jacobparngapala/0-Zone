using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 movement;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    
    void Update()
    {
        float horizontal = 0f;
        float vertical = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed)
                horizontal -= 1f;
            if (Keyboard.current.dKey.isPressed)
                horizontal += 1f;
            if (Keyboard.current.sKey.isPressed)
                vertical -= 1f;
            if (Keyboard.current.wKey.isPressed)
                vertical += 1f;
        }

        movement = new Vector2(horizontal, vertical);

        transform.Translate(
            movement * moveSpeed * Time.deltaTime,
            Space.World
            );

        if (movement != Vector2.zero)
        {
            transform.up = movement;
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement *  moveSpeed * Time.fixedDeltaTime);
    }
}
