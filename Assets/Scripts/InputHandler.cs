using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    [Header("Settings")]
    public BottleGen bottleGen;
    public Camera mainCamera;
    public GameManager gameManager;
    public AdminUIHandler adui;
    public UIHandler ui;
    public LevelDesigner levelDesigner;
    private InputMode inputMode = InputMode.Normal;

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
    public void ToggleShuffleMode() {
        inputMode = inputMode == InputMode.Shuffle
            ? InputMode.Normal
            : InputMode.Shuffle;
    }

    public bool IsShuffleMode() {
        return inputMode == InputMode.Shuffle;
    }

    public void CancelMode() {
        inputMode = InputMode.Normal;
    }

    private void onMouseDown() {
        Vector2 worldPos = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider == null) return;

        Bottle bottle = hit.collider.GetComponent<Bottle>();
        if (bottle == null) return;

        if (adui.admin) {
            if (prev == bottle) {
                prev.anim.SelectedHover(false);
                prev = null;
                adui.BottleSelectedChangeColor();
                return;
            }

            if (prev != null)
                prev.anim.SelectedHover(false);

            prev = bottle;
            prev.anim.SelectedHover(true);
            adui.BottleSelectedChangeColor(prev);

            return;
        }

        if (!gameManager.BottleAvailable(bottle)) {
            bottle.anim.Play(1);
            return;
        }



        gameManager.TryPour(bottle);
    }
}

public enum InputMode {
    Normal,
    Shuffle
}