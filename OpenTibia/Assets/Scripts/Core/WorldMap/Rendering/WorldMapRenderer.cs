using System;
using System.Collections.Generic;
using UnityEngine;

using CommandBuffer = UnityEngine.Rendering.CommandBuffer;

namespace OpenTibiaUnity.Core.WorldMap.Rendering
{
    public struct ClassicStatusDescriptor
    {
        public bool showNames;
        public bool showHealth;
        public bool showMana;
        public bool showMarks;
        public bool showIcons;
    }

    public struct ClassicStatusRectData
    {
        public Matrix4x4 matrix;
        public Color color;
    }

    public struct ClassicStatusFlagData
    {
        public Vector4 uv;
        public Matrix4x4 matrix;
    }

    public class WorldMapRenderer
    {
        private static readonly Vector3 s_creatureStatSize = new Vector3(27, 4, 1);

        private int _drawnCreaturesCount = 0;
        private int _drawnEffectsCount = 0;
        private int _drawnTextualEffectsCount = 0;
        private int _maxZPlane = 0;
        private int _playerZPlane = 0;
        private float _highlightOpacity = Constants.HighlightMinOpacity;
        private Vector2Int _hangPixel = Vector2Int.zero;
        private Vector3Int _hangPattern = Vector3Int.zero;
        private Creatures.Creature _highlightCreature;
        private Dictionary<int, CreatureStatus> _creatureStatusCache = new Dictionary<int, CreatureStatus>();

        private readonly int[] _minZPlane;
        private readonly float[] _highlightOpacities;
        private readonly int[] _creatureCount;
        private readonly RenderAtom[][] _creatureField;
        private readonly RenderAtom[] _drawnCreatures;
        private readonly EffectRenderAtom[] _drawnEffets;
        private readonly RenderAtom[] _drawnTextualEffets;

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

            _drawnEffets = new EffectRenderAtom[Constants.NumEffects];
            _drawnTextualEffets = new RenderAtom[Constants.NumEffects];
            for (int i = 0; i < _drawnTextualEffets.Length; i++) {
                _drawnEffets[i] = new EffectRenderAtom();
                _drawnTextualEffets[i] = new RenderAtom();
            }

            _creaturesMarksView.AddMarkToView(MarkType.ClientMapWindow, Constants.MarkThicknessBold);
            _creaturesMarksView.AddMarkToView(MarkType.Permenant, Constants.MarkThicknessBold);
            _creaturesMarksView.AddMarkToView(MarkType.OneSecondTemp, Constants.MarkThicknessBold);

            _tileCursor.FrameDuration = 100;

            _highlightOpacities = new float[8 * 2 - 2];
            for (int i = 0; i < 8; i++)
                _highlightOpacities[i] = Constants.HighlightMinOpacity + (i / 7f) * (Constants.HighlightMaxOpacity - Constants.HighlightMinOpacity);
            for (int i = 1; i < 7; i++)
                _highlightOpacities[7 + i] = Constants.HighlightMaxOpacity - (i / 7f) * (Constants.HighlightMaxOpacity - Constants.HighlightMinOpacity);
        }
        
        public Rect CalculateClipRect() {
            if (_clipRectRenderCounter == _renderCounter)
                return _clipRect;

            float width = Constants.WorldMapRealWidth;
            float height = Constants.WorldMapRealHeight;

            // (0, 0) respects to the bottomleft
            float x = 2 * Constants.FieldSize + Player.AnimationDelta.x;
            float y = Constants.FieldSize - Player.AnimationDelta.y;

            width /= Constants.WorldMapScreenWidth;
            x /= Constants.WorldMapScreenWidth;
            height /= Constants.WorldMapScreenHeight;
            y /= Constants.WorldMapScreenHeight;

            _clipRect = new Rect(x, y, width, height);
            _clipRectRenderCounter = _renderCounter;
            return _clipRect;
        }

        public RenderError RenderWorldMap(Rect worldMapLayerRect, RenderTexture renderTarget) {
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
                _highlightCreature = CreatureStorage.GetCreatureById(tmpObject.Data);
            else
                _highlightCreature = null;

            var commandBuffer = new CommandBuffer();
            if (renderTarget) {
                commandBuffer.SetRenderTarget(renderTarget);
                commandBuffer.ClearRenderTarget(false, true, Color.black);
            }

            commandBuffer.SetViewMatrix(
                Matrix4x4.TRS(Vector3.zero, Quaternion.identity, ScreenZoom) * 
                OpenTibiaUnity.GameManager.MainCamera.worldToCameraMatrix);

            for (int z = 0; z <= _maxZPlane; z++) {
                for (int i = 0; i < _creatureCount.Length; i++)
                    _creatureCount[i] = 0;

                _drawnEffectsCount = 0;
                InternalUpdateFloor(z);
                InternalDrawFields(commandBuffer, z);

                if (OpenTibiaUnity.GameManager.ClientVersion >= 1200)
                    InternalDrawEffects(commandBuffer);
            }

            if (OptionStorage.ShowLightEffects) {
                var lightmapMesh = _lightmapRenderer.CreateLightmap();
                var position = new Vector3(Constants.FieldSize / 2, Constants.FieldSize / 2, 0);
                var scale = new Vector2(Constants.FieldSize, -Constants.FieldSize);
                var transformation = Matrix4x4.TRS(position, Quaternion.Euler(180, 0, 0), scale);
                commandBuffer.DrawMesh(lightmapMesh, transformation, OpenTibiaUnity.GameManager.LightmapMaterial);
            }

            Graphics.ExecuteCommandBuffer(commandBuffer);
            commandBuffer.Dispose();

            return RenderError.None;
        }

