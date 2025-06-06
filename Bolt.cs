using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using MSCLoader;
using System;
using System.Collections;
using UnityEngine;
using WreckAPI;

namespace RevampedOASIS
{
    public class Bolt : Interactable
    {
        public Renderer renderer { get; private set; }
        public Material materialCache { get; set; }
        public int tightness
        {
            get => _tightness;
            set
            {
                set_tightness(value, true);
            }
        }

        public Action<int> onTightnessChanged;
        public float size = 1;
        public int maxTightness = 8;
        public bool canUseRatchet = true;
        public Vector3 positionStep = new Vector3(0, 0, -0.0025f);
        public Vector3 rotationStep = new Vector3(0, 0, 45);
        public bool disableSound;
        public bool useCustomLayerMask;

        internal Action<int> onTightnessSet;

        internal string boltUID; // Set from part
        GameEvent screwEvent;


        bool canBeBolted;
        bool onCooldown;
        int _tightness;

        static readonly FsmFloat wrenchSize = FsmVariables.GlobalVariables.FindFsmFloat("ToolWrenchSize");
        static readonly FsmBool usingRatchet = FsmVariables.GlobalVariables.FindFsmBool("PlayerHasRatchet");
        static Transform spanner;
        static Material highlightMaterial;
        static FsmBool ratchetSwitch;

        public void Awake()
        {
            renderer = GetComponent<Renderer>();
            if (!useCustomLayerMask) layerMask = 1 << 12;

            gameObject.SetActive(false);
            transform.localPosition += transform.localRotation * positionStep * -maxTightness;
            transform.localRotation *= Quaternion.Euler(rotationStep * -maxTightness);

            if (!spanner)
            {
                spanner = GameObject.Find("PLAYER/Pivot/AnimPivot/Camera/FPSCamera").transform.Find("2Spanner");

                var fsm = spanner.Find("Pivot/Ratchet").GetComponent<PlayMakerFSM>();
                fsm.InitializeFSM();
                ratchetSwitch = fsm.FsmVariables.FindFsmBool("Switch");

                fsm = spanner.Find("Raycast").GetComponents<PlayMakerFSM>()[1];
                fsm.InitializeFSM();
                highlightMaterial = ((SetMaterial)fsm.FsmStates[2].Actions[1]).material.Value;
            }

            screwEvent = new GameEvent(boltUID, OnScrewEvent);
        }

        public override void mouseOver()
        {
            if (wrenchSize.Value == size && (!usingRatchet.Value || canUseRatchet))
            {
                if (!canBeBolted)
                {
                    canBeBolted = true;
                    materialCache = renderer.material;
                    renderer.material = highlightMaterial;
                }

                if (usingRatchet.Value)
                {
                    if (Input.mouseScrollDelta.y != 0)
                    {
                        if (ratchetSwitch.Value) StartCoroutine(tryChangeTightness(1, 0.08f));
                        else StartCoroutine(tryChangeTightness(-1, 0.08f));
                    }
                }
                else
                {
                    if (Input.mouseScrollDelta.y > 0) StartCoroutine(tryChangeTightness(1, 0.28f));
                    else if (Input.mouseScrollDelta.y < 0) StartCoroutine(tryChangeTightness(-1, 0.28f));
                }
            }
            else tryResetMaterial();
        }

        public override void mouseExit() => tryResetMaterial();

        public void OnDisable() => tryResetMaterial();

        IEnumerator tryChangeTightness(int value, float cooldown)
        {
            if (onCooldown || tightness + value > maxTightness || tightness + value < 0) yield break;

            tightness += value;
            onTightnessChanged?.Invoke(value);
            if (!disableSound) MasterAudio.PlaySound3DAndForget("CarBuilding", sourceTrans: transform, variationName: "bolt_screw");

            onCooldown = true;
            yield return new WaitForSeconds(cooldown);
            onCooldown = false;
        }

        void tryResetMaterial()
        {
            if (canBeBolted)
            {
                canBeBolted = false;
                renderer.material = materialCache;
                materialCache = null;
            }
        }

        internal void set_tightness(int value, bool sendEvent)
        {
            onTightnessSet?.Invoke(value - _tightness);
            transform.localPosition += transform.localRotation * positionStep * (value - _tightness);
            transform.localRotation *= Quaternion.Euler(rotationStep * (value - _tightness));
            _tightness = value;

            if (sendEvent)
            {
                using (var p = screwEvent.Writer())
                {
                    p.Write((byte)value);
                    p.Send();
                }
            }
        }

        void OnScrewEvent(GameEventReader p)
        {
            set_tightness(p.ReadByte(), false);
        }
    }
}
