using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

namespace OpenTibiaUnity.Core.WorldMap.Rendering
{
    public class WorldMapRenderer {
        private int _drawnCreaturesCount = 0;
        private int _drawnTextualEffectsCount = 0;
        private int _maxZPlane = 0;
        private int _playerZPlane = 0;
        private float _hangPixelX = 0;
        private float _hangPixelY = 0;
        private int _hangPatternX = 0;
        private int _hangPatternY = 0;
        private int _hangPatternZ = 0;
        private float _highlightOpacity = Constants.HighlightMinOpacity;
        private Vector3Int _helperCoordinate;
        private Vector2Int _helperPoint;
        private readonly int[] _minZPlane;
        private float[] _cachedHighlightOpacities;

        private Vector2 _screenZoom = new Vector2();
        private Vector2 _layerZoom = new Vector2();

        private int[] _creatureCount;
        private readonly RenderAtom[][] _creatureField;
        private readonly RenderAtom[] _drawnCreatures;
        private readonly RenderAtom[] _drawnTextualEffects;
        private List<Components.CreatureStatusPanel> _creaturesStatus;
        private Creatures.Creature _highlightCreature;
        
        private TileCursor _tileCursor = new TileCursor();
        private Appearances.ObjectInstance _previousHang = null;
        private LightmapRenderer _lightmapRenderer = new MeshBasedLightmapRenderer();
        private Appearances.Rendering.MarksView _creaturesMarksView = new Appearances.Rendering.MarksView(0);
        
        private WorldMapStorage _worldMapStorage { get => OpenTibiaUnity.WorldMapStorage; }
        private Creatures.CreatureStorage _creatureStorage { get => OpenTibiaUnity.CreatureStorage; }
        private Creatures.Player _player { get => OpenTibiaUnity.Player; }
        private Options.OptionStorage _optionStorage { get => OpenTibiaUnity.OptionStorage; }
        
        private Rect _lastWorldMapLayerRect = Rect.zero;
        private Rect _blipRect = Rect.zero;
        private Rect _unclamppedClipRect = Rect.zero;

        private uint _renderCounter = 0; // initially the first render
        private uint _lastRenderCounter = 0;
        private uint _blipRectRenderCounter = 0;
        private float _lastRenderTime = 0f;

        public int Framerate { get; private set; } = 0;
        
        public Vector3Int? HighlightTile { get; set; }
        public object HighlightObject { get; set; }

        public WorldMapRenderer() {
            int mapCapacity = Constants.MapSizeX * Constants.MapSizeY;

            _minZPlane = new int[mapCapacity];
            _creatureField = new RenderAtom[mapCapacity][];
            _creatureCount = new int[mapCapacity];
            _drawnCreatures = new RenderAtom[mapCapacity * Constants.MapSizeW];
            _creaturesStatus = new List<Components.CreatureStatusPanel>();

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

            _cachedHighlightOpacities = new float[8 * 2 - 2];
            for (int i = 0; i < 8; i++)
                _cachedHighlightOpacities[i] = Constants.HighlightMinOpacity + (i / 7f) * (Constants.HighlightMaxOpacity - Constants.HighlightMinOpacity);
            for (int i = 1; i < 7; i++)
                _cachedHighlightOpacities[7 + i] = Constants.HighlightMaxOpacity - (i / 7f) * (Constants.HighlightMaxOpacity - Constants.HighlightMinOpacity);
            
            // enable this if you intend to change the name during the game..
            // Creatures.Creature.onNameChange.AddListener(OnCreatureNameChange);
            Creatures.Creature.onSkillChange.AddListener(OnCreatureSkillChange);
        }

        public void DestroyUIElements() {
            foreach (var panel in _creaturesStatus) {
                if (panel && !panel.IsDestroyed())
                    UnityEngine.Object.Destroy(panel.gameObject);
            }

            _creaturesStatus = new List<Components.CreatureStatusPanel>();
        }
        
