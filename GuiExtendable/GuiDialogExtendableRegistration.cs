using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.Client.NoObf;

namespace GuiExtendable
{
    internal static class GuiDialogExtendableRegistration
    {
        public static ICoreClientAPI? ClientApi => GuiExtendableModSystem.ClientApi;

        public static Dictionary<Type, Type> RegisteredCustomGui { get; set; } = [];

        internal static void LoadRegisteredGui()
        {
            if (ClientApi is null)
            {
                return;
            }

            foreach (var customGui in RegisteredCustomGui)
            {
                TryLoadDialog(customGui.Key, ClientApi);
            }
        }

        internal static void TryLoadDialog(Type dialogToReplace, ICoreClientAPI api)
        {
            if (ClientApi is null)
            {
                return;
            }

            var dialogToRegisterType = GetRegisteredDialog(dialogToReplace);
            if (dialogToRegisterType is null)
            {
                return;

            }

            var dialogToRegister = Activator.CreateInstance(dialogToRegisterType, ClientApi) as GuiDialog;
            if (dialogToRegister is null)
            {
                return;
            }

            TryLoadDialog(dialogToReplace, dialogToRegister);
        }

        public static Type? GetRegisteredDialog(Type registeredDialogType) => RegisteredCustomGui.GetValueOrDefault(registeredDialogType);

        /// <summary>
        /// Preregister dialog to avoid composing unnecessary gui for dialogs you want to replace. Your mod must have ExecuteOrder lower than 0.01 to be effective.
        /// </summary>
        /// <param name="dialogToReplace">Dialog to replace. E.g. GuiDialogCharacterExtendable</param>
        /// <param name="dialog">Your custom dialog</param>
        /// <param name="forceReplace">Forces the replacement of dialog key if it was already pre-registered</param>
        public static void TryRegisterDialog(Type dialogToReplace, Type dialog, bool forceReplace = false)
        {
            if (forceReplace && RegisteredCustomGui.ContainsKey(dialogToReplace))
            {
                RegisteredCustomGui.Remove(dialogToReplace);
                RegisteredCustomGui.Add(dialogToReplace, dialog);
            }

            RegisteredCustomGui.TryAdd(dialogToReplace, dialog);
        }

        /// <summary>
        /// Replaces dialog loaded in the game.
        /// </summary>
        /// <param name="dialogToReplace">Type of dialog to be replaced. This is a forceful way and replaces all registered dialogs. E.g. typeof(GuiDialogCharacter)</param>
        /// <param name="customDialog">Instance of your custom dialog</param>
        /// <param name="api">Client api</param>
        public static void TryLoadDialog(Type dialogToReplace, GuiDialog customDialog)
        {
            if (ClientApi is null)
            {
                return;
            }

            var vanillaDialog = GetLoadedDialog(dialogToReplace);
            if (vanillaDialog is null)
            {
                return;
            }

            if (ClientApi.World is not ClientMain client)
            {
                return;
            }

            client.UnregisterDialog(vanillaDialog);
            vanillaDialog.Dispose();

            client.RegisterDialog(customDialog);
        }

        /// <summary>
        /// Finds dialog loaded in engine
        /// </summary>
        /// <typeparam name="T">Type of the dialog. E.g. GuiDialogCharacter</typeparam>
        /// <returns>Instance of the dialog</returns>
        public static T? GetLoadedDialog<T>()
            where T : GuiDialog
        {
            if (ClientApi is null)
            {
                return default;
            }

            return GetLoadedDialog(typeof(T)) as T;
        }

        /// <summary>
        /// Finds dialog loaded in engine by type
        /// </summary>
        /// <param name="dialogType">By type</param>
        /// <returns>Instance of the dialog</returns>
        public static GuiDialog? GetLoadedDialog(Type dialogType)
        {
            if (ClientApi is null)
            {
                return default;
            }

            return ClientApi.Gui.LoadedGuis.Find(x => x.GetType() == dialogType);
        }
    }
}
