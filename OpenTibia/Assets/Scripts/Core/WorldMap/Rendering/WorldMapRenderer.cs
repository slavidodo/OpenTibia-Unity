using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

namespace OpenTibiaUnity.Core.WorldMap.Rendering
{
    public class WorldMapRenderer {
        private int m_DrawnCreaturesCount = 0;
        private int m_DrawnTextualEffectsCount = 0;
        private int m_MaxZPlane = 0;
        private int m_PlayerZPlane = 0;
        private float m_HangPixelX = 0;
        private float m_HangPixelY = 0;
        private int m_HangPatternX = 0;
        private int m_HangPatternY = 0;
        private int m_HangPatternZ = 0;
        private float m_HighlightOpacity = Constants.HighlightMinOpacity;
        private Vector3Int m_HelperCoordinate;
        private Vector2Int m_HelperPoint;
        private readonly int[] m_MinZPlane;
        private float[] m_CachedHighlightOpacities;

        private Vector2 m_ScreenZoom = new Vector2();
        private Vector2 m_LayerZoom = new Vector2();

        private int[] m_CreatureCount;
        private readonly RenderAtom[][] m_CreatureField;
        private readonly RenderAtom[] m_DrawnCreatures;
        private readonly RenderAtom[] m_DrawnTextualEffects;
        private List<Components.CreatureStatusPanel> m_CreaturesStatus;
        private Creatures.Creature m_HighlightCreature;
        
        private TileCursor m_TileCursor = new TileCursor();
        private Appearances.ObjectInstance m_PreviousHang = null;
        private ILightmapRenderer m_LightmapRenderer = new MeshBasedLightmapRenderer();
        private Appearances.Rendering.MarksView m_CreaturesMarksView = new Appearances.Rendering.MarksView(0);
        
        private WorldMapStorage m_WorldMapStorage { get => OpenTibiaUnity.WorldMapStorage; }
        private Creatures.CreatureStorage m_CreatureStorage { get => OpenTibiaUnity.CreatureStorage; }
        private Creatures.Player m_Player { get => OpenTibiaUnity.Player; }
        private Options.OptionStorage m_OptionStorage { get => OpenTibiaUnity.OptionStorage; }
        
        private Rect m_LastWorldMapLayerRect = Rect.zero;
        private Rect m_ClipRect = Rect.zero;
        private Rect m_UnclamppedClipRect = Rect.zero;

        private uint m_RenderCounter = 0; // initially the first render
        private uint m_LastRenderCounter = 0;
        private uint m_ClipRectRenderCounter = 0;
        private float m_LastRenderTime = 0f;

        public int Framerate { get; private set; } = 0;
        
        public Vector3Int? HighlightTile { get; set; }
        public object HighlightObject { get; set; }

        public WorldMapRenderer() {
            int mapCapacity = Constants.MapSizeX * Constants.MapSizeY;

            m_MinZPlane = new int[mapCapacity];
            m_CreatureField = new RenderAtom[mapCapacity][];
            m_CreatureCount = new int[mapCapacity];
            m_DrawnCreatures = new RenderAtom[mapCapacity * Constants.MapSizeW];
            m_CreaturesStatus = new List<Components.CreatureStatusPanel>();

            for (int i = 0; i < mapCapacity; i++) {
                m_CreatureField[i] = new RenderAtom[Constants.MapSizeW];
                for (int j = 0; j < Constants.MapSizeW; j++)
                    m_CreatureField[i][j] = new RenderAtom();
                m_CreatureCount[i] = 0;
            }

            for (int i = 0; i < m_DrawnCreatures.Length; i++)
                m_DrawnCreatures[i] = new RenderAtom();

            m_DrawnTextualEffects = new RenderAtom[Constants.NumEffects];
            for (int i = 0; i < m_DrawnTextualEffects.Length; i++)
                m_DrawnTextualEffects[i] = new RenderAtom();

            m_CreaturesMarksView.AddMarkToView(MarkTypes.ClientMapWindow, Constants.MarkThicknessBold);
            m_CreaturesMarksView.AddMarkToView(MarkTypes.Permenant, Constants.MarkThicknessBold);
            m_CreaturesMarksView.AddMarkToView(MarkTypes.OneSecondTemp, Constants.MarkThicknessBold);

            m_TileCursor.FrameDuration = 100;

            m_CachedHighlightOpacities = new float[8 * 2 - 2];
            for (int i = 0; i < 8; i++)
                m_CachedHighlightOpacities[i] = Constants.HighlightMinOpacity + (i / 7f) * (Constants.HighlightMaxOpacity - Constants.HighlightMinOpacity);
            for (int i = 1; i < 7; i++)
                m_CachedHighlightOpacities[7 + i] = Constants.HighlightMaxOpacity - (i / 7f) * (Constants.HighlightMaxOpacity - Constants.HighlightMinOpacity);
            
            // enable this if you intend to change the name during the game..
            // Creatures.Creature.onNameChange.AddListener(OnCreatureNameChange);
            Creatures.Creature.onSkillChange.AddListener(OnCreatureSkillChange);
        }

