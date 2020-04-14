using System.Collections.Generic;
using System.Linq;

namespace OpenTibiaUnity.Core.WorldMap
{
    public class WorldMapStorage {
        private List<int> _cacheObjectsCount;
        private int _effectsCount = 0;
        private Field[] _fields = new Field[Constants.NumFields];
        private Appearances.AppearanceInstance[] _effects = new Appearances.AppearanceInstance[Constants.NumEffects];

        private List<bool>[] _layerBrightnessInfos = new List<bool>[Constants.MapSizeZ];
        private UnityEngine.Vector3Int _position = UnityEngine.Vector3Int.zero;
        private UnityEngine.Vector3Int _origin = UnityEngine.Vector3Int.zero;
        private int _ambientNextUpdate = 0;
        private int _objectNextUpdate = 0;
        private UnityEngine.Vector3Int _helperCoordinate = UnityEngine.Vector3Int.zero;
        private readonly TMPro.TextMeshProUGUI _textBoxPrefab;

        public int AmbientCurrentBrightness = -1;
        public int AmbientTargetBrightness = -1;
        public UnityEngine.Color32 AmbientCurrentColor = UnityEngine.Color.black;
        public UnityEngine.Color32 AmbientTargetColor = UnityEngine.Color.black;

        public UnityEngine.Vector3Int Position {
            get { return _position; }
            set {
                _position = value;
                PlayerZPlane = value.z <= Constants.GroundLayer ? (Constants.MapSizeZ - 1 - value.z) : Constants.UndergroundLayer;
            }
        }
        public bool Valid { get; set; } = false;
        public bool CacheFullbank { get; set; } = false;
        public bool CacheRefresh { get; set; } = false;
        public int PlayerZPlane { get; private set; } = 0;
        public bool LayoutOnscreenMessages { get; set; } = false;
        public List<OnscreenMessageBox> MessageBoxes { get; } = new List<OnscreenMessageBox>();

        public WorldMapStorage(TMPro.TextMeshProUGUI textBoxPrefab) {
            for (int i = 0; i < _fields.Length; i++)
                _fields[i] = new Field();

            _textBoxPrefab = textBoxPrefab;

            //MessageScreenTarget.BoxBottom
            MessageBoxes.Add(new OnscreenMessageBox(null, null, 0, MessageModeType.None, 1));
            //MessageScreenTarget.BoxLow
            MessageBoxes.Add(new OnscreenMessageBox(null, null, 0, MessageModeType.None, 1));
            //MessageScreenTarget.BoxHigh
            MessageBoxes.Add(new OnscreenMessageBox(null, null, 0, MessageModeType.None, 1));
            //MessageScreenTarget.BoxTop
            MessageBoxes.Add(new OnscreenMessageBox(null, null, 0, MessageModeType.None, 1));

            _cacheObjectsCount = new List<int>(Enumerable.Repeat(0, Constants.MapSizeZ));
            for (int i = 0; i < _layerBrightnessInfos.Length; i++) {
                _layerBrightnessInfos[i] = new List<bool>(Constants.MapSizeX * Constants.MapSizeY);
                for (int j = 0; j < _layerBrightnessInfos[i].Capacity; j++)
                    _layerBrightnessInfos[i].Add(false);
            }
            
        }

        public Appearances.ObjectInstance GetEnvironmentalEffect(UnityEngine.Vector3Int mapPosition) {
            return GetField(mapPosition).EnvironmentalEffect;
        }

        public void SetEnvironmentalEffect(UnityEngine.Vector3Int mapPosition, Appearances.ObjectInstance effectObject) {
            GetField(mapPosition).EnvironmentalEffect = effectObject;
        }

