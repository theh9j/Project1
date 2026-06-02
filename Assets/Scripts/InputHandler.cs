using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    [Header("Settings")]
    public BottleGen bottleGen;
    public Camera mainCamera;
    public GameManager gameManager;
    public UIHandler ui;
    public LevelDesigner levelDesigner;

    private Bottle prev;
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
        if (hit.collider == null) return;

        if (hit.collider.GetComponent<Bottle>() == null) return;

        Bottle bottle = hit.collider.GetComponent<Bottle>();

        if (ui.admin) {
            if (!ui.Selection && (prev == null || prev == bottle)) {
                if (prev == null ) prev = bottle;
                bottle.anim.SelectedHover(true);
                ui.BottleSelectedChangeColor(bottle);

            } else {
                prev.anim.SelectedHover(false);
                prev = null;
                ui.BottleSelectedChangeColor();
            }
            

        } else {
            if (!gameManager.BottleAvailable(bottle)) {
                bottle.anim.Play(1);
                return;
            }

            gameManager.TryPour(bottle);
        }
        
    }
}