        public Rect CalculateClipRect() {
            if (_blipRectRenderCounter == _renderCounter)
                return _blipRect;

            float width = Constants.WorldMapRealWidth;
            float height = Constants.WorldMapRealHeight;

            // (0, 0) respects to the bottomleft
            float x = 2 * Constants.FieldSize + _player.AnimationDelta.x;
            float y = Constants.FieldSize - _player.AnimationDelta.y;

            _unclamppedClipRect = new Rect(x, y, width, height);

            width /= Constants.WorldMapScreenWidth;
            x /= Constants.WorldMapScreenWidth;
            height /= Constants.WorldMapScreenHeight;
            y /= Constants.WorldMapScreenHeight;

            _blipRect = new Rect(x, y, width, height);
            _blipRectRenderCounter = _renderCounter;
            return _blipRect;
        }

        public RenderError Render(Rect worldMapLayerRect) {
            if (_worldMapStorage == null || _creatureStorage == null || _player == null || !_worldMapStorage.Valid)
                return RenderError.WorldMapNotValid;

            _lastWorldMapLayerRect = worldMapLayerRect;
            if (_lastWorldMapLayerRect.width < Constants.WorldMapMinimumWidth || _lastWorldMapLayerRect.height < Constants.WorldMapMinimumHeight)
                return RenderError.SizeNotEffecient;

            var screenSize = new Vector2(Screen.width, Screen.height);
            _screenZoom.Set(
                screenSize.x / Constants.WorldMapScreenWidth,
                screenSize.y / Constants.WorldMapScreenHeight);

            _layerZoom.Set(
                    _lastWorldMapLayerRect.width / Constants.WorldMapRealWidth,
                    _lastWorldMapLayerRect.height / Constants.WorldMapRealHeight);

            _worldMapStorage.Animate();
            _creatureStorage.Animate();
            _highlightOpacity = _cachedHighlightOpacities[(OpenTibiaUnity.TicksMillis / 50) % _cachedHighlightOpacities.Length];

            _drawnCreaturesCount = 0;
            _drawnTextualEffectsCount = 0;
            _renderCounter++;

            if (Time.time - _lastRenderTime > 1f) {
                Framerate = (int)((_renderCounter - _lastRenderCounter) / (Time.time - _lastRenderTime));
                _lastRenderTime = Time.time;
                _lastRenderCounter = _renderCounter;
            }

            _worldMapStorage.ToMap(_player.Position, out _helperCoordinate);
            _playerZPlane = _helperCoordinate.z;

            UpdateMinMaxZPlane();

            if (HighlightObject is Creatures.Creature tmpCreature)
                _highlightCreature = tmpCreature;
            else if (HighlightObject is Appearances.ObjectInstance tmpObject && tmpObject.IsCreature)
                _highlightCreature = _creatureStorage.GetCreature(tmpObject.Data);
            else
                _highlightCreature = null;

            for (int z = 0; z <= _maxZPlane; z++) {
                UpdateFloorCreatures(z);
                publicDrawFields(z);
            }
            
            var lightmapTexture = _lightmapRenderer.CreateLightmap();
            var lightmapRect = new Rect() {
                x = (Constants.FieldSize / 2) * _screenZoom.y,
                y = (Constants.FieldSize / 2) * _screenZoom.y,
                width = Constants.WorldMapScreenWidth * _screenZoom.x,
                height = Constants.WorldMapScreenHeight * _screenZoom.y,
            };

            Graphics.DrawTexture(lightmapRect, lightmapTexture, OpenTibiaUnity.GameManager.LightSurfaceMaterial);

            InternelDrawCreaturesStatus();
            publicUpdateTextualEffects();
            publicUpdateOnscreenMessages();
            
            return RenderError.None;
        }
        
