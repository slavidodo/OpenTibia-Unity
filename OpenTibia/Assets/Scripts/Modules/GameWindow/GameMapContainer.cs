using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.GameWindow
{
    [ExecuteInEditMode]
    public class GameMapContainer : GamePanelContainer
    {
#pragma warning disable CS0649 // never assigned to
        [SerializeField] private RawImage m_WorldMapRawImage;
        [SerializeField] private TMPro.TextMeshProUGUI m_FramecounterText;
#pragma warning restore CS0649 // never assigned to
        
        private Rect m_CachedScreenRect = Rect.zero;
        private bool m_ScreenRectCached = false;

        private int m_LastScreenWidth = 0;
        private int m_LastScreenHeight = 0;
        private int m_LastFramerate = 0;

        private RectTransform worldMapRectTransform {
            get {
                return m_WorldMapRawImage.rectTransform;
            }
        }

        protected override void Start() {
            base.Start();
        }

        protected void Update() {
            if (m_LastScreenWidth != Screen.width || m_LastScreenHeight != Screen.height)
                InvalidateScreenRect();

            StartMouseAction();
        }

        protected void OnGUI() {
            if (Event.current.type == EventType.Repaint)
                RenderWorldMap();
        }

        protected void RenderWorldMap() {
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPaused)
                return;
#endif

            if (!m_ScreenRectCached)
                CacheScreenRect();

            var worldMapRenderer = OpenTibiaUnity.WorldMapRenderer;
            if (worldMapRenderer != null) {
                OpenTibiaUnity.GameManager.WorldMapRenderingTexture.Release();
                RenderTexture.active = OpenTibiaUnity.GameManager.WorldMapRenderingTexture;
                worldMapRenderer.Render(worldMapRectTransform.rect);
                RenderTexture.active = null;

                // setting the clip area
                m_WorldMapRawImage.uvRect = worldMapRenderer.CalculateClipRect();

                if (worldMapRenderer.Framerate != m_LastFramerate) {
                    m_LastFramerate = worldMapRenderer.Framerate;
                    int? ping = OpenTibiaUnity.ProtocolGame?.Ping;
                    m_FramecounterText.text = string.Format("FPS: <color=#{0:X6}>{1}</color>\nPing:{2}", GetFramerateColor(m_LastFramerate), m_LastFramerate, ping.HasValue ? ping.Value : -1);
                }
            } else {
                m_FramecounterText.text = "";
            }
        }

        protected override void OnDestroy() {
            base.OnDestroy();
        }

        private static uint GetFramerateColor(int framerate) {
            if (framerate < 10)
                return 0xFFFF00;
            else if (framerate < 30)
                return 0xF55E5E;
            else if (framerate < 58)
                return 0xFE6500;

            return 0x00EB00;
        }
        
        public void CacheScreenRect() {
            Vector2 size = Vector2.Scale(worldMapRectTransform.rect.size, worldMapRectTransform.lossyScale);
            m_CachedScreenRect = new Rect(worldMapRectTransform.position.x, Screen.height - worldMapRectTransform.position.y, size.x, size.y);
            m_CachedScreenRect.x -= worldMapRectTransform.pivot.x * size.x;
            m_CachedScreenRect.y -= (1.0f - worldMapRectTransform.pivot.y) * size.y;

            m_ScreenRectCached = true;
        }

        public void InvalidateScreenRect() {
            m_ScreenRectCached = false;
        }

        Vector3Int? PointToMap(Vector2 point) {
            return OpenTibiaUnity.GameManager.WorldMapRenderer.PointToMap(point);
        }

        Vector3Int? PointToAbsolute(Vector2 point) {
            return OpenTibiaUnity.GameManager.WorldMapRenderer.PointToAbsolute(point);
        }

        public void StartMouseAction() {
            var creatureStorage = OpenTibiaUnity.CreatureStorage;
            var worldMapStorage = OpenTibiaUnity.WorldMapStorage;
            if (creatureStorage == null || worldMapStorage == null || (!Input.GetMouseButtonUp(0) && !Input.GetMouseButtonUp(1)))
                return;

            var mousePoint = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
            var point = mousePoint - m_CachedScreenRect.position;

            if (point.x > m_CachedScreenRect.size.x || point.y > m_CachedScreenRect.size.y)
                return;

            DetermineAction(point, true);
        }

        public Core.InputManagment.Mapping.MouseBinding? DetermineBinding() {
            var optionStorage = OpenTibiaUnity.OptionStorage;
            if (optionStorage.MousePreset == MousePresets.LeftSmartClick)
                return Core.InputManagment.Mapping.MouseMapping.LeftSmartMapping.DetermineBinding();
            else if (optionStorage.MousePreset == MousePresets.Regular)
                return Core.InputManagment.Mapping.MouseMapping.RefularMapping.DetermineBinding();

            return Core.InputManagment.Mapping.MouseMapping.ClassicMapping.DetermineBinding();
        }

        public AppearanceActions DetermineAction(Vector2 point, bool applyAction = false, bool updateCursor = false, bool updateHighlight = false) {
            var mouseBinding = DetermineBinding();
            if (!mouseBinding.HasValue)
                return AppearanceActions.None;

            var worldMapRenderer = OpenTibiaUnity.WorldMapRenderer;
            var worldMapStorage = OpenTibiaUnity.WorldMapStorage;
            var creatureStorage = OpenTibiaUnity.CreatureStorage;
            var player = OpenTibiaUnity.Player;
            var action = mouseBinding.Value.Action;
            var oldAction = AppearanceActions.None;
            var mapPosition = worldMapRenderer.PointToMap(point);
            Core.Creatures.Creature creature = null;
            Core.Appearances.ObjectInstance foundObject = null;
            Core.Appearances.ObjectInstance topLookObject = null;
            Core.Appearances.ObjectInstance topUseObject = null;

            if (mapPosition.HasValue && mapPosition.Value.z == worldMapStorage.PlayerZPlane) {
                if (action != AppearanceActions.SmartClick && action != AppearanceActions.AutoWalk) {
                    var tmpCreature = worldMapRenderer.PointToCreature(point, true);

                    if (!!tmpCreature && worldMapStorage.GetCreatureObjectForCreature(creature, out foundObject) > -1)
                        creature = tmpCreature;

                    if (!foundObject)
                        worldMapStorage.GetTopLookObject(mapPosition.Value, out topLookObject);
                    else
                        topLookObject = foundObject;

                    worldMapStorage.GetTopUseObject(mapPosition.Value, out topUseObject);

                    oldAction = action;
                    if (!!creature && creature != player) {
                        action = Core.InputManagment.MouseActionHelper.ResolveActionForAppearanceOrCreature(action, creature);
                        topUseObject = null;
                    } else if (!!topUseObject && topUseObject.Type.IsUsable) {
                        action = Core.InputManagment.MouseActionHelper.ResolveActionForAppearanceOrCreature(action, topUseObject);
                    } else if (!!topLookObject) {
                        action = Core.InputManagment.MouseActionHelper.ResolveActionForAppearanceOrCreature(action, topLookObject);
                    }
                } else {
                    action = AppearanceActions.AutoWalk;
                }

                if (!!topUseObject && (topUseObject.Type.DefaultAction == (int)AppearanceActions.None) && oldAction != action && action == AppearanceActions.Look
                && worldMapStorage.GetEnterPossibleFlag(mapPosition.Value.x, mapPosition.Value.y, worldMapStorage.PlayerZPlane, true) != Constants.FieldEnterNotPossible) {
                    action = AppearanceActions.AutoWalk;
                }
            } else {
                action = AppearanceActions.None;
            }

            if (updateCursor) {
                // TODO: Update Cursor
            }

            if (updateHighlight) {
                // TODO: update highlighted tile
            }

            if (applyAction) {
                switch (action) {
                    case AppearanceActions.None: break;
                    case AppearanceActions.Attack:
                        if (!!creature && creature != player)
                            creatureStorage.ToggleAttackTarget(creature, true);
                        break;
                    case AppearanceActions.AutoWalk:
                    case AppearanceActions.AutoWalkHighlight:
                        var cost = OpenTibiaUnity.MiniMapStorage.GetFieldCost(worldMapStorage.ToAbsolute(mapPosition.Value));
                        player.StartAutowalk(worldMapStorage.ToAbsolute(mapPosition.Value), false, true);
                        break;
                    case AppearanceActions.ContextMenu:
                        // TODO: Open Context menu (position alligned to player)
                        break;
                    case AppearanceActions.Look:
                        if (!!topLookObject) {
                            // TODO: look action
                            Vector3Int absolutePosition;
                            if (foundObject == topLookObject)
                                absolutePosition = creature.Position;
                            else
                                absolutePosition = worldMapStorage.ToAbsolute(mapPosition.Value);
                            new Core.InputManagment.GameAction.LookActionImpl(absolutePosition, topLookObject, topLookObject.MapData).Perform();
                        }
                        break;
                    case AppearanceActions.Use:
                    case AppearanceActions.Open:
                        if (topUseObject != null) {
                            var absolutePosition = worldMapStorage.ToAbsolute(mapPosition.Value);
                            new Core.InputManagment.GameAction.UseActionImpl(absolutePosition, topUseObject.Type, topUseObject.MapData, UseActionTarget.Auto).Perform();
                        }
                        break;
                    case AppearanceActions.Talk:
                        // TODO: Greet action
                        break;
                    case AppearanceActions.Unset:
                        break;
                }
            }

            return action;
        }
    }
}