        // Object Operations
        private void AssertNullObject(Appearances.ObjectInstance @object, string functor) {
            if (!@object)
                throw new System.ArgumentNullException(string.Format("%s: %s", functor, "@object can't be null."));
        }
        public Appearances.ObjectInstance AppendObject(UnityEngine.Vector3Int mapPosition, Appearances.ObjectInstance @object) {
            AssertNullObject(@object, "WorldMapStorage.AppendObject");
            var otherObj = GetField(mapPosition).PutObject(@object, Constants.MapSizeW);
            if (!!otherObj && otherObj.IsCreature)
                OpenTibiaUnity.CreatureStorage.MarkOpponentVisible(otherObj.Data, false);
            else if (otherObj == null)
                _cacheObjectsCount[(_origin.z + mapPosition.z) % Constants.MapSizeZ]++;
            
            if (!!@object.Type && @object.Type.IsFullGround)
                CacheFullbank = false;

            return otherObj;
        }
        public Appearances.ObjectInstance PutObject(UnityEngine.Vector3Int mapPosition, Appearances.ObjectInstance @object) {
            AssertNullObject(@object, "WorldMapStorage.PutObject");
            return InsertObject(mapPosition, -1, @object);
        }
        public Appearances.ObjectInstance InsertObject(UnityEngine.Vector3Int mapPosition, int stackPos, Appearances.ObjectInstance @object) {
            AssertNullObject(@object, "WorldMapStorage.InsertObject");
            var otherObj = GetField(mapPosition).PutObject(@object, stackPos);
            if (!!otherObj && otherObj.IsCreature)
                OpenTibiaUnity.CreatureStorage.MarkOpponentVisible(otherObj.Data, false);
            else if (!otherObj)
                _cacheObjectsCount[(_origin.z + mapPosition.z) % Constants.MapSizeZ]++;

            if (!!otherObj && otherObj.Type.IsFullGround || @object.Type.IsFullGround)
                CacheFullbank = false;

            return otherObj;
        }
        public Appearances.ObjectInstance ChangeObject(UnityEngine.Vector3Int mapPosition, int stackPos, Appearances.ObjectInstance @object) {
            AssertNullObject(@object, "WorldMapStorage.ChangeObject");
            Appearances.ObjectInstance otherObj = GetField(mapPosition).ChangeObject(@object, stackPos);
            if (!!otherObj && otherObj.IsCreature &&
                !!@object && @object.IsCreature && @object.Data != otherObj.Data)
                OpenTibiaUnity.CreatureStorage.MarkOpponentVisible(otherObj.Data, false);

            if (!!otherObj && otherObj.Type.IsFullGround || @object.Type.IsFullGround)
                CacheFullbank = false;

            return otherObj;
        }
        public Appearances.ObjectInstance DeleteObject(UnityEngine.Vector3Int mapPosition, int stackPos, bool visibleRemark = true) {
            Appearances.ObjectInstance otherObj = GetField(mapPosition).DeleteObject(stackPos);
            if (visibleRemark && !!otherObj && otherObj.IsCreature)
                OpenTibiaUnity.CreatureStorage.MarkOpponentVisible(otherObj.Data, false);

            if (!!otherObj)
                _cacheObjectsCount[(_origin.z + mapPosition.z) % Constants.MapSizeZ]--;

            if (!!otherObj && otherObj.Type.IsFullGround)
                CacheFullbank = false;

            return otherObj;
        }

        public void AppendEffect(UnityEngine.Vector3Int absolutePosition, Appearances.AppearanceInstance effect) {
            UnityEngine.Vector3Int? mapPosition = ToMapInternal(absolutePosition);

            int index = -1;
            Field field = null;
            if (mapPosition != null) {
                index = ToIndexInternal(mapPosition.Value);
                field = _fields[index];
            }

            if (!!field && effect is Appearances.TextualEffectInstance textualEffect) {
                for (int i = field.EffectsCount - 1; i > 0; i--) {
                    var otherEffect = field.Effects[i];
                    if (otherEffect is Appearances.TextualEffectInstance && textualEffect.Merge(otherEffect))
                        return;
                }
            }

            if (_effectsCount < Constants.NumEffects) {
                effect.MapField = index;
                effect.MapData = 0;
                if (!!field)
                    field.AppendEffect(effect);

                _effects[_effectsCount] = effect;
                _effectsCount++;
            }
        }
        public void MoveEffect(UnityEngine.Vector3Int absolutePosition, int effectIndex) {
            if (effectIndex < 0 || effectIndex >= _effectsCount)
                return;

            UnityEngine.Vector3Int? mapPosition = ToMapInternal(absolutePosition);
            int index = -1;
            if (mapPosition != null)
                index = ToIndexInternal(mapPosition.Value);

            var effect = _effects[effectIndex];
            if (effect.MapField == index)
                return;

            if (effect.MapField > -1) {
                _fields[effect.MapField].DeleteEffect(effect.MapData);
                effect.MapField = -1;
                effect.MapData = 0;
            }

            if (index > -1) {
                effect.MapField = index;
                effect.MapData = 0;
                _fields[index].AppendEffect(effect);
            }
        }
        private void DeleteEffect(int effectIndex) {
            if (effectIndex < 0 || effectIndex >= _effectsCount)
                throw new System.Exception("WorldMapStorage.DeleteEffect: effect is out of range.");

            var effect = _effects[effectIndex];
            if (effect.MapField != -1)
                _fields[effect.MapField].DeleteEffect(effect.MapData);

            effect.MapData = -1;
            effect.MapField = -1;
            _effectsCount--;

            if (effectIndex < _effectsCount)
                _effects[effectIndex] = _effects[_effectsCount];
            
            _effects[_effectsCount] = null;
        }

