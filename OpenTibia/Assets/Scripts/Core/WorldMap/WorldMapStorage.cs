using System.Collections.Generic;
using System.Linq;

namespace OpenTibiaUnity.Core.WorldMap
{
    internal class WorldMapStorage {
        private List<int> m_CacheObjectsCount;
        private int m_EffectsCount = 0;
        private Field[] m_Fields = new Field[Constants.NumFields];
        private Appearances.AppearanceInstance[] m_Effects = new Appearances.AppearanceInstance[Constants.NumEffects];

        private List<bool>[] m_LayerBrightnessInfos = new List<bool>[Constants.MapSizeZ];
        private UnityEngine.Vector3Int m_Position = UnityEngine.Vector3Int.zero;
        private UnityEngine.Vector3Int m_Origin = UnityEngine.Vector3Int.zero;
        private int m_AmbientNextUpdate = 0;
        private int m_ObjectNextUpdate = 0;
        private UnityEngine.Vector3Int m_HelperCoordinate = UnityEngine.Vector3Int.zero;
        private readonly TMPro.TextMeshProUGUI m_TextBoxPrefab;

        internal int AmbientCurrentBrightness = -1;
        internal int AmbientTargetBrightness = -1;
        internal UnityEngine.Color32 AmbientCurrentColor = UnityEngine.Color.black;
        internal UnityEngine.Color32 AmbientTargetColor = UnityEngine.Color.black;

        internal UnityEngine.Vector3Int Position {
            get { return m_Position; }
            set {
                m_Position = value;
                PlayerZPlane = value.z <= Constants.GroundLayer ? (Constants.MapSizeZ - 1 - value.z) : Constants.UndergroundLayer;
            }
        }
        internal bool Valid { get; set; } = false;
        internal bool CacheFullbank { get; set; } = false;
        internal bool CacheUnsight { get; set; } = false;
        internal int PlayerZPlane { get; private set; } = 0;
        internal bool LayoutOnscreenMessages { get; set; } = false;
        internal List<OnscreenMessageBox> MessageBoxes { get; } = new List<OnscreenMessageBox>();

        internal WorldMapStorage(TMPro.TextMeshProUGUI textBoxBottom,
            TMPro.TextMeshProUGUI textBoxLow,
            TMPro.TextMeshProUGUI textBoxHigh,
            TMPro.TextMeshProUGUI textBoxTop,
            TMPro.TextMeshProUGUI textBoxPrefab) {

            for (int i = 0; i < m_Fields.Length; i++) {
                m_Fields[i] = new Field();
            }

            m_TextBoxPrefab = textBoxPrefab;

            //MessageScreenTarget.BoxBottom
            MessageBoxes.Add(new OnscreenMessageBox(null, null, 0, MessageModeType.None, 1, textBoxBottom));
            //MessageScreenTarget.BoxLow
            MessageBoxes.Add(new OnscreenMessageBox(null, null, 0, MessageModeType.None, 1, textBoxLow));
            //MessageScreenTarget.BoxHigh
            MessageBoxes.Add(new OnscreenMessageBox(null, null, 0, MessageModeType.None, 1, textBoxHigh));
            //MessageScreenTarget.BoxTop
            MessageBoxes.Add(new OnscreenMessageBox(null, null, 0, MessageModeType.None, 1, textBoxTop));

            m_CacheObjectsCount = new List<int>(Enumerable.Repeat(0, Constants.MapSizeZ));
            for (int i = 0; i < m_LayerBrightnessInfos.Length; i++) {
                m_LayerBrightnessInfos[i] = new List<bool>(Constants.MapSizeX * Constants.MapSizeY);
                for (int j = 0; j < m_LayerBrightnessInfos[i].Capacity; j++)
                    m_LayerBrightnessInfos[i].Add(false);
            }
            
        }

        internal Appearances.ObjectInstance GetEnvironmentalEffect(UnityEngine.Vector3Int mapPosition) {
            return GetField(mapPosition).EnvironmentalEffect;
        }

