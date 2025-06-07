using HutongGames.PlayMaker;

namespace RevampedOASIS
{
    public static class CursorGUI
    {
        public static bool assemble
        {
            get => GUIassemble.Value;
            set => GUIassemble.Value = value;
        }
        public static bool buy
        {
            get => GUIbuy.Value;
            set => GUIbuy.Value = value;
        }
        public static bool disassemble
        {
            get => GUIdisassemble.Value;
            set => GUIdisassemble.Value = value;
        }
        public static bool drive
        {
            get => GUIdrive.Value;
            set => GUIdrive.Value = value;
        }
        public static bool passenger
        {
            get => GUIpassenger.Value;
            set => GUIpassenger.Value = value;
        }
        public static bool use
        {
            get => GUIuse.Value;
            set => GUIuse.Value = value;
        }
        public static string gear
        {
            get => GUIgear.Value;
            set => GUIgear.Value = value;
        }
        public static string interaction
        {
            get => GUIinteraction.Value;
            set => GUIinteraction.Value = value;
        }
        public static string subtitle
        {
            get => GUIsubtitle.Value;
            set => GUIsubtitle.Value = value;
        }

        static readonly FsmBool GUIassemble = FsmVariables.GlobalVariables.FindFsmBool("GUIassemble");
        static readonly FsmBool GUIbuy = FsmVariables.GlobalVariables.FindFsmBool("GUIbuy");
        static readonly FsmBool GUIdisassemble = FsmVariables.GlobalVariables.FindFsmBool("GUIdisassemble");
        static readonly FsmBool GUIdrive = FsmVariables.GlobalVariables.FindFsmBool("GUIdrive");
        static readonly FsmBool GUIpassenger = FsmVariables.GlobalVariables.FindFsmBool("GUIpassenger");
        static readonly FsmBool GUIuse = FsmVariables.GlobalVariables.FindFsmBool("GUIuse");
        static readonly FsmString GUIgear = FsmVariables.GlobalVariables.FindFsmString("GUIgear");
        static readonly FsmString GUIinteraction = FsmVariables.GlobalVariables.FindFsmString("GUIinteraction");
        static readonly FsmString GUIsubtitle = FsmVariables.GlobalVariables.FindFsmString("GUIsubtitle");

        public static bool GetValue(this CursorType cursorType)
        {
            var variable = getVariable(cursorType);
            if (variable != null) return variable.Value;
            return false;
        }

        public static void SetValue(this CursorType cursorType, bool value)
        {
            var variable = getVariable(cursorType);
            if (variable != null) variable.Value = value;
        }

        static FsmBool getVariable(CursorType cursorType)
        {
            switch (cursorType)
            {
                case CursorType.Assemble:
                    return GUIassemble;
                case CursorType.Buy:
                    return GUIbuy;
                case CursorType.Disassemble:
                    return GUIdisassemble;
                case CursorType.Drive:
                    return GUIdrive;
                case CursorType.Passenger:
                    return GUIpassenger;
                case CursorType.Use:
                    return GUIuse;
                default:
                    return null;
            }
        }
    }

    public enum CursorType
    {
        None,
        Assemble,
        Buy,
        Disassemble,
        Drive,
        Passenger,
        Use
    }
}