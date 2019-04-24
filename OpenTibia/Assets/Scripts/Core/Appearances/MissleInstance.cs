using UnityEngine;

namespace OpenTibiaUnity.Core.Appearances
{
    public sealed class MissileInstance : AppearanceInstance
    {
        private static int s_UniqueCounter = 0;

        private readonly int m_UniqueID;
        private readonly int m_PatternX;
        private readonly int m_PatternY;
        private readonly int m_AnimationEnd = 0;
        private Vector3Int m_Target;
        private Vector3Int m_Position;

        private Vector2 m_AnimationDelta;
        private readonly Vector3 m_AnimationSpeed;

        public readonly int AnimationDirection;

        public Vector3Int Target { get => m_Target; }
        public Vector3Int Position { get => m_Position; }
        public Vector3 AnimationDelta {
            get {
                var delta = m_AnimationDelta;
                delta.x += (m_Target.x - m_Position.x) * Constants.FieldSize;
                delta.y += (m_Target.y - m_Position.y) * Constants.FieldSize;
                return delta;
            }
        }

        public MissileInstance(uint id, AppearanceType type, Vector3Int fromPosition, Vector3Int toPosition) : base(id, type) {
            m_UniqueID = s_UniqueCounter++;
            m_AnimationDelta = new Vector2Int(toPosition.x - fromPosition.x, toPosition.y - fromPosition.y);
            if (m_AnimationDelta.x == 0) {
                if (m_AnimationDelta.y <= 0) {
                    AnimationDirection = 0;
                } else {
                    AnimationDirection = 4;
                }
            } else if (m_AnimationDelta.x > 0) {
                if (256 * m_AnimationDelta.y > 618 * m_AnimationDelta.x) {
                    AnimationDirection = 4;
                } else if (256 * m_AnimationDelta.y > 106 * m_AnimationDelta.x) {
                    AnimationDirection = 3;
                } else if (256 * m_AnimationDelta.y > -106 * m_AnimationDelta.x) {
                    AnimationDirection = 2;
                } else if (256 * m_AnimationDelta.y > -618 * m_AnimationDelta.x) {
                    AnimationDirection = 1;
                } else {
                    AnimationDirection = 0;
                }
            } else if (-256 * m_AnimationDelta.y < 618 * m_AnimationDelta.x) {
                AnimationDirection = 4;
            } else if (-256 * m_AnimationDelta.y < 106 * m_AnimationDelta.x) {
                AnimationDirection = 5;
            } else if (-256 * m_AnimationDelta.y < -106 * m_AnimationDelta.x) {
                AnimationDirection = 6;
            } else if (-256 * m_AnimationDelta.y < -618 * m_AnimationDelta.x) {
                AnimationDirection = 7;
            } else {
                AnimationDirection = 0;
            }

            switch (AnimationDirection) {
                case 0:
                    m_PatternX = 1;
                    m_PatternY = 0;
                    break;
                case 1:
                    m_PatternX = 2;
                    m_PatternY = 0;
                    break;
                case 2:
                    m_PatternX = 2;
                    m_PatternY = 1;
                    break;
                case 3:
                    m_PatternX = 2;
                    m_PatternY = 2;
                    break;
                case 4:
                    m_PatternX = 1;
                    m_PatternY = 2;
                    break;
                case 5:
                    m_PatternX = 0;
                    m_PatternY = 2;
                    break;
                case 6:
                    m_PatternX = 0;
                    m_PatternY = 1;
                    break;
                case 7:
                    m_PatternX = 0;
                    m_PatternY = 0;
                    break;
            }

            float duration = Mathf.Sqrt(m_AnimationDelta.magnitude) * 150;
            m_AnimationDelta.x *= -Constants.FieldSize;
            m_AnimationDelta.y *= -Constants.FieldSize;
            m_AnimationSpeed = new Vector3(m_AnimationDelta.x, m_AnimationDelta.y, duration);
            m_AnimationEnd = OpenTibiaUnity.TicksMillis + (int)duration;
            m_Target = toPosition;
            m_Position = fromPosition;
        }

        public override int GetSpriteIndex(int layer, int patternX, int patternY, int patternZ) {
            int idleIndex = (int)Proto.Appearances.FrameGroupType.Idle;
            int phase = layer >= 0 ? (int)(layer % m_Type.FrameGroups[idleIndex].Phases) : Phase;
            return (int)(((phase * m_Type.FrameGroups[idleIndex].PatternDepth + 0)
                * m_Type.FrameGroups[idleIndex].PatternHeight + m_PatternY)
                * m_Type.FrameGroups[idleIndex].PatternWidth + m_PatternX);
        }

        public override bool Animate(int ticks, int delay = 0) {
            base.Animate(ticks, delay);

            int elapsedMillis = ticks - (m_AnimationEnd - (int)m_AnimationSpeed.z);
            if (elapsedMillis <= 0) {
                m_AnimationDelta.x = m_AnimationSpeed.x;
                m_AnimationDelta.y = m_AnimationSpeed.y;
            } else if (elapsedMillis >= m_AnimationSpeed.z) {
                m_AnimationDelta.x = 0;
                m_AnimationDelta.y = 0;
            } else {
                m_AnimationDelta.x = m_AnimationSpeed.x - (int)(m_AnimationSpeed.x / m_AnimationSpeed.z * elapsedMillis + 0.5f);
                m_AnimationDelta.y = m_AnimationSpeed.y - (int)(m_AnimationSpeed.y / m_AnimationSpeed.z * elapsedMillis + 0.5f);
            }

            if ((m_AnimationDelta.x == 0 && m_AnimationDelta.y == 0) || ticks >= m_AnimationEnd)
                return false;

            var oldPosition = m_Position;
            float mX = (m_Target.x + 1) * Constants.FieldSize - m_Type.DisplacementX + m_AnimationDelta.x;
            float mY = (m_Target.y + 1) * Constants.FieldSize - m_Type.DisplacementY + m_AnimationDelta.y;
            m_Position.x = (int)((mX - 1) / Constants.FieldSize);
            m_Position.y = (int)((mY - 1) / Constants.FieldSize);
            return true;
        }
    }
}