        public int GetTopLookObject(UnityEngine.Vector3Int mapPosition, out Appearances.ObjectInstance @object) {
            return GetField(mapPosition).GetTopLookObject(out @object);
        }
        public int GetTopLookObject(UnityEngine.Vector3Int mapPosition) {
            return GetField(mapPosition).GetTopLookObject(out Appearances.ObjectInstance @object);
        }
        public int GetTopMultiUseObject(UnityEngine.Vector3Int mapPosition, out Appearances.ObjectInstance @object) {
            return GetField(mapPosition).GetTopMultiUseObject(out @object);
        }
        public int GetTopMultiUseObject(UnityEngine.Vector3Int mapPosition) {
            return GetField(mapPosition).GetTopMultiUseObject(out Appearances.ObjectInstance @object);
        }
        public int GetTopUseObject(UnityEngine.Vector3Int mapPosition, out Appearances.ObjectInstance @object) {
            return GetField(mapPosition).GetTopUseObject(out @object);
        }
        public int GetTopUseObject(UnityEngine.Vector3Int mapPosition) {
            return GetField(mapPosition).GetTopUseObject(out Appearances.ObjectInstance @object);
        }
        public int GetTopMoveObject(UnityEngine.Vector3Int mapPosition, out Appearances.ObjectInstance @object) {
            return GetField(mapPosition).GetTopMoveObject(out @object);
        }
        public int GetTopMoveObject(UnityEngine.Vector3Int mapPosition) {
            return GetField(mapPosition).GetTopMoveObject(out Appearances.ObjectInstance @object);
        }
        public int GetTopCreatureObject(UnityEngine.Vector3Int mapPosition, out Appearances.ObjectInstance @object) {
            return GetField(mapPosition).GetTopCreatureObject(out @object);
        }
        public int GetTopCreatureObject(UnityEngine.Vector3Int mapPosition) {
            return GetField(mapPosition).GetTopCreatureObject(out Appearances.ObjectInstance @object);
        }
        
        public Appearances.ObjectInstance GetObject(UnityEngine.Vector3Int mapPosition, int stackPos) {
            return GetField(mapPosition).GetObject(stackPos);
        }
        public Appearances.ObjectInstance GetObject(int mapX, int mapY, int mapZ, int stackPos) {
            return _fields[ToIndexInternal(mapX, mapY, mapZ)].GetObject(stackPos);
        }
        public int GetObjectPerLayer(int z) {
            if (z < 0 || z > Constants.MapSizeZ)
                throw new System.Exception("WorldMapStorage.GetObjectPerLayer: z=" + z + " is out of range");

            return _cacheObjectsCount[(_origin.z + z) % Constants.MapSizeZ];
        }
        public int GetCreatureObjectForCreature(Creatures.Creature creature, out Appearances.ObjectInstance @object) {
            if (!!creature) {
                var mapPosition = ToMap(creature.Position);
                return GetField(mapPosition).GetCreatureObjectForCreatureId(creature.Id, out @object);
            }

            @object = null;
            return -1;
        }
        public int GetCreatureObjectForCreature(Creatures.Creature creature) {
            Appearances.ObjectInstance _;
            return GetCreatureObjectForCreature(creature, out _);
        }

        public OnscreenMessageBox AddOnscreenMessage(MessageModeType mode, string text) {
            return AddOnscreenMessage(null, -1, null, 0, mode, text, int.MaxValue);
        }

        public OnscreenMessageBox AddOnscreenMessage(UnityEngine.Vector3Int? absolutePosition, int statementId, string speaker, int speakerLevel, MessageModeType mode, string text) {
            return AddOnscreenMessage(absolutePosition, statementId, speaker, speakerLevel, mode, text, int.MaxValue);
        }