        public void RenderOnscreenText(Rect viewport, RenderTexture renderTarget) {
            _realScreenTranslation = viewport.position;

            var commandBuffer = new CommandBuffer();
            commandBuffer.name = "OnscreenText";
            if (renderTarget) {
                commandBuffer.SetRenderTarget(renderTarget);
                commandBuffer.ClearRenderTarget(false, true, Utils.GraphicsUtility.TransparentColor);
            }

            InternelDrawCreatureStatus(commandBuffer);
            InternalDrawTextualEffects(commandBuffer);
            InternalDrawOnscreenMessages(commandBuffer);

            Graphics.ExecuteCommandBuffer(commandBuffer);
            commandBuffer.Dispose();
        }
        
        private void UpdateMinMaxZPlane() {
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
            RenderAtom renderAtom;
            
            bool aboveGround = Player.Position.z <= 7;
            int brightness = aboveGround ? WorldMapStorage.AmbientCurrentBrightness : 0;
            float levelSeparator = OptionStorage.FixedLightLevelSeparator / 100f;

            for (int x = 0; x < Constants.MapSizeX; x++) {
                for (int y = 0; y < Constants.MapSizeY; y++) {
                    Field field = WorldMapStorage.GetField(x, y, z);

                    // update field light
                    // todo; this shouldn't be called every frame unless light probes has changed
                    if (OptionStorage.ShowLightEffects) {
#if DEBUG
                        UnityEngine.Profiling.Profiler.BeginSample("Lightmesh Level-Separator");
#endif
                        var colorIndex = _lightmapRenderer.ToColorIndex(x, y);
                        if (z == _playerZPlane && z > 0) {
                            var color32 = _lightmapRenderer[colorIndex];
                            _lightmapRenderer[colorIndex] = Utils.Utility.MulColor32(color32, levelSeparator);
                        }

                        Appearances.ObjectInstance @object;
                        if (z == 0 || (@object = field.ObjectsRenderer[0]) != null && @object.Type.IsGround) {
                            var color = _lightmapRenderer[colorIndex];
                            _lightmapRenderer.SetFieldBrightness(x, y, brightness, aboveGround);
                            if (z > 0 && field.CacheTranslucent) {
                                color = Utils.Utility.MulColor32(color, levelSeparator);
                                var alterColor = _lightmapRenderer[colorIndex];
                                if (color.r < alterColor.r) color.r = alterColor.r;
                                if (color.g < alterColor.g) color.g = alterColor.g;
                                if (color.b < alterColor.b) color.b = alterColor.b;
                                _lightmapRenderer[colorIndex] = color;
                            }
                        }

                        if (x > 0 && y > 0 && z < 7 && z == _playerZPlane + WorldMapStorage.Position.z - 8 && WorldMapStorage.IsTranslucent(x - 1, y - 1, z + 1))
                            _lightmapRenderer.SetFieldBrightness(x, y, WorldMapStorage.AmbientCurrentBrightness, aboveGround);
#if DEBUG
                        UnityEngine.Profiling.Profiler.EndSample();
#endif
                    }

                    for (int i = field.ObjectsCount - 1; i >= 0; i--) {
                        var @object = field.ObjectsRenderer[i];
                        if (!@object.IsCreature)
                            continue;

                        var creature = CreatureStorage.GetCreatureById(@object.Data);
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
        
        private void InternalDrawFields(CommandBuffer commandBuffer, int z) {
            var absolutePosition = WorldMapStorage.ToAbsolute(new Vector3Int(0, 0, z));
            int size = Constants.MapSizeX + Constants.MapSizeY;
            for (int i = 0; i < size; i++) {
                int y = Math.Max(i - Constants.MapSizeX + 1, 0);
                int x = Math.Min(i, Constants.MapSizeX - 1);
                while (x >= 0 && y < Constants.MapSizeY) {
                    InternalDrawField(commandBuffer,
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
                
                if (OptionStorage.HighlightMouseTarget && HighlightTile.HasValue && HighlightTile.Value.z == z)
                    _tileCursor.Draw(commandBuffer, (HighlightTile.Value.x + 1) * Constants.FieldSize, (HighlightTile.Value.y + 1) * Constants.FieldSize, OpenTibiaUnity.TicksMillis);
            }
        }

        private void InternalDrawField(CommandBuffer commandBuffer, int rectX, int rectY, int absoluteX, int absoluteY, int absoluteZ, int positionX, int positionY, int positionZ, bool drawLyingObjects) {

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
                InternalDrawFieldObjects(commandBuffer, rectX, rectY, absoluteX, absoluteY, absoluteZ, positionX, positionY, positionZ, field, ref objectsHeight);

            // draw creatures
            InternalDrawFieldCreatures(commandBuffer, positionX, positionY, positionZ, drawLyingObjects, isCovered, objectsHeight);

            // draw effects
            InternalDrawFieldEffects(commandBuffer, rectX, rectY, absoluteX, absoluteY, absoluteZ, positionZ, drawLyingObjects, field, objectsHeight);

            // draw top objects
            if (drawLyingObjects)
                InternalDrawFieldTopObjects(commandBuffer, rectX, rectY, absoluteX, absoluteY, absoluteZ, field);
        }

        private void InternalDrawFieldObjects(CommandBuffer commandBuffer, int rectX, int rectY, int absoluteX, int absoluteY, int absoluteZ, int positionX, int positionY, int positionZ, Field field, ref int objectsHeight) {
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
                    int lightX = (rectX - objectsHeight - (int)type.OffsetX) / Constants.FieldSize;
                    int lightY = (rectY - objectsHeight - (int)type.OffsetY) / Constants.FieldSize;
                    Color32 color32 = Colors.ColorFrom8Bit((byte)type.LightColor);

                    _lightmapRenderer.SetLightSource(lightX, lightY, positionZ, type.Brightness, color32);
                }

                var screenPosition = new Vector2Int(rectX - objectsHeight, rectY - objectsHeight);
                bool highlighted = HighlightObject is Appearances.ObjectInstance && HighlightObject == @object;
                @object.Draw(commandBuffer, screenPosition, absoluteX, absoluteY, absoluteZ, highlighted, _highlightOpacity);

                isLying = isLying || type.IsLyingCorpse;
                if (type.IsHangable && @object.Hang == Appearances.AppearanceInstance.HookSouth)
                    hangObject = @object;

                if (type.HasElevation)
                    objectsHeight = Mathf.Min(objectsHeight + (int)type.Elevation, Constants.FieldHeight);
            }

            // lying tile, draw lying tile
            if (isLying) {
                if (positionX > 0 && positionY > 0)
                    InternalDrawField(commandBuffer, rectX - Constants.FieldSize, rectY - Constants.FieldSize, absoluteX - 1, absoluteY - 1, absoluteZ, positionX - 1, positionY - 1, positionZ, false);
                else if (positionX > 0)
                    InternalDrawField(commandBuffer, rectX - Constants.FieldSize, rectY, absoluteX - 1, absoluteY, absoluteZ, positionX - 1, positionY, positionZ, false);
                else if (positionY > 0)
                    InternalDrawField(commandBuffer, rectX, rectY - Constants.FieldSize, absoluteX, absoluteY - 1, absoluteZ, positionX, positionY - 1, positionZ, false);
            }

            // draw hang object
            if (!!_previousHang) {
                bool highlighted = HighlightObject is Appearances.ObjectInstance && HighlightObject == _previousHang;
                _previousHang.Draw(commandBuffer, _hangPixel, _hangPattern.x, _hangPattern.y, _hangPattern.z, highlighted, _highlightOpacity);
                _previousHang = null;
            }

            if (!!hangObject) {
                _previousHang = hangObject;
                _hangPixel.Set(rectX, rectY);
                _hangPattern.Set(absoluteX, absoluteY, absoluteZ);
            }
        }

        private void InternalDrawFieldCreatures(CommandBuffer commandBuffer, int positionX, int positionY, int positionZ, bool drawLyingObjects, bool isCovered, int objectsHeight) {
            RenderAtom[] renderAtomArray = _creatureField[positionY * Constants.MapSizeX + positionX];
            int creatureCount = _creatureCount[positionY * Constants.MapSizeX + positionX];
            for (int i = creatureCount - 1; i >= 0; i--) {
                RenderAtom renderAtom;
                Creatures.Creature creature;
                if (!(renderAtom = renderAtomArray[i]) || !(creature = renderAtom.Object as Creatures.Creature) || !creature.Outfit)
                    continue;

                if (drawLyingObjects) {
                    renderAtom.x -= objectsHeight;
                    renderAtom.y -= objectsHeight;
                }

                bool highlighted = !!_highlightCreature && _highlightCreature.Id == creature.Id;

                // marks
                if (creature.Marks.AnyMarkSet())
                    _creaturesMarksView.DrawMarks(commandBuffer, creature.Marks, renderAtom.x, renderAtom.y);

                var offset = Vector2Int.zero;
                if (isCovered && !!creature.MountOutfit) {
                    offset += creature.MountOutfit.Type.Offset;
                    var screenPosition = new Vector2Int(renderAtom.x + offset.x, renderAtom.y + offset.y);
                    creature.MountOutfit.Draw(commandBuffer, screenPosition, (int)creature.Direction, 0, 0, highlighted, _highlightOpacity);
                }

                if (isCovered) {
                    offset += creature.Outfit.Type.Offset;
                    var screenPosition = new Vector2Int(renderAtom.x + offset.x, renderAtom.y + offset.y);
                    creature.Outfit.Draw(commandBuffer, screenPosition, (int)creature.Direction, 0, !!creature.MountOutfit ? 1 : 0, highlighted, _highlightOpacity);
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

        private void InternalDrawFieldEffects(CommandBuffer commandBuffer, float rectX, float rectY, int absoluteX, int absoluteY, int absoluteZ, int positionZ, bool drawLyingObjects, Field field, int objectsHeight) {
            int effectRectX = (int)rectX - objectsHeight;
            int effectRectY = (int)rectY - objectsHeight;
            int effectLightX = effectRectX / Constants.FieldSize;
            int effectLightY = effectRectY / Constants.FieldSize;

            int totalTextualEffectsWidth = 0;
            int lastTextualEffectY = 0;

            int i = field.EffectsCount - 1;
            while (i >= 0 && (_drawnEffectsCount + _drawnTextualEffectsCount) <= Constants.NumEffects) {
                var effect = field.Effects[i];
                i--;

                if (!effect)
                    continue;

                if (effect is Appearances.TextualEffectInstance textualEffect) {
                    if (drawLyingObjects) {
                        var renderAtom = _drawnTextualEffets[_drawnTextualEffectsCount];
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
                } else {
                    var screenPosition = new Vector2Int(effectRectX, effectRectY);
                    uint brightness = effect.Type.Brightness;
                    if (effect is Appearances.MissileInstance missile) {
                        screenPosition.x += missile.AnimationDelta.x;
                        screenPosition.y += missile.AnimationDelta.y;

                        uint activeBrightness = (uint)((Math.Min(effect.Phase, effect.Type.FrameGroups[0].SpriteInfo.Phases + 1 - effect.Phase) * effect.Type.Brightness + 2) / 3);
                        brightness = Math.Min(brightness, activeBrightness);
                    }

                    if (OpenTibiaUnity.GameManager.ClientVersion >= 1200) {
                        var renderAtom = _drawnEffets[_drawnEffectsCount];
                        int x = screenPosition.x;
                        int y = screenPosition.y;

                        renderAtom.Update(effect, x, y, (int)brightness, absoluteX, absoluteY, absoluteZ, positionZ, effectLightX, effectLightY);
                        _drawnEffectsCount++;
                    } else {
                        effect.Draw(commandBuffer, screenPosition, absoluteX, absoluteY, absoluteZ);
                        if (drawLyingObjects && OptionStorage.ShowLightEffects && effect.Type.IsLight) {
                            var color = Colors.ColorFrom8Bit((byte)effect.Type.LightColor);
                            _lightmapRenderer.SetLightSource(effectLightX, effectLightY, positionZ, brightness, color);
                        }
                    }
                }
            }
        }

        private void InternalDrawFieldTopObjects(CommandBuffer commandBuffer, int rectX, int rectY, int absoluteX, int absoluteY, int absoluteZ, Field field) {
            var screenPosition = new Vector2Int(rectX, rectY);
            for (int i = 0; i < field.ObjectsCount; i++) {
                var @object = field.ObjectsRenderer[i];
                if (@object.Type.IsTop) {
                    bool highlighted = HighlightObject is Appearances.ObjectInstance && HighlightObject == @object;
                    @object.Draw(commandBuffer, screenPosition, absoluteX, absoluteY, absoluteZ, highlighted, _highlightOpacity);
                }
            }
        }

        private void InternelDrawCreatureStatus(CommandBuffer commandBuffer) {
            int playerIndex = -1;

            var gameManager = OpenTibiaUnity.GameManager;
            var optionStorage = OpenTibiaUnity.OptionStorage;

            var rectDrawData = new List<ClassicStatusRectData>(Constants.MaxCreatureCount * 4);
            var flagDrawData = new List<ClassicStatusFlagData>(Constants.MaxCreatureCount * 4);
            var speechDrawData = new List<ClassicStatusFlagData>(Constants.MaxCreatureCount * 4);

            var descriptor = new ClassicStatusDescriptor() {
                showNames = optionStorage.ShowNameForOtherCreatures,
                showMana = false,
                showMarks = optionStorage.ShowMarksForOtherCreatures,
                showIcons = optionStorage.ShowNPCIcons,
            };

            for (int i = 0; i < _drawnCreaturesCount; i++) {
                var renderAtom = _drawnCreatures[i];
                var creature = renderAtom.Object as Creatures.Creature;
                if (!creature)
                    continue;

                if (creature.Id == Player.Id) {
                    playerIndex = i;
                } else if (optionStorage.ShowHUDForOtherCreatures) {
                    int positionX = (renderAtom.x - Constants.FieldSize) / Constants.FieldSize;
                    int positionY = (renderAtom.y - Constants.FieldSize) / Constants.FieldSize;

                    descriptor.showHealth = optionStorage.ShowHealthForOtherCreatures && (!creature.IsNPC || !gameManager.GetFeature(GameFeature.GameHideNpcNames));
                    InternelDrawCreatureStatusClassic(commandBuffer, creature, rectDrawData, flagDrawData, speechDrawData,
                        renderAtom.x - Constants.FieldSize, renderAtom.y - Constants.FieldSize,
                        renderAtom.z >= _minZPlane[positionY * Constants.MapSizeX + positionX], descriptor);
                }
            }

            if (playerIndex != -1) {
                var renderAtom = _drawnCreatures[playerIndex];
                descriptor.showNames = optionStorage.ShowNameForOwnCharacter;
                descriptor.showHealth = optionStorage.ShowHealthForOwnCharacter;
                descriptor.showMana = optionStorage.ShowManaForOwnCharacter;
                descriptor.showMarks = optionStorage.ShowMarksForOwnCharacter;
                descriptor.showIcons = false;

                InternelDrawCreatureStatusClassic(commandBuffer, Player, rectDrawData, flagDrawData, speechDrawData,
                    renderAtom.x - Constants.FieldSize, renderAtom.y - Constants.FieldSize, true, descriptor);
            }

            var appearanceMaterial = OpenTibiaUnity.GameManager.AppearanceTypeMaterial;
            var coloredMaterial = OpenTibiaUnity.GameManager.ColoredMaterial;
            if (SystemInfo.supportsInstancing) {
                var matrixArray = new Matrix4x4[rectDrawData.Count];
                var exArray = new Vector4[rectDrawData.Count];

                for (int i = 0; i < rectDrawData.Count; i++) {
                    var data = rectDrawData[i];
                    matrixArray[i] = data.matrix;
                    exArray[i] = data.color;
                }

                var matriciesToPass = new Matrix4x4[1023];
                var exToPass = new Vector4[1023];

                for (int i = 0; i < matrixArray.Length; i += 1023) {
                    int sliceSize = Mathf.Min(1023, matrixArray.Length - i);
                    Array.Copy(matrixArray, i, matriciesToPass, 0, sliceSize);
                    Array.Copy(exArray, i, exToPass, 0, sliceSize);
                    MaterialPropertyBlock props = new MaterialPropertyBlock();
                    props.SetVectorArray("_Color", exToPass);
                    Utils.GraphicsUtility.DrawInstanced(commandBuffer, matriciesToPass, sliceSize, coloredMaterial, props);
                }

                var drawDataArray = new List<ClassicStatusFlagData>[] { flagDrawData, speechDrawData };
                var textureArray = new Texture2D[] { OpenTibiaUnity.GameManager.StateFlagsTexture, OpenTibiaUnity.GameManager.SpeechFlagsTexture };

                for (int i = 0; i < drawDataArray.Length; i++) {
                    var drawData = drawDataArray[i];
                    var texture = textureArray[i];

                    matrixArray = new Matrix4x4[drawData.Count];
                    exArray = new Vector4[drawData.Count];

                    for (int j = 0; j < drawData.Count; j++) {
                        var data = drawData[j];
                        matrixArray[j] = data.matrix;
                        exArray[j] = data.uv;
                    }

                    for (int j = 0; j < matrixArray.Length; j += 1023) {
                        int sliceSize = Mathf.Min(1023, matrixArray.Length - j);
                        Array.Copy(matrixArray, j, matriciesToPass, 0, sliceSize);
                        Array.Copy(exArray, j, exToPass, 0, sliceSize);
                        MaterialPropertyBlock props = new MaterialPropertyBlock();
                        props.SetTexture("_MainTex", texture);
                        props.SetVectorArray("_MainTex_UV", exToPass);
                        Utils.GraphicsUtility.DrawInstanced(commandBuffer, matriciesToPass, sliceSize, appearanceMaterial, props);
                    }
                }
            } else {
                foreach (var data in rectDrawData) {
                    MaterialPropertyBlock props = new MaterialPropertyBlock();
                    props.SetColor("_Color", data.color);
                    Utils.GraphicsUtility.Draw(commandBuffer, data.matrix, coloredMaterial, props);
                }

                var statesTexture = OpenTibiaUnity.GameManager.StateFlagsTexture;
                foreach (var data in flagDrawData) {
                    MaterialPropertyBlock props = new MaterialPropertyBlock();
                    props.SetTexture("_MainTex", statesTexture);
                    props.SetVector("_MainTex_UV", data.uv);
                    Utils.GraphicsUtility.Draw(commandBuffer, data.matrix, appearanceMaterial, props);
                }

                var speechTexture = OpenTibiaUnity.GameManager.SpeechFlagsTexture;
                foreach (var data in speechDrawData) {
                    MaterialPropertyBlock props = new MaterialPropertyBlock();
                    props.SetTexture("_MainTex", speechTexture);
                    props.SetVector("_MainTex_UV", data.uv);
                    Utils.GraphicsUtility.Draw(commandBuffer, data.matrix, appearanceMaterial, props);
                }
            }
        }

        private void InternalDrawEffects(CommandBuffer commandBuffer) {
            for (int i = 0; i < _drawnEffectsCount; i++) {
                var renderAtom = _drawnEffets[i];
                if (renderAtom.Object is Appearances.AppearanceInstance effect) {
                    var screenPosition = new Vector2Int(renderAtom.x, renderAtom.y);
                    effect.Draw(commandBuffer, screenPosition, renderAtom.fieldX, renderAtom.fieldY, renderAtom.fieldZ);
                    if (OptionStorage.ShowLightEffects && effect.Type.IsLight) {
                        var color = Colors.ColorFrom8Bit((byte)effect.Type.LightColor);
                        _lightmapRenderer.SetLightSource(renderAtom.lightX, renderAtom.lightY, renderAtom.positionZ, (uint)renderAtom.z, color);
                    }
                }
            }
        }

        private void InternalDrawTextualEffects(CommandBuffer commandBuffer) {
            for (int i = 0; i < _drawnTextualEffectsCount; i++) {
                var renderAtom = _drawnTextualEffets[i];
                if (renderAtom.Object is Appearances.TextualEffectInstance textualEffect) {
                    var pos = new Vector2(renderAtom.x, renderAtom.y);
                    pos.x = (pos.x - 2 * Constants.FieldSize) * LayerZoom.x + _realScreenTranslation.x;
                    pos.y = (pos.y - 2 * Constants.FieldSize) * LayerZoom.y + _realScreenTranslation.y;
                    pos.x -= Player.AnimationDelta.x * LayerZoom.x;
                    pos.y -= Player.AnimationDelta.y * LayerZoom.y;

                    textualEffect.Draw(commandBuffer, new Vector2Int((int)pos.x, (int)pos.y), 0, 0, 0);
                }
            }
        }
        
        private void InternalDrawOnscreenMessages(CommandBuffer commandBuffer) {
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
                        message.Draw(commandBuffer, screenPosition + _realScreenTranslation);
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
                
                message.Draw(commandBuffer, messagePosition + _realScreenTranslation);
            }
        }

        private void InternelDrawCreatureStatusClassic(CommandBuffer commandBuffer, Creatures.Creature creature,
                                                       List<ClassicStatusRectData> rectData,
                                                       List<ClassicStatusFlagData> flagData, List<ClassicStatusFlagData> speechData,
                                                       int rectX, int rectY, bool visible, ClassicStatusDescriptor descriptor) {
            bool isLocalPlayer = creature.Id == Player.Id;
            Color32 healthColor, manaColor;
            int healthIdentifier;
            if (visible) {
                healthIdentifier = creature.HealthPercent;
                healthColor = Creatures.Creature.GetHealthColor(creature.HealthPercent);
                manaColor = Colors.ColorFromRGB(0, 0, 255);
            } else {
                healthIdentifier = 192;
                healthColor = Colors.ColorFromRGB(192, 192, 192);
                manaColor = Colors.ColorFromRGB(192, 192, 192);
            }

            if (OptionStorage.ShowLightEffects) {
                float mod = _lightmapRenderer.CalculateCreatureBrightnessFactor(creature, isLocalPlayer);
                healthIdentifier += (byte)(255 * mod);
                healthColor = Utils.Utility.MulColor32(healthColor, mod);
                manaColor = Utils.Utility.MulColor32(manaColor, mod);
            }

            float minimumY = 0;
            float neededHeight = 0;
            CreatureStatus status = null;
            if (descriptor.showNames) {
                status = GetCreatureStatusCache(creature.Name, healthIdentifier, healthColor);
                neededHeight += status.Height / 2 + 1;
                minimumY = status.Height;
            }

            if (descriptor.showHealth)
                neededHeight += 4 + 1;

            if (descriptor.showMana)
                neededHeight += 4 + 1;

            var initialScreenPosition = new Vector2 {
                x = (rectX - Constants.FieldSize / 2 - Player.AnimationDelta.x) * LayerZoom.x + _realScreenTranslation.x,
                y = (rectY - Constants.FieldSize - Player.AnimationDelta.y) * LayerZoom.y + _realScreenTranslation.y
            };

            initialScreenPosition.y = Mathf.Clamp(initialScreenPosition.y, minimumY, Constants.WorldMapRealHeight * LayerZoom.y - neededHeight);

            var screenPosition = initialScreenPosition;
            screenPosition.y -= neededHeight;
            if (screenPosition.y < minimumY)
                screenPosition.y = minimumY;

            if (descriptor.showNames) {
                status.Draw(commandBuffer, screenPosition);
                screenPosition.y += (status.Height / 2) + 1;
            }

            if (descriptor.showHealth || descriptor.showMana) {
                var basePosition = new Vector2(screenPosition.x - 27 / 2f, screenPosition.y);
                var actualPosition = new Vector2(basePosition.x + 1, basePosition.y + 1);

                if (descriptor.showHealth) {
                    rectData.Add(new ClassicStatusRectData() {
                        matrix = Matrix4x4.TRS(basePosition, Quaternion.Euler(180, 0, 0), s_creatureStatSize),
                        color = Color.black
                    });

                    rectData.Add(new ClassicStatusRectData() {
                        matrix = Matrix4x4.TRS(actualPosition, Quaternion.Euler(180, 0, 0), new Vector3(creature.HealthPercent / 4f, 2, 1)),
                        color = healthColor
                    });

                    screenPosition.y += 4 + 1;
                    basePosition.y += 4 + 1;
                    actualPosition.y += 4 + 1;
                }

                if (descriptor.showMana) {
                    rectData.Add(new ClassicStatusRectData() {
                        matrix = Matrix4x4.TRS(basePosition, Quaternion.Euler(180, 0, 0), s_creatureStatSize),
                        color = Color.black
                    });

                    rectData.Add(new ClassicStatusRectData() {
                        matrix = Matrix4x4.TRS(actualPosition, Quaternion.Euler(180, 0, 0), new Vector3(creature.ManaPercent / 4f, 2, 1)),
                        color = manaColor
                    });

                    screenPosition.y += 4 + 1;
                }
            }

            if (descriptor.showMarks && !creature.IsNPC || descriptor.showIcons && creature.IsNPC)
                InternalDrawCreatureFlags(creature, flagData, speechData, (int)initialScreenPosition.x, (int)initialScreenPosition.y, visible);
        }

        private void InternalDrawCreatureFlags(Creatures.Creature creature, List<ClassicStatusFlagData> flagData,
                                               List<ClassicStatusFlagData> speechData, float rectX, float rectY, bool visible) {
            if (!creature.HasFlag)
                return;
            
            var screenPosition = new Vector2(rectX + 16 - Constants.StateFlagSize + 4, rectY + 1);
            var flagSize = new Vector2(Constants.StateFlagSize, Constants.StateFlagSize);
            int dX = 0;
            
            var flagsTexture = OpenTibiaUnity.GameManager.StateFlagsTexture;
            if (creature.PartyFlag > PartyFlag.None) {
                var r = NormalizeFlagRect(GetPartyFlagTextureRect(creature.PartyFlag), flagsTexture);
                flagData.Add(new ClassicStatusFlagData {
                    matrix = Matrix4x4.TRS(screenPosition, Quaternion.Euler(180, 0, 0), flagSize),
                    uv = new Vector4(r.width, r.height, r.x, r.y)
                });

                dX += Constants.StateFlagGap + Constants.StateFlagSize;
                screenPosition.x += Constants.StateFlagGap + Constants.StateFlagSize;
            }

            if (creature.PkFlag > PkFlag.None) {
                var r = NormalizeFlagRect(GetPKFlagTextureRect(creature.PkFlag), flagsTexture);
                flagData.Add(new ClassicStatusFlagData {
                    matrix = Matrix4x4.TRS(screenPosition, Quaternion.Euler(180, 0, 0), flagSize),
                    uv = new Vector4(r.width, r.height, r.x, r.y)
                });

                screenPosition.x += Constants.StateFlagGap + Constants.StateFlagSize;
            }

            if (creature.SummonType > SummonType.None) {
                var r = NormalizeFlagRect(GetSummonFlagTextureRect(creature.SummonType), flagsTexture);
                flagData.Add(new ClassicStatusFlagData {
                    matrix = Matrix4x4.TRS(screenPosition, Quaternion.Euler(180, 0, 0), flagSize),
                    uv = new Vector4(r.width, r.height, r.x, r.y)
                });

                dX += Constants.StateFlagGap + Constants.StateFlagSize;
                screenPosition.x += Constants.StateFlagGap + Constants.StateFlagSize;
            }

            var speechCategory = creature.SpeechCategory;
            if (speechCategory == SpeechCategory.QuestTrader)
                speechCategory = OpenTibiaUnity.TicksMillis % 2048 <= 1024 ? SpeechCategory.Quest : SpeechCategory.Trader;

            if (speechCategory > SpeechCategory.None) {
                var speechTexture = OpenTibiaUnity.GameManager.SpeechFlagsTexture;
                var r = NormalizeFlagRect(GetSpeechFlagTextureRect(speechCategory), speechTexture);
                speechData.Add(new ClassicStatusFlagData {
                    matrix = Matrix4x4.TRS(screenPosition, Quaternion.Euler(180, 0, 0), new Vector2(Constants.SpeechFlagSize, Constants.SpeechFlagSize)),
                    uv = new Vector4(r.width, r.height, r.x, r.y)
                });

                dX += Constants.StateFlagGap + Constants.SpeechFlagSize;
                screenPosition.x += Constants.StateFlagGap + Constants.SpeechFlagSize;
            }
            
            if (dX > 0)
                screenPosition.x -= dX;
            
            var gameManager = OpenTibiaUnity.GameManager;
            if (!gameManager.GetFeature(GameFeature.GameCreatureMarks) || dX > 0)
                screenPosition.y += Constants.StateFlagGap + Constants.StateFlagSize;
            
            if (creature.GuildFlag > GuildFlag.None) {
                var r = NormalizeFlagRect(GetGuildFlagTextureRect(creature.GuildFlag), flagsTexture);
                flagData.Add(new ClassicStatusFlagData {
                    matrix = Matrix4x4.TRS(screenPosition, Quaternion.Euler(180, 0, 0), flagSize),
                    uv = new Vector4(r.width, r.height, r.x, r.y)
                });

                screenPosition.y += Constants.StateFlagGap + Constants.StateFlagSize;
            }
            
            if (creature.RisknessFlag > RisknessFlag.None) {
                var r = NormalizeFlagRect(GetRisknessFlagTextureRect(creature.RisknessFlag), flagsTexture);
                flagData.Add(new ClassicStatusFlagData {
                    matrix = Matrix4x4.TRS(screenPosition, Quaternion.Euler(180, 0, 0), flagSize),
                    uv = new Vector4(r.width, r.height, r.x, r.y)
                });
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
            float lastMagnitude;
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

        private CreatureStatus GetCreatureStatusCache(string name, int identifier, Color color) {
            int hashCode = string.Format("{0}#{1}", name, identifier).GetHashCode();
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

        private Rect GetPKFlagTextureRect(PkFlag pkFlag) {
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
        
        public Vector3Int? PointToMap(Vector2 point, bool restrictedToPlayerZPlane) {
            if (WorldMapStorage == null || (restrictedToPlayerZPlane && !Player))
                return null;

            int x = (int)(point.x / (Constants.FieldSize * LayerZoom.x)) + 1;
            int y = (int)(point.y / (Constants.FieldSize * LayerZoom.y)) + 1;
            if (x < 0 || x > Constants.MapWidth || y < 0 || y > Constants.MapHeight)
                return null;


            int z;
            if (restrictedToPlayerZPlane) {
                z = WorldMapStorage.ToMap(Player.Position).z;
            } else {
                int minZ = _minZPlane[y * Constants.MapSizeX + x];
                for (z = _maxZPlane; z > minZ; z--) {
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
                }
            }

            if (z < 0 || z >= Constants.MapSizeZ)
                return null;
            return new Vector3Int(x, y, z);
        }

        public Vector3Int? PointToAbsolute(Vector2 point, bool restrictedToPlayerZPlane) {
            var mapPosition = PointToMap(point, restrictedToPlayerZPlane);
            if (mapPosition.HasValue)
                return WorldMapStorage.ToAbsolute(mapPosition.Value);
            return null;
        }

        public Creatures.Creature PointToCreature(Vector2 point, bool restrictedToPlayerZPlane) {
            if (WorldMapStorage == null || (restrictedToPlayerZPlane && !Player))
                return null;

            int rectX = (int)(point.x / LayerZoom.x) + Constants.FieldSize;
            int rectY = (int)(point.y / LayerZoom.y) + Constants.FieldSize;

            int mapX = rectX / Constants.FieldSize;
            int mapY = rectY / Constants.FieldSize;

            if (mapX < 0 || mapX > Constants.MapWidth || mapY < 0 || mapY > Constants.MapHeight)
                return null;

            int minZ = _minZPlane[mapY * Constants.MapSizeX + mapX];
            if (restrictedToPlayerZPlane)
                minZ = WorldMapStorage.ToMap(Player.Position).z;

            for (int i = 0; i < _drawnCreaturesCount; i++) {
                var renderAtom = _drawnCreatures[i];
                int renderX = renderAtom.x - Constants.FieldSize / 2;
                int renderY = renderAtom.y - Constants.FieldSize / 2;

                if (!(Math.Abs(renderX - rectX) > Constants.FieldSize / 2 || Math.Abs(renderY - rectY) > Constants.FieldSize / 2)) {
                    var creature = renderAtom.Object as Creatures.Creature;
                    if (!!creature && WorldMapStorage.IsVisible(creature.Position, true) && WorldMapStorage.ToMap(creature.Position).z >= minZ)
                        return creature;
                }
            }

            return null;
        }
    }
}