        internal void SetEnvironmentalEffect(UnityEngine.Vector3Int mapPosition, Appearances.ObjectInstance effectObject) {
            GetField(mapPosition).EnvironmentalEffect = effectObject;
        }

        // Object Operations
        private void AssertNullObject(Appearances.ObjectInstance @object, string functor) {
            if (!@object)
                throw new System.ArgumentNullException(string.Format("%s: %s", functor, "@object can't be null."));
        }
        internal Appearances.ObjectInstance AppendObject(UnityEngine.Vector3Int mapPosition, Appearances.ObjectInstance @object) {
            AssertNullObject(@object, "WorldMapStorage.AppendObject");
            var otherObj = GetField(mapPosition).PutObject(@object, Constants.MapSizeW);
            if (!!otherObj && otherObj.IsCreature)
                OpenTibiaUnity.CreatureStorage.MarkOpponentVisible(otherObj.Data, false);
            else if (otherObj == null)
                m_CacheObjectsCount[(m_Origin.z + mapPosition.z) % Constants.MapSizeZ]++;
            
            if (!!@object.Type && @object.Type.IsFullGround)
                CacheFullbank = false;

            return otherObj;
        }
        internal Appearances.ObjectInstance PutObject(UnityEngine.Vector3Int mapPosition, Appearances.ObjectInstance @object) {
            AssertNullObject(@object, "WorldMapStorage.PutObject");
            return InsertObject(mapPosition, -1, @object);
        }
        internal Appearances.ObjectInstance InsertObject(UnityEngine.Vector3Int mapPosition, int stackPos, Appearances.ObjectInstance @object) {
            AssertNullObject(@object, "WorldMapStorage.InsertObject");
            var otherObj = GetField(mapPosition).PutObject(@object, stackPos);
            if (!!otherObj && otherObj.IsCreature)
                OpenTibiaUnity.CreatureStorage.MarkOpponentVisible(otherObj.Data, false);
            else if (!otherObj)
                m_CacheObjectsCount[(m_Origin.z + mapPosition.z) % Constants.MapSizeZ]++;

            if (!!otherObj && otherObj.Type.IsFullGround || @object.Type.IsFullGround)
                CacheFullbank = false;

            return otherObj;
        }
        internal Appearances.ObjectInstance ChangeObject(UnityEngine.Vector3Int mapPosition, int stackPos, Appearances.ObjectInstance @object) {
            AssertNullObject(@object, "WorldMapStorage.ChangeObject");
            Appearances.ObjectInstance otherObj = GetField(mapPosition).ChangeObject(@object, stackPos);
            if (!!otherObj && otherObj.IsCreature &&
                !!@object && @object.IsCreature && @object.Data != otherObj.Data)
                OpenTibiaUnity.CreatureStorage.MarkOpponentVisible(otherObj.Data, false);

            if (!!otherObj && otherObj.Type.IsFullGround || @object.Type.IsFullGround)
                CacheFullbank = false;

            return otherObj;
        }
        internal Appearances.ObjectInstance DeleteObject(UnityEngine.Vector3Int mapPosition, int stackPos) {
            Appearances.ObjectInstance otherObj = GetField(mapPosition).DeleteObject(stackPos);
            if (!!otherObj && otherObj.IsCreature)
                OpenTibiaUnity.CreatureStorage.MarkOpponentVisible(otherObj.Data, false);

            if (!!otherObj)
                m_CacheObjectsCount[(m_Origin.z + mapPosition.z) % Constants.MapSizeZ]--;

            if (!!otherObj && otherObj.Type.IsFullGround)
                CacheFullbank = false;

            return otherObj;
        }

