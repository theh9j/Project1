using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
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

        if (Mouse.current.leftButton.wasPressedThisFrame && inputMode != InputMode.Tutorial) {
            onMouseDown();
        }

        if (inputMode == InputMode.Shuffle) {
            ui.ShuffleUnderlay(true);
        } else {
            ui.ShuffleUnderlay(false);
        }

    }

    public IEnumerator WaitForAction() {
        yield return new WaitUntil(() =>
            (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame) ||
            (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasReleasedThisFrame)
        );
    }

    public bool CheckForInput() {
        if ((Mouse.current != null && Mouse.current.leftButton.isPressed) ||
            (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)) { Debug.Log("Tapped"); return true; }
        return false;
    }

    public void ToggleTutorialMode() {
        inputMode = InputMode.Tutorial;
    }

    public void ToggleShuffleMode() {
        inputMode = inputMode == InputMode.Shuffle
            ? InputMode.Normal
            : InputMode.Shuffle;
    }

    public void CancelMode() {
        inputMode = InputMode.Normal;
    }

    private void onMouseDown() {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        Vector2 worldPos = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider == null) { CancelMode(); return; }

        Bottle bottle = hit.collider.GetComponent<Bottle>();
        if (bottle == null) { CancelMode(); return; }


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

        if (inputMode == InputMode.Shuffle) {
            bool res = gameManager.ShuffleBottle(bottle);
            if (res) ui.ShuffleUpdate();
            CancelMode();
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
    Shuffle,
    Tutorial
}