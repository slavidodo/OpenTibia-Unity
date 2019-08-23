using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace OpenTibiaUnity.Core.Utils
{
    [ExecuteInEditMode]
    public class CursorController : MonoBehaviour
    {
        [Tooltip("Animation frame between each sprite in seconds")]
        [SerializeField] private float _defaultAnimationTick = 0.15f;

        [SerializeField] private Texture2D[] _defaultTextures = null;
        [SerializeField] private Texture2D[] _defaultDisabledTextures = null;
        [SerializeField] private Texture2D[] _nResizeTextures = null;
        [SerializeField] private Texture2D[] _eResizeTextures = null;
        [SerializeField] private Texture2D[] _handTextures = null;
        [SerializeField] private Texture2D[] _crosshairTextures = null;
        [SerializeField] private Texture2D[] _crosshairDisabledTextures = null;
        [SerializeField] private Texture2D[] _scanTextures = null;
        [SerializeField] private Texture2D[] _attackTextures = null;
        [SerializeField] private Texture2D[] _walkTextures = null;
        [SerializeField] private Texture2D[] _useTextures = null;
        [SerializeField] private Texture2D[] _talkTextures = null;
        [SerializeField] private Texture2D[] _lookTextures = null;
        [SerializeField] private Texture2D[] _openTextures = null;
        [SerializeField] private Texture2D[] _lootTextures = null;

        private bool _customCursor = false;
        private bool _sequenceAnimation = false;
        private float _lastAnimationTime = 0;
        private float _animationTick = 0;
        private int _animationFrame = 0;
        private CursorState _cursorState = CursorState.Default;
        private CursorPriority _cursorPriority = CursorPriority.Low;
        private Texture2D[] _currentTextures = null;

        public void Start() {
            SetCursorState(CursorState.Default, CursorPriority.High);
        }

        private void Update() {
            if (_customCursor && _sequenceAnimation) {
                if (_lastAnimationTime == 0 || Time.time - _lastAnimationTime > _animationTick) {
                    _lastAnimationTime = Time.time;

                    Cursor.SetCursor(_currentTextures[_animationFrame], Vector2.zero, CursorMode.Auto);
                    _animationFrame = (_animationFrame + 1) % _currentTextures.Length;
                }
            }
        }

        private static CursorState GetCursorForAction(AppearanceActions action) {
            switch (action) {
                case AppearanceActions.Attack:
                    return CursorState.Attack;

                case AppearanceActions.AutoWalk:
                case AppearanceActions.AutoWalkHighlight:
                    return CursorState.Walk;

                case AppearanceActions.Look:
                    return CursorState.Look;

                case AppearanceActions.Use:
                    return CursorState.Use;

                case AppearanceActions.Open:
                    return CursorState.Open;

                case AppearanceActions.Talk:
                    return CursorState.Talk;

                case AppearanceActions.Loot:
                    return CursorState.Loot;

                default:
                    return CursorState.Default;
            }
        }

        public void SetCursorState(AppearanceActions action, CursorPriority priority) {
            SetCursorState(GetCursorForAction(action), priority);
        }

        public void SetCursorState(CursorState cursorState, CursorPriority priority) {
            if (_cursorState != cursorState) {
                // priority must match the priority of the original cursor
                if (priority < _cursorPriority)
                    return;

                // usually default cursor can be overriten by any action
                if (cursorState == CursorState.Default)
                    priority = CursorPriority.Low;

                _cursorState = cursorState;
                _cursorPriority = priority;

                switch (cursorState) {
                    case CursorState.Default:
                        _currentTextures = _defaultTextures;
                        break;
                    case CursorState.DefaultDisabled:
                        _currentTextures = _defaultDisabledTextures;
                        break;
                    case CursorState.NResize:
                        _currentTextures = _nResizeTextures;
                        break;
                    case CursorState.EResize:
                        _currentTextures = _eResizeTextures;
                        break;
                    case CursorState.Hand:
                        _currentTextures = _handTextures;
                        break;
                    case CursorState.Crosshair:
                        _currentTextures = _crosshairTextures;
                        break;
                    case CursorState.CrosshairDisabled:
                        _currentTextures = _crosshairDisabledTextures;
                        break;
                    case CursorState.Scan:
                        _currentTextures = _scanTextures;
                        break;
                    case CursorState.Attack:
                        _currentTextures = _attackTextures;
                        break;
                    case CursorState.Walk:
                        _currentTextures = _walkTextures;
                        break;
                    case CursorState.Use:
                        _currentTextures = _useTextures;
                        break;
                    case CursorState.Talk:
                        _currentTextures = _talkTextures;
                        break;
                    case CursorState.Look:
                        _currentTextures = _lookTextures;
                        break;
                    case CursorState.Open:
                        _currentTextures = _openTextures;
                        break;
                    case CursorState.Loot:
                        _currentTextures = _lootTextures;
                        break;

                    default:
                        _currentTextures = null;
                        break;
                }

                if (_currentTextures != null && _currentTextures.Length > 0) {
                    _customCursor = true;
                    _sequenceAnimation = _currentTextures.Length > 1;
                    _animationFrame = 0;
                    
                    if (!_sequenceAnimation) {
                        Cursor.SetCursor(_currentTextures[0], Vector2.zero, CursorMode.Auto);
                    } else if (_defaultAnimationTick == 0) {
                        _animationTick = 1f / _currentTextures.Length;
                    } else {
                        _animationTick = _defaultAnimationTick;
                    }
                } else {
                    _customCursor = false;
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                }
            }
        }
    }
}
