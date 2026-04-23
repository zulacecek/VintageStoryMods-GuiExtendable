using Cairo;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.Client.NoObf;

namespace GuiExtendable.ExtendableGuiDialogs
{
    public class GuiDialogCharacterExtendable : GuiDialogCharacter
    {
        public GuiDialogCharacterExtendable(ICoreClientAPI capi)
            : base(capi)
        {
            rendertabhandlers = [ComposeCharacterTabCustom];
        }

        public virtual double UnscaledSlotPadding => GuiElementItemSlotGridBase.unscaledSlotPadding;
        public virtual string CharacterComposerName => "playercharacter";
        public virtual string LeftClothingSlotsName => "leftSlots";
        public virtual string RightClothingSlotsName => "rightSlots";

        public IInventory CharacterInventory
        {
            get => characterInv;
            set => characterInv = value;
        }

        public ElementBounds RenderEntityBounds
        {
            get => insetSlotBounds;
            set => insetSlotBounds = value;
        }

        public float Yaw
        {
            get => yaw;
            set => yaw = value;
        }
        public bool RotateCharacter
        {
            get => rotateCharacter;
            set => rotateCharacter = value;
        }

        public bool ShowArmorSlots
        {
            get => showArmorSlots;
            set => showArmorSlots = value;
        }

        public int CurrentTab
        {
            get => curTab;
            set => curTab = value;
        }

        public Size2d MainTabInnerSize
        {
            get => mainTabInnerSize;
            set => mainTabInnerSize = value;
        }

        public Vec4f LightPosition
        {
            get => lighPos;
            set => lighPos = value;
        }

        public Matrixf Matrix
        {
            get => mat;
            set => mat = value;
        }

        public override event Action? ComposeExtraGuis;

        public override event Action<int>? TabClicked;

        public virtual List<int> RenderEntityOnTabIds { get; set; } = [0];

        public virtual void TryRegisterArmorIcons()
        {
            if (!ShouldRegisterArmorIcons)
            {
                return;
            }

            var customIcons = capi.Gui.Icons.CustomIcons;
            customIcons["armorhead"] = GetSvgIconSource("textures/icons/character/armor-helmet.svg");
            customIcons["armorbody"] = GetSvgIconSource("textures/icons/character/armor-body.svg");
            customIcons["armorlegs"] = GetSvgIconSource("textures/icons/character/armor-legs.svg");
        }

        public virtual IconRendererDelegate GetSvgIconSource(string path) => capi.Gui.Icons.SvgIconSource(new AssetLocation(path));

        public virtual bool ShouldRegisterArmorIcons => !capi.Gui.Icons.CustomIcons.ContainsKey("left_hand");

        public virtual void ComposeCharacterTabCustom(GuiComposer composer)
        {
            TryRegisterArmorIcons();

            ElementBounds leftClothingSlots = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0.0, 20.0 + UnscaledSlotPadding, 1, 6).FixedGrow(0.0, UnscaledSlotPadding - 6);

            var armorSlots = TryComposeArmorSlots();
            if (armorSlots.Count > 0)
            {
                var lastArmorSlot = armorSlots.Last();
                leftClothingSlots.FixedRightOf(lastArmorSlot.Value, 10.0);
            }

            RenderEntityBounds = ElementBounds.Fixed(0.0, 22.0 + UnscaledSlotPadding, 190.0, leftClothingSlots.fixedHeight - 2.0 * UnscaledSlotPadding - 4.0);
            RenderEntityBounds.FixedRightOf(leftClothingSlots, 10.0);

            ElementBounds rightClothingSlots = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0.0, 20.0 + UnscaledSlotPadding, 1, 6).FixedGrow(0.0, UnscaledSlotPadding - 6);
            rightClothingSlots.FixedRightOf(RenderEntityBounds, 10.0);

            foreach (var armorSlot in armorSlots)
            {
                var armorSlotElementBounds = armorSlot.Value;
                composer.AddItemSlotGrid(CharacterInventory, SendInventoryPacket, 1, [armorSlot.Key], armorSlotElementBounds, armorSlotElementBounds.Name);
            }