        public void DestroyUIElements() {
            foreach (var panel in m_CreaturesStatus)
                UnityEngine.Object.Destroy(panel.gameObject);

            m_CreaturesStatus = new List<Components.CreatureStatusPanel>();
        }
        
        public Rect CalculateClipRect() {
            if (m_ClipRectRenderCounter == m_RenderCounter)
                return m_ClipRect;

            float width = Constants.WorldMapRealWidth;
            float height = Constants.WorldMapRealHeight;

            // (0, 0) respects to the bottomleft
            float x = 2 * Constants.FieldSize + m_Player.AnimationDelta.x;
            float y = Constants.FieldSize - m_Player.AnimationDelta.y;

            m_UnclamppedClipRect = new Rect(x, y, width, height);

            width /= Constants.WorldMapScreenWidth;
            x /= Constants.WorldMapScreenWidth;
            height /= Constants.WorldMapScreenHeight;
            y /= Constants.WorldMapScreenHeight;

            m_ClipRect = new Rect(x, y, width, height);
            m_ClipRectRenderCounter = m_RenderCounter;
            return m_ClipRect;
        }

        public RenderError Render(Rect worldMapLayerRect) {
            if (m_WorldMapStorage == null || m_CreatureStorage == null || m_Player == null || !m_WorldMapStorage.Valid)
                return RenderError.WorldMapNotValid;

            m_LastWorldMapLayerRect = worldMapLayerRect;
            if (m_LastWorldMapLayerRect.width < Constants.WorldMapMinimumWidth || m_LastWorldMapLayerRect.height < Constants.WorldMapMinimumHeight)
                return RenderError.SizeNotEffecient;

            var screenSize = new Vector2(Screen.width, Screen.height);
            m_ScreenZoom.Set(
                screenSize.x / Constants.WorldMapScreenWidth,
                screenSize.y / Constants.WorldMapScreenHeight);

            m_LayerZoom.Set(
                    m_LastWorldMapLayerRect.width / Constants.WorldMapRealWidth,
                    m_LastWorldMapLayerRect.height / Constants.WorldMapRealHeight);

            m_WorldMapStorage.Animate();
            m_CreatureStorage.Animate();
            m_HighlightOpacity = m_CachedHighlightOpacities[(OpenTibiaUnity.TicksMillis / 50) % m_CachedHighlightOpacities.Length];

            m_DrawnCreaturesCount = 0;
            m_DrawnTextualEffectsCount = 0;
            m_RenderCounter++;

            if (Time.time - m_LastRenderTime > 1f) {
                Framerate = (int)((m_RenderCounter - m_LastRenderCounter) / (Time.time - m_LastRenderTime));
                m_LastRenderTime = Time.time;
                m_LastRenderCounter = m_RenderCounter;
            }

            m_WorldMapStorage.ToMap(m_Player.Position, out m_HelperCoordinate);
            m_PlayerZPlane = m_HelperCoordinate.z;

            UpdateMinMaxZPlane();

            if (HighlightObject is Creatures.Creature tmpCreature)
                m_HighlightCreature = tmpCreature;
            else if (HighlightObject is Appearances.ObjectInstance tmpObject && tmpObject.IsCreature)
                m_HighlightCreature = m_CreatureStorage.GetCreature(tmpObject.Data);
            else
                m_HighlightCreature = null;

            for (int z = 0; z <= m_MaxZPlane; z++) {
                UpdateFloorCreatures(z);
                InternalDrawFields(z);
            }

            var a = m_WorldMapStorage.ToMapClosest(new Vector3Int(32918, 32071, 7));
            var b = m_MinZPlane[a.y * Constants.MapSizeX + a.x];

            var lightmapTexture = m_LightmapRenderer.CreateLightmap();
            var lightmapRect = new Rect() {
                x = Constants.FieldSize / 2 * m_ScreenZoom.y,
                y = Constants.FieldSize / 2 * m_ScreenZoom.y,
                width = (Constants.WorldMapScreenWidth) * m_ScreenZoom.x,
                height = Constants.WorldMapScreenHeight * m_ScreenZoom.y,
            };

            Graphics.DrawTexture(lightmapRect, lightmapTexture, OpenTibiaUnity.GameManager.LightSurfaceMaterial);

            InternelDrawCreaturesStatus();
            InternalUpdateTextualEffects();
            InternalUpdateOnscreenMessages();
            
            return RenderError.None;
        }
        
