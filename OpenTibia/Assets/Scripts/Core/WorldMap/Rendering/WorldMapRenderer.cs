using System;
using System.Collections.Generic;
using UnityEngine;

namespace OpenTibiaUnity.Core.WorldMap.Rendering
{
    public class WorldMapRenderer {
        private int _drawnCreaturesCount = 0;
        private int _drawnTextualEffectsCount = 0;
        private int _maxZPlane = 0;
        private int _playerZPlane = 0;
        private float _highlightOpacity = Constants.HighlightMinOpacity;
        private Vector2 _hangPixel = Vector2.zero;
        private Vector3Int _hangPattern = Vector3Int.zero;
        private Creatures.Creature _highlightCreature;
        private Dictionary<int, CreatureStatus> _creatureStatusCache = new Dictionary<int, CreatureStatus>();

        private readonly int[] _minZPlane;
        private readonly float[] _highlightOpacities;
        private readonly int[] _creatureCount;
        private readonly RenderAtom[][] _creatureField;
        private readonly RenderAtom[] _drawnCreatures;
        private readonly RenderAtom[] _drawnTextualEffects;

        private TileCursor _tileCursor = new TileCursor();
        private Appearances.ObjectInstance _previousHang = null;
        private LightmapRenderer _lightmapRenderer = new MeshBasedLightmapRenderer();
        private Appearances.Rendering.MarksView _creaturesMarksView = new Appearances.Rendering.MarksView(0);
        
        protected WorldMapStorage WorldMapStorage { get => OpenTibiaUnity.WorldMapStorage; }
        protected Creatures.CreatureStorage CreatureStorage { get => OpenTibiaUnity.CreatureStorage; }
        protected Creatures.Player Player { get => OpenTibiaUnity.Player; }
        protected Options.OptionStorage OptionStorage { get => OpenTibiaUnity.OptionStorage; }
        
        private Rect _lastWorldMapLayerRect = Rect.zero;
        private Rect _clipRect = Rect.zero;
        private Rect _unclamppedClipRect = Rect.zero;
        private Vector2 _realScreenTranslation = Vector2.zero;

        private uint _renderCounter = 0; // initially the first render
        private uint _lastRenderCounter = 0;
        private uint _clipRectRenderCounter = 0;
        private float _lastRenderTime = 0f;

        public int Framerate { get; private set; } = 0;
        
        public Vector3Int? HighlightTile { get; set; }
        public object HighlightObject { get; set; }

        public Vector2 ScreenZoom { get; private set; } = new Vector2();
        public Vector2 LayerZoom { get; private set; } = new Vector2();

        public WorldMapRenderer() {
            int mapCapacity = Constants.MapSizeX * Constants.MapSizeY;

            _minZPlane = new int[mapCapacity];
            _creatureField = new RenderAtom[mapCapacity][];
            _creatureCount = new int[mapCapacity];
            _drawnCreatures = new RenderAtom[mapCapacity * Constants.MapSizeW];

            for (int i = 0; i < mapCapacity; i++) {
                _creatureField[i] = new RenderAtom[Constants.MapSizeW];
                for (int j = 0; j < Constants.MapSizeW; j++)
                    _creatureField[i][j] = new RenderAtom();
                _creatureCount[i] = 0;
            }

            for (int i = 0; i < _drawnCreatures.Length; i++)
                _drawnCreatures[i] = new RenderAtom();

            _drawnTextualEffects = new RenderAtom[Constants.NumEffects];
            for (int i = 0; i < _drawnTextualEffects.Length; i++)
                _drawnTextualEffects[i] = new RenderAtom();

            _creaturesMarksView.AddMarkToView(MarkType.ClientMapWindow, Constants.MarkThicknessBold);
            _creaturesMarksView.AddMarkToView(MarkType.Permenant, Constants.MarkThicknessBold);
            _creaturesMarksView.AddMarkToView(MarkType.OneSecondTemp, Constants.MarkThicknessBold);

            _tileCursor.FrameDuration = 100;

            _highlightOpacities = new float[8 * 2 - 2];
            for (int i = 0; i < 8; i++)
                _highlightOpacities[i] = Constants.HighlightMinOpacity + (i / 7f) * (Constants.HighlightMaxOpacity - Constants.HighlightMinOpacity);
            for (int i = 1; i < 7; i++)
                _highlightOpacities[7 + i] = Constants.HighlightMaxOpacity - (i / 7f) * (Constants.HighlightMaxOpacity - Constants.HighlightMinOpacity);

            // enable this if you intend to change the name during the game..
        }
        
        public Rect CalculateClipRect() {
            if (_clipRectRenderCounter == _renderCounter)
                return _clipRect;

            float width = Constants.WorldMapRealWidth;
            float height = Constants.WorldMapRealHeight;

            // (0, 0) respects to the bottomleft
            float x = 2 * Constants.FieldSize + Player.AnimationDelta.x;
            float y = Constants.FieldSize - Player.AnimationDelta.y;

            _unclamppedClipRect = new Rect(x, y, width, height);

            width /= Constants.WorldMapScreenWidth;
            x /= Constants.WorldMapScreenWidth;
            height /= Constants.WorldMapScreenHeight;
            y /= Constants.WorldMapScreenHeight;

            _clipRect = new Rect(x, y, width, height);
            _clipRectRenderCounter = _renderCounter;
            return _clipRect;
        }

