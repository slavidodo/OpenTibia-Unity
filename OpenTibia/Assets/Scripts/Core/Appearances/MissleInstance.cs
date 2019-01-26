namespace OpenTibiaUnity.Core.Appearances
{
    public sealed class MissileInstance : AppearanceInstance
    {
        private int m_PatternX;
        private int m_PatternY;
        private int m_AnimationEnd = 0;
        private UnityEngine.Vector3Int m_Target;
        private UnityEngine.Vector3Int m_Position;
        private UnityEngine.Vector3Int m_AnimationDelta;
        private UnityEngine.Vector3Int m_AnimationSpeed;

        public int AnimationDirection { get; }

        public UnityEngine.Vector3Int Target { get { return m_Target; } }
        public UnityEngine.Vector3Int Position { get { return m_Position; } }
        public UnityEngine.Vector3Int AnimationDelta {
            get {
                var delta = m_AnimationDelta;
                delta.x = (m_Target.x - m_Position.x) * Constants.FieldSize;
                delta.y = (m_Target.y - m_Position.y) * Constants.FieldSize;
                return delta;
            }
        }

        public MissileInstance(uint id, AppearanceType type, UnityEngine.Vector3Int fromPosition, UnityEngine.Vector3Int toPosition) : base(id, type) {
            m_AnimationDelta = toPosition - fromPosition;
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

            double animationMagnitude = System.Math.Sqrt(m_AnimationDelta.x * m_AnimationDelta.x + m_AnimationDelta.y * m_AnimationDelta.y);
            double loc6 = System.Math.Sqrt(animationMagnitude) * 150;
            m_AnimationDelta.x = m_AnimationDelta.x * -Constants.FieldSize;
            m_AnimationDelta.y = m_AnimationDelta.y * -Constants.FieldSize;
            m_AnimationDelta.z = 0;
            m_AnimationSpeed = new UnityEngine.Vector3Int(m_AnimationDelta.x, m_AnimationDelta.y, (int)loc6);
            m_AnimationEnd = OpenTibiaUnity.TicksMillis + (int)loc6;
            m_Target = toPosition;
            m_Position = fromPosition;
        }

        public override int GetSpriteIndex(int layer, int patternX, int patternY, int patternZ) {
            int phase = layer >= 0 ? (int)(layer % m_Type.FrameGroups[(int)Proto.Appearances001.FrameGroupType.Idle].Phases) : Phase;
            return (int)(((phase * m_Type.FrameGroups[(int)Proto.Appearances001.FrameGroupType.Idle].PatternDepth + 0)
                * m_Type.FrameGroups[(int)Proto.Appearances001.FrameGroupType.Idle].PatternHeight + m_PatternY)
                * m_Type.FrameGroups[(int)Proto.Appearances001.FrameGroupType.Idle].PatternWidth + m_PatternX);
        }

        public override bool Animate(int ticks, int delay = 0) {
            base.Animate(ticks, delay);

            int diff = ticks - (m_AnimationEnd - m_AnimationSpeed.z);
            if (diff <= 0) {
                m_AnimationDelta.x = m_AnimationSpeed.x;
                m_AnimationDelta.y = m_AnimationSpeed.y;
            } else if (diff >= m_AnimationSpeed.z) {
                m_AnimationDelta.x = 0;
                m_AnimationDelta.y = 0;
            } else {
                m_AnimationDelta.x -= (int)(m_AnimationSpeed.x / m_AnimationSpeed.z * diff + 0.5f);
                m_AnimationDelta.y -= (int)(m_AnimationSpeed.y / m_AnimationSpeed.z * diff + 0.5f);
            }

            if (m_AnimationDelta.x == 0 && m_AnimationDelta.y == 0 || ticks >= m_AnimationEnd)
                return false;

            float _loc4_ = (Target.x + 1) * Constants.FieldSize - m_Type.DisplacementX + m_AnimationDelta.x;
            float _loc5_ = (Target.y + 1) * Constants.FieldSize - m_Type.DisplacementY + m_AnimationDelta.y;
            m_Position.x = (int)((_loc4_ - 1) / Constants.FieldSize);
            m_Position.y = (int)((_loc5_ - 1) / Constants.FieldSize);
            return true;
        }
    }
}