        private void UpdateMinMaxZPlane() {
            if (!m_WorldMapStorage.CacheUnsight) {
                m_MaxZPlane = Constants.MapSizeZ - 1;
                while (m_MaxZPlane > m_PlayerZPlane && m_WorldMapStorage.GetObjectPerLayer(m_MaxZPlane) <= 0) {
                    m_MaxZPlane--;
                }

                for (int x = Constants.PlayerOffsetX - 1; x <= Constants.PlayerOffsetX + 1; x++) {
                    for (int y = Constants.PlayerOffsetY - 1; y <= Constants.PlayerOffsetY + 1; y++) {
                        if (!(x != Constants.PlayerOffsetX && y != Constants.PlayerOffsetY || !m_WorldMapStorage.IsLookPossible(x, y, m_PlayerZPlane))) {
                            int z = m_PlayerZPlane + 1;
                            while (z - 1 < m_MaxZPlane && x + m_PlayerZPlane - z >= 0 && y + m_PlayerZPlane - z >= 0) {
                                var obj = m_WorldMapStorage.GetObject(x + m_PlayerZPlane - z, y + m_PlayerZPlane - z, z, 0);
                                if (!!obj && !!obj.Type && obj.Type.IsGround && !obj.Type.IsDontHide) {
                                    m_MaxZPlane = z - 1;
                                    continue;
                                }

                                obj = m_WorldMapStorage.GetObject(x, y, z, 0);
                                if (!!obj && !!obj.Type && (obj.Type.IsGround || obj.Type.IsBottom) && !obj.Type.IsDontHide) {
                                    m_MaxZPlane = z - 1;
                                    continue;
                                }
                                z++;
                            }
                        }
                    }
                }

                m_WorldMapStorage.CacheUnsight = true;
            }

            if (!m_WorldMapStorage.CacheFullbank) {
                for (int y = 0; y < Constants.MapSizeY; y++) {
                    for (int x = 0; x < Constants.MapSizeX; x++) {
                        int index = y * Constants.MapSizeX + x;

                        m_MinZPlane[index] = 0;
                        for (int z = m_MaxZPlane; z > 0; z--) {
                            bool covered = true, done = false;
                            for (int ix = 0; ix < 2 && !done; ix++) {
                                for (int iy = 0; iy < 2 && !done; iy++) {
                                    int mx = x + ix;
                                    int my = y + iy;
                                    if (mx < Constants.MapSizeX && my < Constants.MapSizeY) {
                                        Appearances.ObjectInstance obj = m_WorldMapStorage.GetObject(mx, my, z, 0);
                                        if (!obj || (!!obj.Type && !obj.Type.IsFullGround)) {
                                            covered = false;
                                            done = true;
                                        }
                                    }
                                }
                            }
                            
                            if (covered)
                                m_MinZPlane[index] = z;
                        }
                    }
                }

                m_WorldMapStorage.CacheFullbank = true;
            }
        }
        
        private void UpdateFloorCreatures(int z) {
            RenderAtom renderAtom = null;
            
            bool aboveGround = m_Player.Position.z <= 7;
            float optionsLevelSeparator = m_OptionStorage.LightLevelSeparator / 100f;
            int defaultBrightness = m_WorldMapStorage.AmbientCurrentBrightness;
            int brightness = aboveGround ? m_WorldMapStorage.AmbientCurrentBrightness : 0;
            for (int i = 0; i < m_CreatureCount.Length; i++) {
                m_CreatureCount[i] = 0;
            }
            
            for (int x = 0; x < Constants.MapSizeX; x++) {
                for (int y = 0; y < Constants.MapSizeY; y++) {
                    Field field = m_WorldMapStorage.GetField(x, y, z);
                    if (m_OptionStorage.ShowLightEffects) {
                        var colorIndex = m_LightmapRenderer.ToColorIndex(x, y);
                        if (z == m_PlayerZPlane && z > 0)
                            m_LightmapRenderer[colorIndex] = ILightmapRenderer.MulColor32(m_LightmapRenderer[colorIndex], optionsLevelSeparator);

                        Appearances.ObjectInstance obj;
                        if (z == 0 || (obj = field.ObjectsRenderer[0]) != null && obj.Type.IsGround) {
                            var color = m_LightmapRenderer[colorIndex];
                            m_LightmapRenderer.SetFieldBrightness(x, y, brightness, aboveGround);
                            if (z > 0 && field.CacheTranslucent) {
                                color = ILightmapRenderer.MulColor32(color, optionsLevelSeparator);
                                var alterColor = m_LightmapRenderer[colorIndex];
                                color.r = Math.Max(color.r, alterColor.r);
                                color.g = Math.Max(color.g, alterColor.g);
                                color.b = Math.Max(color.b, alterColor.b);
                                m_LightmapRenderer[colorIndex] = color;
                            }
                        }

                        if (x > 0 && y > 0 && z < 7 && z == m_PlayerZPlane + m_WorldMapStorage.Position.z - 8 && m_WorldMapStorage.IsTranslucent(x - 1, y - 1, z + 1))
                            m_LightmapRenderer.SetFieldBrightness(x, y, defaultBrightness, aboveGround);
                    }
                    
                    for (int i = field.ObjectsCount - 1; i >= 0; i--) {
                        var obj = field.ObjectsRenderer[i];
                        if (!obj || !obj.IsCreature)
                            continue;

                        var creature = m_CreatureStorage.GetCreature(obj.Data);
                        if (!creature)
                            continue;

                        Vector2Int displacement = new Vector2Int(); ;
                        if (!!creature.MountOutfit && !!creature.MountOutfit.Type) {
                            displacement = creature.MountOutfit.Type.DisplacementVector2;
                        } else if (!!creature.Outfit && !!creature.Outfit.Type) {
                            displacement = creature.Outfit.Type.DisplacementVector2;
                        }

                        int positionX = (x + 1) * Constants.FieldSize + creature.AnimationDelta.x - displacement.x;
                        int positionY = (y + 1) * Constants.FieldSize + creature.AnimationDelta.y - displacement.y;
                        int fieldX = (positionX - 1) / Constants.FieldSize;
                        int fieldY = (positionY - 1) / Constants.FieldSize;

                        int fieldIndex = fieldY * Constants.MapSizeX + fieldX;
                        if (!(fieldIndex < 0 || fieldIndex >= Constants.MapSizeX * Constants.MapSizeY)) {
                            RenderAtom[] fieldAtoms = m_CreatureField[fieldIndex];
                            int j = 0;
                            while (j < m_CreatureCount[fieldIndex] && !!(renderAtom = fieldAtoms[j]) && (renderAtom.y < positionY || renderAtom.y == positionY && renderAtom.x <= positionX))
                                j++;

                            if (j < Constants.MapSizeZ) {
                                if (m_CreatureCount[fieldIndex] < Constants.MapSizeW) {
                                    m_CreatureCount[fieldIndex]++;
                                }

                                renderAtom = fieldAtoms[m_CreatureCount[fieldIndex] - 1];
                                for (int k = m_CreatureCount[fieldIndex] - 1; k > j; k--)
                                    fieldAtoms[k] = fieldAtoms[k - 1];

                                fieldAtoms[j] = renderAtom;
                                renderAtom.Update(creature, positionX, positionY, z, fieldX, fieldY);
                            }
                        }
                    }
                }
            }
        }
        