            composer
                .AddItemSlotGrid(CharacterInventory, SendInventoryPacket, 1, GetLeftCloathingSlotIds(), leftClothingSlots, LeftClothingSlotsName)
                .AddInset(RenderEntityBounds, 0)
                .AddItemSlotGrid(CharacterInventory, SendInventoryPacket, 1, GetRightCloathingSlotIds(), rightClothingSlots, RightClothingSlotsName);
        }

        public virtual SortedList<int, ElementBounds> TryComposeArmorSlots()
        {
            SortedList<int, ElementBounds> value = [];
            if (!ShowArmorSlots)
            {
                return value;
            }

            ElementBounds armorSlotHeadBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0.0, 20.0 + UnscaledSlotPadding, 1, 1).FixedGrow(0.0, UnscaledSlotPadding);
            armorSlotHeadBounds.Name = "armorSlotsHead";
            value.Add((int)EnumCharacterDressType.ArmorHead, armorSlotHeadBounds);

            ElementBounds armorSlotBodyBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0.0, 20.0 + UnscaledSlotPadding + 102.0, 1, 1).FixedGrow(0.0, UnscaledSlotPadding);
            armorSlotBodyBounds.Name = "armorSlotsBody";
            value.Add((int)EnumCharacterDressType.ArmorBody, armorSlotBodyBounds);

            ElementBounds armorSlotLegsBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0.0, 20.0 + UnscaledSlotPadding + 204.0, 1, 1).FixedGrow(0.0, UnscaledSlotPadding);
            armorSlotLegsBounds.Name = "armorSlotsLegs";
            value.Add((int)EnumCharacterDressType.ArmorLegs, armorSlotLegsBounds);

            return value;
        }

        public virtual void ComposeGui()
        {
            CharacterInventory = capi.World.Player.InventoryManager.GetOwnInventory("character");
            ElementBounds backgroundElementBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);

            SizeBackgroundElementBounds(backgroundElementBounds);

            ElementBounds composerBaseElementBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.LeftMiddle).WithFixedAlignmentOffset(GuiStyle.DialogToScreenPadding, 0.0);

            var characterClassKey = "characterClass";
            string characterClassText = capi.World.Player.Entity.WatchedAttributes.GetString(characterClassKey);
            string characterNameAndClassText = Lang.Get("characterdialog-title-nameandclass", capi.World.Player.PlayerName, Lang.Get($"{characterClassKey.ToLowerInvariant()}-{characterClassText}"));
            if (!Lang.HasTranslation($"{characterClassKey.ToLowerInvariant()}-{characterClassText}"))
            {
                characterNameAndClassText = capi.World.Player.PlayerName;
            }

            ElementBounds tabsElementBounds = ElementBounds.Fixed(5.0, -24.0, 350.0, 25.0);

            ClearComposers();

            var tabsKey = "tabs";
            var font = CairoFont.WhiteSmallText().WithWeight((FontWeight)1);
            Composers[CharacterComposerName] = capi.Gui.CreateCompo(CharacterComposerName, composerBaseElementBounds)
                .AddShadedDialogBG(backgroundElementBounds)
                .AddDialogTitleBar(characterNameAndClassText, OnTitleBarClose)
                .AddHorizontalTabs([.. Tabs], tabsElementBounds, OnTabClickedCustom, font, font, tabsKey)
                .BeginChildElements(backgroundElementBounds);

            Composers[CharacterComposerName].GetHorizontalTabs(tabsKey).activeElement = CurrentTab;

            rendertabhandlers[CurrentTab](Composers[CharacterComposerName]);

            Composers[CharacterComposerName].EndChildElements().Compose();

            ComposeExtraGuis?.Invoke();

            TryReduceMainTabSize(backgroundElementBounds);
        }

        public virtual void SizeBackgroundElementBounds(ElementBounds backgroundElementBounds)
        {
            if (RenderEntityOnTabIds.Contains(CurrentTab))
            {
                backgroundElementBounds.BothSizing = ElementSizing.FitToChildren;
            }
            else
            {
                backgroundElementBounds.BothSizing = ElementSizing.Fixed;
                backgroundElementBounds.fixedWidth = MainTabInnerSize.Width;
                backgroundElementBounds.fixedHeight = MainTabInnerSize.Height;
            }
        }

        public virtual void TryReduceMainTabSize(ElementBounds backgroundElementBounds)
        {
            if (RenderEntityOnTabIds.Contains(CurrentTab))
            {
                MainTabInnerSize.Width = backgroundElementBounds.InnerWidth / RuntimeEnv.GUIScale;
                MainTabInnerSize.Height = backgroundElementBounds.InnerHeight / RuntimeEnv.GUIScale;
            }
        }

        public virtual void OnTabClickedCustom(int tabindex)
        {
            TabClicked?.Invoke(tabindex);
            CurrentTab = tabindex;
            ComposeGui();
        }

        public override void OnMouseDown(MouseEvent args)
        {
            base.OnMouseDown(args);
            RotateCharacter = RenderEntityBounds.PointInside(args.X, args.Y);
        }

        public override void OnMouseUp(MouseEvent args)
        {
            base.OnMouseUp(args);
            RotateCharacter = false;
        }

        public override void OnMouseMove(MouseEvent args)
        {
            base.OnMouseMove(args);
            if (RotateCharacter)
            {
                Yaw -= args.DeltaX / 100f;
            }
        }

        public override void OnRenderGUI(float deltaTime)
        {
            TryRenderEntity(deltaTime);
            base.OnRenderGUI(deltaTime);
        }

        public void TryRenderEntity(float deltaTime)
        {
            if (!RenderEntityOnTabIds.Contains(CurrentTab) || CurrentTab == 0)
            {
                return;
            }

            capi.Render.GlPushMatrix();

            if (Focused)
            {
                capi.Render.GlTranslate(0f, 0f, 150f);
            }

            double padding = GuiElement.scaled(GuiElementItemSlotGridBase.unscaledSlotPadding);

            capi.Render.GlRotate(-14f, 1f, 0f, 0f);

            Matrix.Identity();
            Matrix.RotateXDeg(-14f);

            var lightPositionUniformName = "lightPosition";
            Vec4f vec4f = Matrix.TransformVector(LightPosition);
            capi.Render.CurrentActiveShader.Uniform(lightPositionUniformName, new Vec3f(vec4f.X, vec4f.Y, vec4f.Z));
            capi.Render.RenderEntityToGui(deltaTime, capi.World.Player.Entity, CalculateRenderEntityX(padding), CalculateRenderEntityY(padding), CalculateRenderEntityZ(padding), Yaw, CalculateRenderEntitySize(padding), -1);
            capi.Render.GlPopMatrix();
            capi.Render.CurrentActiveShader.Uniform(lightPositionUniformName, new Vec3f(1f, -1f, 0f).Normalize());

            if (!RenderEntityBounds.PointInside(capi.Input.MouseX, capi.Input.MouseY) && !RotateCharacter)
            {
                Yaw += (float)(Math.Sin(capi.World.ElapsedMilliseconds / 1000f) / 200.0);
            }
        }

        public virtual double CalculateRenderEntityX(double padding) => RenderEntityBounds.renderX + padding - GuiElement.scaled(41.0);

        public virtual double CalculateRenderEntityY(double padding) => RenderEntityBounds.renderY + padding - GuiElement.scaled(30.0);

        public virtual double CalculateRenderEntityZ(double padding) => GuiElement.scaled(250.0) + padding;

        public virtual float CalculateRenderEntitySize(double padding) => (float)(GuiElement.scaled(135.0) + padding);

        public override void OnGuiOpened()
        {
            ComposeGui();
            var gameMode = capi.World.Player.WorldData.CurrentGameMode;
            if ((gameMode == EnumGameMode.Guest || gameMode == EnumGameMode.Survival) && CharacterInventory != null)
            {
                CharacterInventory.Open(capi.World.Player);
            }
        }

        public override bool TryOpen()
        {
            return base.TryOpen();
        }

        public override void OnGuiClosed()
        {
            if (CharacterInventory != null)
            {
                CharacterInventory.Close(capi.World.Player);
                Composers[CharacterComposerName].GetSlotGrid(LeftClothingSlotsName)?.OnGuiClosed(capi);
                Composers[CharacterComposerName].GetSlotGrid(RightClothingSlotsName)?.OnGuiClosed(capi);
            }

            CurrentTab = 0;
        }

        public virtual int[] GetLeftCloathingSlotIds()
        {
            return [
                (int)EnumCharacterDressType.Head,
                (int)EnumCharacterDressType.Shoulder,
                (int)EnumCharacterDressType.UpperBody,
                (int)EnumCharacterDressType.UpperBodyOver,
                (int)EnumCharacterDressType.LowerBody,
                (int)EnumCharacterDressType.Foot,
            ];
        }

        public virtual int[] GetRightCloathingSlotIds()
        {
            return [
                (int)EnumCharacterDressType.Neck,
                (int)EnumCharacterDressType.Emblem,
                (int)EnumCharacterDressType.Face,
                (int)EnumCharacterDressType.Arm,
                (int)EnumCharacterDressType.Hand,
                (int)EnumCharacterDressType.Waist,
            ];
        }

        public void SendInventoryPacket(object packet)
        {
            capi.Network.SendPacketClient(packet);
        }
    }
}
