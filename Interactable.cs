using System;
using System.Collections.Generic;
using UnityEngine;

namespace RevampedOASIS
{
    public abstract class Interactable : MonoBehaviour
    {
        public LayerMask layerMask = ~(1 << 2);
        public float maxInteractionDistance = 1;

        public bool isMouseOver { get; private set; }
        bool[] mouseButtonsDown = new bool[3];

        public CursorType hoverCursor = CursorType.None;
        public string hoverCursorText = string.Empty;
        public KeyCode interactionKeyCode = KeyCode.None;
        public string interactionKey = string.Empty;

        static readonly Dictionary<int, RaycastHit> raycasts = new Dictionary<int, RaycastHit>();
        static Camera camera;
        static int lastFrame;
        static Ray ray;

        public static bool raycast(out RaycastHit hit, float distance, int layerMask)
        {
            if (!camera) camera = Camera.main;
            if (Time.frameCount != lastFrame)
            {
                raycasts.Clear();
                ray = camera.ScreenPointToRay(Input.mousePosition);
                lastFrame = Time.frameCount;
            }

            if (raycasts.TryGetValue(layerMask, out var cache))
            {
                if (cache.collider || cache.distance >= distance)
                {
                    hit = cache;
                    return cache.distance <= distance;
                }

                Physics.Raycast(ray.origin + ray.direction * cache.distance, ray.direction, out hit, distance - cache.distance, layerMask);
                hit.distance += cache.distance;
                raycasts[layerMask] = hit;
                return hit.collider;
            }
            else
            {
                Physics.Raycast(ray, out hit, distance, layerMask);
                raycasts.Add(layerMask, hit);
                return hit.collider;
            }
        }

        public virtual void update() { }
        public virtual void mouseEnter() { }
        public virtual void mouseOver() { }
        public virtual void mouseExit() { }
        public virtual void mouseDown(int button) { }
        public virtual void mouseHold(int button) { }
        public virtual void mouseUp(int button) { }
        public virtual void keyDown(KeyCode key) { }
        public virtual bool canInteract() { return true; }
        public virtual void interacted() { }

        public void Update()
        {
            if (raycast(out var hit, maxInteractionDistance, layerMask) && hit.collider.gameObject == gameObject)
            {
                if (!isMouseOver)
                {
                    isMouseOver = true;
                    mouseEnter();
                }

                mouseOver();
                UpdateMouseButtons(true);
                CheckKeyDown();

                if (canInteract())
                {
                    if (hoverCursor != CursorType.None) hoverCursor.SetValue(true);
                    if (hoverCursorText != string.Empty) CursorGUI.interaction = hoverCursorText;

                    if ((interactionKeyCode != KeyCode.None && Input.GetKeyDown(interactionKeyCode)) ||
                        (interactionKey != "" && cInput.GetKeyDown(interactionKey))) interacted();
                }
            }
            else if (isMouseOver)
            {
                isMouseOver = false;
                mouseExit();
                UpdateMouseButtons(false);
                if (hoverCursor != CursorType.None) hoverCursor.SetValue(false);
                if (hoverCursorText != string.Empty) CursorGUI.interaction = "";
            }

            update();
        }

        void UpdateMouseButtons(bool mouseOver)
        {
            if (!mouseOver)
            {
                for (int i = 0; i < mouseButtonsDown.Length; i++)
                {
                    if (mouseButtonsDown[i]) mouseUp(i);
                    mouseButtonsDown[i] = false;
                }

                return;
            }

            for (int i = 0; i < mouseButtonsDown.Length; i++)
            {
                if (Input.GetMouseButtonDown(i)) mouseDown(i);
                if (Input.GetMouseButtonUp(i)) mouseUp(i);

                bool hold = Input.GetMouseButton(i);
                if (hold) mouseHold(i);
                mouseButtonsDown[i] = hold;
            }
        }

        void CheckKeyDown()
        {
            if (!Input.anyKeyDown) return;

            var values = Enum.GetValues(typeof(KeyCode)) as KeyCode[];
            for (int i = 0; i < values.Length; i++)
            {
                if (Input.GetKeyDown(values[i])) keyDown(values[i]);
            }
        }
    }
}