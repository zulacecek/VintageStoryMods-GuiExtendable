using GuiExtendable.ExtendableGuiDialogs;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.Client.NoObf;

namespace GuiExtendable
{
    public static class GuiExtendableLib
    {
        /// <summary>
        /// Handler to be fired after initialization of this library is done.
        /// </summary>
        public static Action? StartPreAfter { get; set; }
        internal static bool StartPreAfterDone { get; private set; }

        internal static void TriggerStartPreAfter()
        {
            StartPreAfter?.Invoke();
            StartPreAfterDone = true;
        }

        /// <summary>
        /// Registers handler to be fired after initialization of this library is done.
        /// Fires immediately if the initialization was done already.
        /// </summary>
        /// <param name="action">Handler to fire</param>
        /// <returns>True if the handler was fired immediately. False if it was registered for later.</returns>
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