        internal void AppendEffect(UnityEngine.Vector3Int absolutePosition, Appearances.AppearanceInstance effect) {
            UnityEngine.Vector3Int? mapPosition = ToMapInternal(absolutePosition);

            int index = -1;
            Field field = null;
            if (mapPosition != null) {
                index = ToIndexInternal(mapPosition.Value);
                field = m_Fields[index];
            }

            if (!!field && effect is Appearances.TextualEffectInstance textualEffect) {
                for (int i = field.EffectsCount - 1; i > 0; i--) {
                    var otherEffect = field.Effects[i];
                    if (otherEffect is Appearances.TextualEffectInstance && textualEffect.Merge(otherEffect))
                        return;
                }
            }

            if (m_EffectsCount < Constants.NumEffects) {
                effect.MapField = index;
                effect.MapData = 0;
                if (!!field)
                    field.AppendEffect(effect);

                m_Effects[m_EffectsCount] = effect;
                m_EffectsCount++;
            }
        }
        internal void MoveEffect(UnityEngine.Vector3Int absolutePosition, int effectIndex) {
            if (effectIndex < 0 || effectIndex >= m_EffectsCount)
                return;

            UnityEngine.Vector3Int? mapPosition = ToMapInternal(absolutePosition);
            int index = -1;
            if (mapPosition != null)
                index = ToIndexInternal(mapPosition.Value);

            var effect = m_Effects[effectIndex];
            if (effect.MapField == index)
                return;

            if (effect.MapField > -1) {
                m_Fields[effect.MapField].DeleteEffect(effect.MapData);
                effect.MapField = -1;
                effect.MapData = 0;
            }

            if (index > -1) {
                effect.MapField = index;
                effect.MapData = 0;
                m_Fields[index].AppendEffect(effect);
            }
        }
        private void DeleteEffect(int effectIndex) {
            if (effectIndex < 0 || effectIndex >= m_EffectsCount)
                throw new System.Exception("WorldMapStorage.DeleteEffect: effect is out of range.");

            var effect = m_Effects[effectIndex];
            if (effect.MapField != -1)
                m_Fields[effect.MapField].DeleteEffect(effect.MapData);

            effect.MapData = -1;
            effect.MapField = -1;
            m_EffectsCount--;

            if (effectIndex < m_EffectsCount)
                m_Effects[effectIndex] = m_Effects[m_EffectsCount];
            
            m_Effects[m_EffectsCount] = null;
        }

        internal int GetTopLookObject(UnityEngine.Vector3Int mapPosition, out Appearances.ObjectInstance @object) {
            return GetField(mapPosition).GetTopLookObject(out @object);
        }
        internal int GetTopLookObject(UnityEngine.Vector3Int mapPosition) {
            return GetField(mapPosition).GetTopLookObject(out Appearances.ObjectInstance @object);
        }
        internal int GetTopMultiUseObject(UnityEngine.Vector3Int mapPosition, out Appearances.ObjectInstance @object) {
            return GetField(mapPosition).GetTopMultiUseObject(out @object);
        }
        internal int GetTopMultiUseObject(UnityEngine.Vector3Int mapPosition) {
            return GetField(mapPosition).GetTopMultiUseObject(out Appearances.ObjectInstance @object);
        }
        internal int GetTopUseObject(UnityEngine.Vector3Int mapPosition, out Appearances.ObjectInstance @object) {
            return GetField(mapPosition).GetTopUseObject(out @object);
        }
        internal int GetTopUseObject(UnityEngine.Vector3Int mapPosition) {
            return GetField(mapPosition).GetTopUseObject(out Appearances.ObjectInstance @object);
        }
        internal int GetTopMoveObject(UnityEngine.Vector3Int mapPosition, out Appearances.ObjectInstance @object) {
            return GetField(mapPosition).GetTopMoveObject(out @object);
        }
        internal int GetTopMoveObject(UnityEngine.Vector3Int mapPosition) {
            return GetField(mapPosition).GetTopMoveObject(out Appearances.ObjectInstance @object);
        }
        internal int GetTopCreatureObject(UnityEngine.Vector3Int mapPosition, out Appearances.ObjectInstance @object) {
            return GetField(mapPosition).GetTopCreatureObject(out @object);
        }
        internal int GetTopCreatureObject(UnityEngine.Vector3Int mapPosition) {
            return GetField(mapPosition).GetTopCreatureObject(out Appearances.ObjectInstance @object);
        }
        