        public OnscreenMessageBox AddOnscreenMessage(UnityEngine.Vector3Int? absolutePosition, int statementId, string speaker, int speakerLevel, MessageModeType mode, Utils.UnionStrInt text, int color) {
            var messageFilterSet = OpenTibiaUnity.OptionStorage.GetMessageFilterSet(Chat.MessageFilterSet.DefaultSet);
            var messageMode = messageFilterSet.GetMessageMode(mode);
            if (messageMode == null || !messageMode.ShowOnScreen || messageMode.ScreenTarget == MessageScreenTargets.None)
                return null;
            
            var nameFilterSet = OpenTibiaUnity.OptionStorage.GetNameFilterSet(Chat.NameFilterSet.DefaultSet);
            if (!messageMode.IgnoreNameFilter && (nameFilterSet == null || !nameFilterSet.AcceptMessage(mode, speaker, text)))
                return null;
            
            var screenTarget = messageMode.ScreenTarget;
            if (screenTarget == MessageScreenTargets.EffectCoordinate) {
                if (!absolutePosition.HasValue)
                    throw new System.Exception("WorldMapStorage.AddOnscreenMessage: Missing co-ordinate.");
                
                if (text.IsInt)
                    AddTextualEffect(absolutePosition.Value, color, ((int)text).ToString());
                return null;
            }

            OnscreenMessageBox messageBox = null;
            OnscreenMessage message = null;
            if (screenTarget == MessageScreenTargets.BoxCoordinate) {
                if (!absolutePosition.HasValue)
                    throw new System.Exception("WorldMapStorage.AddOnscreenMessage: Missing co-ordinate.");

                bool visible = true;

                for (int i = (int)screenTarget; i < MessageBoxes.Count; i++) {
                    var tmpMessageBox = MessageBoxes[i];
                    if (tmpMessageBox.Position == null || tmpMessageBox.Position == absolutePosition.Value) {
                        if (tmpMessageBox.Speaker == speaker && tmpMessageBox.Mode == mode) {
                            messageBox = tmpMessageBox;
                            break;
                        }

                        visible = false;
                    }
                }

                if (messageBox == null) {
                    messageBox = new OnscreenMessageBox(absolutePosition, speaker, speakerLevel, mode, Constants.NumOnscreenMessages) {
                        Visible = visible
                    };

                    var tmpMessage = messageMode.GetOnscreenMessageHeader(speaker, speakerLevel);
                    if (tmpMessage != null) {
                        message = new OnscreenMessage(-1, speaker, speakerLevel, mode, tmpMessage);
                        message.FormatMessage(null, messageMode.TextARGB, messageMode.HighlightARGB);
                        message.TTL = int.MaxValue;
                        messageBox.AppendMessage(message);
                    }

                    MessageBoxes.Add(messageBox);
                }
            } else {
                messageBox = MessageBoxes[(int)screenTarget];
            }

            message = new OnscreenMessage(statementId, speaker, speakerLevel, mode, text);
            message.FormatMessage(messageMode.GetOnscreenMessagePrefix(speaker, speakerLevel), messageMode.TextARGB, messageMode.HighlightARGB);
            messageBox.AppendMessage(message);
            messageBox.Visible = true;
            InvalidateOnscreenMessages();
            return messageBox;
        }

        public void AddTextualEffect(UnityEngine.Vector3Int absolutePosition, int color, string text, int value =- 1, bool mergable = true) {
            var textualEffect = OpenTibiaUnity.AppearanceStorage.CreateTextualEffect(color, text, value);
            AppendEffect(absolutePosition, textualEffect);
        }

        public void ExpireOldestMessages() {
            int minExpirationTime = int.MaxValue;
            int minExpirationStartIndex = -1;

            for (int i = MessageBoxes.Count - 1; i >= 0; i--) {
                int currentMinExpirationTime = MessageBoxes[i].MinExpirationTime;
                if (currentMinExpirationTime < minExpirationTime) {
                    minExpirationTime = currentMinExpirationTime;
                    minExpirationStartIndex = i;
                }
            }

            if (minExpirationStartIndex > -1) {
                var messageBox = MessageBoxes[minExpirationStartIndex];
                int count = messageBox.ExpireOldestMessage();
                if (count > 0)
                    LayoutOnscreenMessages = true;

                if (messageBox.Empty) {
                    if (minExpirationStartIndex >= (int)MessageScreenTargets.BoxCoordinate) {
                        messageBox.RemoveMessages();
                        // todo destroy text mesh in a separate job
                        MessageBoxes.RemoveAt(minExpirationStartIndex);
                        LayoutOnscreenMessages = true;
                    } else {
                        // bottom -> top exist all the time, we just set their visiblity to false
                        // to avoid unnessecary calculations
                        messageBox.Visible = false;
                    }
                }
            }
        }

        private int ToIndexInternal(UnityEngine.Vector3Int mapPosition) {
            return ToIndexInternal(mapPosition.x, mapPosition.y, mapPosition.z);
        }