        public RenderError RenderWorldMap(Rect worldMapLayerRect) {
            if (WorldMapStorage == null || CreatureStorage == null || Player == null || !WorldMapStorage.Valid)
                return RenderError.WorldMapNotValid;
            
            _lastWorldMapLayerRect = worldMapLayerRect;
            if (_lastWorldMapLayerRect.width < Constants.WorldMapMinimumWidth || _lastWorldMapLayerRect.height < Constants.WorldMapMinimumHeight)
                return RenderError.SizeNotEffecient;
            
            var screenSize = new Vector2(Screen.width, Screen.height);
            ScreenZoom = new Vector2(screenSize.x / Constants.WorldMapScreenWidth,
                            screenSize.y / Constants.WorldMapScreenHeight);
            
            LayerZoom = new Vector2(_lastWorldMapLayerRect.width / Constants.WorldMapRealWidth,
                           _lastWorldMapLayerRect.height / Constants.WorldMapRealHeight);
            
            WorldMapStorage.Animate();
            CreatureStorage.Animate();
            _highlightOpacity = _highlightOpacities[(OpenTibiaUnity.TicksMillis / Constants.HighlightObjectOpacityInterval) % _highlightOpacities.Length];

            _drawnCreaturesCount = 0;
            _drawnTextualEffectsCount = 0;
            _renderCounter++;
            
            if (Time.time - _lastRenderTime > 0.5f) {
                var currentTime = Time.time;
                Framerate = (int)((_renderCounter - _lastRenderCounter) / (currentTime - _lastRenderTime));
                _lastRenderTime = currentTime;
                _lastRenderCounter = _renderCounter;
            }
            
            _playerZPlane = WorldMapStorage.ToMap(Player.Position).z;
            
            UpdateMinMaxZPlane();
            
            if (HighlightObject is Creatures.Creature tmpCreature)
                _highlightCreature = tmpCreature;
            else if (HighlightObject is Appearances.ObjectInstance tmpObject && tmpObject.IsCreature)
                _highlightCreature = CreatureStorage.GetCreature(tmpObject.Data);
            else
                _highlightCreature = null;
            
            for (int z = 0; z <= _maxZPlane; z++) {
                for (int i = 0; i < _creatureCount.Length; i++)
                    _creatureCount[i] = 0;
            
                InternalUpdateFloor(z);
                InternalDrawFields(z);
            }
            
            if (OptionStorage.ShowLightEffects) {
                var lightmapTexture = _lightmapRenderer.CreateLightmap();
                var lightmapRect = new Rect() {
                    x = (Constants.FieldSize / 2) * ScreenZoom.y,
                    y = (Constants.FieldSize / 2) * ScreenZoom.y,
                    width = Constants.WorldMapScreenWidth * ScreenZoom.x,
                    height = Constants.WorldMapScreenHeight * ScreenZoom.y,
                };
            
                Graphics.DrawTexture(lightmapRect, lightmapTexture, OpenTibiaUnity.GameManager.LightSurfaceMaterial);
            }
            
            return RenderError.None;
        }

        public void RenderOnscreenText(Rect viewport) {
            _realScreenTranslation = viewport.position;

            InternelDrawCreatureStatus();
            InternalDrawTextualEffects();
            InternalDrawOnscreenMessages();
        }
        
        private void UpdateMinMaxZPlane() {
            if (!WorldMapStorage.CacheUnsight) {
                _maxZPlane = Constants.MapSizeZ - 1;
                while (_maxZPlane > _playerZPlane && WorldMapStorage.GetObjectPerLayer(_maxZPlane) <= 0)
                    _maxZPlane--;

                for (int x = Constants.PlayerOffsetX - 1; x <= Constants.PlayerOffsetX + 1; x++) {
                    for (int y = Constants.PlayerOffsetY - 1; y <= Constants.PlayerOffsetY + 1; y++) {
                        if (!(x != Constants.PlayerOffsetX && y != Constants.PlayerOffsetY || !WorldMapStorage.IsLookPossible(x, y, _playerZPlane))) {
                            int z = _playerZPlane + 1;
                            while (z - 1 < _maxZPlane && x + _playerZPlane - z >= 0 && y + _playerZPlane - z >= 0) {
                                var @object = WorldMapStorage.GetObject(x + _playerZPlane - z, y + _playerZPlane - z, z, 0);
                                if (!!@object && !!@object.Type && @object.Type.IsGround && !@object.Type.IsDontHide) {
                                    _maxZPlane = z - 1;
                                    continue;
                                }

                                @object = WorldMapStorage.GetObject(x, y, z, 0);
                                if (!!@object && !!@object.Type && (@object.Type.IsGround || @object.Type.IsBottom) && !@object.Type.IsDontHide) {
                                    _maxZPlane = z - 1;
                                    continue;
                                }
                                z++;
                            }
                        }
                    }
                }

                WorldMapStorage.CacheUnsight = true;
            }

            if (!WorldMapStorage.CacheFullbank) {
                for (int y = 0; y < Constants.MapSizeY; y++) {
                    for (int x = 0; x < Constants.MapSizeX; x++) {
                        int index = y * Constants.MapSizeX + x;

                        _minZPlane[index] = 0;
                        for (int z = _maxZPlane; z > 0; z--) {
                            bool covered = true, done = false;
                            for (int ix = 0; ix < 2 && !done; ix++) {
                                for (int iy = 0; iy < 2 && !done; iy++) {
                                    int mx = x + ix;
                                    int my = y + iy;
                                    if (mx < Constants.MapSizeX && my < Constants.MapSizeY) {
                                        Appearances.ObjectInstance @object = WorldMapStorage.GetObject(mx, my, z, 0);
                                        if (!@object || (!!@object.Type && !@object.Type.IsFullGround)) {
                                            covered = false;
                                            done = true;
                                        }
                                    }
                                }
                            }
                            
                            if (covered)
                                _minZPlane[index] = z;
                        }
                    }
                }

                WorldMapStorage.CacheFullbank = true;
            }
        }
        