        private void InternalDrawFields(int z) {
            m_HelperCoordinate.Set(0, 0, z);
            m_WorldMapStorage.ToAbsolute(m_HelperCoordinate, out m_HelperCoordinate);
            int size = Constants.MapSizeX + Constants.MapSizeY;
            for (int i = 0; i < size; i++) {
                int y = Math.Max(i - Constants.MapSizeX + 1, 0);
                int x = Math.Min(i, Constants.MapSizeX - 1);
                while (x >= 0 && y < Constants.MapSizeY) {
                    InternalDrawField(
                        (x + 1) * Constants.FieldSize,
                        (y + 1) * Constants.FieldSize,
                        m_HelperCoordinate.x + x,
                        m_HelperCoordinate.y + y,
                        m_HelperCoordinate.z,
                        x, y, z,
                        true);
                    x--;
                    y++;
                }
                
                if (m_OptionStorage.HighlightMouseTarget && HighlightTile.HasValue && HighlightTile.Value.z == z) {
                    m_TileCursor.DrawTo((HighlightTile.Value.x + 1f) * Constants.FieldSize, (HighlightTile.Value.y + 1f) * Constants.FieldSize, m_ScreenZoom, OpenTibiaUnity.TicksMillis);
                }
            }
        }

        private void InternalDrawField(float rectX, float rectY, int absoluteX, int absoluteY, int absoluteZ, int positionX, int positionY, int positionZ, bool drawLyingObjects) {

            Field field = m_WorldMapStorage.GetField(positionX, positionY, positionZ);

            int objCount = field.ObjectsCount;
            int index = positionY * Constants.MapSizeX + positionX;
            
            bool isCovered = positionZ > m_MinZPlane[index]
                || (positionX == 0 || positionZ >= m_MinZPlane[index - 1])
                || (positionY == 0 || positionZ >= m_MinZPlane[index - Constants.MapSizeX])
                || (positionX == 0 && positionY == 0 || positionZ >= m_MinZPlane[index - Constants.MapSizeX - 1]);

            int objectsHeight = 0;
            Appearances.ObjectInstance previousHang = null;

            // Draw objects
            if (drawLyingObjects && objCount > 0 && isCovered) {
                //Appearances.ObjectInstance obj;
                bool isLying = false;

                // draw Items in reverse order
                for (int i = 0; i < objCount; i++) {
                    var obj = field.ObjectsRenderer[i];
                    var type = obj.Type;
                    if (obj.IsCreature || type.IsTop)
                        break;

                    if (m_OptionStorage.ShowLightEffects && type.IsLight) {
                        // check how correct those are
                        int lightX = ((int)rectX - objectsHeight - (int)type.DisplacementX) / Constants.FieldSize;
                        int lightY = ((int)rectY - objectsHeight - (int)type.DisplacementY) / Constants.FieldSize;
                        Color32 color32 = Colors.ColorFrom8Bit((byte)type.LightColor);

                        m_LightmapRenderer.SetLightSource(lightX, lightY, positionZ, type.Brightness, color32);
                    }

                    var screenPosition = new Vector2(rectX - objectsHeight, rectY - objectsHeight);
                    bool highlighted = HighlightObject is Appearances.ObjectInstance && HighlightObject == obj;
                    obj.DrawTo(screenPosition, m_ScreenZoom, absoluteX, absoluteY, absoluteZ, highlighted, m_HighlightOpacity);

                    isLying = isLying || type.IsLyingCorpse;
                    if (type.IsHangable && obj.Hang == Appearances.AppearanceInstance.HookSouth)
                        previousHang = obj;

                    if (type.HasElevation)
                        objectsHeight = Mathf.Min(objectsHeight + (int)type.Elevation, Constants.FieldHeight);
                }

                // lying tile, draw lying tile
                if (isLying) {
                    if (positionX > 0 && positionY > 0) {
                        InternalDrawField(rectX - Constants.FieldSize, rectY - Constants.FieldSize, absoluteX - 1, absoluteY - 1, absoluteZ, positionX - 1, positionY - 1, positionZ, false);
                    } else if (positionX > 0) {
                        InternalDrawField(rectX - Constants.FieldSize, rectY, absoluteX - 1, absoluteY, absoluteZ, positionX - 1, positionY, positionZ, false);
                    } else if (positionY > 0) {
                        InternalDrawField(rectX, rectY - Constants.FieldSize, absoluteX, absoluteY - 1, absoluteZ, positionX, positionY - 1, positionZ, false);
                    }
                }

                // draw hang object
                if (!!m_PreviousHang) {
                    var screenPosition = new Vector2(m_HangPixelX, m_HangPixelY);
                    bool highlighted = HighlightObject is Appearances.ObjectInstance && HighlightObject == m_PreviousHang;
                    m_PreviousHang.DrawTo(screenPosition, m_ScreenZoom, m_HangPatternX, m_HangPatternY, m_HangPatternZ, highlighted, m_HighlightOpacity);
                    m_PreviousHang = null;
                }

                if (!!previousHang) {
                    m_PreviousHang = previousHang;
                    m_HangPixelX = rectX;
                    m_HangPixelY = rectY;
                    m_HangPatternX = absoluteX;
                    m_HangPatternY = absoluteY;
                    m_HangPatternZ = absoluteZ;
                }
            }

            // draw creatures
            InternalFieldDrawCreatures(positionX, positionY, positionZ, drawLyingObjects, isCovered, objectsHeight);

            // draw effects
            InternalFieldDrawEffects(rectX, rectY, absoluteX, absoluteY, absoluteZ, positionZ, drawLyingObjects, field, objectsHeight);

            // TODO: this should be drawn on a separate render texture (atmosphere texture)
            // this is likely to be fog and such effects
            //if (!!field.EnvironmentalEffect) {
            //    var screenPosition = new Vector2(rectX, rectY);
            //    field.EnvironmentalEffect.DrawTo(screenPosition, m_ScreenZoom, absoluteX, absoluteY, absoluteZ);
            //}
            
            if (drawLyingObjects) {
                for (int i = 0; i < objCount; i++) {
                    var obj = field.ObjectsRenderer[i];
                    if (!!obj && !!obj.Type && obj.Type.IsTop) {
                        var screenPosition = new Vector2(rectX, rectY);
                        bool highlighted = HighlightObject is Appearances.ObjectInstance && HighlightObject == obj;
                        obj.DrawTo(screenPosition, m_ScreenZoom, absoluteX, absoluteY, absoluteZ, highlighted, m_HighlightOpacity);
                    }
                }
            }
        }