        private void UpdateMinMaxZPlane() {
            if (!_worldMapStorage.CacheUnsight) {
                _maxZPlane = Constants.MapSizeZ - 1;
                while (_maxZPlane > _playerZPlane && _worldMapStorage.GetObjectPerLayer(_maxZPlane) <= 0) {
                    _maxZPlane--;
                }

                for (int x = Constants.PlayerOffsetX - 1; x <= Constants.PlayerOffsetX + 1; x++) {
                    for (int y = Constants.PlayerOffsetY - 1; y <= Constants.PlayerOffsetY + 1; y++) {
                        if (!(x != Constants.PlayerOffsetX && y != Constants.PlayerOffsetY || !_worldMapStorage.IsLookPossible(x, y, _playerZPlane))) {
                            int z = _playerZPlane + 1;
                            while (z - 1 < _maxZPlane && x + _playerZPlane - z >= 0 && y + _playerZPlane - z >= 0) {
                                var @object = _worldMapStorage.GetObject(x + _playerZPlane - z, y + _playerZPlane - z, z, 0);
                                if (!!@object && !!@object.Type && @object.Type.IsGround && !@object.Type.IsDontHide) {
                                    _maxZPlane = z - 1;
                                    continue;
                                }

                                @object = _worldMapStorage.GetObject(x, y, z, 0);
                                if (!!@object && !!@object.Type && (@object.Type.IsGround || @object.Type.IsBottom) && !@object.Type.IsDontHide) {
                                    _maxZPlane = z - 1;
                                    continue;
                                }
                                z++;
                            }
                        }
                    }
                }

                _worldMapStorage.CacheUnsight = true;
            }

            if (!_worldMapStorage.CacheFullbank) {
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
                                        Appearances.ObjectInstance @object = _worldMapStorage.GetObject(mx, my, z, 0);
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

                _worldMapStorage.CacheFullbank = true;
            }
        }
        
