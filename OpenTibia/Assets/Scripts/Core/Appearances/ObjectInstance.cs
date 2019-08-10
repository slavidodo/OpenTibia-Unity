using UnityEngine;

namespace OpenTibiaUnity.Core.Appearances
{
    internal class ObjectInstance : AppearanceInstance
    {
        protected int m_Hang = 0;
        protected int m_SpecialPatternX = 0;
        protected int m_SpecialPatternY = 0;
        protected Marks m_Marks = new Marks();
        protected uint m_Data;
        protected bool m_HasSpecialPattern = false;

        internal uint Data {
            get { return m_Data; }
            set { if (m_Data != value) { m_Data = value; UpdateSpecialPattern(); } }
        }

        internal int Hang {
            get { return m_Hang; }
            set { if (m_Hang != value) { m_Hang = value; UpdateSpecialPattern(); } }
        }

        internal Marks Marks {
            get { return m_Marks; }
        }

        internal bool HasMark {
            get { return !!Marks && Marks.IsMarkSet(MarkType.Permenant); }
        }

        internal bool IsCreature {
            get { return !!Type ? Type.IsCreature : false; }
        }

        internal ObjectInstance(uint id, AppearanceType type, uint data) : base(id, type) {
            m_Data = data;
            UpdateSpecialPattern();
        }

        internal override int GetSpriteIndex(int layer, int patternX, int patternY, int patternZ) {
            patternX = m_SpecialPatternX > 0 ? m_SpecialPatternX : patternX;
            patternY = m_SpecialPatternY > 0 ? m_SpecialPatternY : patternY;
            return base.GetSpriteIndex(layer, patternX, patternY, patternZ);
        }

        internal override void DrawTo(Vector2 screenPosition, Vector2 zoom, int patternX, int patternY, int patternZ, bool highlighted = false, float highlightOpacity = 0) {
            if (m_HasSpecialPattern) {
                patternX = -1;
                patternY = -1;
            }
            
            base.DrawTo(screenPosition, zoom, patternX, patternY, patternZ, highlighted, highlightOpacity);
        }

        protected void UpdateSpecialPattern() {
            m_HasSpecialPattern = false;
            if (!m_Type || m_Type.IsCreature)
                return;

            if (m_Type.IsStackable) {
                m_HasSpecialPattern = true;
                if (m_Data < 2) {
                    m_SpecialPatternX = 0;
                    m_SpecialPatternY = 0;
                } else if (m_Data == 2) {
                    m_SpecialPatternX = 1;
                    m_SpecialPatternY = 0;
                } else if (m_Data == 3) {
                    m_SpecialPatternX = 2;
                    m_SpecialPatternY = 0;
                } else if (m_Data == 4) {
                    m_SpecialPatternX = 3;
                    m_SpecialPatternY = 0;
                } else if (m_Data < 10) {
                    m_SpecialPatternX = 0;
                    m_SpecialPatternY = 1;
                } else if (m_Data < 25) {
                    m_SpecialPatternX = 1;
                    m_SpecialPatternY = 1;
                } else if (m_Data < 50) {
                    m_SpecialPatternX = 2;
                    m_SpecialPatternY = 1;
                } else {
                    m_SpecialPatternX = 3;
                    m_SpecialPatternY = 1;
                }

                m_SpecialPatternX = m_SpecialPatternX % (int)m_Type.FrameGroups[(int)Protobuf.Shared.FrameGroupType.Idle].SpriteInfo.PatternWidth;
                m_SpecialPatternY = m_SpecialPatternY % (int)m_Type.FrameGroups[(int)Protobuf.Shared.FrameGroupType.Idle].SpriteInfo.PatternHeight;
            } else if (m_Type.IsSplash || m_Type.IsFluidContainer) {
                m_HasSpecialPattern = true;
                FluidsColor color = FluidsColor.Transparent;
                if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameNewFluids)) {
                    switch ((FluidsType)m_Data) {
                        case FluidsType.None:
                            color = FluidsColor.Transparent;
                            break;
                        case FluidsType.Water:
                            color = FluidsColor.Blue;
                            break;
                        case FluidsType.Mana:
                            color = FluidsColor.Purple;
                            break;
                        case FluidsType.Beer:
                            color = FluidsColor.Brown;
                            break;
                        case FluidsType.Oil:
                            color = FluidsColor.Brown;
                            break;
                        case FluidsType.Blood:
                            color = FluidsColor.Red;
                            break;
                        case FluidsType.Slime:
                            color = FluidsColor.Green;
                            break;
                        case FluidsType.Mud:
                            color = FluidsColor.Brown;
                            break;
                        case FluidsType.Lemonade:
                            color = FluidsColor.Yellow;
                            break;
                        case FluidsType.Milk:
                            color = FluidsColor.White;
                            break;
                        case FluidsType.Wine:
                            color = FluidsColor.Purple;
                            break;
                        case FluidsType.Health:
                            color = FluidsColor.Red;
                            break;
                        case FluidsType.Urine:
                            color = FluidsColor.Yellow;
                            break;
                        case FluidsType.Rum:
                            color = FluidsColor.Brown;
                            break;
                        case FluidsType.FruidJuice:
                            color = FluidsColor.Yellow;
                            break;
                        case FluidsType.CoconutMilk:
                            color = FluidsColor.White;
                            break;
                        case FluidsType.Tea:
                            color = FluidsColor.Brown;
                            break;
                        case FluidsType.Mead:
                            color = FluidsColor.Brown;
                            break;
                        default:
                            color = FluidsColor.Blue;
                            break;
                    }
                } else {
                    color = (FluidsColor)m_Data;
                }
                
                m_SpecialPatternX = ((int)color & 3) % (int)m_Type.FrameGroups[(int)Protobuf.Shared.FrameGroupType.Idle].SpriteInfo.PatternWidth;
                m_SpecialPatternY = ((int)color >> 2) % (int)m_Type.FrameGroups[(int)Protobuf.Shared.FrameGroupType.Idle].SpriteInfo.PatternHeight;
            } else if (m_Type.IsHangable) {
                m_HasSpecialPattern = true;
                if (m_Hang == AppearanceInstance.HookSouth) {
                    m_SpecialPatternX = m_Type.FrameGroups[(int)Protobuf.Shared.FrameGroupType.Idle].SpriteInfo.PatternWidth >= 2 ? 1 : 0;
                    m_SpecialPatternY = 0;
                } else if (m_Hang == AppearanceInstance.HookEast) {
                    m_SpecialPatternX = m_Type.FrameGroups[(int)Protobuf.Shared.FrameGroupType.Idle].SpriteInfo.PatternWidth >= 3 ? 2 : 0;
                    m_SpecialPatternY = 0;
                } else {
                    m_SpecialPatternX = 0;
                    m_SpecialPatternY = 0;
                }
            }
        }
    }
}
