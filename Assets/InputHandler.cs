using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    [Header("Settings")]
    public BottleGen bottleGen;
    public Camera mainCamera;
    public GameManager gameManager;


    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.escapeKey.isPressed)
        {
            bottleGen.ClearBottles();
        }

        if (Mouse.current.leftButton.wasPressedThisFrame) {
            onMouseDown();
        }

    }

    private void onMouseDown() {
        Vector2 worldPos = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit.collider == null) Debug.Log("No hit");

        if (hit.collider.GetComponent<Bottle>() == null) return;

        Bottle bottle = hit.collider.GetComponent<Bottle>();
        Debug.Log(Mouse.current.position.ReadValue().ToString());

        if (!gameManager.BottleAvailable(bottle)) {
            bottle.anim.PlayShake();
            return;
        }
        gameManager.TryPour(bottle);
    }
}
