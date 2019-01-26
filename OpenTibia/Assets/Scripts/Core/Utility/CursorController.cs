using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace OpenTibiaUnity.Core.Utility
{
    [ExecuteInEditMode]
    public class CursorController : MonoBehaviour
    {
#pragma warning disable CS0649 // never assigned to
        [Tooltip("Animation frame between each sprite in seconds")]
        [SerializeField] private float m_DefaultAnimationTick = 0.15f;

        [SerializeField] private Texture2D[] m_DefaultTextures;
        [SerializeField] private Texture2D[] m_DefaultDisabledTextures;
        [SerializeField] private Texture2D[] m_NResizeTextures;
        [SerializeField] private Texture2D[] m_EResizeTextures;
        [SerializeField] private Texture2D[] m_HandTextures;
        [SerializeField] private Texture2D[] m_CrosshairTextures;
        [SerializeField] private Texture2D[] m_CrosshairDisabledTextures;
        [SerializeField] private Texture2D[] m_ScanTextures;
        [SerializeField] private Texture2D[] m_AttackTextures;
        [SerializeField] private Texture2D[] m_WalkTextures;
        [SerializeField] private Texture2D[] m_UseTextures;
        [SerializeField] private Texture2D[] m_TalkTextures;
        [SerializeField] private Texture2D[] m_LookTextures;
        [SerializeField] private Texture2D[] m_OpenTextures;
        [SerializeField] private Texture2D[] m_LootTextures;
#pragma warning restore CS0649 // never assigned to

        private bool m_CustomCursor = false;
        private bool m_SequenceAnimation = false;
        private float m_LastAnimationTime = 0;
        private float m_AnimationTick = 0;
        private int m_AnimationFrame = 0;
        private CursorState m_CursorState = CursorState.Default;
        private CursorPriority m_CursorPriority = CursorPriority.Low;
        private Texture2D[] m_CurrentTextures = null;
        private List<Tuple<CursorState, CursorPriority>> m_CursorStack;

        private void Awake() {
            m_CursorStack = new List<Tuple<CursorState, CursorPriority>>();
        }

        public void Start() {
            SetCursorState(CursorState.Default, CursorPriority.Low);
        }

        private void Update() {
            if (m_CustomCursor && m_SequenceAnimation) {
                if (m_LastAnimationTime == 0 || Time.time - m_LastAnimationTime > m_AnimationTick) {
                    m_LastAnimationTime = Time.time;

                    Cursor.SetCursor(m_CurrentTextures[m_AnimationFrame], Vector2.zero, CursorMode.Auto);
                    m_AnimationFrame = (m_AnimationFrame + 1) % m_CurrentTextures.Length;
                }
            }
        }

        public void SetCursorState(CursorState cursorState, CursorPriority priority) {
            if (m_CursorState != cursorState) {
                if (cursorState == CursorState.Default) {
                    var tuple = PopCursorFromStack();
                    cursorState = tuple.Item1;
                    priority = tuple.Item2;
                } else if (m_CursorPriority > priority) {
                    PushCursorToStack(cursorState, priority);
                    return;
                }
                
                if (cursorState != CursorState.Default && m_CursorState != CursorState.Default) {
                    // a higher priority cursor will overlap //
                    PushCursorToStack(m_CursorState, m_CursorPriority);
                }

                m_CursorState = cursorState;
                m_CursorPriority = priority;

                switch (cursorState) {
                    case CursorState.Default:
                        m_CurrentTextures = m_DefaultTextures;
                        break;
                    case CursorState.DefaultDisabled:
                        m_CurrentTextures = m_DefaultDisabledTextures;
                        break;
                    case CursorState.NResize:
                        m_CurrentTextures = m_NResizeTextures;
                        break;
                    case CursorState.EResize:
                        m_CurrentTextures = m_EResizeTextures;
                        break;
                    case CursorState.Hand:
                        m_CurrentTextures = m_HandTextures;
                        break;
                    case CursorState.Crosshair:
                        m_CurrentTextures = m_CrosshairTextures;
                        break;
                    case CursorState.CrosshairDisabled:
                        m_CurrentTextures = m_CrosshairDisabledTextures;
                        break;
                    case CursorState.Scan:
                        m_CurrentTextures = m_ScanTextures;
                        break;
                    case CursorState.Attack:
                        m_CurrentTextures = m_AttackTextures;
                        break;
                    case CursorState.Walk:
                        m_CurrentTextures = m_WalkTextures;
                        break;
                    case CursorState.Use:
                        m_CurrentTextures = m_UseTextures;
                        break;
                    case CursorState.Talk:
                        m_CurrentTextures = m_TalkTextures;
                        break;
                    case CursorState.Look:
                        m_CurrentTextures = m_LookTextures;
                        break;
                    case CursorState.Open:
                        m_CurrentTextures = m_OpenTextures;
                        break;
                    case CursorState.Loot:
                        m_CurrentTextures = m_LootTextures;
                        break;

                    default:
                        m_CurrentTextures = null;
                        break;
                }

                if (m_CurrentTextures != null && m_CurrentTextures.Length > 0) {
                    m_CustomCursor = true;
                    m_SequenceAnimation = m_CurrentTextures.Length > 1;
                    m_AnimationFrame = 0;
                    
                    if (!m_SequenceAnimation) {
                        Cursor.SetCursor(m_CurrentTextures[0], Vector2.zero, CursorMode.Auto);
                    } else if (m_DefaultAnimationTick == 0) {
                        m_AnimationTick = 1f / m_CurrentTextures.Length;
                    } else {
                        m_AnimationTick = m_DefaultAnimationTick;
                    }
                } else {
                    m_CustomCursor = false;
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                }
            }
        }

        private void PushCursorToStack(CursorState cursorState, CursorPriority priority) {
            if (m_CursorStack == null)
                m_CursorStack = new List<Tuple<CursorState, CursorPriority>>();

            if (m_CursorStack.Count == 0) {
                m_CursorStack.Add(Tuple.Create(cursorState, priority));
                return;
            }

            var index = m_CursorStack.FindLastIndex((t) => t.Item2 == priority);
            if (index == -1) {
                if (priority == CursorPriority.High) { // high at the end
                    m_CursorStack.Add(Tuple.Create(cursorState, priority));
                } else if (priority == CursorPriority.Medium) {
                    index = m_CursorStack.FindLastIndex((t) => t.Item2 == CursorPriority.Low);
                    if (index == -1)
                        m_CursorStack.Insert(0, Tuple.Create(cursorState, priority));
                    else
                        m_CursorStack.Insert(index, Tuple.Create(cursorState, priority));
                } else if (priority == CursorPriority.Low) {
                    m_CursorStack.Insert(0, Tuple.Create(cursorState, priority));
                }
            } else {
                m_CursorStack.Insert(index, Tuple.Create(cursorState, priority));
            }
        }

        private Tuple<CursorState, CursorPriority> PopCursorFromStack() {
            if (m_CursorStack == null || m_CursorStack.Count == 0)
                return Tuple.Create(CursorState.Default, CursorPriority.Low);

            var tuple = m_CursorStack[m_CursorStack.Count - 1];
            m_CursorStack.RemoveAt(m_CursorStack.Count - 1);
            return tuple;
        }
    }
}