        private void UpdateFloorCreatures(int z) {
            RenderAtom renderAtom = null;
            
            bool aboveGround = _player.Position.z <= 7;
            float optionsLevelSeparator = _optionStorage.LightLevelSeparator / 100f;
            int defaultBrightness = _worldMapStorage.AmbientCurrentBrightness;
            int brightness = aboveGround ? _worldMapStorage.AmbientCurrentBrightness : 0;
            for (int i = 0; i < _creatureCount.Length; i++) {
                _creatureCount[i] = 0;
            }
            
            for (int x = 0; x < Constants.MapSizeX; x++) {
                for (int y = 0; y < Constants.MapSizeY; y++) {
                    Field field = _worldMapStorage.GetField(x, y, z);
                    if (_optionStorage.ShowLightEffects) {
                        var colorIndex = _lightmapRenderer.ToColorIndex(x, y);
                        if (z == _playerZPlane && z > 0)
                            _lightmapRenderer[colorIndex] = Utils.Utility.MulColor32(_lightmapRenderer[colorIndex], optionsLevelSeparator);

                        Appearances.ObjectInstance @object;
                        if (z == 0 || (@object = field.ObjectsRenderer[0]) != null && @object.Type.IsGround) {
                            var color = _lightmapRenderer[colorIndex];
                            _lightmapRenderer.SetFieldBrightness(x, y, brightness, aboveGround);
                            if (z > 0 && field.CacheTranslucent) {
                                color = Utils.Utility.MulColor32(color, optionsLevelSeparator);
                                var alterColor = _lightmapRenderer[colorIndex];
                                color.r = Math.Max(color.r, alterColor.r);
                                color.g = Math.Max(color.g, alterColor.g);
                                color.b = Math.Max(color.b, alterColor.b);
                                _lightmapRenderer[colorIndex] = color;
                            }
                        }

                        if (x > 0 && y > 0 && z < 7 && z == _playerZPlane + _worldMapStorage.Position.z - 8 && _worldMapStorage.IsTranslucent(x - 1, y - 1, z + 1))
                            _lightmapRenderer.SetFieldBrightness(x, y, defaultBrightness, aboveGround);
                    }
                    
                    for (int i = field.ObjectsCount - 1; i >= 0; i--) {
                        var @object = field.ObjectsRenderer[i];
                        if (!@object.IsCreature)
                            continue;

                        var creature = _creatureStorage.GetCreature(@object.Data);
                        if (!creature)
                            continue;

                        Vector2Int displacement = new Vector2Int(); ;
                        if (!!creature.MountOutfit && !!creature.MountOutfit.Type) {
                            displacement = creature.MountOutfit.Type.Offset;
                        } else if (!!creature.Outfit && !!creature.Outfit.Type) {
                            displacement = creature.Outfit.Type.Offset;
                        }

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
                                if (_creatureCount[fieldIndex] < Constants.MapSizeW) {
                                    _creatureCount[fieldIndex]++;
                                }

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
        
        private void publicDrawFields(int z) {
            _helperCoordinate.Set(0, 0, z);
            _worldMapStorage.ToAbsolute(_helperCoordinate, out _helperCoordinate);
            int size = Constants.MapSizeX + Constants.MapSizeY;
            for (int i = 0; i < size; i++) {
                int y = Math.Max(i - Constants.MapSizeX + 1, 0);
                int x = Math.Min(i, Constants.MapSizeX - 1);
                while (x >= 0 && y < Constants.MapSizeY) {
                    publicDrawField(
                        (x + 1) * Constants.FieldSize,
                        (y + 1) * Constants.FieldSize,
                        _helperCoordinate.x + x,
                        _helperCoordinate.y + y,
                        _helperCoordinate.z,
                        x, y, z,
                        true);
                    x--;
                    y++;
                }
                
                if (_optionStorage.HighlightMouseTarget && HighlightTile.HasValue && HighlightTile.Value.z == z) {
                    _tileCursor.DrawTo((HighlightTile.Value.x + 1f) * Constants.FieldSize, (HighlightTile.Value.y + 1f) * Constants.FieldSize, _screenZoom, OpenTibiaUnity.TicksMillis);
                }
            }
        }

        private void publicDrawField(float rectX, float rectY, int absoluteX, int absoluteY, int absoluteZ, int positionX, int positionY, int positionZ, bool drawLyingObjects) {

            Field field = _worldMapStorage.GetField(positionX, positionY, positionZ);

            int objCount = field.ObjectsCount;
            int index = positionY * Constants.MapSizeX + positionX;
            
            bool isCovered = positionZ > _minZPlane[index]
                || (positionX == 0 || positionZ >= _minZPlane[index - 1])
                || (positionY == 0 || positionZ >= _minZPlane[index - Constants.MapSizeX])
                || (positionX == 0 && positionY == 0 || positionZ >= _minZPlane[index - Constants.MapSizeX - 1]);

            int objectsHeight = 0;
            Appearances.ObjectInstance previousHang = null;

            // Draw objects
            if (drawLyingObjects && objCount > 0 && isCovered) {
                //Appearances.ObjectInstance @object;
                bool isLying = false;

                // draw Items in reverse order
                for (int i = 0; i < objCount; i++) {
                    var @object = field.ObjectsRenderer[i];
                    var type = @object.Type;
                    if (@object.IsCreature || type.IsTop)
                        break;

                    if (_optionStorage.ShowLightEffects && type.IsLight) {
                        // check how correct those are
                        int lightX = ((int)rectX - objectsHeight - (int)type.OffsetX) / Constants.FieldSize;
                        int lightY = ((int)rectY - objectsHeight - (int)type.OffsetY) / Constants.FieldSize;
                        Color32 color32 = Colors.ColorFrom8Bit((byte)type.LightColor);

                        _lightmapRenderer.SetLightSource(lightX, lightY, positionZ, type.Brightness, color32);
                    }

                    var screenPosition = new Vector2(rectX - objectsHeight, rectY - objectsHeight);
                    bool highlighted = HighlightObject is Appearances.ObjectInstance && HighlightObject == @object;
                    @object.DrawTo(screenPosition, _screenZoom, absoluteX, absoluteY, absoluteZ, highlighted, _highlightOpacity);

                    isLying = isLying || type.IsLyingCorpse;
                    if (type.IsHangable && @object.Hang == Appearances.AppearanceInstance.HookSouth)
                        previousHang = @object;

                    if (type.HasElevation)
                        objectsHeight = Mathf.Min(objectsHeight + (int)type.Elevation, Constants.FieldHeight);
                }

                // lying tile, draw lying tile
                if (isLying) {
                    if (positionX > 0 && positionY > 0) {
                        publicDrawField(rectX - Constants.FieldSize, rectY - Constants.FieldSize, absoluteX - 1, absoluteY - 1, absoluteZ, positionX - 1, positionY - 1, positionZ, false);
                    } else if (positionX > 0) {
                        publicDrawField(rectX - Constants.FieldSize, rectY, absoluteX - 1, absoluteY, absoluteZ, positionX - 1, positionY, positionZ, false);
                    } else if (positionY > 0) {
                        publicDrawField(rectX, rectY - Constants.FieldSize, absoluteX, absoluteY - 1, absoluteZ, positionX, positionY - 1, positionZ, false);
                    }
                }

                // draw hang object
                if (!!_previousHang) {
                    var screenPosition = new Vector2(_hangPixelX, _hangPixelY);
                    bool highlighted = HighlightObject is Appearances.ObjectInstance && HighlightObject == _previousHang;
                    _previousHang.DrawTo(screenPosition, _screenZoom, _hangPatternX, _hangPatternY, _hangPatternZ, highlighted, _highlightOpacity);
                    _previousHang = null;
                }

                if (!!previousHang) {
                    _previousHang = previousHang;
                    _hangPixelX = rectX;
                    _hangPixelY = rectY;
                    _hangPatternX = absoluteX;
                    _hangPatternY = absoluteY;
                    _hangPatternZ = absoluteZ;
                }
            }

            // draw creatures
            publicFieldDrawCreatures(positionX, positionY, positionZ, drawLyingObjects, isCovered, objectsHeight);

            // draw effects
            publicFieldDrawEffects(rectX, rectY, absoluteX, absoluteY, absoluteZ, positionZ, drawLyingObjects, field, objectsHeight);

            // TODO: this should be drawn on a separate render texture (atmosphere texture)
            // this is likely to be fog and such effects
            //if (!!field.EnvironmentalEffect) {
            //    var screenPosition = new Vector2(rectX, rectY);
            //    field.EnvironmentalEffect.DrawTo(screenPosition, _screenZoom, absoluteX, absoluteY, absoluteZ);
            //}
            
            if (drawLyingObjects) {
                for (int i = 0; i < objCount; i++) {
                    var @object = field.ObjectsRenderer[i];
                    if (@object.Type.IsTop) {
                        var screenPosition = new Vector2(rectX, rectY);
                        bool highlighted = HighlightObject is Appearances.ObjectInstance && HighlightObject == @object;
                        @object.DrawTo(screenPosition, _screenZoom, absoluteX, absoluteY, absoluteZ, highlighted, _highlightOpacity);
                    }
                }
            }
        }

        private void publicFieldDrawCreatures(int positionX, int positionY, int positionZ, bool drawLyingObjects, bool isCovered, int objectsHeight) {
            RenderAtom[] renderAtomArray = _creatureField[positionY * Constants.MapSizeX + positionX];
            int creatureCount = _creatureCount[positionY * Constants.MapSizeX + positionX];
            for (int i = 0; i < creatureCount; i++) {
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
                _creaturesMarksView.DrawMarks(creature.Marks, renderAtom.x, renderAtom.y, _screenZoom);

                _helperPoint.Set(0, 0);
                if (isCovered && !!creature.MountOutfit) {
                    _helperPoint += creature.MountOutfit.Type.Offset;
                    var screenPosition = new Vector2(renderAtom.x + _helperPoint.x, renderAtom.y + _helperPoint.y);
                    creature.MountOutfit.DrawTo(screenPosition, _screenZoom, (int)creature.Direction, 0, 0, highlighted, _highlightOpacity);
                }

                if (isCovered) {
                    _helperPoint += creature.Outfit.Type.Offset;
                    var screenPosition = new Vector2(renderAtom.x + _helperPoint.x, renderAtom.y + _helperPoint.y);
                    creature.Outfit.DrawTo(screenPosition, _screenZoom, (int)creature.Direction, 0, !!creature.MountOutfit ? 1 : 0, highlighted, _highlightOpacity);
                }

                if (positionZ == _playerZPlane && (_creatureStorage.IsOpponent(creature) || creature.Id == _player.Id)) {
                    _drawnCreatures[_drawnCreaturesCount].Assign(renderAtom);
                    _drawnCreaturesCount++;
                }

                if (isCovered && drawLyingObjects && _optionStorage.ShowLightEffects) {
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

                    if (creature.Id == _player.Id && creature.Brightness < 2) {
                        var color = new Color32(255, 255, 255, 255);
                        _lightmapRenderer.SetLightSource(lightX, lightY, positionZ, 2, color);
                    }
                }
            }
        }

        private void publicFieldDrawEffects(float rectX, float rectY, int absoluteX, int absoluteY, int absoluteZ, int positionZ, bool drawLyingObjects, Field field, int objectsHeight) {
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
                        int x = effectRectX - Constants.FieldSize / 2 + totalTextualEffectsWidth;
                        int y = effectRectY - Constants.FieldSize - 2 * textualEffect.Phase;
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
                    effect.DrawTo(screenPosition, _screenZoom, absoluteX, absoluteY, absoluteZ);

                    if (drawLyingObjects && _optionStorage.ShowLightEffects && effect.Type.IsLight) {
                        var color = Colors.ColorFrom8Bit((byte)effect.Type.LightColor);
                        _lightmapRenderer.SetLightSource(effectLightX, effectLightY, positionZ, effect.Type.Brightness, color);
                    }
                } else { // EffectInstance
                    var screenPosition = new Vector2(effectRectX, effectRectY);
                    effect.DrawTo(screenPosition, _screenZoom, absoluteX, absoluteY, absoluteZ);

                    if (drawLyingObjects && _optionStorage.ShowLightEffects && effect.Type.IsLight) {
                        uint activeBrightness = (uint)((Math.Min(effect.Phase, effect.Type.FrameGroups[0].SpriteInfo.Phases + 1 - effect.Phase) * effect.Type.Brightness + 2) / 3);
                        var color = Colors.ColorFrom8Bit((byte)effect.Type.LightColor);
                        _lightmapRenderer.SetLightSource(effectLightX, effectLightY, positionZ, Math.Min(activeBrightness, effect.Type.Brightness), color);
                    }
                }
            }
        }

        private void InternelDrawCreaturesStatus() {
            var optionStorage = OpenTibiaUnity.OptionStorage;
            
            int myPlayerIndex = -1;
            for (int i = 0; i < _drawnCreaturesCount; i++) {
                var renderAtom = _drawnCreatures[i];
                var creature = renderAtom.Object as Creatures.Creature;
                if (!!creature) {
                    if (creature == _player) {
                        myPlayerIndex = i;
                        continue;
                    }
                    
                    bool isExplicitlyVisible = renderAtom.z >= _minZPlane[renderAtom.fieldY * Constants.MapSizeX + renderAtom.fieldX];
                    publicDrawCreatureStatusClassic(
                        renderAtom, renderAtom.x, renderAtom.y, isExplicitlyVisible,
                        optionStorage.ShowNameForOtherCreatures,
                        optionStorage.ShowHealthForOtherCreatures && !creature.IsNPC,
                        false,
                        optionStorage.ShowMarksForOtherCreatures);
                }
            }

            if (myPlayerIndex != -1) {
                var renderAtom = _drawnCreatures[myPlayerIndex];
                publicDrawCreatureStatusClassic(renderAtom, renderAtom.x, renderAtom.y, true,
                    optionStorage.ShowNameForOwnCharacter,
                    optionStorage.ShowHealthForOwnCharacter,
                    optionStorage.ShowManaForOwnCharacter && OpenTibiaUnity.GameManager.ClientVersion >= 1100,
                    optionStorage.ShowMarksForOwnCharacter);
            }

            _creaturesStatus = _creaturesStatus.Where((x) => {
                if (x.CachedRenderCount == _renderCounter)
                    return true;

                UnityEngine.Object.Destroy(x.gameObject);
                return false;
            }).ToList();
        }

        private void publicUpdateTextualEffects() {
            for (int i = 0; i < _drawnTextualEffectsCount; i++) {
                var renderAtom = _drawnTextualEffects[i];
                if (renderAtom.Object is Appearances.TextualEffectInstance textualEffect) {
                    Vector3Int mapPosition = _worldMapStorage.ToMapClosest(new Vector3Int(renderAtom.x, renderAtom.y, renderAtom.z));
                    float x = (renderAtom.x - _player.AnimationDelta.x - Constants.FieldSize) * _layerZoom.x;
                    float y = (renderAtom.y - _player.AnimationDelta.y - Constants.FieldSize) * _layerZoom.y;
                    textualEffect.UpdateTextMeshPosition(x, -y);
                }
            }
        }
        
        private void publicUpdateOnscreenMessages() {
            if (_worldMapStorage.LayoutOnscreenMessages) {
                LayoutOnscreenMessages();
                _worldMapStorage.LayoutOnscreenMessages = false;
            }

            int animationDeltaX = 0;
            int animationDeltaY = 0;

            if (_player != null) {
                animationDeltaX = _player.AnimationDelta.x;
                animationDeltaY = _player.AnimationDelta.y;
            }

            var messageBoxes = _worldMapStorage.MessageBoxes;
            var length = messageBoxes.Count - 1;
            while (length >= (int)MessageScreenTargets.BoxCoordinate) {
                var messageBox = messageBoxes[length];
                Vector3Int mapPosition = _worldMapStorage.ToMapClosest(messageBox.Position.Value);
                float x = ((mapPosition.x - 0.5f) * Constants.FieldSize - _player.AnimationDelta.x) * _layerZoom.x;
                float y = ((mapPosition.y - 1f) * Constants.FieldSize - _player.AnimationDelta.y) * _layerZoom.y;
                messageBox.UpdateTextMeshPosition(x, -y);
                length--;
            }
        }

        private void publicDrawCreatureStatusClassic(RenderAtom renderAtom, float rectX, float rectY, bool isVisible, bool drawNames, bool drawHealth, bool drawMana, bool drawFlags) {
            var creature = renderAtom.Object as Creatures.Creature;
            if (creature.Position.z != _player.Position.z) {
                RemoveCreatureStatusPanel(creature.Id);
                return;
            }

            var statusPanel = AddOrGetCreatureStatusPanel(creature);
            statusPanel.CachedRenderCount = _renderCounter;
            statusPanel.UpdateCreatureMisc(isVisible, _lightmapRenderer.CalculateCreatureBrightnessFactor(creature, creature.Id == _player.Id));
            statusPanel.SetDrawingProperties(drawNames, drawHealth, drawMana);
            statusPanel.SetFlags(drawFlags, creature.PartyFlag, creature.PKFlag, creature.SummonTypeFlag, creature.SpeechCategory, creature.GuildFlag);

            // the animation delta is already applied earlier on creatures check
            // but we need to add the animation delta of the screen
            float x = renderAtom.x - 2 * Constants.FieldSize - _player.AnimationDelta.x + 27 / 2f;
            float y = renderAtom.y - 2 * Constants.FieldSize - _player.AnimationDelta.y - 8;
            
            statusPanel.rectTransform.anchoredPosition = new Vector2(x * _layerZoom.x, -y * _layerZoom.y);
        }

        private void publicDrawCreatureStatusHUD(RenderAtom renderAtom, int rectX, int rectY, bool isVisible, bool drawNames, bool drawHealth, bool drawMana, bool drawFlags) {
            // TODO
        }

        private Components.CreatureStatusPanel AddOrGetCreatureStatusPanel(Creatures.Creature creature) {
            int index = 0;
            int lastIndex = _creaturesStatus.Count - 1;
            while (index <= lastIndex) {
                int tmpIndex = index + lastIndex >> 1;
                var foundPanel = _creaturesStatus[tmpIndex];
                if (foundPanel.Creature_id < creature.Id)
                    index = tmpIndex + 1;
                else if (foundPanel.Creature_id > creature.Id)
                    lastIndex = tmpIndex - 1;
                else
                    return foundPanel;
            }

            var panel = UnityEngine.Object.Instantiate(OpenTibiaUnity.GameManager.CreatureStatusPanelPrefab,
                    OpenTibiaUnity.GameManager.CreatureStatusContainer);

            panel.Creature_id = creature.Id;
            panel.name = "CreatureStatus_" + creature.Name;
            panel.UpdateProperties(creature.Name, creature.HealthPercent, creature.ManaPercent);

            _creaturesStatus.Insert(index, panel);
            return panel;
        }

        private bool RemoveCreatureStatusPanel(uint _id) {
            int lastIndex = _creaturesStatus.Count - 1;
            int index = 0;
            int foundIndex = -1;
            Components.CreatureStatusPanel foundPanel = null;
            while (index < lastIndex) {
                int tmpIndex = index + lastIndex >> 1;
                foundPanel = _creaturesStatus[tmpIndex];
                if (foundPanel.Creature_id > _id) {
                    index = tmpIndex + 1;
                } else if (foundPanel.Creature_id < _id) {
                    lastIndex = tmpIndex - 1;
                } else {
                    foundIndex = tmpIndex;
                    break;
                }
            }

            if (foundIndex == -1)
                return false;

            _creaturesStatus.RemoveAt(foundIndex);
            UnityEngine.Object.Destroy(foundPanel.gameObject);
            return true;
        }

        public Components.CreatureStatusPanel FindCreatureStatusPanel(Creatures.Creature creature) {
            int index = 0;
            int lastIndex = _creaturesStatus.Count - 1;
            while (index <= lastIndex) {
                int tmpIndex = index + lastIndex >> 1;
                var foundPanel = _creaturesStatus[tmpIndex];
                if (foundPanel.Creature_id < creature.Id)
                    index = tmpIndex + 1;
                else if (foundPanel.Creature_id > creature.Id)
                    lastIndex = tmpIndex - 1;
                else
                    return foundPanel;
            }

            return null;
        }
        
        private void LayoutOnscreenMessages() {
            foreach (var messageBox in _worldMapStorage.MessageBoxes) {
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

        public void OnCreatureSkillChange(Creatures.Creature creature, SkillType skillType, Creatures.Skill skill) {
            Components.CreatureStatusPanel statusPanel = FindCreatureStatusPanel(creature);
            if (!statusPanel)
                return;

            if (skillType == SkillType.HealthPercent) {
                statusPanel.SetHealthPercent((int)skill.Level);
                statusPanel.UpdateHealthColor();
            } else if (skillType == SkillType.Mana) {
                statusPanel.SetMana((int)skill.Level, (int)skill.BaseLevel);
            } else if (skillType == SkillType.Health) {
                statusPanel.SetHealth((int)skill.Level, (int)skill.BaseLevel);
                statusPanel.UpdateHealthColor();
            }
        }

        public Vector3Int? PointToMap(Vector2 point) {
            int x = (int)(point.x / (Constants.FieldSize * _layerZoom.x)) + 1;
            int y = (int)(point.y / (Constants.FieldSize * _layerZoom.y)) + 1;

            if (x < 0 || x > Constants.MapWidth || y < 0 || y > Constants.MapHeight)
                return null;

            Vector3Int mapPosition = new Vector3Int(x, y, 0);

            int minZ = _minZPlane[y * Constants.MapSizeX + x];
            int z = _maxZPlane;

            while (z > minZ) {
                var field = _worldMapStorage.GetField(x, y, z);
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
                return _worldMapStorage.ToAbsolute(mapPosition.Value);
            return null;
        }

        public Creatures.Creature PointToCreature(Vector2 point, bool restrictedToPlayerZPlane) {
            if (_worldMapStorage == null || (restrictedToPlayerZPlane && !_player))
                return null;
            
            Vector3Int? tmpPosition = PointToMap(point);
            if (!tmpPosition.HasValue)
                return null;
            
            var mapPosition = tmpPosition.Value;
            if (restrictedToPlayerZPlane)
                mapPosition.z = _worldMapStorage.ToMap(_player.Position).z;
            
            var field = _worldMapStorage.GetField(mapPosition);
            if (!field)
                return null;

            int index = field.GetTopCreatureObject(out Appearances.ObjectInstance @object);
            if (index == -1)
                return null;
            
            return _creatureStorage.GetCreature(@object.Data);
        }
    } // class 
} // ns