        private int ToIndexInternal(int mapX, int mapY, int mapZ) {
            if (mapX < 0 || mapX >= Constants.MapSizeX || mapY < 0 || mapY >= Constants.MapSizeY || mapZ < 0 || mapZ >= Constants.MapSizeZ)
                throw new System.ArgumentException($"WorldMapStorage.ToIndexInternal: Input co-oridnate ({mapX}, {mapY}, {mapZ}) is out of range.");

            return ((mapZ + _origin.z) % Constants.MapSizeZ * Constants.MapSizeX + (mapX + _origin.x) % Constants.MapSizeX) * Constants.MapSizeY + (mapY + _origin.y) % Constants.MapSizeY;
        }

        public Field GetField(UnityEngine.Vector3Int mapPosition) {
            return GetField(mapPosition.x, mapPosition.y, mapPosition.z);
        }

        public Field GetField(int mapX, int mapY, int mapZ) {
            return _fields[ToIndexInternal(mapX, mapY, mapZ)];
        }

        public void SetAmbientLight(UnityEngine.Color color, int intensity) {
            var mustUpdateAmbient = AmbientTargetBrightness < 0;
            AmbientTargetColor = color;
            AmbientTargetBrightness = intensity;
            if (mustUpdateAmbient) {
                AmbientCurrentColor = AmbientTargetColor;
                AmbientCurrentBrightness = AmbientTargetBrightness;
            }
        }

        public void Reset() {
            ResetMap();
            InvalidateOnscreenMessages();
        }

        public void ResetMap() {
            _position = UnityEngine.Vector3Int.zero;
            PlayerZPlane = 0;
            _origin = UnityEngine.Vector3Int.zero;

            for (int i = 0; i < _fields.Length; i++)
                _fields[i].Reset();

            _effects = new Appearances.AppearanceInstance[Constants.NumEffects];
            _effectsCount = 0;

            _cacheObjectsCount.Clear();
            _cacheObjectsCount.AddRange(Enumerable.Repeat(0, Constants.MapSizeZ));

            CacheFullbank = false;
            Valid = false;
            _ambientNextUpdate = 0;
            _objectNextUpdate = 0;
        }

        public void ResetField(UnityEngine.Vector3Int mapPosition, bool resetCreatures = true, bool resetEffects = true) {
            int index = ToIndexInternal(mapPosition);
            Field field = _fields[index];
            _cacheObjectsCount[(_origin.z + mapPosition.z) % Constants.MapSizeZ] = 
                _cacheObjectsCount[(_origin.z + mapPosition.z) % Constants.MapSizeZ] - field.ObjectsCount;
            CacheFullbank = false;

            if (resetCreatures) {
                var creatureStorage = OpenTibiaUnity.CreatureStorage;
                for (int i = field.ObjectsCount - 1; i > 0; i--) {
                    var @object = field.ObjectsNetwork[i];
                    if (@object.IsCreature) {
                        creatureStorage.MarkOpponentVisible(@object, false);
                    }
                }
            }

            field.ResetObjects();

            if (resetEffects) {
                for (int i = field.EffectsCount - 1; i > 0; i--) {
                    var effect = field.Effects[i];
                    if (effect.MapData == index) {
                        DeleteEffect(i);
                    }
                }

                field.ResetEffects();
            }
        }

        public void ResetField(int x, int y, int z, bool resetCreatures = true, bool resetEffects = true) {
            ResetField(new UnityEngine.Vector3Int(x, y, z), resetCreatures, resetEffects);
        }

        public void RefreshFields() {
            if (!Valid || !CacheRefresh)
                return;

            CacheRefresh = false;
            for (int i = 0; i < _fields.Length; i++)
                _fields[i].UpdateObjectsCache();
        }

        public void InvalidateFieldsTRS() {
            for (int i = 0; i < _fields.Length; i++)
                _fields[i].InvalidateObjectsTRS();
        }

        public void InvalidateOnscreenMessages() {
            LayoutOnscreenMessages = true;
        }

        public void UpdateMiniMap(UnityEngine.Vector3Int mapPosition) {
            GetField(mapPosition).UpdateMiniMap();
        }

        public uint GetMiniMapColour(UnityEngine.Vector3Int mapPosition) {
            return GetField(mapPosition).MiniMapColor;
        }

        public int GetMiniMapCost(UnityEngine.Vector3Int mapPosition) {
            return GetField(mapPosition).MiniMapCost;
        }

        public int GetMiniMapCost(int mapX, int mapY, int mapZ) {
            return GetField(mapX, mapY, mapZ).MiniMapCost;
        }

