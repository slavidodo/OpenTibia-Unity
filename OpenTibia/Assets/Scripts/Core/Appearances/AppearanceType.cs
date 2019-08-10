namespace OpenTibiaUnity.Core.Appearances
{
    internal class AppearanceType {
        internal readonly Protobuf.Appearances.Appearance ProtoAppearance;

        internal string Name { get => ProtoAppearance?.Name; }
        protected Protobuf.Appearances.AppearanceFlags AppearanceFlags { get => ProtoAppearance?.Flags; }
        internal Google.Protobuf.Collections.RepeatedField<Protobuf.Appearances.FrameGroup> FrameGroups { get => ProtoAppearance?.FrameGroups; }
        internal bool HasAppearanceFlags { get => AppearanceFlags != null; }
        internal Protobuf.Appearances.AppearanceFlagGround Ground { get => HasAppearanceFlags ? AppearanceFlags.Ground : null; }
        internal bool IsGround { get => Ground != null; }
        internal uint GroundSpeed { get => IsGround ? Ground.Speed : 0; }
        internal bool IsGroundBorder { get => HasAppearanceFlags ? AppearanceFlags.GroundBorder : false; }
        internal bool IsBottom { get => HasAppearanceFlags ? AppearanceFlags.Bottom : false; }
        internal bool IsTop { get => HasAppearanceFlags ? AppearanceFlags.Top : false; }
        internal bool IsContainer { get => HasAppearanceFlags ? AppearanceFlags.Container : false; }
        internal bool IsStackable { get => HasAppearanceFlags ? AppearanceFlags.Stackable : false; }
        internal bool IsUsable { get => HasAppearanceFlags ? AppearanceFlags.Use : false; }
        internal bool IsForceUse { get => HasAppearanceFlags ? AppearanceFlags.ForceUse : false; }
        internal bool IsMultiUse { get => HasAppearanceFlags ? AppearanceFlags.MultiUse : false; }
        internal bool IsWritable { get => HasAppearanceFlags && AppearanceFlags.Writable != null; }
        internal uint WritableLength { get => IsWritable ? AppearanceFlags.Writable.MaxTextLength : 0; }
        internal bool IsWritableOnce { get => HasAppearanceFlags && AppearanceFlags.WritableOnce != null; }
        internal uint WritableOnceLength { get => IsWritableOnce ? AppearanceFlags.WritableOnce.MaxTextLengthOnce : 0; }
        internal uint MaxTextLen { get => WritableLength != 0 ? WritableLength : WritableOnceLength; }
        internal bool IsFluidContainer { get => HasAppearanceFlags ? AppearanceFlags.FluidContainer : false; }
        internal bool IsSplash { get => HasAppearanceFlags ? AppearanceFlags.Splash : false; }
        internal bool IsUnpassable { get => HasAppearanceFlags ? AppearanceFlags.Unpassable : false; }
        internal bool IsUnmovable { get => HasAppearanceFlags ? AppearanceFlags.Unmoveable : false; }
        internal bool IsUnsight { get => HasAppearanceFlags ? AppearanceFlags.Unsight : false; }
        internal bool IsBlockPath { get => HasAppearanceFlags ? AppearanceFlags.BlockPath : false; }
        internal bool IsNoMoveAnimation { get => HasAppearanceFlags ? AppearanceFlags.NoMoveAnimation : false; }
        internal bool IsPickupable { get => HasAppearanceFlags ? AppearanceFlags.Pickupable : false; }
        internal bool IsHangable { get => HasAppearanceFlags ? AppearanceFlags.Hangable : false; }
        internal Protobuf.Appearances.AppearanceFlagHook Hook { get => HasAppearanceFlags ? AppearanceFlags.Hook : null; }
        internal bool IsHook { get => Hook != null; }
        internal bool IsHookSouth { get => IsHook ? Hook.Type == Protobuf.Shared.HookType.South : false; }
        internal bool IsHookEast { get => IsHook ? Hook.Type == Protobuf.Shared.HookType.East : false; }
        internal bool IsRotateable { get => HasAppearanceFlags ? AppearanceFlags.Rotateable : false; }
        internal Protobuf.Appearances.AppearanceFlagLight Light { get => HasAppearanceFlags ? AppearanceFlags.Light : null; }
        internal bool IsLight { get => Light != null; }
        internal uint LightColor { get => IsLight ? Light.Color : 0; }
        internal uint Brightness { get => IsLight ? Light.Intensity : 0; }
        internal bool IsDontHide { get => HasAppearanceFlags ? AppearanceFlags.DontHide : false; }
        internal bool IsTranslucent { get => HasAppearanceFlags ? AppearanceFlags.Translucent : false; }
        private Protobuf.Appearances.AppearanceFlagOffset m_Offset { get => HasAppearanceFlags ? AppearanceFlags.Offset : null; }
        internal bool HasDisplacement { get => m_Offset != null; }
        internal uint OffsetX { get => HasDisplacement ? m_Offset.X : 0; }
        internal uint OffsetY { get => HasDisplacement ? m_Offset.Y : 0; }
        internal UnityEngine.Vector2Int Offset { get => new UnityEngine.Vector2Int((int)OffsetX, (int)OffsetY); }
        internal bool HasElevation { get => HasAppearanceFlags && AppearanceFlags.Elevation != null; }
        internal uint Elevation { get => HasElevation ? AppearanceFlags.Elevation.Elevation : 0; }
        internal bool IsLyingCorpse { get => HasAppearanceFlags ? AppearanceFlags.LyingCorpse : false; }
        internal bool IsAnimateAlways { get => HasAppearanceFlags ? AppearanceFlags.AnimateAlways : false; }
        internal bool IsMiniMap { get => HasAppearanceFlags && AppearanceFlags.Minimap != null; }
        internal uint MiniMapColor { get => IsMiniMap ? AppearanceFlags.Minimap.Color : 0; }
        internal bool HasLensHelp { get => HasAppearanceFlags && AppearanceFlags.LensHelp != null; }
        internal uint LensHelp { get => HasLensHelp ? AppearanceFlags.LensHelp.ID : 0; }
        internal bool IsFullGround { get => HasAppearanceFlags ? AppearanceFlags.FullGround : false; }
        internal bool IsIgnoreLook { get => HasAppearanceFlags ? AppearanceFlags.Look : false; }
        internal bool IsCloth { get => HasAppearanceFlags && AppearanceFlags.Cloth != null; }
        internal uint Cloth { get => IsCloth ? AppearanceFlags.Cloth.Slot : 0; }
        internal Protobuf.Appearances.AppearanceFlagMarket Market { get => HasAppearanceFlags ? AppearanceFlags.Market: null; }
        internal bool IsMarket { get => Market != null; }
        internal bool HasDefaultAction { get => HasAppearanceFlags && AppearanceFlags.DefaultAction != null; }
        internal Protobuf.Shared.PlayerAction DefaultAction { get => HasDefaultAction ? AppearanceFlags.DefaultAction.Action : Protobuf.Shared.PlayerAction.ActionNone; }
        internal bool IsWrappable { get => HasAppearanceFlags ? AppearanceFlags.Wrapable : false; }
        internal bool IsUnwrappable { get => HasAppearanceFlags ? AppearanceFlags.UnWrapable : false; }
        internal bool IsTopEffect { get => HasAppearanceFlags ? AppearanceFlags.TopEffect : false; }
        // TODO NpcSaleData
        // TODO ExpiringObject
        internal bool IsCorpse { get => HasAppearanceFlags ? false : false; } // TODO
        internal bool IsPlayerCorpse { get => HasAppearanceFlags ? false : false; } // TODO

        internal bool IsAnimation { get; }
        internal bool IsCachable { get; }
        
        internal uint ID { get; }
        internal bool IsCreature { get => ID == AppearanceInstance.Creature; }
        internal AppearanceCategory Category { get; }
        internal int IdleAnimationPhases { get; } = 0;
        internal int WalkingAnimationPhases { get; } = 0;
        internal int AnimationPhases { get; }

        internal int BoundingSquare { get; }

        internal AppearanceType(uint id, Protobuf.Appearances.Appearance appearance, AppearanceCategory category) {
            ID = id;
            Category = category;
            ProtoAppearance = appearance;
            BoundingSquare = 0;
            IsAnimation = false;
            IsCachable = false;

            if (FrameGroups != null) {
                for (int i = 0; i < FrameGroups.Count; i++) {
                    var spriteInfo = FrameGroups[i].SpriteInfo;
                    IsAnimation = IsAnimation || spriteInfo.IsAnimation;
                    BoundingSquare = System.Math.Max(BoundingSquare, (int)spriteInfo.BoundingSquare);

                    if (i == (int)Protobuf.Shared.FrameGroupType.Idle)
                        IdleAnimationPhases = (int)spriteInfo.Phases;
                    else if (i == (int)Protobuf.Shared.FrameGroupType.Walking)
                        WalkingAnimationPhases = (int)spriteInfo.Phases;
                }
                
                IsCachable = !IsAnimation && !IsHangable && !IsLight && BoundingSquare + System.Math.Max(OffsetX, OffsetY) <= Constants.FieldCacheSize;
            }

            AnimationPhases = IdleAnimationPhases + WalkingAnimationPhases;
        }

        public static bool operator !(AppearanceType instance) {
            return instance == null;
        }

        public static bool operator true(AppearanceType instance) {
            return !!instance;
        }

        public static bool operator false(AppearanceType instance) {
            return !instance;
        }
    }
}