        private void InternalFieldDrawCreatures(int positionX, int positionY, int positionZ, bool drawLyingObjects, bool isCovered, int objectsHeight) {
            RenderAtom[] renderAtomArray = m_CreatureField[positionY * Constants.MapSizeX + positionX];
            int creatureCount = m_CreatureCount[positionY * Constants.MapSizeX + positionX];
            for (int i = 0; i < creatureCount; i++) {
                RenderAtom renderAtom = null;
                Creatures.Creature creature = null;
                if (!(renderAtom = renderAtomArray[i]) || !(creature = renderAtom.Object as Creatures.Creature) || !creature.Outfit)
                    continue;

                if (drawLyingObjects) {
                    renderAtom.x -= objectsHeight;
                    renderAtom.y -= objectsHeight;
                }

                bool highlighted = !!m_HighlightCreature && m_HighlightCreature.ID == creature.ID;

                // marks
                m_CreaturesMarksView.DrawMarks(creature.Marks, renderAtom.x, renderAtom.y, m_ScreenZoom);

                m_HelperPoint.Set(0, 0);
                if (isCovered && !!creature.MountOutfit) {
                    m_HelperPoint += creature.MountOutfit.Type.DisplacementVector2;
                    var screenPosition = new Vector2(renderAtom.x + m_HelperPoint.x, renderAtom.y + m_HelperPoint.y);
                    creature.MountOutfit.DrawTo(screenPosition, m_ScreenZoom, (int)creature.Direction, 0, 0, highlighted, m_HighlightOpacity);
                }

                if (isCovered) {
                    m_HelperPoint += creature.Outfit.Type.DisplacementVector2;
                    var screenPosition = new Vector2(renderAtom.x + m_HelperPoint.x, renderAtom.y + m_HelperPoint.y);
                    creature.Outfit.DrawTo(screenPosition, m_ScreenZoom, (int)creature.Direction, 0, !!creature.MountOutfit ? 1 : 0, highlighted, m_HighlightOpacity);
                }

                if (positionZ == m_PlayerZPlane && (m_CreatureStorage.IsOpponent(creature) || creature.ID == m_Player.ID)) {
                    m_DrawnCreatures[m_DrawnCreaturesCount].Assign(renderAtom);
                    m_DrawnCreaturesCount++;
                }

                if (isCovered && drawLyingObjects && m_OptionStorage.ShowLightEffects) {
                    int lightX = renderAtom.x / Constants.FieldSize;
                    int lightY = renderAtom.y / Constants.FieldSize;

                    m_LightmapRenderer.SetLightSource(lightX, lightY, positionZ, (uint)creature.Brightness, creature.LightColor);

                    if (!!creature.MountOutfit && creature.MountOutfit.Type.IsLight) {
                        var color = Colors.ColorFrom8Bit((byte)creature.MountOutfit.Type.LightColor);
                        m_LightmapRenderer.SetLightSource(lightX, lightY, positionZ, creature.MountOutfit.Type.Brightness, color);
                    }

                    if (!!creature.Outfit && creature.Outfit.Type.IsLight) {
                        var color = Colors.ColorFrom8Bit((byte)creature.Outfit.Type.LightColor);
                        m_LightmapRenderer.SetLightSource(lightX, lightY, positionZ, creature.Outfit.Type.Brightness, color);
                    }

                    if (creature.ID == m_Player.ID && creature.Brightness < 2) {
                        var color = new Color32(255, 255, 255, 255);
                        m_LightmapRenderer.SetLightSource(lightX, lightY, positionZ, 2, color);
                    }
                }
            }
        }