        public void ScrollMap(int dx, int dy, int dz = 0) {
            if (dx < -Constants.MapSizeX || dx > Constants.MapSizeX)
                throw new System.ArgumentException("WorldMapStorage.ScrollMap: X=" + dx + " is out of range.");
            else if (dy < -Constants.MapSizeY || dy > Constants.MapSizeY)
                throw new System.ArgumentException("WorldMapStorage.ScrollMap: Y=" + dy + " is out of range.");
            else if (dz < -Constants.MapSizeZ || dz > Constants.MapSizeZ)
                throw new System.ArgumentException("WorldMapStorage.ScrollMap: Z=" + dz + " is out of range.");
            else if(dx * dy + dy * dz + dx * dz != 0)
                throw new System.ArgumentException("WorldMapStorage.ScrollMap: Only one of the agruments  may be != 0.");

            if (dx != 0) {
                int startx;
                int endx;
                if (dx > 0) {
                    startx = Constants.MapSizeX - dx;
                    endx = Constants.MapSizeX;
                } else {
                    startx = 0;
                    endx = -dx;
                }

                for (int tmpx = startx; tmpx < endx; tmpx++) {
                    for (int tmpy = 0; tmpy < Constants.MapSizeY; tmpy++) {
                        for (int tmpz = 0; tmpz < Constants.MapSizeZ; tmpz++) {
                            ResetField(tmpx, tmpy, tmpz);
                        }
                    }
                }

                _origin.x -= dx;
                if (_origin.x < 0)
                    _origin.x += Constants.MapSizeX;

                _origin.x %= Constants.MapSizeX;
            }

            if (dy != 0) {
                int starty;
                int endy;
                if (dy > 0) {
                    starty = Constants.MapSizeY - dy;
                    endy = Constants.MapSizeY;
                } else {
                    starty = 0;
                    endy = -dy;
                }

                for (int tmpx = 0; tmpx < Constants.MapSizeX; tmpx++) {
                    for (int tmpy = starty; tmpy < endy; tmpy++) {
                        for (int tmpz = 0; tmpz < Constants.MapSizeZ; tmpz++)
                            ResetField(tmpx, tmpy, tmpz);
                    }
                }

                _origin.y -= dy;
                if (_origin.y < 0)
                    _origin.y += Constants.MapSizeY;

                _origin.y %= Constants.MapSizeY;
            }

            if (dz != 0) {
                int startz;
                int endz;
                if (dy > 0) {
                    startz = Constants.MapSizeZ - dz;
                    endz = Constants.MapSizeZ;
                } else {
                    startz = 0;
                    endz = -dz;
                }

                for (int tmpx = 0; tmpx < Constants.MapSizeX; tmpx++) {
                    for (int tmpy = 0; tmpy < Constants.MapSizeY; tmpy++) {
                        for (int tmpz = startz; tmpz < endz; tmpz++)
                            ResetField(tmpx, tmpy, tmpz);
                    }
                }

                _origin.z -= dz;
                if (_origin.z < 0)
                    _origin.z += Constants.MapSizeZ;

                _origin.z %= Constants.MapSizeZ;
                if (dz > 0) {
                    for (int tmpx = 0; tmpx < Constants.MapSizeX; tmpx++) {
                        for (int tmpy = 0; tmpy < Constants.MapSizeY; tmpy++) {
                            for (int tmpz = Constants.MapSizeZ - Constants.UndergroundLayer - 1; tmpz < Constants.MapSizeZ; tmpz++)
                                ResetField(tmpx, tmpy, tmpz);
                        }
                    }
                }
            }
        }

        public UnityEngine.Vector3Int ToMap(UnityEngine.Vector3Int absolutePosition) {
            UnityEngine.Vector3Int? mapPosition = ToMapInternal(absolutePosition);
            if (mapPosition == null)
                throw new System.ArgumentException("WorldMapStorage.ToMap: Input co-ordinate " + absolutePosition + " is out of range (m=" + _position + ").");

            return mapPosition.Value;
        }

        public void ToMap(UnityEngine.Vector3Int absolutePosition, out UnityEngine.Vector3Int outPosition) {
            outPosition = ToMap(absolutePosition);
        }

        public UnityEngine.Vector3Int ToMapClosest(UnityEngine.Vector3Int absolutePosition) {
            int dZ = _position.z - absolutePosition.z;
            int x = UnityEngine.Mathf.Max(0, UnityEngine.Mathf.Min(absolutePosition.x - (_position.x - Constants.PlayerOffsetX) - dZ, Constants.MapSizeX - 1));
            int y = UnityEngine.Mathf.Max(0, UnityEngine.Mathf.Min(absolutePosition.y - (_position.y - Constants.PlayerOffsetY) - dZ, Constants.MapSizeY - 1));
            int z = UnityEngine.Mathf.Max(0, UnityEngine.Mathf.Min(PlayerZPlane + dZ, Constants.MapSizeZ - 1));

            return new UnityEngine.Vector3Int(x, y, z);
        }

