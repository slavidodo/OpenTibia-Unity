namespace OpenTibiaUnity.Core.Appearances
{
    public class AppearanceType {
        protected readonly Proto.Appearances.Appearance m_ProtoAppearance;

        protected Proto.Appearances.AppearanceFlags AppearanceFlags { get => m_ProtoAppearance?.Flags; }
        public Google.Protobuf.Collections.RepeatedField<Proto.Appearances.FrameGroup> FrameGroups { get => m_ProtoAppearance?.FrameGroups; }
        public bool HasAppearanceFlags { get => AppearanceFlags != null; }
        public Proto.Appearances.Ground Ground { get => HasAppearanceFlags ? AppearanceFlags.Ground : null; }
        public bool IsGround { get => Ground != null; }
        public uint GroundSpeed { get => IsGround ? Ground.Speed : 0; }
        public bool IsGroundBorder { get => HasAppearanceFlags ? AppearanceFlags.GroundBorder : false; }
        public bool IsBottom { get => HasAppearanceFlags ? AppearanceFlags.Bottom : false; }
        public bool IsTop { get => HasAppearanceFlags ? AppearanceFlags.Top : false; }
        public bool IsContainer { get => HasAppearanceFlags ? AppearanceFlags.Container : false; }
        public bool IsStackable { get => HasAppearanceFlags ? AppearanceFlags.Stackable : false; }
        public bool IsUsable { get => HasAppearanceFlags ? AppearanceFlags.Use : false; }
        public bool IsForceUse { get => HasAppearanceFlags ? AppearanceFlags.ForceUse : false; }
        public bool IsMultiUse { get => HasAppearanceFlags ? AppearanceFlags.MultiUse : false; }
        public bool IsWritable { get => HasAppearanceFlags && AppearanceFlags.Writable != null; }
        public uint WritableLength { get => IsWritable ? AppearanceFlags.Writable.Length : 0; }
        public bool IsWritableOnce { get => HasAppearanceFlags && AppearanceFlags.WritableOnce != null; }
        public uint WritableOnceLength { get => IsWritableOnce ? AppearanceFlags.WritableOnce.Length : 0; }
        public uint MaxTextLen { get => WritableLength != 0 ? WritableLength : WritableOnceLength; }
        public bool IsFluidContainer { get => HasAppearanceFlags ? AppearanceFlags.FluidContainer : false; }
        public bool IsSplash { get => HasAppearanceFlags ? AppearanceFlags.Splash : false; }
        public bool IsUnpassable { get => HasAppearanceFlags ? AppearanceFlags.Unpassable : false; }
        public bool IsUnmovable { get => HasAppearanceFlags ? AppearanceFlags.Unmoveable : false; }
        public bool IsUnsight { get => HasAppearanceFlags ? AppearanceFlags.Unsight : false; }
        public bool IsBlockPath { get => HasAppearanceFlags ? AppearanceFlags.BlockPath : false; }
        public bool IsNoMoveAnimation { get => HasAppearanceFlags ? AppearanceFlags.NoMoveAnimation : false; }
        public bool IsPickupable { get => HasAppearanceFlags ? AppearanceFlags.Pickupable : false; }
        public bool IsHangable { get => HasAppearanceFlags ? AppearanceFlags.Hangable : false; }
        public Proto.Appearances.Hook Hook { get => HasAppearanceFlags ? AppearanceFlags.Hook : null; }
        public bool IsHook { get => Hook != null; }
        public bool IsHookSouth { get => IsHook ? Hook.Type == Proto.Appearances.HookType.South : false; }
        public bool IsHookEast { get => IsHook ? Hook.Type == Proto.Appearances.HookType.East : false; }
        public bool IsRotateable { get => HasAppearanceFlags ? AppearanceFlags.Rotateable : false; }
        public Proto.Appearances.LightInfo LightInfo { get => HasAppearanceFlags ? AppearanceFlags.Light : null; }
        public bool IsLight { get => LightInfo != null; }
        public uint LightColor { get => IsLight ? LightInfo.Color : 0; }
        public uint Brightness { get => IsLight ? LightInfo.Intensity : 0; }
        public bool IsDontHide { get => HasAppearanceFlags ? AppearanceFlags.DontHide : false; }
        public bool IsTranslucent { get => HasAppearanceFlags ? AppearanceFlags.Translucent : false; }
        public Proto.Appearances.Displacement Displacement { get => HasAppearanceFlags ? AppearanceFlags.Displacement : null; }
        public bool HasDisplacement { get => Displacement != null; }
        public uint DisplacementX { get => HasDisplacement ? Displacement.X : 0; }
        public uint DisplacementY { get => HasDisplacement ? Displacement.Y : 0; }
        public UnityEngine.Vector2Int DisplacementVector2 { get => new UnityEngine.Vector2Int((int)DisplacementX, (int)DisplacementY); }
        public bool HasElevation { get => HasAppearanceFlags && AppearanceFlags.Elevation != null; }
        public uint Elevation { get => HasElevation ? AppearanceFlags.Elevation.Elevation_ : 0; }
        public bool IsLyingCorpse { get => HasAppearanceFlags ? AppearanceFlags.LyingCorpse : false; }
        public bool IsAnimateAlways { get => HasAppearanceFlags ? AppearanceFlags.AnimateAlways : false; }
        public bool IsMiniMap { get => HasAppearanceFlags && AppearanceFlags.Minimap != null; }
        public uint MiniMapColor { get => IsMiniMap ? AppearanceFlags.Minimap.Color : 0; }
        public bool HasLensHelp { get => HasAppearanceFlags && AppearanceFlags.LensHelp != null; }
        public uint LensHelp { get => HasLensHelp ? AppearanceFlags.LensHelp.Id : 0; }
        public bool IsFullGround { get => HasAppearanceFlags ? AppearanceFlags.FullGround : false; }
        public bool IsIgnoreLook { get => HasAppearanceFlags ? AppearanceFlags.Look : false; }
        public bool IsCloth { get => HasAppearanceFlags && AppearanceFlags.Cloth != null; }
        public uint Cloth { get => IsCloth ? AppearanceFlags.Cloth.Slot : 0; }
        public Proto.Appearances.MarketInfo MarketInfo { get => HasAppearanceFlags ? AppearanceFlags.Market: null; }
        public bool IsMarket { get => MarketInfo != null; }
        public bool HasDefaultAction { get => HasAppearanceFlags && AppearanceFlags.DefaultAction != null; }
        public Proto.Appearances.PlayerAction DefaultAction { get => HasDefaultAction ? AppearanceFlags.DefaultAction.Action : Proto.Appearances.PlayerAction.None; }
        public bool IsWrappable { get => HasAppearanceFlags ? AppearanceFlags.Wrapable : false; }
        public bool IsUnwrappable { get => HasAppearanceFlags ? AppearanceFlags.UnWrapable : false; }
        public bool IsTopEffect { get => HasAppearanceFlags ? AppearanceFlags.TopEffect : false; }

        public bool IsAnimation { get; }
        public bool IsCachable { get; }
        
        public uint ID { get; }
        public bool IsCreature { get => ID == AppearanceInstance.Creature; }
        public AppearanceCategory Category { get; }
        public int IdleAnimationPhases { get; } = 0;
        public int WalkingAnimationPhases { get; } = 0;
        public int AnimationPhases { get; }

        public AppearanceType(uint id, Proto.Appearances.Appearance appearance, AppearanceCategory category) {
            ID = id;
            Category = category;
            m_ProtoAppearance = appearance;
            
            if (FrameGroups != null) {
                bool animation = false;
                int exactSize = 0;
                for (int i = 0; i < FrameGroups.Count; i++) {
                    var frameGroup = FrameGroups[i];
                    animation = animation || frameGroup.IsAnimation;
                    exactSize = System.Math.Max(exactSize, (int)frameGroup.ExactSize);

                    if (i == (int)Proto.Appearances.FrameGroupType.Idle)
                        IdleAnimationPhases = (int)frameGroup.Phases;
                    else if (i == (int)Proto.Appearances.FrameGroupType.Walking)
                        WalkingAnimationPhases = (int)frameGroup.Phases;
                }

                IsAnimation = animation;
                IsCachable = !IsAnimation && !IsHangable && !IsLight && exactSize + System.Math.Max(DisplacementX, DisplacementY) <= Constants.FieldCacheSize;
            } else {
                IsAnimation = false;
                IsCachable = false;
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
