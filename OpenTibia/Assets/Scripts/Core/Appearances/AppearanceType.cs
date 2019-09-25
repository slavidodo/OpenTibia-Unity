namespace OpenTibiaUnity.Core.Appearances
{
    public class AppearanceType {
        public readonly Protobuf.Appearances.Appearance ProtoAppearance;

        public string Name { get => ProtoAppearance?.Name; }
        protected Protobuf.Appearances.AppearanceFlags AppearanceFlags { get => ProtoAppearance?.Flags; }
        public Google.Protobuf.Collections.RepeatedField<Protobuf.Appearances.FrameGroup> FrameGroups { get => ProtoAppearance?.FrameGroups; }
        public bool HasAppearanceFlags { get => AppearanceFlags != null; }
        public Protobuf.Appearances.AppearanceFlagGround Ground { get => HasAppearanceFlags ? AppearanceFlags.Ground : null; }
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
        public uint WritableLength { get => IsWritable ? AppearanceFlags.Writable.MaxTextLength : 0; }
        public bool IsWritableOnce { get => HasAppearanceFlags && AppearanceFlags.WritableOnce != null; }
        public uint WritableOnceLength { get => IsWritableOnce ? AppearanceFlags.WritableOnce.MaxTextLengthOnce : 0; }
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
        public Protobuf.Appearances.AppearanceFlagHook Hook { get => HasAppearanceFlags ? AppearanceFlags.Hook : null; }
        public bool IsHook { get => Hook != null; }
        public bool IsHookSouth { get => IsHook ? Hook.Type == Protobuf.Shared.HookType.South : false; }
        public bool IsHookEast { get => IsHook ? Hook.Type == Protobuf.Shared.HookType.East : false; }
        public bool IsRotateable { get => HasAppearanceFlags ? AppearanceFlags.Rotateable : false; }
        public Protobuf.Appearances.AppearanceFlagLight Light { get => HasAppearanceFlags ? AppearanceFlags.Light : null; }
        public bool IsLight { get => Light != null; }
        public uint LightColor { get => IsLight ? Light.Color : 0; }
        public uint Brightness { get => IsLight ? Light.Intensity : 0; }
        public bool IsDontHide { get => HasAppearanceFlags ? AppearanceFlags.DontHide : false; }
        public bool IsTranslucent { get => HasAppearanceFlags ? AppearanceFlags.Translucent : false; }
        private Protobuf.Appearances.AppearanceFlagOffset _offset { get => HasAppearanceFlags ? AppearanceFlags.Offset : null; }
        public bool HasDisplacement { get => _offset != null; }
        public uint OffsetX { get => HasDisplacement ? _offset.X : 0; }
        public uint OffsetY { get => HasDisplacement ? _offset.Y : 0; }
        public UnityEngine.Vector2Int Offset { get => new UnityEngine.Vector2Int((int)OffsetX, (int)OffsetY); }
        public bool HasElevation { get => HasAppearanceFlags && AppearanceFlags.Height != null; }
        public uint Elevation { get => HasElevation ? AppearanceFlags.Height.Elevation : 0; }
        public bool IsLyingCorpse { get => HasAppearanceFlags ? AppearanceFlags.LyingCorpse : false; }
        public bool IsAnimateAlways { get => HasAppearanceFlags ? AppearanceFlags.AnimateAlways : false; }
        public bool IsAutomap { get => HasAppearanceFlags && AppearanceFlags.Automap != null; }
        public uint AutomapColor { get => IsAutomap ? AppearanceFlags.Automap.Color : 0; }
        public bool HasLensHelp { get => HasAppearanceFlags && AppearanceFlags.LensHelp != null; }
        public uint LensHelp { get => HasLensHelp ? AppearanceFlags.LensHelp.ID : 0; }
        public bool IsFullGround { get => HasAppearanceFlags ? AppearanceFlags.FullGround : false; }
        public bool IsIgnoreLook { get => HasAppearanceFlags ? AppearanceFlags.IgnoreLook : false; }
        public bool IsCloth { get => HasAppearanceFlags && AppearanceFlags.Clothes != null; }
        public uint Cloth { get => IsCloth ? AppearanceFlags.Clothes.Slot : 0; }
        public Protobuf.Appearances.AppearanceFlagMarket Market { get => HasAppearanceFlags ? AppearanceFlags.Market: null; }
        public bool IsMarket { get => Market != null; }
        public bool HasDefaultAction { get => HasAppearanceFlags && AppearanceFlags.DefaultAction != null; }
        public Protobuf.Shared.PlayerAction DefaultAction { get => HasDefaultAction ? AppearanceFlags.DefaultAction.Action : Protobuf.Shared.PlayerAction.ActionNone; }
        public bool IsWrappable { get => HasAppearanceFlags ? AppearanceFlags.Wrapable : false; }
        public bool IsUnwrappable { get => HasAppearanceFlags ? AppearanceFlags.UnWrapable : false; }
        public bool IsTopEffect { get => HasAppearanceFlags ? AppearanceFlags.TopEffect : false; }
        public Google.Protobuf.Collections.RepeatedField<Protobuf.Appearances.AppearanceFlagNPC> NPCSaleData {
            get => HasAppearanceFlags ? AppearanceFlags.NpcSaleData : null;
        }
        public bool IsExpiredChanged { get => HasAppearanceFlags && AppearanceFlags.ChangedToExpire != null; }
        public uint FormerObjectTypeId { get => IsExpiredChanged ? AppearanceFlags.ChangedToExpire.FormerObjectTypeID : 0; }
        public bool IsCorpse { get => HasAppearanceFlags && AppearanceFlags.Corpse; }
        public bool IsPlayerCorpse { get => HasAppearanceFlags && AppearanceFlags.PlayerCorpse; }
        public bool IsCyclopediaType { get => HasAppearanceFlags && AppearanceFlags.CyclopediaItem != null; }
        public uint CyclopediaObjectTypeId { get => IsCyclopediaType ? AppearanceFlags.CyclopediaItem.CyclopediaType : 0; }

        public bool IsAnimation { get; }
        public bool IsCachable { get; }
        
        public uint Id { get; }
        public bool IsCreature { get => Id == AppearanceInstance.Creature; }
        public AppearanceCategory Category { get; }
        public int IdleAnimationPhases { get; } = 0;
        public int WalkingAnimationPhases { get; } = 0;
        public int AnimationPhases { get; }

        public int BoundingSquare { get; }

        public AppearanceType(uint id, Protobuf.Appearances.Appearance appearance, AppearanceCategory category) {
            Id = id;
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