        private void InternalUpdateFloor(int z) {
            RenderAtom renderAtom = null;
            
            bool aboveGround = Player.Position.z <= 7;
            int brightness = aboveGround ? WorldMapStorage.AmbientCurrentBrightness : 0;
            for (int i = 0; i < _creatureCount.Length; i++)
                _creatureCount[i] = 0;
            
            for (int x = 0; x < Constants.MapSizeX; x++) {
                for (int y = 0; y < Constants.MapSizeY; y++) {
                    Field field = WorldMapStorage.GetField(x, y, z);

                    // update field light
                    // todo; this shouldn't be called every frame unless
                    // light probes has changed
                    if (OptionStorage.ShowLightEffects) {
                        var colorIndex = _lightmapRenderer.ToColorIndex(x, y);
                        if (z == _playerZPlane && z > 0)
                            _lightmapRenderer[colorIndex] = Utils.Utility.MulColor32(_lightmapRenderer[colorIndex], OptionStorage.LightLevelSeparator / 100f);

                        Appearances.ObjectInstance @object;
                        if (z == 0 || (@object = field.ObjectsRenderer[0]) != null && @object.Type.IsGround) {
                            var color = _lightmapRenderer[colorIndex];
                            _lightmapRenderer.SetFieldBrightness(x, y, brightness, aboveGround);
                            if (z > 0 && field.CacheTranslucent) {
                                color = Utils.Utility.MulColor32(color, OptionStorage.LightLevelSeparator / 100f);
                                var alterColor = _lightmapRenderer[colorIndex];
                                if (color.r < alterColor.r)
                                    color.r = alterColor.r;
                                if (color.g < alterColor.g)
                                    color.g = alterColor.g;
                                if (color.b < alterColor.b)
                                    color.b = alterColor.b;
                                _lightmapRenderer[colorIndex] = color;
                            }
                        }

                        if (x > 0 && y > 0 && z < 7 && z == _playerZPlane + WorldMapStorage.Position.z - 8 && WorldMapStorage.IsTranslucent(x - 1, y - 1, z + 1))
                            _lightmapRenderer.SetFieldBrightness(x, y, WorldMapStorage.AmbientCurrentBrightness, aboveGround);
                    }

                    for (int i = field.ObjectsCount - 1; i >= 0; i--) {
                        var @object = field.ObjectsRenderer[i];
                        if (!@object.IsCreature)
                            continue;

                        var creature = CreatureStorage.GetCreature(@object.Data);
                        if (!creature)
                            continue;

                        Vector2Int displacement = Vector2Int.zero;
                        if (!!creature.MountOutfit && !!creature.MountOutfit.Type)
                            displacement = creature.MountOutfit.Type.Offset;
                        else if (!!creature.Outfit && !!creature.Outfit.Type)
                            displacement = creature.Outfit.Type.Offset;

                        int positionX = (x + 1) * Constants.FieldSize + creature.AnimationDelta.x - displacement.x;
                        int positionY = (y + 1) * Constants.FieldSize + creature.AnimationDelta.y - displacement.y;
                        int fieldX = (positionX - 1) / Constants.FieldSize;
                        int fieldY = (positionY - 1) / Constants.FieldSize;

                        int fieldIndex = fieldY * Constants.MapSizeX + fieldX;
                        if (!(fieldIndex < 0 || fieldIndex >= Constants.MapSizeX * Constants.MapSizeY)) {
                            RenderAtom[] fieldAtoms = _creatureField[fieldIndex];
                            int j = 0;
                            while (j < _creatureCount[fieldIndex] && !!(renderAtom = fieldAtoms[j]) && (renderAtom.y < positionY || renderAtom.y == positionY && renderAtom.x <= positionX))
                                j++;

                            if (j < Constants.MapSizeZ) {
                                if (_creatureCount[fieldIndex] < Constants.MapSizeW)
                                    _creatureCount[fieldIndex]++;

                                renderAtom = fieldAtoms[_creatureCount[fieldIndex] - 1];
                                for (int k = _creatureCount[fieldIndex] - 1; k > j; k--)
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
            var absolutePosition = WorldMapStorage.ToAbsolute(new Vector3Int(0, 0, z));
            int size = Constants.MapSizeX + Constants.MapSizeY;
            for (int i = 0; i < size; i++) {
                int y = Math.Max(i - Constants.MapSizeX + 1, 0);
                int x = Math.Min(i, Constants.MapSizeX - 1);
                while (x >= 0 && y < Constants.MapSizeY) {
                    InternalDrawField(
                        (x + 1) * Constants.FieldSize,
                        (y + 1) * Constants.FieldSize,
                        absolutePosition.x + x,
                        absolutePosition.y + y,
                        absolutePosition.z,
                        x, y, z,
                        true);
                    x--;
                    y++;
                }
                
                if (OptionStorage.HighlightMouseTarget && HighlightTile.HasValue && HighlightTile.Value.z == z) {
                    _tileCursor.DrawTo((HighlightTile.Value.x + 1f) * Constants.FieldSize, (HighlightTile.Value.y + 1f) * Constants.FieldSize, ScreenZoom, OpenTibiaUnity.TicksMillis);
                }
            }
        }

        private void InternalDrawField(float rectX, float rectY, int absoluteX, int absoluteY, int absoluteZ, int positionX, int positionY, int positionZ, bool drawLyingObjects) {

            Field field = WorldMapStorage.GetField(positionX, positionY, positionZ);
            
            int objectsCount = field.ObjectsCount;
            int fieldIndex = positionY * Constants.MapSizeX + positionX;
            
            bool isCovered = positionZ > _minZPlane[fieldIndex]
                || (positionX == 0 || positionZ >= _minZPlane[fieldIndex - 1])
                || (positionY == 0 || positionZ >= _minZPlane[fieldIndex - Constants.MapSizeX])
                || (positionX == 0 && positionY == 0 || positionZ >= _minZPlane[fieldIndex - Constants.MapSizeX - 1]);
            
            int objectsHeight = 0;
            // draw ground/bottom objects
            if (drawLyingObjects && objectsCount > 0 && isCovered)
                InternalDrawFieldObjects(rectX, rectY, absoluteX, absoluteY, absoluteZ, positionX, positionY, positionZ, field, ref objectsHeight);

            // draw creatures
            InternalDrawFieldCreatures(positionX, positionY, positionZ, drawLyingObjects, isCovered, objectsHeight);

            // draw effects
            InternalDrawFieldEffects(rectX, rectY, absoluteX, absoluteY, absoluteZ, positionZ, drawLyingObjects, field, objectsHeight);

            // draw top objects
            if (drawLyingObjects)
                InternalDrawFieldTopObjects(rectX, rectY, absoluteX, absoluteY, absoluteZ, field);
        }

        private void InternalDrawFieldObjects(float rectX, float rectY, int absoluteX, int absoluteY, int absoluteZ, int positionX, int positionY, int positionZ, Field field, ref int objectsHeight) {
            Appearances.ObjectInstance hangObject = null;
            bool isLying = false;

            // draw Items in reverse order
            for (int i = 0; i < field.ObjectsCount; i++) {
                var @object = field.ObjectsRenderer[i];
                var type = @object.Type;
                if (@object.IsCreature || type.IsTop)
                    break;

                if (OptionStorage.ShowLightEffects && type.IsLight) {
                    // check how correct those are
                    int lightX = ((int)rectX - objectsHeight - (int)type.OffsetX) / Constants.FieldSize;
                    int lightY = ((int)rectY - objectsHeight - (int)type.OffsetY) / Constants.FieldSize;
                    Color32 color32 = Colors.ColorFrom8Bit((byte)type.LightColor);

                    _lightmapRenderer.SetLightSource(lightX, lightY, positionZ, type.Brightness, color32);
                }

                var screenPosition = new Vector2(rectX - objectsHeight, rectY - objectsHeight);
                bool highlighted = HighlightObject is Appearances.ObjectInstance && HighlightObject == @object;
                @object.Draw(screenPosition, ScreenZoom, absoluteX, absoluteY, absoluteZ, highlighted, _highlightOpacity);

                isLying = isLying || type.IsLyingCorpse;
                if (type.IsHangable && @object.Hang == Appearances.AppearanceInstance.HookSouth)
                    hangObject = @object;

                if (type.HasElevation)
                    objectsHeight = Mathf.Min(objectsHeight + (int)type.Elevation, Constants.FieldHeight);
            }

            // lying tile, draw lying tile
            if (isLying) {
                if (positionX > 0 && positionY > 0)
                    InternalDrawField(rectX - Constants.FieldSize, rectY - Constants.FieldSize, absoluteX - 1, absoluteY - 1, absoluteZ, positionX - 1, positionY - 1, positionZ, false);
                else if (positionX > 0)
                    InternalDrawField(rectX - Constants.FieldSize, rectY, absoluteX - 1, absoluteY, absoluteZ, positionX - 1, positionY, positionZ, false);
                else if (positionY > 0)
                    InternalDrawField(rectX, rectY - Constants.FieldSize, absoluteX, absoluteY - 1, absoluteZ, positionX, positionY - 1, positionZ, false);
            }

            // draw hang object
            if (!!_previousHang) {
                bool highlighted = HighlightObject is Appearances.ObjectInstance && HighlightObject == _previousHang;
                _previousHang.Draw(_hangPixel, ScreenZoom, _hangPattern.x, _hangPattern.y, _hangPattern.z, highlighted, _highlightOpacity);
                _previousHang = null;
            }

            if (!!hangObject) {
                _previousHang = hangObject;
                _hangPixel.Set(rectX, rectY);
                _hangPattern.Set(absoluteX, absoluteY, absoluteZ);
            }
        }

        private void InternalDrawFieldCreatures(int positionX, int positionY, int positionZ, bool drawLyingObjects, bool isCovered, int objectsHeight) {
            RenderAtom[] renderAtomArray = _creatureField[positionY * Constants.MapSizeX + positionX];
            int creatureCount = _creatureCount[positionY * Constants.MapSizeX + positionX];
            for (int i = creatureCount - 1; i >= 0; i--) {
                RenderAtom renderAtom = null;
                Creatures.Creature creature = null;
                if (!(renderAtom = renderAtomArray[i]) || !(creature = renderAtom.Object as Creatures.Creature) || !creature.Outfit)
                    continue;

                if (drawLyingObjects) {
                    renderAtom.x -= objectsHeight;
                    renderAtom.y -= objectsHeight;
                }

                bool highlighted = !!_highlightCreature && _highlightCreature.Id == creature.Id;

                // marks
                _creaturesMarksView.DrawMarks(creature.Marks, renderAtom.x, renderAtom.y, ScreenZoom);

                var offset = Vector2Int.zero;
                if (isCovered && !!creature.MountOutfit) {
                    offset += creature.MountOutfit.Type.Offset;
                    var screenPosition = new Vector2(renderAtom.x + offset.x, renderAtom.y + offset.y);
                    creature.MountOutfit.Draw(screenPosition, ScreenZoom, (int)creature.Direction, 0, 0, highlighted, _highlightOpacity);
                }

                if (isCovered) {
                    offset += creature.Outfit.Type.Offset;
                    var screenPosition = new Vector2(renderAtom.x + offset.x, renderAtom.y + offset.y);
                    creature.Outfit.Draw(screenPosition, ScreenZoom, (int)creature.Direction, 0, !!creature.MountOutfit ? 1 : 0, highlighted, _highlightOpacity);
                }

                if (positionZ == _playerZPlane && (CreatureStorage.IsOpponent(creature) || creature.Id == Player.Id)) {
                    _drawnCreatures[_drawnCreaturesCount].Assign(renderAtom);
                    _drawnCreaturesCount++;
                }

                if (isCovered && drawLyingObjects && OptionStorage.ShowLightEffects) {
                    int lightX = renderAtom.x / Constants.FieldSize;
                    int lightY = renderAtom.y / Constants.FieldSize;

                    _lightmapRenderer.SetLightSource(lightX, lightY, positionZ, (uint)creature.Brightness, creature.LightColor);

                    if (!!creature.MountOutfit && creature.MountOutfit.Type.IsLight) {
                        var color = Colors.ColorFrom8Bit((byte)creature.MountOutfit.Type.LightColor);
                        _lightmapRenderer.SetLightSource(lightX, lightY, positionZ, creature.MountOutfit.Type.Brightness, color);
                    }

                    if (!!creature.Outfit && creature.Outfit.Type.IsLight) {
                        var color = Colors.ColorFrom8Bit((byte)creature.Outfit.Type.LightColor);
                        _lightmapRenderer.SetLightSource(lightX, lightY, positionZ, creature.Outfit.Type.Brightness, color);
                    }

                    if (creature.Id == Player.Id && creature.Brightness < 2) {
                        var color = new Color32(255, 255, 255, 255);
                        _lightmapRenderer.SetLightSource(lightX, lightY, positionZ, 2, color);
                    }
                }
            }
        }

        private void InternalDrawFieldEffects(float rectX, float rectY, int absoluteX, int absoluteY, int absoluteZ, int positionZ, bool drawLyingObjects, Field field, int objectsHeight) {
            int effectRectX = (int)rectX - objectsHeight;
            int effectRectY = (int)rectY - objectsHeight;
            int effectLightX = effectRectX / Constants.FieldSize;
            int effectLightY = effectRectY / Constants.FieldSize;

            int totalTextualEffectsWidth = 0;
            int lastTextualEffectY = 0;

            for (int i = field.EffectsCount - 1; i >= 0; i--) {
                var effect = field.Effects[i];
                if (!effect)
                    continue;

                if (effect is Appearances.TextualEffectInstance textualEffect) {
                    if (drawLyingObjects) {
                        var renderAtom = _drawnTextualEffects[_drawnTextualEffectsCount];
                        int x = effectRectX + Constants.FieldSize / 2 + totalTextualEffectsWidth;
                        int y = effectRectY + Constants.FieldSize / 8 - 2 * textualEffect.Phase;
                        renderAtom.Update(textualEffect, x, y, 0);

                        if (renderAtom.y + textualEffect.Height > lastTextualEffectY)
                            totalTextualEffectsWidth += (int)textualEffect.Width;

                        if (totalTextualEffectsWidth < 2 * Constants.FieldSize) {
                            _drawnTextualEffectsCount++;
                            lastTextualEffectY = renderAtom.y;
                        }
                    }
                } else if (effect is Appearances.MissileInstance missileEffect) {
                    var screenPosition = new Vector2(effectRectX + missileEffect.AnimationDelta.x, effectRectY + missileEffect.AnimationDelta.y);
                    effect.Draw(screenPosition, ScreenZoom, absoluteX, absoluteY, absoluteZ);

                    if (drawLyingObjects && OptionStorage.ShowLightEffects && effect.Type.IsLight) {
                        var color = Colors.ColorFrom8Bit((byte)effect.Type.LightColor);
                        _lightmapRenderer.SetLightSource(effectLightX, effectLightY, positionZ, effect.Type.Brightness, color);
                    }
                } else { // EffectInstance
                    var screenPosition = new Vector2(effectRectX, effectRectY);
                    effect.Draw(screenPosition, ScreenZoom, absoluteX, absoluteY, absoluteZ);

                    if (drawLyingObjects && OptionStorage.ShowLightEffects && effect.Type.IsLight) {
                        uint activeBrightness = (uint)((Math.Min(effect.Phase, effect.Type.FrameGroups[0].SpriteInfo.Phases + 1 - effect.Phase) * effect.Type.Brightness + 2) / 3);
                        var color = Colors.ColorFrom8Bit((byte)effect.Type.LightColor);
                        _lightmapRenderer.SetLightSource(effectLightX, effectLightY, positionZ, Math.Min(activeBrightness, effect.Type.Brightness), color);
                    }
                }
            }
        }

        private void InternalDrawFieldTopObjects(float rectX, float rectY, int absoluteX, int absoluteY, int absoluteZ, Field field) {
            for (int i = 0; i < field.ObjectsCount; i++) {
                var @object = field.ObjectsRenderer[i];
                if (@object.Type.IsTop) {
                    var screenPosition = new Vector2(rectX, rectY);
                    bool highlighted = HighlightObject is Appearances.ObjectInstance && HighlightObject == @object;
                    @object.Draw(screenPosition, ScreenZoom, absoluteX, absoluteY, absoluteZ, highlighted, _highlightOpacity);
                }
            }
        }

        private void InternelDrawCreatureStatus() {
            int playerIndex = -1;

            var gameManager = OpenTibiaUnity.GameManager;
            var optionStorage = OpenTibiaUnity.OptionStorage;

            for (int i = 0; i < _drawnCreaturesCount; i++) {
                var renderAtom = _drawnCreatures[i];
                if (renderAtom.Object is Creatures.Creature creature) {
                    if (creature.Id == Player.Id) {
                        playerIndex = i;
                    } else if (optionStorage.ShowHUDForOtherCreatures) {
                        int positionX = renderAtom.x / Constants.FieldSize;
                        int positionY = renderAtom.y / Constants.FieldSize;
                        var explicitlyVisible = renderAtom.z >= _minZPlane[positionY * Constants.MapSizeX + positionX];
                        
                        InternelDrawCreatureStatusClassic(creature, renderAtom.x, renderAtom.y, explicitlyVisible,
                            optionStorage.ShowNameForOtherCreatures,
                            optionStorage.ShowHealthForOtherCreatures && (!creature.IsNPC || !gameManager.GetFeature(GameFeature.GameHideNpcNames)),
                            false,
                            optionStorage.ShowMarksForOtherCreatures,
                            optionStorage.ShowNPCIcons);
                    }
                }
            }

            if (playerIndex != -1) {
                var renderAtom = _drawnCreatures[playerIndex];
                int positionX = renderAtom.x / Constants.FieldSize;
                int positionY = renderAtom.y / Constants.FieldSize;
                var explicitlyVisible = renderAtom.z >= _minZPlane[positionY * Constants.MapSizeX + positionX];

                InternelDrawCreatureStatusClassic(renderAtom.Object as Creatures.Creature, renderAtom.x, renderAtom.y, explicitlyVisible,
                           optionStorage.ShowNameForOwnCharacter,
                           optionStorage.ShowHealthForOwnCharacter,
                           optionStorage.ShowManaForOwnCharacter,
                           optionStorage.ShowMarksForOwnCharacter,
                           false);
            }
        }

        private void InternalDrawTextualEffects() {
            for (int i = 0; i < _drawnTextualEffectsCount; i++) {
                var renderAtom = _drawnTextualEffects[i];
                if (renderAtom.Object is Appearances.TextualEffectInstance textualEffect) {
                    var screenPosition = new Vector2(renderAtom.x, renderAtom.y);
                    screenPosition.x = (screenPosition.x - 2 * Constants.FieldSize) * LayerZoom.x + _realScreenTranslation.x;
                    screenPosition.y = (screenPosition.y - 2 * Constants.FieldSize) * LayerZoom.y + _realScreenTranslation.y;
                    screenPosition.x -= Player.AnimationDelta.x * LayerZoom.x;
                    screenPosition.y -= Player.AnimationDelta.y * LayerZoom.y;

                    textualEffect.Draw(screenPosition, Vector2.one, 0, 0, 0);
                }
            }
        }
        
        private void InternalDrawOnscreenMessages() {
            if (WorldMapStorage.LayoutOnscreenMessages) {
                InternalLayoutOnscreenMessages();
                WorldMapStorage.LayoutOnscreenMessages = false;
            }

            var messageBoxes = WorldMapStorage.MessageBoxes;
            
            float animationDeltaX = 0;
            float animationDeltaY = 0;
            if (Player != null) {
                animationDeltaX = Player.AnimationDelta.x * LayerZoom.x;
                animationDeltaY = Player.AnimationDelta.y * LayerZoom.y;
            }
            
            float bottomBoxHeight = 0;
            var bottomBox = messageBoxes[(int)MessageScreenTargets.BoxBottom];
            if (bottomBox != null && bottomBox.Visible && !bottomBox.Empty)
                bottomBoxHeight = bottomBox.Height;
            
            for (int i = messageBoxes.Count - 1; i >= (int)MessageScreenTargets.BoxLow; i--) {
                var messageBox = messageBoxes[i];
                if (messageBox != null && messageBox.Visible && !messageBox.Empty) {
                    var messagePosition = new Vector2(messageBox.X, messageBox.Y);

                    // if this text should be fixed to player's screen, then move it with his animation
                    if ((messageBox.Fixing & OnscreenMessageFixing.X) == 0)
                        messagePosition.x -= animationDeltaX;
                    if ((messageBox.Fixing & OnscreenMessageFixing.Y) == 0)
                        messagePosition.y -= animationDeltaY;
                    
                    //messagePosition.x = Mathf.Clamp(messagePosition.x, messageBox.Width / 2, Constants.WorldMapRealWidth - messageBox.Width / 2);
                    //messagePosition.y = Mathf.Clamp(messagePosition.y, messageBox.Height / 2, Constants.WorldMapRealHeight - bottomBoxHeight - messageBox.Height / 2);
                    
                    for (int j = 0; j < messageBox.VisibleMessages; j++) {
                        var message = messageBox.GetMessage(j);
                        var screenPosition = Vector2.zero;
                        screenPosition.x = messagePosition.x;

                        // if we have passed half the message, start aligning to the bottom
                        screenPosition.y = messagePosition.y - (messageBox.Height - message.Height) / 2;
                        message.Draw(screenPosition + _realScreenTranslation);
                        messagePosition.y += message.Height;
                    }
                }
            }

            if (bottomBox != null && bottomBox.Visible && !bottomBox.Empty) {
                var message = bottomBox.GetMessage(0);
                
                var messagePosition = new Vector2() {
                    x = Constants.WorldMapRealWidth * LayerZoom.x / 2,
                    y = Constants.WorldMapRealHeight * LayerZoom.y - (message.Height + Constants.OnscreenMessageGap) / 2
                };
                
                message.Draw(messagePosition + _realScreenTranslation);
            }
        }
        
        private void InternelDrawCreatureStatusClassic(Creatures.Creature creature, float rectX, float rectY, bool visible, bool showNames, bool showHealth, bool showMana, bool showMarks, bool showIcons) {
            bool isLocalPlayer = creature.Id == Player.Id;
            Color32 healthColor, manaColor;
            if (visible) {
                healthColor = Creatures.Creature.GetHealthColor(creature.HealthPercent);
                manaColor = Colors.ColorFromRGB(0, 0, 255);
            } else {
                healthColor = Colors.ColorFromRGB(192, 192, 192);
                manaColor = Colors.ColorFromRGB(192, 192, 192);
            }

            if (OptionStorage.ShowLightEffects) {
                float mod = _lightmapRenderer.CalculateCreatureBrightnessFactor(creature, isLocalPlayer);
                healthColor = Utils.Utility.MulColor32(healthColor, mod);
                manaColor = Utils.Utility.MulColor32(manaColor, mod);
            }

            var initialScreenPosition = new Vector2 {
                x = (rectX - 1.5f * Constants.FieldSize - Player.AnimationDelta.x) * LayerZoom.x + _realScreenTranslation.x,
                y = (rectY - 2 * Constants.FieldSize - Player.AnimationDelta.y) * LayerZoom.y + _realScreenTranslation.y
            };

            var screenPosition = initialScreenPosition;

            CreatureStatus status = null;
            if (showNames) {
                status = GetCreatureStatusCache(creature.Name, healthColor);
                screenPosition.y -= status.Height / 2;
            }

            if (showHealth)
                screenPosition.y -= 4 + 1;

            if (showMana)
                screenPosition.y -= 4 + 1;
            
            if (showNames) {
                status.Draw(screenPosition);
                screenPosition.y += (status.Height / 2) + 1;
            }

            if (showHealth || showMana) {
                var baseRect = new Rect {
                    x = screenPosition.x - 27 / 2f,
                    y = screenPosition.y + 4,
                    width = 27,
                    height = 4,
                };

                var actualRect = new Rect {
                    x = baseRect.x + 1,
                    y = baseRect.y - 1,
                    width = (creature.HealthPercent / 4f),
                    height = 2,
                };

                if (showHealth) {
                    Utils.GraphicsUtility.DrawRect(baseRect, Vector3.one, Color.black);
                    Utils.GraphicsUtility.DrawRect(actualRect, Vector3.one, healthColor);
                    screenPosition.y += 4 + 1;
                    baseRect.y += 4 + 1;
                    actualRect.y += 4 + 1;
                }

                if (showMana) {
                    actualRect.width = (creature.ManaPercent / 4f);
                
                    Utils.GraphicsUtility.DrawRect(baseRect, Vector3.one, Color.black);
                    Utils.GraphicsUtility.DrawRect(actualRect, Vector3.one, manaColor);
                    screenPosition.y += 4 + 1;
                }
            }

            if (showMarks && !creature.IsNPC || showIcons && creature.IsNPC)
                InternalDrawCreatureFlags(creature, initialScreenPosition.x, initialScreenPosition.y, visible);
        }

        private void InternalDrawCreatureFlags(Creatures.Creature creature, float rectX, float rectY, bool visible) {
            if (!(creature.IsHuman || creature.IsSummon || creature.IsNPC) || !creature.HasFlag)
                return;
            
            var screenPosition = new Vector2(rectX + 16 - Constants.StateFlagSize + 4, rectY + 1);
            var flagSize = new Vector2(Constants.StateFlagSize, Constants.StateFlagSize);
            int dX = 0;
            
            var flagsTexture = OpenTibiaUnity.GameManager.StateFlagsTexture;
            if (creature.PartyFlag > PartyFlag.None) {
                var textureRect = NormalizeFlagRect(GetPartyFlagTextureRect(creature.PartyFlag), flagsTexture);
                Graphics.DrawTexture(new Rect(screenPosition, flagSize), flagsTexture, textureRect, 0, 0, 0, 0);

                dX += Constants.StateFlagGap + Constants.StateFlagSize;
                screenPosition.x += Constants.StateFlagGap + Constants.StateFlagSize;
            }

            if (creature.PKFlag > PKFlag.None) {
                var textureRect = NormalizeFlagRect(GetPKFlagTextureRect(creature.PKFlag), flagsTexture);
                Graphics.DrawTexture(new Rect(screenPosition, flagSize), flagsTexture, textureRect, 0, 0, 0, 0);

                screenPosition.x += Constants.StateFlagGap + Constants.StateFlagSize;
            }

            if (creature.SummonType > SummonType.None) {
                var textureRect = NormalizeFlagRect(GetSummonFlagTextureRect(creature.SummonType), flagsTexture);
                Graphics.DrawTexture(new Rect(screenPosition, flagSize), flagsTexture, textureRect, 0, 0, 0, 0);

                dX += Constants.StateFlagGap + Constants.StateFlagSize;
                screenPosition.x += Constants.StateFlagGap + Constants.StateFlagSize;
            }

            var speechCategory = creature.SpeechCategory;
            if (speechCategory == SpeechCategory.QuestTrader)
                speechCategory = OpenTibiaUnity.TicksMillis % 2048 <= 1024 ? SpeechCategory.Quest : SpeechCategory.Trader;

            if (speechCategory > SpeechCategory.None) {
                var speechTexture = OpenTibiaUnity.GameManager.SpeechFlagsTexture;
                var textureRect = NormalizeFlagRect(GetSpeechFlagTextureRect(speechCategory), speechTexture);
                var screenRect = new Rect(screenPosition, new Vector2(Constants.SpeechFlagSize, Constants.SpeechFlagSize));
                Graphics.DrawTexture(screenRect, speechTexture, textureRect, 0, 0, 0, 0);

                dX += Constants.StateFlagGap + Constants.SpeechFlagSize;
                screenPosition.x += Constants.StateFlagGap + Constants.SpeechFlagSize;
            }
            
            if (dX > 0)
                screenPosition.x -= dX;
            
            var gameManager = OpenTibiaUnity.GameManager;
            if ((gameManager.GetFeature(GameFeature.GameCreatureMarks) && gameManager.ClientVersion < 1185) || dX > 0)
                screenPosition.y += Constants.StateFlagGap + Constants.StateFlagSize;
            
            if (creature.GuildFlag > GuildFlag.None) {
                var textureRect = NormalizeFlagRect(GetGuildFlagTextureRect(creature.GuildFlag), flagsTexture);
                Graphics.DrawTexture(new Rect(screenPosition, flagSize), flagsTexture, textureRect, 0, 0, 0, 0);

                screenPosition.y += Constants.StateFlagGap + Constants.StateFlagSize;
            }
            
            if (creature.RisknessFlag > RisknessFlag.None) {
                var textureRect = NormalizeFlagRect(GetRisknessFlagTextureRect(creature.RisknessFlag), flagsTexture);
                Graphics.DrawTexture(new Rect(screenPosition, flagSize), flagsTexture, textureRect, 0, 0, 0, 0);
            }
        }

        private void InternalLayoutOnscreenMessages() {
            var messageBoxes = WorldMapStorage.MessageBoxes;
            for (int i = messageBoxes.Count - 1; i >= 0; i--) {
                var messageBox = WorldMapStorage.MessageBoxes[i];
                if (messageBox != null && messageBox.Visible && !messageBox.Empty)
                    messageBox.ArrangeMessages();
            }
            
            var helperPoint = new Vector2();

            var points = new List<Vector2>();
            var workerRects = new List<Rect>();
            var actualRects = new List<Rect>();

            // the screen zoom is mapped to the size of the field, so
            // in order to match the preferred zoom of the text, we must
            // multiply the field/screen size by the precaculated zoom
            // of the text
            var fieldSize = new Vector2(Constants.FieldSize, Constants.FieldSize) * LayerZoom;
            var unscaledSize = _lastWorldMapLayerRect.size;

            for (int i = messageBoxes.Count - 1; i >= (int)MessageScreenTargets.BoxLow; i--) {
                var messageBox = WorldMapStorage.MessageBoxes[i];
                if (messageBox != null && messageBox.Visible && !messageBox.Empty) {
                    if (i == (int)MessageScreenTargets.BoxLow) {
                        messageBox.Fixing = OnscreenMessageFixing.Both;
                        helperPoint.x = unscaledSize.x / 2;
                        helperPoint.y = (unscaledSize.y + fieldSize.y + messageBox.Height + Constants.OnscreenMessageGap) / 2;
                    } else if (i == (int)MessageScreenTargets.BoxHigh) {
                        messageBox.Fixing = OnscreenMessageFixing.Both;
                        helperPoint.x = unscaledSize.x / 2;
                        helperPoint.y = unscaledSize.y / 2;
                    } else if (i == (int)MessageScreenTargets.BoxTop) {
                        messageBox.Fixing = OnscreenMessageFixing.Both;
                        helperPoint.x = unscaledSize.x / 2;
                        helperPoint.y = unscaledSize.y / 4;
                    } else {
                        messageBox.Fixing = OnscreenMessageFixing.None;
                        var closestMapPosition = WorldMapStorage.ToMapClosest(messageBox.Position.Value);
                        helperPoint.x = (closestMapPosition.x - 0.5f) * fieldSize.x + Mathf.Max(0, Constants.FieldSize * Constants.MapWidth * LayerZoom.x - unscaledSize.x) / 2;
                        helperPoint.y = (closestMapPosition.y - 1) * fieldSize.y + Mathf.Max(0, Constants.FieldSize * Constants.MapHeight * LayerZoom.y - unscaledSize.y) / 2;
                    }

                    float w1 = messageBox.Width + Constants.OnscreenMessageGap;
                    float h1 = messageBox.Height + Constants.OnscreenMessageGap;
                    float x1 = helperPoint.x - w1 / 2;
                    float y1 = helperPoint.y - h1 / 2;

                    points.Add(new Vector2(0, 0));
                    workerRects.Add(new Rect(x1, y1, w1, h1));
                    actualRects.Add(new Rect(x1 + w1 / 2, y1 - h1 / 2, w1, h1));
                }
            }

            float sumMagnitude = float.MaxValue;
            float lastMagnitude = 0;
            do {
                lastMagnitude = sumMagnitude;
                sumMagnitude = 0;
                for (int i = workerRects.Count - 1; i >= 0; i--) {
                    Vector2 point = Vector2.zero;
                    var rect = workerRects[i];
                    var tmpLength = rect.size.magnitude;

                    // Explanation:
                    // for every other rectangle, calculate the intersection area
                    // the intersections' sizes are summed together, and then used to move the
                    // the resepctive rectangle either left/right
                    for (int j = workerRects.Count - 1; j >= 0; j--) {
                        if (i == j)
                            continue;

                        var otherRect = workerRects[j];
                        var intersection = Utils.Utility.Intersect(rect, otherRect);
                        if (intersection != Rect.zero) {
                            var center = new Vector2() {
                                x = Mathf.Floor(rect.x + rect.width / 2) - Mathf.Floor(otherRect.x + otherRect.width / 2),
                                y = Mathf.Floor(rect.y + rect.height / 2) - Mathf.Floor(otherRect.y + otherRect.height / 2)
                            };
                            
                            sumMagnitude += intersection.width * intersection.height * Mathf.Pow(0.5f, center.magnitude / tmpLength - 1);

                            if (center.x < 0)
                                point.x -= intersection.width;
                            else if (center.x > 0)
                                point.x += intersection.width;

                            if (center.y < 0)
                                point.y -= intersection.height;
                            else if (center.y > 0)
                                point.y += intersection.height;

                            if (center.x == 0 && center.y == 0 && j == workerRects.Count - 1)
                                point.x += i - j;
                        }
                    }

                    points[i] = point;
                }

                for (int i = workerRects.Count - 1; i >= 0; i--) {
                    var point = points[i];
                    var rect = workerRects[i];
                    var centerRect = actualRects[i];
                    rect.x = Mathf.Clamp(rect.x + (point.x > 0 ? 1 : -1), centerRect.xMin, centerRect.xMax);
                    rect.y = Mathf.Clamp(rect.y + (point.y > 0 ? 1 : -1), centerRect.yMin, centerRect.yMax);
                    workerRects[i] = rect;
                }
            } while (sumMagnitude < lastMagnitude);

            int pointIndex = 0;
            for (int i = messageBoxes.Count - 1; i >= (int)MessageScreenTargets.BoxLow; i--) {
                var messageBox = WorldMapStorage.MessageBoxes[i];
                if (messageBox != null && messageBox.Visible && !messageBox.Empty) {
                    var rect = workerRects[pointIndex];
                    var otherRect = actualRects[pointIndex];

                    messageBox.X = Mathf.Max(otherRect.xMin, Mathf.Min(rect.x, otherRect.xMax)) + Constants.OnscreenMessageGap / 2;
                    messageBox.Y = Mathf.Max(otherRect.yMin, Mathf.Min(rect.y, otherRect.yMax)) + Constants.OnscreenMessageGap / 2;
                    pointIndex++;
                }
            }
        }

        private CreatureStatus GetCreatureStatusCache(string name, Color32 color) {
            int hashCode = (name + color.ToString()).GetHashCode();
            if (_creatureStatusCache.TryGetValue(hashCode, out CreatureStatus creatureStatus))
                return creatureStatus;

            creatureStatus = new CreatureStatus(name, color);
            _creatureStatusCache.Add(hashCode, creatureStatus);
            return creatureStatus;
        }

        private Rect NormalizeFlagRect(Rect rect, Texture2D tex2D) {
            rect.x /= tex2D.width;
            rect.y = (tex2D.height - rect.y) / tex2D.height;
            rect.width /= tex2D.width;
            rect.height /= tex2D.height;
            return rect;
        }

        private Rect GetPartyFlagTextureRect(PartyFlag partyFlag) {
            if (partyFlag == PartyFlag.Leader_SharedXP_Inactive_Innocent)
                partyFlag = PartyFlag.Leader_SharedXP_Inactive_Guilty;
            else if (partyFlag == PartyFlag.Member_SharedXP_Inactive_Innocent)
                partyFlag = PartyFlag.Leader_SharedXP_Inactive_Guilty;
            else if (partyFlag == PartyFlag.Other)
                partyFlag = PartyFlag.Member_SharedXP_Inactive_Innocent;

            return new Rect {
                x = (int)partyFlag * Constants.StateFlagSize,
                y = Constants.StateFlagSize,
                width = Constants.StateFlagSize,
                height = Constants.StateFlagSize
            };
        }

        private Rect GetPKFlagTextureRect(PKFlag pkFlag) {
            return new Rect {
                x = (int)pkFlag * Constants.StateFlagSize,
                y = 2 * Constants.StateFlagSize,
                width = Constants.StateFlagSize,
                height = Constants.StateFlagSize
            };
        }

        private Rect GetGuildFlagTextureRect(GuildFlag guildFlag) {
            return new Rect {
                x = (int)guildFlag * Constants.StateFlagSize,
                y = 3 * Constants.StateFlagSize,
                width = Constants.StateFlagSize,
                height = Constants.StateFlagSize
            };
        }

        private Rect GetSummonFlagTextureRect(SummonType summonType) {
            return new Rect {
                x = (int)summonType * Constants.StateFlagSize,
                y = 4 * Constants.StateFlagSize,
                width = Constants.StateFlagSize,
                height = Constants.StateFlagSize
            };
        }

        private Rect GetRisknessFlagTextureRect(RisknessFlag riskness) {
            return new Rect {
                x = (int)riskness * Constants.StateFlagSize,
                y = 5 * Constants.StateFlagSize,
                width = Constants.StateFlagSize,
                height = Constants.StateFlagSize
            };
        }

        private Rect GetSpeechFlagTextureRect(SpeechCategory speechCategory) {
            int x = 0;
            switch (speechCategory) {
                case SpeechCategory.Normal: x = 1; break;
                case SpeechCategory.Trader: x = 2; break;
                case SpeechCategory.Quest: x = 3; break;
                case SpeechCategory.QuestTrader: x = 4; break;
            }

            return new Rect {
                x = x * Constants.SpeechFlagSize,
                y = 0,
                width = Constants.SpeechFlagSize,
                height = Constants.SpeechFlagSize
            };
        }
        
        public Vector3Int? PointToMap(Vector2 point) {
            int x = (int)(point.x / (Constants.FieldSize * LayerZoom.x)) + 1;
            int y = (int)(point.y / (Constants.FieldSize * LayerZoom.y)) + 1;

            if (x < 0 || x > Constants.MapWidth || y < 0 || y > Constants.MapHeight)
                return null;

            Vector3Int mapPosition = new Vector3Int(x, y, 0);

            int minZ = _minZPlane[y * Constants.MapSizeX + x];
            int z = _maxZPlane;

            while (z > minZ) {
                var field = WorldMapStorage.GetField(x, y, z);
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
                return WorldMapStorage.ToAbsolute(mapPosition.Value);
            return null;
        }

        public Creatures.Creature PointToCreature(Vector2 point, bool restrictedToPlayerZPlane) {
            if (WorldMapStorage == null || (restrictedToPlayerZPlane && !Player))
                return null;
            
            Vector3Int? tmpPosition = PointToMap(point);
            if (!tmpPosition.HasValue)
                return null;
            
            var mapPosition = tmpPosition.Value;
            if (restrictedToPlayerZPlane)
                mapPosition.z = WorldMapStorage.ToMap(Player.Position).z;
            
            var field = WorldMapStorage.GetField(mapPosition);
            if (!field)
                return null;

            int index = field.GetTopCreatureObject(out Appearances.ObjectInstance @object);
            if (index == -1)
                return null;
            
            return CreatureStorage.GetCreature(@object.Data);
        }
    }
}