        public void ToMapClosest(UnityEngine.Vector3Int absolutePosition, out UnityEngine.Vector3Int outPosition) {
            outPosition = ToMapClosest(absolutePosition);
        }

        private UnityEngine.Vector3Int? ToMapInternal(UnityEngine.Vector3Int absolutePosition) {
            return ToMapInternal(absolutePosition.x, absolutePosition.y, absolutePosition.z);
        }

        private UnityEngine.Vector3Int? ToMapInternal(int absoluteX, int absoluteY, int absoluteZ) {
            int dZ = _position.z - absoluteZ;
            absoluteX -= (_position.x - Constants.PlayerOffsetX) + dZ;
            absoluteY -= (_position.y - Constants.PlayerOffsetY) + dZ;
            absoluteZ = PlayerZPlane + dZ;

            if (absoluteX < 0 || absoluteX >= Constants.MapSizeX || absoluteY < 0 || absoluteY >= Constants.MapSizeY || absoluteZ < 0 || absoluteZ >= Constants.MapSizeZ)
                return null;

            return new UnityEngine.Vector3Int(absoluteX, absoluteY, absoluteZ);
        }

        public UnityEngine.Vector3Int ToAbsolute(UnityEngine.Vector3Int mapPosition) {
            return ToAbsolute(mapPosition.x, mapPosition.y, mapPosition.z);
        }

        public UnityEngine.Vector3Int ToAbsolute(int mapX, int mapY, int mapZ) {
            if (mapX < 0 || mapX >= Constants.MapSizeX || mapY < 0 || mapY >= Constants.MapSizeY || mapZ < 0 || mapZ >= Constants.MapSizeZ)
                throw new System.ArgumentException($"WorldMapStorage.ToAbsolute: Input co-oridnate ({mapX}, {mapY}, {mapZ}) is out of range.");

            int diffz = mapZ - PlayerZPlane;

            return new UnityEngine.Vector3Int() {
                x = mapX + (_position.x - Constants.PlayerOffsetX) + diffz,
                y = mapY + (_position.y - Constants.PlayerOffsetY) + diffz,
                z = _position.z - diffz
            };
        }

        public void ToAbsolute(UnityEngine.Vector3Int mapPosition, out UnityEngine.Vector3Int absolutePosition) {
            absolutePosition = ToAbsolute(mapPosition);
        }

        public void ToAbsolute(int x, int y, int z, out UnityEngine.Vector3Int absolutePosition) {
            absolutePosition = ToAbsolute(x, y, z);
        }

        public bool IsVisible(UnityEngine.Vector3Int absolutePosition, bool param4) {
            UnityEngine.Vector3Int? mapPosition = ToMapInternal(absolutePosition);
            return mapPosition.HasValue && (param4 || _position.z == absolutePosition.z);
        }

        public bool IsVisible(int absoluteX, int absoluteY, int absoluteZ, bool ignorePlayerFloor) {
            UnityEngine.Vector3Int? mapPosition = ToMapInternal(absoluteX, absoluteY, absoluteZ);
            return mapPosition.HasValue && (ignorePlayerFloor || _position.z == absoluteZ);
        }

        public bool IsLookPossible(int x, int y, int z) {
            return !_fields[ToIndexInternal(x, y, z)].CacheUnsight;
        }

        public bool IsTranslucent(int x, int y, int z) {
            return _fields[ToIndexInternal(x, y, z)].CacheTranslucent;
        }
        