        private void InternalFieldDrawEffects(float rectX, float rectY, int absoluteX, int absoluteY, int absoluteZ, int positionZ, bool drawLyingObjects, Field field, int objectsHeight) {
            int effectRectX = (int)rectX - objectsHeight;
            int effectRectY = (int)rectY - objectsHeight;
            int effectLightX = effectRectX / Constants.FieldSize;
            int effectLightY = effectRectY / Constants.FieldSize;

            int loc29 = 0;
            int loc30 = 0;

            for (int i = field.EffectsCount - 1; i >= 0; i--) {
                var effect = field.Effects[i];
                if (!effect)
                    continue;

                if (effect is Appearances.TextualEffectInstance) {
                    if (drawLyingObjects) {
                        var renderAtom = m_DrawnTextualEffects[m_DrawnTextualEffectsCount];
                        renderAtom.Update(effect, effectRectX - Constants.FieldSize / 2 + loc29, effectRectY - Constants.FieldSize - 2 * effect.Phase, 0);

                        if (renderAtom.y + (effect as Appearances.TextualEffectInstance).Height > loc30)
                            loc29 += (effect as Appearances.TextualEffectInstance).Width;

                        if (loc29 < 2 * Constants.FieldSize) {
                            m_DrawnTextualEffectsCount++;
                            loc30 = renderAtom.x;
                        }
                    }
                } else if (effect is Appearances.MissileInstance) {
                    var missile = effect as Appearances.MissileInstance;
                    var screenPosition = new Vector2(effectRectX + missile.AnimationDelta.x, effectRectY + missile.AnimationDelta.y);
                    effect.DrawTo(screenPosition, m_ScreenZoom, absoluteX, absoluteY, absoluteZ);

                    if (drawLyingObjects && m_OptionStorage.ShowLightEffects && effect.Type.IsLight) {
                        var color = Colors.ColorFrom8Bit((byte)effect.Type.LightColor);
                        m_LightmapRenderer.SetLightSource(effectLightX, effectLightY, positionZ, effect.Type.Brightness, color);
                    }
                } else { // EffectInstance
                    var screenPosition = new Vector2(effectRectX, effectRectY);
                    effect.DrawTo(screenPosition, m_ScreenZoom, absoluteX, absoluteY, absoluteZ);

                    if (drawLyingObjects && m_OptionStorage.ShowLightEffects && effect.Type.IsLight) {
                        uint activeBrightness = (uint)((Math.Min(effect.Phase, effect.Type.FrameGroups[0].Phases + 1 - effect.Phase) * effect.Type.Brightness + 2) / 3);
                        var color = Colors.ColorFrom8Bit((byte)effect.Type.LightColor);
                        m_LightmapRenderer.SetLightSource(effectLightX, effectLightY, positionZ, Math.Min(activeBrightness, effect.Type.Brightness), color);
                    }
                }
            }
        }