        internal Appearances.ObjectInstance GetObject(UnityEngine.Vector3Int mapPosition, int stackPos) {
            return GetField(mapPosition).GetObject(stackPos);
        }
        internal Appearances.ObjectInstance GetObject(int mapX, int mapY, int mapZ, int stackPos) {
            return m_Fields[ToIndexInternal(mapX, mapY, mapZ)].GetObject(stackPos);
        }
        internal int GetObjectPerLayer(int z) {
            if (z < 0 || z > Constants.MapSizeZ)
                throw new System.Exception("WorldMapStorage.GetObjectPerLayer: z=" + z + " is out of range");

            return m_CacheObjectsCount[(m_Origin.z + z) % Constants.MapSizeZ];
        }
        internal int GetCreatureObjectForCreature(Creatures.Creature creature, out Appearances.ObjectInstance @object) {
            if (!!creature) {
                var mapPosition = ToMap(creature.Position);
                return GetField(mapPosition).GetCreatureObjectForCreatureID(creature.ID, out @object);
            }

            @object = null;
            return -1;
        }
        internal int GetCreatureObjectForCreature(Creatures.Creature creature) {
            Appearances.ObjectInstance _;
            return GetCreatureObjectForCreature(creature, out _);
        }

        internal OnscreenMessageBox AddOnscreenMessage(MessageModeType mode, string text) {
            return AddOnscreenMessage(null, -1, null, 0, mode, text, int.MaxValue);
        }

        internal OnscreenMessageBox AddOnscreenMessage(UnityEngine.Vector3Int? absolutePosition, int statementID, string speaker, int speakerLevel, MessageModeType mode, string text) {
            return AddOnscreenMessage(absolutePosition, statementID, speaker, speakerLevel, mode, text, int.MaxValue);
        }

