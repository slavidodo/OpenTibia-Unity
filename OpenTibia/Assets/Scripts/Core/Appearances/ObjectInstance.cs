using UnityEngine;

namespace OpenTibiaUnity.Core.Appearances
{
    public class ObjectInstance : AppearanceInstance
    {
        protected int m_Hang = 0;
        protected int m_SpecialPatternX = 0;
        protected int m_SpecialPatternY = 0;
        protected Marks m_Marks = new Marks();
        protected uint m_Data;
        protected bool m_HasSpecialPattern = false;

        public uint Data {
            get { return m_Data; }
            set { if (m_Data != value) { m_Data = value; UpdateSpecialPattern(); } }
        }

        public int Hang {
            get { return m_Hang; }
            set { if (m_Hang != value) { m_Hang = value; UpdateSpecialPattern(); } }
        }

        public Marks Marks {
            get { return m_Marks; }
        }

        public bool HasMark {
            get { return !!Marks && Marks.IsMarkSet(Marks.MarkType_Permenant); }
        }

        public bool IsCreature {
            get { return !!Type ? Type.IsCreature : false; }
        }

        public ObjectInstance(uint id, AppearanceType type, uint data) : base(id, type) {
            m_Data = data;
            UpdateSpecialPattern();
        }

        public override int GetSpriteIndex(int _, int patternX, int patternY, int patternZ) {
            patternX = m_SpecialPatternX > 0 ? m_SpecialPatternX : patternX;
            patternY = m_SpecialPatternY > 0 ? m_SpecialPatternY : patternY;
            return base.GetSpriteIndex(_, patternX, patternY, patternZ);
        }

        public override void DrawTo(Vector2 screenPosition, Vector2 zoom, int patternX, int patternY, int patternZ) {
            int tmpPatternX = patternX;
            int tmpPatternY = patternY;
            if (m_HasSpecialPattern) {
                tmpPatternX = -1;
                tmpPatternY = -1;
            }

            var cachedInformation = GetSprite(-1, tmpPatternX, tmpPatternY, patternZ, m_Type.FrameGroups[m_ActiveFrameGroup].IsAnimation);
            InternalDrawTo(screenPosition.x, screenPosition.y, zoom, cachedInformation);
        }

        protected void UpdateSpecialPattern() {
            m_HasSpecialPattern = false;
            if (!m_Type || m_ID == AppearanceInstance.Creature)
                return;

            if (m_Type.IsCulmative) {
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

                m_SpecialPatternX = m_SpecialPatternX % (int)m_Type.FrameGroups[(int)Proto.Appearances001.FrameGroupType.Idle].PatternWidth;
                m_SpecialPatternY = m_SpecialPatternY % (int)m_Type.FrameGroups[(int)Proto.Appearances001.FrameGroupType.Idle].PatternHeight;
            } else if (m_Type.IsLiquidPool || m_Type.IsLiquidContainer) {
                m_HasSpecialPattern = true;
                int color = 0;
                switch (m_Data) {
                    case 0:
                        color = 0;
                        break;
                    case 1:
                        color = 1;
                        break;
                    case 2:
                        color = 7;
                        break;
                    case 3:
                        color = 3;
                        break;
                    case 4:
                        color = 3;
                        break;
                    case 5:
                        color = 2;
                        break;
                    case 6:
                        color = 4;
                        break;
                    case 7:
                        color = 3;
                        break;
                    case 8:
                        color = 5;
                        break;
                    case 9:
                        color = 6;
                        break;
                    case 10:
                        color = 7;
                        break;
                    case 11:
                        color = 2;
                        break;
                    case 12:
                        color = 5;
                        break;
                    case 13:
                        color = 3;
                        break;
                    case 14:
                        color = 5;
                        break;
                    case 15:
                        color = 6;
                        break;
                    case 16:
                        color = 3;
                        break;
                    case 17:
                        color = 3;
                        break;
                    default:
                        color = 1;
                        break;
                }

                m_SpecialPatternX = (color & 3) % (int)m_Type.FrameGroups[(int)Proto.Appearances001.FrameGroupType.Idle].PatternWidth;
                m_SpecialPatternY = (color >> 2) % (int)m_Type.FrameGroups[(int)Proto.Appearances001.FrameGroupType.Idle].PatternHeight;
            } else if (m_Type.IsHangable) {
                m_HasSpecialPattern = true;
                if (m_Hang == AppearanceInstance.HookSouth) {
                    m_SpecialPatternX = m_Type.FrameGroups[(int)Proto.Appearances001.FrameGroupType.Idle].PatternWidth >= 2 ? 1 : 0;
                    m_SpecialPatternY = 0;
                } else if (m_Hang == AppearanceInstance.HookEast) {
                    m_SpecialPatternX = m_Type.FrameGroups[(int)Proto.Appearances001.FrameGroupType.Idle].PatternWidth >= 3 ? 2 : 0;
                    m_SpecialPatternY = 0;
                } else {
                    m_SpecialPatternX = 0;
                    m_SpecialPatternY = 0;
                }
            }
        }


    }
}
