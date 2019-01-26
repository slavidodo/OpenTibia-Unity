using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Appearances
{
    public class AppearanceType {
        Proto.Appearances001.Appearance m_ProtoAppearance;
        
        protected Proto.Appearances001.AppearanceFlags AppearanceFlags {
            get {
                return m_ProtoAppearance != null ? m_ProtoAppearance.Flags : null;
            }
        }

        public Google.Protobuf.Collections.RepeatedField<Proto.Appearances001.FrameGroup> FrameGroups {
            get {
                return m_ProtoAppearance != null ? m_ProtoAppearance.FrameGroups : null;
            }
        }

        public bool HasAppearanceFlags {
            get => AppearanceFlags != null;
        }

        public uint Ground {
            get => HasAppearanceFlags ? AppearanceFlags.Ground : 0;
        }
        
        public bool IsGroundBorder {
            get => HasAppearanceFlags ? AppearanceFlags.GroundBorder : false;
        }

        public bool IsBottom {
            get { return HasAppearanceFlags ? AppearanceFlags.OnBottom : false; }
        }

        public bool IsTop {
            get { return HasAppearanceFlags ? AppearanceFlags.OnTop : false; }
        }

        public bool IsContainer {
            get { return HasAppearanceFlags ? AppearanceFlags.Container : false; }
        }

        public bool IsStackable {
            get { return HasAppearanceFlags ? AppearanceFlags.Stackable : false; }
        }

        public bool IsForceUse {
            get { return HasAppearanceFlags ? AppearanceFlags.Stackable : false; }
        }

        public bool IsMultiUse {
            get { return HasAppearanceFlags ? AppearanceFlags.MultiUse : false; }
        }

        public uint Writable {
            get { return HasAppearanceFlags ? AppearanceFlags.Writable : 0; }
        }

        public uint WritableOnce {
            get { return HasAppearanceFlags ? AppearanceFlags.WritableOnce : 0; }
        }

        public bool IsFluidContainer {
            get { return HasAppearanceFlags ? AppearanceFlags.FluidContainer : false; }
        }

        public bool IsSplash {
            get { return HasAppearanceFlags ? AppearanceFlags.Splash : false; }
        }

        public bool IsNotWalkable {
            get { return HasAppearanceFlags ? AppearanceFlags.NotWalkable : false; }
        }

        public bool IsNotMoveable {
            get { return HasAppearanceFlags ? AppearanceFlags.NotMoveable : false; }
        }

        public bool IsBlockProjectile {
            get { return HasAppearanceFlags ? AppearanceFlags.BlockProjectile : false; }
        }

        public bool IsNotPathable {
            get { return HasAppearanceFlags ? AppearanceFlags.NotPathable : false; }
        }

        public bool IsNoMoveAnimation {
            get { return HasAppearanceFlags ? AppearanceFlags.NoMoveAnimation : false; }
        }

        public bool IsPickupable {
            get { return HasAppearanceFlags ? AppearanceFlags.Pickupable : false; }
        }

        public bool IsHangable {
            get { return HasAppearanceFlags ? AppearanceFlags.Hangable : false; }
        }

        public bool IsHookSouth {
            get { return HasAppearanceFlags ? AppearanceFlags.HookSouth : false; }
        }

        public bool IsHookEast {
            get { return HasAppearanceFlags ? AppearanceFlags.HookEast : false; }
        }

        public bool IsRotateable {
            get { return HasAppearanceFlags ? AppearanceFlags.Rotateable : false; }
        }

        public Proto.Appearances001.LightInfo LightInfo {
            get { return HasAppearanceFlags ? AppearanceFlags.Light : null; }
        }

        public bool IsDontHide {
            get { return HasAppearanceFlags ? AppearanceFlags.DontHide : false; }
        }

        public bool IsTranslucent {
            get { return HasAppearanceFlags ? AppearanceFlags.Translucent : false; }
        }

        private UnityEngine.Vector2Int m_Displacement = UnityEngine.Vector2Int.zero;
        public UnityEngine.Vector2Int Displacement {
            get { return m_Displacement; }
        }
        
        public uint Elevation {
            get { return HasAppearanceFlags ? AppearanceFlags.Elevation : 0; }
        }

        public bool IsLyingCorpse {
            get { return HasAppearanceFlags ? AppearanceFlags.LyingCorpse : false; }
        }

        public bool IsAnimateAlways {
            get { return HasAppearanceFlags ? AppearanceFlags.AnimateAlways : false; }
        }

        public uint MiniMapColor {
            get { return HasAppearanceFlags ? AppearanceFlags.MiniMapColor : 0; }
        }

        public uint LensHelp {
            get { return HasAppearanceFlags ? AppearanceFlags.LensHelp : 0; }
        }

        public bool IsFullGround {
            get { return HasAppearanceFlags ? AppearanceFlags.FullGround : false; }
        }

        public bool IsIgnoreLook {
            get { return HasAppearanceFlags ? AppearanceFlags.Look : false; }
        }

        public uint Cloth {
            get { return HasAppearanceFlags ? AppearanceFlags.Cloth : 0; }
        }

        public Proto.Appearances001.MarketInfo MarketInfo {
            get { return HasAppearanceFlags ? AppearanceFlags.Market: null; }
        }

        public uint DefaultAction {
            get { return HasAppearanceFlags ? AppearanceFlags.DefaultAction : 0; }
        }

        public bool IsWrappable {
            get { return HasAppearanceFlags ? AppearanceFlags.Wrapable : false; }
        }

        public bool IsUnwrappable {
            get { return HasAppearanceFlags ? AppearanceFlags.UnWrapable : false; }
        }

        public bool IsTopEffect {
            get { return HasAppearanceFlags ? AppearanceFlags.TopEffect : false; }
        }

        public bool IsUsable {
            get { return HasAppearanceFlags ? AppearanceFlags.Usable : false; }
        }

        // Helper
        public bool IsAnimation { get; private set; }
        public bool IsCachable { get; private set; }
        //

        public uint Waypoints { get { return Ground; } }
        public bool IsGround { get { return Ground != 0; } }
        public bool IsBank { get { return IsGround; } }
        public bool IsClip { get { return IsGroundBorder; } }
        public bool IsWritable { get { return Writable != 0; } }
        public bool IsWritableOnce { get { return WritableOnce != 0; } }
        public uint MaxTextLen { get { return Writable != 0 ? Writable : WritableOnce; } }
        public bool IsCulmative { get { return IsStackable; } }
        public bool IsLiquidContainer { get { return IsFluidContainer; } }
        public bool IsLiquidPool { get { return IsSplash; } }
        public bool IsUnpassable {  get { return IsNotWalkable; } }
        public bool IsUnmoveable {  get { return IsNotMoveable; } }
        public bool IsUnsight { get { return IsBlockProjectile; } }
        public bool IsAvoid { get { return IsNotPathable; } }
        public bool PreventMoveAnimation { get { return IsNoMoveAnimation; } }
        public bool IsTakeable { get { return IsPickupable; } }
        public bool IsLight { get { return LightInfo != null; } }
        public uint LightColor { get { return IsLight ? LightInfo.Color : 0; } }
        public uint Brightness { get { return IsLight ? LightInfo.Intensity : 0; } }
        public int DisplacementX { get { return m_Displacement.x; } }
        public int DisplacementY { get { return m_Displacement.y; } }
        public bool IsLyingObject { get { return IsLyingCorpse; } }
        public bool IsAutoMap { get { return MiniMapColor != 0; } }
        public uint AutoMapColor { get { return MiniMapColor; } }
        public bool IsFullBank { get { return IsFullGround; } }
        public bool IsCloth { get { return Cloth != 0; } }
        public bool IsMarket { get { return MarketInfo != null; } }

        public bool IsCreature {
            get {
                return ID == AppearanceInstance.Creature || ID == AppearanceInstance.OutdatedCreature || ID == AppearanceInstance.UnknownCreature;
            }
        }

        public uint ID { get; }

        public AppearanceType(uint id, Proto.Appearances001.Appearance appearance) {
            ID = id;
            m_ProtoAppearance = appearance;

            if (HasAppearanceFlags && AppearanceFlags.Displacement != null) {
                m_Displacement = new UnityEngine.Vector2Int() {
                    x = (int)appearance.Flags.Displacement.X,
                    y = (int)appearance.Flags.Displacement.Y
                };
            }

            if (m_ProtoAppearance != null && m_ProtoAppearance.FrameGroups != null) {
                bool animation = false;
                int exactSize = 0;
                foreach (var fg in m_ProtoAppearance.FrameGroups) {
                    animation = animation || fg.IsAnimation;
                    exactSize = System.Math.Max(exactSize, (int)fg.ExactSize);
                }

                IsAnimation = animation;

                // TODO, this is used to cache a whole tile (using texture blit)
                IsCachable = !IsAnimation && !IsHangable && !IsLight && exactSize + System.Math.Max(DisplacementX, DisplacementY) <= Constants.FieldCacheSize;
            } else {
                IsCachable = false;
            }
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