        private void InternelDrawCreaturesStatus() {
            var optionStorage = OpenTibiaUnity.OptionStorage;
            
            int myPlayerIndex = -1;
            for (int i = 0; i < m_DrawnCreaturesCount; i++) {
                var renderAtom = m_DrawnCreatures[i];
                var creature = renderAtom.Object as Creatures.Creature;
                if (!!creature) {
                    if (creature == m_Player) {
                        myPlayerIndex = i;
                        continue;
                    }
                    
                    bool isExplicitlyVisible = renderAtom.z >= m_MinZPlane[renderAtom.fieldY * Constants.MapSizeX + renderAtom.fieldX];
                    InternalDrawCreatureStatusClassic(
                        renderAtom, renderAtom.x, renderAtom.y, isExplicitlyVisible,
                        optionStorage.ShowNameForOtherCreatures,
                        optionStorage.ShowHealthForOtherCreatures && !creature.IsNPC,
                        false,
                        optionStorage.ShowMarksForOtherCreatures);
                }
            }

            if (myPlayerIndex != -1) {
                var renderAtom = m_DrawnCreatures[myPlayerIndex];
                InternalDrawCreatureStatusClassic(renderAtom, renderAtom.x, renderAtom.y, true,
                    optionStorage.ShowNameForOwnCharacter,
                    optionStorage.ShowHealthForOwnCharacter,
                    optionStorage.ShowManaForOwnCharacter && OpenTibiaUnity.GameManager.ClientVersion >= 1100,
                    optionStorage.ShowMarksForOwnCharacter);
            }

            m_CreaturesStatus = m_CreaturesStatus.Where((x) => {
                if (x.CachedRenderCount == m_RenderCounter)
                    return true;

                UnityEngine.Object.Destroy(x.gameObject);
                return false;
            }).ToList();
        }

        private void InternalUpdateTextualEffects() {
            // TODO
            for (int i = 0; i < m_DrawnTextualEffectsCount; i++) {
                var renderAtom = m_DrawnTextualEffects[i];
                if (renderAtom.Object is Appearances.TextualEffectInstance textualEffect) {

                }
            }
        }
        
        private void InternalUpdateOnscreenMessages() {
            if (m_WorldMapStorage.LayoutOnscreenMessages) {
                LayoutOnscreenMessages();
                m_WorldMapStorage.LayoutOnscreenMessages = false;
            }

            int animationDeltaX = 0;
            int animationDeltaY = 0;

            if (m_Player != null) {
                animationDeltaX = m_Player.AnimationDelta.x;
                animationDeltaY = m_Player.AnimationDelta.y;
            }

            var messageBoxes = m_WorldMapStorage.MessageBoxes;
            var length = messageBoxes.Count - 1;
            while (length >= (int)MessageScreenTargets.BoxCoordinate) {
                var messageBox = messageBoxes[length];
                Vector3Int mapPosition = m_WorldMapStorage.ToMapClosest(messageBox.Position.Value);
                float x = ((mapPosition.x - 0.5f) * Constants.FieldSize - m_Player.AnimationDelta.x) * m_LayerZoom.x;
                float y = ((mapPosition.y - 1f) * Constants.FieldSize - m_Player.AnimationDelta.y) * m_LayerZoom.y;
                messageBox.UpdateTextMeshPosition(x, -y);
                length--;
            }
        }

        private void InternalDrawCreatureStatusClassic(RenderAtom renderAtom, float rectX, float rectY, bool isVisible, bool drawNames, bool drawHealth, bool drawMana, bool drawFlags) {
            var creature = renderAtom.Object as Creatures.Creature;
            if (creature.Position.z != m_Player.Position.z) {
                RemoveCreatureStatusPanel(creature.ID);
                return;
            }

            var statusPanel = AddOrGetCreatureStatusPanel(creature);
            statusPanel.CachedRenderCount = m_RenderCounter;
            statusPanel.UpdateCreatureMisc(isVisible, m_LightmapRenderer.CalculateCreatureBrightnessFactor(creature, creature.ID == m_Player.ID));
            statusPanel.SetDrawingProperties(drawNames, drawHealth, drawMana);
            statusPanel.SetFlags(drawFlags, creature.PartyFlag, creature.PKFlag, creature.SummonTypeFlag, creature.SpeechCategory, creature.GuildFlag);

            // the animation delta is already applied earlier on creatures check
            // but we need to add the animation delta of the screen
            float x = renderAtom.x - 2 * Constants.FieldSize - m_Player.AnimationDelta.x + 27 / 2f;
            float y = renderAtom.y - 2 * Constants.FieldSize - m_Player.AnimationDelta.y - 8;

            var rectTransform = statusPanel.transform as RectTransform;
            rectTransform.anchoredPosition = new Vector2(x * m_LayerZoom.x, -y * m_LayerZoom.y);
        }

        private void InternalDrawCreatureStatusHUD(RenderAtom renderAtom, int rectX, int rectY, bool isVisible, bool drawNames, bool drawHealth, bool drawMana, bool drawFlags) {
            // TODO
        }

        private Components.CreatureStatusPanel AddOrGetCreatureStatusPanel(Creatures.Creature creature) {
            int index = 0;
            int lastIndex = m_CreaturesStatus.Count - 1;
            while (index <= lastIndex) {
                int tmpIndex = index + lastIndex >> 1;
                var foundPanel = m_CreaturesStatus[tmpIndex];
                if (foundPanel.CreatureID < creature.ID)
                    index = tmpIndex + 1;
                else if (foundPanel.CreatureID > creature.ID)
                    lastIndex = tmpIndex - 1;
                else
                    return foundPanel;
            }

            var panel = UnityEngine.Object.Instantiate(OpenTibiaUnity.GameManager.CreatureStatusPanelPrefab,
                    OpenTibiaUnity.GameManager.CreatureStatusContainer);

            panel.CreatureID = creature.ID;
            panel.name = "CreatureStatus_" + creature.Name;
            panel.UpdateProperties(creature.Name, creature.HealthPercent, creature.ManaPercent);

            m_CreaturesStatus.Insert(index, panel);
            return panel;
        }

