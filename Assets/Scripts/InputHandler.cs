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
    private InputMode previousInput = InputMode.Invalid;

    private Bottle prev;
    void Update()
    {
        if (Keyboard.current.escapeKey.isPressed)
        {
            bottleGen.ClearBottles();
        }

        if ((
            (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) || 
            (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)) && 
            inputMode != InputMode.Tutorial) {
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
            ((Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
            (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame))
        );
    }

    public IEnumerator WaitForRelease() {
        yield return new WaitUntil(() => 
            ((Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame) ||
            (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasReleasedThisFrame))
        );
    }

    public bool CheckForInput() {
        if ((Mouse.current != null && Mouse.current.leftButton.isPressed) ||
            (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)) { return true; }
        return false;
    }

    private void ToggleModes(InputMode preferInput) {
        previousInput = inputMode;
        inputMode = preferInput;
    }

    public void UndoModes() {
        if (previousInput == InputMode.Invalid || previousInput == InputMode.Paused) return;
        inputMode = previousInput;
        previousInput = InputMode.Invalid;
    }

    private bool TryGetPointerPosition(out Vector2 screenPos) {
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed) {
            screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
            return true;
        }

        if (Mouse.current != null) {
            screenPos = Mouse.current.position.ReadValue();
            return true;
        }

        screenPos = default;
        return false;
    }

    private bool IsPointerOverUI() {
        if (EventSystem.current == null) return false;

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed) {
            int touchId = Touchscreen.current.primaryTouch.touchId.ReadValue();
            return EventSystem.current.IsPointerOverGameObject(touchId);
        }

        return EventSystem.current.IsPointerOverGameObject();
    }

    private void onMouseDown() {
        if (IsPointerOverUI()) return;

        if (!TryGetPointerPosition(out Vector2 screenPos)) return;

        Vector2 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider == null) {
            if (inputMode == InputMode.Shuffle) CancelMode();
            return; 
        }

        Bottle bottle = hit.collider.GetComponent<Bottle>();
        if (bottle == null) {
            if (inputMode == InputMode.Shuffle) CancelMode();
            return; 
        }


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
    public void ToggleTutorialMode() {
        ToggleModes(InputMode.Tutorial);
    }

    public void ToggleShuffleMode() {
        if (inputMode == InputMode.Shuffle) UndoModes();
        else ToggleModes(InputMode.Shuffle);
    }

    public void CancelMode() {
        ToggleModes(InputMode.Normal);
    }

    public void GamePause() {
        ToggleModes(InputMode.Paused);
    }
}

public enum InputMode {
    Invalid,
    Normal,
    Shuffle,
    Paused,
    Tutorial
}