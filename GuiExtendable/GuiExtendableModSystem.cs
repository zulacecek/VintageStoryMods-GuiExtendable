using GuiExtendable.ExtendableGuiDialogs;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.Client.NoObf;

namespace GuiExtendable
{
    public class GuiExtendableModSystem : ModSystem
    {
        public override double ExecuteOrder() => 0.01;

        public static ICoreClientAPI? ClientApi { get; set; }

        public override void StartPre(ICoreAPI api)
        {
            base.StartPre(api);

            if (api is not ICoreClientAPI clientApi || clientApi is null)
            {
                return;
            }

            ClientApi = clientApi;

            GuiDialogExtendableRegistration.TryRegisterDialog(typeof(GuiDialogCharacter), typeof(GuiDialogCharacterExtendable));

            GuiDialogExtendableRegistration.LoadRegisteredGui();

            GuiExtendableLib.TriggerStartPreAfter();
        }
    }
}