        internal OnscreenMessageBox AddOnscreenMessage(UnityEngine.Vector3Int? absolutePosition, int statementID, string speaker, int speakerLevel, MessageModeType mode, Utility.UnionStrInt text, int color) {
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
                    var textMesh = UnityEngine.Object.Instantiate(m_TextBoxPrefab, OpenTibiaUnity.GameManager.OnscreenMessagesContainer);
                    messageBox = new OnscreenMessageBox(absolutePosition, speaker, speakerLevel, mode, Constants.NumOnscreenMessages, textMesh) {
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

            message = new OnscreenMessage(statementID, speaker, speakerLevel, mode, text);
            message.FormatMessage(messageMode.GetOnscreenMessagePrefix(speaker, speakerLevel), messageMode.TextARGB, messageMode.HighlightARGB);
            messageBox.AppendMessage(message);
            messageBox.Visible = true;
            InvalidateOnscreenMessages();
            return messageBox;
        }

        internal void AddTextualEffect(UnityEngine.Vector3Int absolutePosition, int color, string text, bool mergable = true) {
            var textMesh = UnityEngine.Object.Instantiate(m_TextBoxPrefab, OpenTibiaUnity.GameManager.OnscreenMessagesContainer);
            var textualEffect = OpenTibiaUnity.AppearanceStorage.CreateTextualEffect(color, text, textMesh);
            AppendEffect(absolutePosition, textualEffect);
        }

        internal void ExpireOldestMessages() {
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
                        messageBox.DestroyTextMesh(); // remove text mesh associated with it.
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

            return ((mapZ + m_Origin.z) % Constants.MapSizeZ * Constants.MapSizeX + (mapX + m_Origin.x) % Constants.MapSizeX) * Constants.MapSizeY + (mapY + m_Origin.y) % Constants.MapSizeY;
        }

        internal Field GetField(UnityEngine.Vector3Int mapPosition) {
            return GetField(mapPosition.x, mapPosition.y, mapPosition.z);
        }

        internal Field GetField(int mapX, int mapY, int mapZ) {
            return m_Fields[ToIndexInternal(mapX, mapY, mapZ)];
        }

        internal void SetAmbientLight(UnityEngine.Color color, int intensity) {
            var mustUpdateAmbient = AmbientTargetBrightness < 0;
            AmbientTargetColor = color;
            AmbientTargetBrightness = intensity;
            if (mustUpdateAmbient) {
                AmbientCurrentColor = AmbientTargetColor;
                AmbientCurrentBrightness = AmbientTargetBrightness;
            }
        }

        internal void Reset() {
            ResetMap();
            InvalidateOnscreenMessages();
        }

        internal void ResetMap() {
            m_Position = UnityEngine.Vector3Int.zero;
            PlayerZPlane = 0;
            m_Origin = UnityEngine.Vector3Int.zero;

            for (int i = 0; i < m_Fields.Length; i++)
                m_Fields[i].Reset();

            m_Effects = new Appearances.AppearanceInstance[Constants.NumEffects];
            m_EffectsCount = 0;

            m_CacheObjectsCount.Clear();
            m_CacheObjectsCount.AddRange(Enumerable.Repeat(0, Constants.MapSizeZ));

            CacheFullbank = false;
            Valid = false;
            m_AmbientNextUpdate = 0;
            m_ObjectNextUpdate = 0;
        }

        internal void ResetField(UnityEngine.Vector3Int mapPosition, bool resetCreatures = true, bool resetEffects = true) {
            int index = ToIndexInternal(mapPosition);
            Field field = m_Fields[index];
            m_CacheObjectsCount[(m_Origin.z + mapPosition.z) % Constants.MapSizeZ] = 
                m_CacheObjectsCount[(m_Origin.z + mapPosition.z) % Constants.MapSizeZ] - field.ObjectsCount;
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

        internal void ResetField(int x, int y, int z, bool resetCreatures = true, bool resetEffects = true) {
            ResetField(new UnityEngine.Vector3Int(x, y, z), resetCreatures, resetEffects);
        }

        internal void RefreshFields() {
            for (int z = 0; z < Constants.MapSizeZ; z++) {
                for (int x = 0; x < Constants.MapSizeX; x++) {
                    for (int y = 0; y < Constants.MapSizeY; y++) {
                        GetField(x, y, z).UpdateObjectsCache();
                    }
                }
            }
        }

        internal void InvalidateOnscreenMessages() {
            LayoutOnscreenMessages = true;
        }

        internal void UpdateMiniMap(UnityEngine.Vector3Int mapPosition) {
            GetField(mapPosition).UpdateMiniMap();
        }

        internal uint GetMiniMapColour(UnityEngine.Vector3Int mapPosition) {
            return GetField(mapPosition).MiniMapColor;
        }

        internal int GetMiniMapCost(UnityEngine.Vector3Int mapPosition) {
            return GetField(mapPosition).MiniMapCost;
        }

        internal int GetMiniMapCost(int mapX, int mapY, int mapZ) {
            return GetField(mapX, mapY, mapZ).MiniMapCost;
        }

        internal void ScrollMap(int dx, int dy, int dz = 0) {
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

                m_Origin.x -= dx;
                if (m_Origin.x < 0)
                    m_Origin.x += Constants.MapSizeX;

                m_Origin.x %= Constants.MapSizeX;
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

                m_Origin.y -= dy;
                if (m_Origin.y < 0)
                    m_Origin.y += Constants.MapSizeY;

                m_Origin.y %= Constants.MapSizeY;
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

                m_Origin.z -= dz;
                if (m_Origin.z < 0)
                    m_Origin.z += Constants.MapSizeZ;

                m_Origin.z %= Constants.MapSizeZ;
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

        internal UnityEngine.Vector3Int ToMap(UnityEngine.Vector3Int absolutePosition) {
            UnityEngine.Vector3Int? mapPosition = ToMapInternal(absolutePosition);
            if (mapPosition == null)
                throw new System.ArgumentException("WorldMapStorage.ToMap: Input co-ordinate " + absolutePosition + " is out of range (m=" + m_Position + ").");

            return mapPosition.Value;
        }

        internal void ToMap(UnityEngine.Vector3Int absolutePosition, out UnityEngine.Vector3Int outPosition) {
            outPosition = ToMap(absolutePosition);
        }

        internal UnityEngine.Vector3Int ToMapClosest(UnityEngine.Vector3Int absolutePosition) {
            int dZ = m_Position.z - absolutePosition.z;
            int x = UnityEngine.Mathf.Max(0, UnityEngine.Mathf.Min(absolutePosition.x - (m_Position.x - Constants.PlayerOffsetX) - dZ, Constants.MapSizeX - 1));
            int y = UnityEngine.Mathf.Max(0, UnityEngine.Mathf.Min(absolutePosition.y - (m_Position.y - Constants.PlayerOffsetY) - dZ, Constants.MapSizeY - 1));
            int z = UnityEngine.Mathf.Max(0, UnityEngine.Mathf.Min(PlayerZPlane + dZ, Constants.MapSizeZ - 1));

            return new UnityEngine.Vector3Int(x, y, z);
        }

        internal void ToMapClosest(UnityEngine.Vector3Int absolutePosition, out UnityEngine.Vector3Int outPosition) {
            outPosition = ToMapClosest(absolutePosition);
        }

        private UnityEngine.Vector3Int? ToMapInternal(UnityEngine.Vector3Int absolutePosition) {
            return ToMapInternal(absolutePosition.x, absolutePosition.y, absolutePosition.z);
        }

        private UnityEngine.Vector3Int? ToMapInternal(int absoluteX, int absoluteY, int absoluteZ) {
            int dZ = m_Position.z - absoluteZ;
            absoluteX -= (m_Position.x - Constants.PlayerOffsetX) + dZ;
            absoluteY -= (m_Position.y - Constants.PlayerOffsetY) + dZ;
            absoluteZ = PlayerZPlane + dZ;

            if (absoluteX < 0 || absoluteX >= Constants.MapSizeX || absoluteY < 0 || absoluteY >= Constants.MapSizeY || absoluteZ < 0 || absoluteZ >= Constants.MapSizeZ)
                return null;

            return new UnityEngine.Vector3Int(absoluteX, absoluteY, absoluteZ);
        }

        internal UnityEngine.Vector3Int ToAbsolute(UnityEngine.Vector3Int mapPosition) {
            return ToAbsolute(mapPosition.x, mapPosition.y, mapPosition.z);
        }

        internal UnityEngine.Vector3Int ToAbsolute(int mapX, int mapY, int mapZ) {
            if (mapX < 0 || mapX >= Constants.MapSizeX || mapY < 0 || mapY >= Constants.MapSizeY || mapZ < 0 || mapZ >= Constants.MapSizeZ)
                throw new System.ArgumentException($"WorldMapStorage.ToAbsolute: Input co-oridnate ({mapX}, {mapY}, {mapZ}) is out of range.");

            int diffz = mapZ - PlayerZPlane;

            return new UnityEngine.Vector3Int() {
                x = mapX + (m_Position.x - Constants.PlayerOffsetX) + diffz,
                y = mapY + (m_Position.y - Constants.PlayerOffsetY) + diffz,
                z = m_Position.z - diffz
            };
        }

        internal void ToAbsolute(UnityEngine.Vector3Int mapPosition, out UnityEngine.Vector3Int absolutePosition) {
            absolutePosition = ToAbsolute(mapPosition);
        }

        internal void ToAbsolute(int x, int y, int z, out UnityEngine.Vector3Int absolutePosition) {
            absolutePosition = ToAbsolute(x, y, z);
        }

        internal bool IsVisible(UnityEngine.Vector3Int absolutePosition, bool param4) {
            UnityEngine.Vector3Int? mapPosition = ToMapInternal(absolutePosition);
            return mapPosition.HasValue && (param4 || m_Position.z == absolutePosition.z);
        }

        internal bool IsVisible(int x, int y, int z, bool param4) {
            UnityEngine.Vector3Int? mapPosition = ToMapInternal(x, y, z);
            return mapPosition.HasValue && (param4 || m_Position.z == z);
        }

        internal bool IsLookPossible(int x, int y, int z) {
            return !m_Fields[ToIndexInternal(x, y, z)].CacheUnsight;
        }

        internal bool IsTranslucent(int x, int y, int z) {
            return m_Fields[ToIndexInternal(x, y, z)].CacheTranslucent;
        }
        
        internal void Animate() {
            var ticks = OpenTibiaUnity.TicksMillis;
            if (ticks >= m_ObjectNextUpdate) {
                for (int i = m_EffectsCount - 1; i >= 0; i--) {
                    var effect = m_Effects[i];
                    if (!effect.Animate(ticks)) {
                        if (effect is Appearances.TextualEffectInstance textualEffect)
                            textualEffect.DestroyTextMesh(); // make sure to destroy resources associated with this effect :)
                        DeleteEffect(i);
                    } else if (effect is Appearances.MissileInstance missleEffect) {
                        MoveEffect(missleEffect.Position, i);
                    }
                }

                for (int i = Constants.NumFields - 1; i >= 0; i--) {
                    var field = m_Fields[i];
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
                        messageBox.DestroyTextMesh();
                        MessageBoxes.RemoveAt(i);
                        LayoutOnscreenMessages = true;
                    }
                }

                m_ObjectNextUpdate = ticks + Constants.ObjectsUpdateInterval;
            }

            if (ticks >= m_AmbientNextUpdate) {
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
                
                m_AmbientNextUpdate = ticks + Constants.AmbientUpdateInterval;
            }
        }

        internal int GetFieldHeight(int x, int y, int z) {
            int height = 0;
            var field = GetField(x, y, z);
            for (int i = field.ObjectsCount -1; i >= 0; i--) {
                var @object = field.ObjectsNetwork[i];
                if (@object.Type.Elevation != 0)
                    height++;
            }

            return height;
        }

        internal EnterPossibleFlag GetEnterPossibleFlag(int x, int y, int z, bool param4) {
            var field = GetField(x, y, z);
            if (param4 && x < Constants.MapSizeX - 1 && y < Constants.MapSizeY - 1 && z > 0 && field.ObjectsCount > 0 && !field.ObjectsNetwork[0].Type.IsGround && GetFieldHeight(x + 1, y + 1, z - 1) > 2)
                return EnterPossibleFlag.Possible;

            if (param4 && GetFieldHeight(Constants.PlayerOffsetX, Constants.PlayerOffsetY, PlayerZPlane) > 2)
                return EnterPossibleFlag.Possible;

            Creatures.Creature creature = null;
            for (int i = 0; i < field.ObjectsCount; i++) {
                var @object = field.ObjectsNetwork[i];
                if (@object.IsCreature && creature == null) {
                    creature = OpenTibiaUnity.CreatureStorage.GetCreature(@object.Data);
                    if (!creature.Trapper && creature.Unpassable)
                        return creature.IsHuman && OpenTibiaUnity.GameManager.ClientVersion > 854
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

        internal List<bool> GetLightBlockingTilesForZLayer(int z) {
            if (z < 0 || z >= Constants.MapSizeZ)
                throw new System.ArgumentException("WorldMapStorage.GetLightBlockingTilesForZLayer: Input Z co-oridnate (" + z + ") is out of range.");

            var list = m_LayerBrightnessInfos[z];
            for (int y = 0; y < Constants.MapSizeY; y++) {
                for (int x = 0; x < Constants.MapSizeX; x++) {
                    int fieldIndex = ((z + m_Origin.z) % Constants.MapSizeZ * Constants.MapSizeX
                        + (x + m_Origin.x) % Constants.MapSizeX) * Constants.MapSizeY
                        + (y + m_Origin.y) % Constants.MapSizeY;

                    int index = y * Constants.MapSizeX + x;

                    var field = m_Fields[fieldIndex];
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
