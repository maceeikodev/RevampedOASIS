using System;
using UnityEngine;
using WreckAPI;

namespace RevampedOASIS
{
    public class BasePart : Interactable
    {
        public string tagCache { get; set; }
        public int attachedTo
        {
            get => _attachedTo;
            set
            {
                set_attachedTo(value, true);
            }
        }

        public Rigidbody rigidbody { get; internal set; }
        public int tightness { get; private set; }

        public string partUID;
        public bool sendInitialSync = true;
        GameEvent attachUpdateEvent;

        public Collider[] triggers;
        public Bolt[] bolts;
        public bool disableSound;
        public bool useCustomLayerMask;

        public event Action<int> onAttach;
        public event Action<int> onDetach;
        public event Func<bool> canAttach;
        public event Func<bool> canDetach;

        int inTriggerIndex = -1;
        int _attachedTo = -1;

        public void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
            if (bolts != null)
            {
                for (var i = 0; i < bolts.Length; i++) bolts[i].onTightnessSet += (deltaTightness) => tightness += deltaTightness;
            }
            if (!useCustomLayerMask) layerMask = 1 << 19;

            attachUpdateEvent = new GameEvent(partUID + "_attach", OnAttachUpdateEvent);
            if (WreckMPGlobals.IsHost && sendInitialSync)
            {
                WreckMPGlobals.OnMemberReady(SendAttachEvent);
            }
        }

        public override void mouseOver()
        {
            if (attachedTo < 0) return;
            if (tightness > 0)
            {
                CursorGUI.disassemble = false;
                return;
            }
            if (canDetach != null && !canDetach.Invoke()) return;

            if (Input.GetMouseButtonDown(1))
            {
                var index = attachedTo;
                detach();
                onDetach?.Invoke(index);
                if (!disableSound) MasterAudio.PlaySound3DAndForget("CarBuilding", sourceTrans: transform, variationName: "disassemble");
                CursorGUI.disassemble = false;
            }
            else CursorGUI.disassemble = true;
        }

        public override void mouseExit()
        {
            if (attachedTo >= 0) CursorGUI.disassemble = false;
        }

        public void OnTriggerEnter(Collider other)
        {
            if (!transform.parent) return;

            var i = Array.IndexOf(triggers, other);
            if (i >= 0) inTriggerIndex = i;
        }

        public void OnTriggerExit(Collider other)
        {
            if (inTriggerIndex >= 0 && other == triggers[inTriggerIndex])
            {
                inTriggerIndex = -1;
                CursorGUI.assemble = false;
            }
        }

        public void LateUpdate()
        {
            if (attachedTo >= 0 || inTriggerIndex < 0 || (canAttach != null && !canAttach.Invoke())) return;

            if (Input.GetMouseButtonDown(0))
            {
                attach(inTriggerIndex);
                onAttach?.Invoke(inTriggerIndex);
                inTriggerIndex = -1;
                if (!disableSound) MasterAudio.PlaySound3DAndForget("CarBuilding", sourceTrans: transform, variationName: "assemble");
                CursorGUI.assemble = false;
            }
            else CursorGUI.assemble = true;
        }

        public virtual void attach(int index)
        {
            if (bolts != null)
            {
                for (var i = 0; i < bolts.Length; i++) bolts[i].gameObject.SetActive(true);
            }
            if (!rigidbody) rigidbody = GetComponent<Rigidbody>();

            transform.SetParent(triggers[index].transform);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            if (tag == "Untagged") tagCache = "PART";
            else tagCache = tag;
            tag = "Untagged";

            if (attachedTo >= 0) triggers[attachedTo].enabled = true;
            triggers[index].enabled = false;
            _attachedTo = index;
        }

        public virtual void detach()
        {
            if (bolts != null)
            {
                for (var i = 0; i < bolts.Length; i++)
                {
                    bolts[i].gameObject.SetActive(false);
                    bolts[i].tightness = 0;
                }
            }

            transform.SetParent(null);
            tag = tagCache;
            tagCache = null;

            triggers[attachedTo].enabled = true;
            _attachedTo = -1;
        }

        void set_attachedTo(int value, bool sendEvent)
        {
            if (value == _attachedTo) return;

            if (value < 0) detach();
            else attach(value);

            _attachedTo = value;

            if (sendEvent)
            {
                SendAttachEvent(0);
            }
        }

        void SendAttachEvent(ulong target)
        {
            using (var p = attachUpdateEvent.Writer())
            {
                p.Write(attachedTo);
                p.Send(target);
            }
        }

        void OnAttachUpdateEvent(GameEventReader p)
        {
            set_attachedTo(p.ReadInt32(), false);
        }
    }
}