        private bool RemoveCreatureStatusPanel(uint ID) {
            int lastIndex = m_CreaturesStatus.Count - 1;
            int index = 0;
            int foundIndex = -1;
            Components.CreatureStatusPanel foundPanel = null;
            while (index < lastIndex) {
                int tmpIndex = index + lastIndex >> 1;
                foundPanel = m_CreaturesStatus[tmpIndex];
                if (foundPanel.CreatureID > ID) {
                    index = tmpIndex + 1;
                } else if (foundPanel.CreatureID < ID) {
                    lastIndex = tmpIndex - 1;
                } else {
                    foundIndex = tmpIndex;
                    break;
                }
            }

            if (foundIndex == -1)
                return false;

            m_CreaturesStatus.RemoveAt(foundIndex);
            UnityEngine.Object.Destroy(foundPanel.gameObject);
            return true;
        }

        public Components.CreatureStatusPanel FindCreatureStatusPanel(Creatures.Creature creature) {
            int index = 0;
            int lastIndex = m_CreaturesStatus.Count - 1;
            while (index <= lastIndex) {
                int tmpIndex = index + lastIndex >> 1;
                var foundPanel = m_CreaturesStatus[tmpIndex];
                if (foundPanel.CreatureID < creature.ID)
                    index = tmpIndex + 1;
                else if (foundPanel.CreatureID > creature.ID)
                    lastIndex = tmpIndex - 1;
                else
                    return foundPanel;
            }

            return null;
        }
        
        private void LayoutOnscreenMessages() {
            foreach (var messageBox in m_WorldMapStorage.MessageBoxes) {
                if (messageBox.Visible) {
                    messageBox.ResetTextMesh();
                    if (!messageBox.Empty)
                        messageBox.ArrangeMessages();
                }
            }
        }

        public void OnCreatureNameChange(Creatures.Creature creature, string newName, string oldName) {
            Components.CreatureStatusPanel statusPanel = FindCreatureStatusPanel(creature);
            if (statusPanel)
                statusPanel.SetCharacterName(newName);
        }

        public void OnCreatureSkillChange(Creatures.Creature creature, SkillTypes skillType, Creatures.SkillStruct skill) {
            Components.CreatureStatusPanel statusPanel = FindCreatureStatusPanel(creature);
            if (!statusPanel)
                return;

            if (skillType == SkillTypes.HealthPercent) {
                statusPanel.SetHealthPercent(skill.level);
                statusPanel.UpdateHealthColor();
            } else if (skillType == SkillTypes.Mana) {
                statusPanel.SetMana(skill.level, skill.baseLevel);
            } else if (skillType == SkillTypes.Health) {
                statusPanel.SetHealth(skill.level, skill.baseLevel);
                statusPanel.UpdateHealthColor();
            }
        }

        public Vector3Int? PointToMap(Vector2 point) {
            int x = (int)(point.x / (Constants.FieldSize * m_LayerZoom.x)) + 1;
            int y = (int)(point.y / (Constants.FieldSize * m_LayerZoom.y)) + 1;

            if (x < 0 || x > Constants.MapWidth || y < 0 || y > Constants.MapHeight)
                return null;

            Vector3Int mapPosition = new Vector3Int(x, y, 0);

            int minZ = m_MinZPlane[y * Constants.MapSizeX + x];
            int z = m_MaxZPlane;

            while (z > minZ) {
                var field = m_WorldMapStorage.GetField(x, y, z);
                for (int i = 0; i < field.ObjectsCount; i++) {
                    var type = field.ObjectsRenderer[i].Type;
                    if ((type.IsGround || type.IsBottom) && !type.IsIgnoreLook) {
                        minZ = -1;
                        break;
                    }
                }

                if (minZ < 0)
                    break;

                z--;
            }

            if (z < 0 || z >= Constants.MapSizeZ)
                return null;

            mapPosition.z = z;
            return mapPosition;
        }

        public Vector3Int? PointToAbsolute(Vector2 point) {
            var mapPosition = PointToMap(point);
            if (mapPosition.HasValue)
                return m_WorldMapStorage.ToAbsolute(mapPosition.Value);
            return null;
        }

        public Creatures.Creature PointToCreature(Vector2 point, bool restrictedToPlayerZPlane) {
            if (m_WorldMapStorage == null || (restrictedToPlayerZPlane && !m_Player))
                return null;
            
            Vector3Int? tmpPosition = PointToMap(point);
            if (!tmpPosition.HasValue)
                return null;
            
            var mapPosition = tmpPosition.Value;
            if (restrictedToPlayerZPlane)
                mapPosition.z = m_WorldMapStorage.ToMap(m_Player.Position).z;
            
            var field = m_WorldMapStorage.GetField(mapPosition);
            if (!field)
                return null;

            int index = field.GetTopCreatureObject(out Appearances.ObjectInstance obj);
            if (index == -1)
                return null;
            
            return m_CreatureStorage.GetCreature(obj.Data);
        }
    } // class 
} // ns
