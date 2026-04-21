using GuiExtendable.ExtendableGuiDialogs;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.Client.NoObf;

namespace GuiExtendable
{
    public static class GuiExtendableLib
    {
        public static Action? StartPreAfter { get; set; }
        public static bool StartPreAfterDone { get; private set; }

        internal static void TriggerStartPreAfter()
        {
            StartPreAfter?.Invoke();
            StartPreAfterDone = true;
        }

        public static bool ExecuteAfterStartPre(Action action)
        {
            if (StartPreAfterDone)
            {
                action.Invoke();
                return true;
            }

            StartPreAfter += action;
            return false;
        }
    }
}