        public void Animate() {
            var ticks = OpenTibiaUnity.TicksMillis;
            if (ticks >= _objectNextUpdate) {
                for (int i = _effectsCount - 1; i >= 0; i--) {
                    var effect = _effects[i];
                    if (!effect.Animate(ticks))
                        DeleteEffect(i);
                    else if (effect is Appearances.MissileInstance missleEffect)
                        MoveEffect(missleEffect.Position, i);
                }

                for (int i = Constants.NumFields - 1; i >= 0; i--) {
                    var field = _fields[i];
                    for (int j = field.ObjectsCount - 1; j >= 0; j--)
                        field.ObjectsNetwork[j].Animate(ticks);

                    if (field.EnvironmentalEffect) {
                        // TODO[priority=verylow]
                    }
                }

                for (int i = MessageBoxes.Count - 1; i >= 0; i--) {
                    var messageBox = MessageBoxes[i];
                    if (messageBox.ExpireMessages(ticks) > 0)
                        LayoutOnscreenMessages = true;

                    if (messageBox.Empty && i >= (int)MessageScreenTargets.BoxCoordinate) {
                        int nextIndex = i + 1;
                        while (nextIndex < MessageBoxes.Count) {
                            var otherMessageBox = MessageBoxes[nextIndex];
                            if (!otherMessageBox.Position.HasValue || otherMessageBox.Position == messageBox.Position) {
                                otherMessageBox.Visible = true;
                                break;
                            }

                            nextIndex++;
                        }

                        messageBox.RemoveMessages();
                        MessageBoxes.RemoveAt(i);
                        LayoutOnscreenMessages = true;
                    }
                }

                _objectNextUpdate = ticks + Constants.ObjectsUpdateInterval;
            }

            if (ticks >= _ambientNextUpdate) {
                if (AmbientCurrentColor.r < AmbientTargetColor.r)
                    AmbientCurrentColor.r++;
                else if (AmbientCurrentColor.r < AmbientTargetColor.r)
                    AmbientCurrentColor.r--;

                if (AmbientCurrentColor.g < AmbientTargetColor.g)
                    AmbientCurrentColor.g++;
                else if (AmbientCurrentColor.g < AmbientTargetColor.g)
                    AmbientCurrentColor.g--;

                if (AmbientCurrentColor.b < AmbientTargetColor.b)
                    AmbientCurrentColor.b++;
                else if (AmbientCurrentColor.b < AmbientTargetColor.b)
                    AmbientCurrentColor.b--;
                
                if (AmbientCurrentBrightness < AmbientTargetBrightness)
                    AmbientCurrentBrightness++;
                else if (AmbientCurrentBrightness > AmbientTargetBrightness)
                    AmbientCurrentBrightness--;
                
                _ambientNextUpdate = ticks + Constants.AmbientUpdateInterval;
            }
        }

        public int GetFieldHeight(int x, int y, int z) {
            int height = 0;
            var field = GetField(x, y, z);
            for (int i = field.ObjectsCount -1; i >= 0; i--) {
                var @object = field.ObjectsNetwork[i];
                if (@object.Type.Elevation != 0)
                    height++;
            }

            return height;
        }

        public EnterPossibleFlag GetEnterPossibleFlag(int x, int y, int z, bool param4) {
            var field = GetField(x, y, z);
            if (param4 && x < Constants.MapSizeX - 1 && y < Constants.MapSizeY - 1 && z > 0 && field.ObjectsCount > 0 && !field.ObjectsNetwork[0].Type.IsGround && GetFieldHeight(x + 1, y + 1, z - 1) > 2)
                return EnterPossibleFlag.Possible;

            if (param4 && GetFieldHeight(Constants.PlayerOffsetX, Constants.PlayerOffsetY, PlayerZPlane) > 2)
                return EnterPossibleFlag.Possible;

            Creatures.Creature creature = null;
            for (int i = 0; i < field.ObjectsCount; i++) {
                var @object = field.ObjectsNetwork[i];
                if (@object.IsCreature && creature == null) {
                    creature = OpenTibiaUnity.CreatureStorage.GetCreatureById(@object.Data);
                    if (!creature.Trapper && creature.Unpassable)
                        return creature.IsHuman && OpenTibiaUnity.GameManager.ClientVersion > 953
                            ? EnterPossibleFlag.PossibleNoAnimation : EnterPossibleFlag.NotPossible;
                } else {
                    if (@object.Type.IsUnpassable)
                        return EnterPossibleFlag.NotPossible;
                    else if (@object.Type.IsNoMoveAnimation)
                        return EnterPossibleFlag.PossibleNoAnimation;
                }
            }

            return EnterPossibleFlag.Possible;
        }

        public List<bool> GetLightBlockingTilesForZLayer(int z) {
            if (z < 0 || z >= Constants.MapSizeZ)
                throw new System.ArgumentException("WorldMapStorage.GetLightBlockingTilesForZLayer: Input Z co-oridnate (" + z + ") is out of range.");

            var list = _layerBrightnessInfos[z];
            for (int y = 0; y < Constants.MapSizeY; y++) {
                for (int x = 0; x < Constants.MapSizeX; x++) {
                    int fieldIndex = ((z + _origin.z) % Constants.MapSizeZ * Constants.MapSizeX
                        + (x + _origin.x) % Constants.MapSizeX) * Constants.MapSizeY
                        + (y + _origin.y) % Constants.MapSizeY;
                    int index = y * Constants.MapSizeX + x;

                    var field = _fields[fieldIndex];
                    var @object = field.GetObject(0);

                    if (@object == null || !@object.Type.IsGround)
                        list[index] = true;
                    else
                        list[index] = false;
                }
            }

            return list;
        }
    }
}
