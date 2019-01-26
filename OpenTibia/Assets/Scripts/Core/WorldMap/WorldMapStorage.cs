using System.Collections.Generic;
using System.Linq;

namespace OpenTibiaUnity.Core.WorldMap
{
    public class WorldMapStorage {
        List<int> m_CacheObjectsCount;
        int m_EffectsCount = 0;
        Field[] m_Fields = new Field[Constants.NumFields];
        Appearances.AppearanceInstance[] m_Effects = new Appearances.AppearanceInstance[Constants.NumEffects];

        List<bool>[] m_LayerBrightnessInfos = new List<bool>[Constants.MapSizeZ];
        UnityEngine.Vector3Int m_Position = UnityEngine.Vector3Int.zero;
        UnityEngine.Vector3Int m_Origin = UnityEngine.Vector3Int.zero;
        int m_AmbientNextUpdate = 0;
        int m_ObjectNextUpdate = 0;
        UnityEngine.Vector3Int m_HelperCoordinate = UnityEngine.Vector3Int.zero;
        TMPro.TextMeshProUGUI m_TextBoxPrefab;

        public int AmbientCurrentBrightness = -1;
        public int AmbientTargetBrightness = -1;
        public UnityEngine.Color32 AmbientCurrentColor = UnityEngine.Color.black;
        public UnityEngine.Color32 AmbientTargetColor = UnityEngine.Color.black;

        public UnityEngine.Vector3Int Position {
            get { return m_Position; }
            set {
                m_Position = value;
                PlayerZPlane = value.z <= Constants.GroundLayer ? (Constants.MapSizeZ - 1 - value.z) : Constants.UndergroundLayer;
            }
        }
        public bool Valid { get; set; } = false;
        public bool CacheFullbank { get; set; } = false;
        public int PlayerZPlane { get; private set; } = 0;
        public bool LayoutOnscreenMessages { get; set; } = false;
        public List<OnscreenMessageBox> MessageBoxes { get; } = new List<OnscreenMessageBox>();

        public WorldMapStorage(TMPro.TextMeshProUGUI textBoxBottom,
            TMPro.TextMeshProUGUI textBoxLow,
            TMPro.TextMeshProUGUI textBoxHigh,
            TMPro.TextMeshProUGUI textBoxTop,
            TMPro.TextMeshProUGUI textBoxPrefab) {

            for (int i = 0; i < m_Fields.Length; i++) {
                m_Fields[i] = new Field();
            }

            m_TextBoxPrefab = textBoxPrefab;

            //MessageScreenTarget.BoxBottom
            MessageBoxes.Add(new OnscreenMessageBox(null, null, 0, MessageModes.None, 1, textBoxBottom));
            //MessageScreenTarget.BoxLow
            MessageBoxes.Add(new OnscreenMessageBox(null, null, 0, MessageModes.None, 1, textBoxLow));
            //MessageScreenTarget.BoxHigh
            MessageBoxes.Add(new OnscreenMessageBox(null, null, 0, MessageModes.None, 1, textBoxHigh));
            //MessageScreenTarget.BoxTop
            MessageBoxes.Add(new OnscreenMessageBox(null, null, 0, MessageModes.None, 1, textBoxTop));

            m_CacheObjectsCount = new List<int>(Enumerable.Repeat(0, Constants.MapSizeZ));
            for (int i = 0; i < m_LayerBrightnessInfos.Length; i++) {
                m_LayerBrightnessInfos[i] = new List<bool>(Constants.MapSizeX * Constants.MapSizeY);
                for (int j = 0; j < m_LayerBrightnessInfos[i].Capacity; j++)
                    m_LayerBrightnessInfos[i].Add(false);
            }
            
        }

        public Appearances.ObjectInstance GetEnvironmentalEffect(UnityEngine.Vector3Int position) {
            return m_Fields[ToIndexInternal(position)].EnvironmentalEffect;
        }

        public void SetEnvironmentalEffect(UnityEngine.Vector3Int position, Appearances.ObjectInstance effectObject) {
            m_Fields[ToIndexInternal(position)].EnvironmentalEffect = effectObject;
        }

        // Object Operations
        private void AssertNullObject(Appearances.ObjectInstance obj, string functor) {
            if (!obj)
                throw new System.ArgumentNullException(functor + ": obj can't be null.");
        }
        public Appearances.ObjectInstance AppendObject(UnityEngine.Vector3Int position, Appearances.ObjectInstance obj) {
            AssertNullObject(obj, "WorldMapStorage.AppendObject");
            Appearances.ObjectInstance otherObj = m_Fields[ToIndexInternal(position)].PutObject(obj, Constants.MapSizeW);
            if (!!otherObj && otherObj.ID == Appearances.AppearanceInstance.Creature)
                OpenTibiaUnity.CreatureStorage.MarkOpponentVisible(otherObj.Data, false);
            else if (otherObj == null)
                m_CacheObjectsCount[(m_Origin.z + position.z) % Constants.MapSizeZ]++;
            
            if (!!obj.Type && obj.Type.IsFullBank)
                CacheFullbank = false;

            return otherObj;
        }
        public Appearances.ObjectInstance PutObject(UnityEngine.Vector3Int position, Appearances.ObjectInstance obj) {
            AssertNullObject(obj, "WorldMapStorage.PutObject");
            return InsertObject(position, -1, obj);
        }
        public Appearances.ObjectInstance InsertObject(UnityEngine.Vector3Int position, int stackPos, Appearances.ObjectInstance obj) {
            AssertNullObject(obj, "WorldMapStorage.InsertObject");
            Appearances.ObjectInstance otherObj = m_Fields[ToIndexInternal(position)].PutObject(obj, stackPos);
            if (!!otherObj && otherObj.ID == Appearances.AppearanceInstance.Creature)
                OpenTibiaUnity.CreatureStorage.MarkOpponentVisible(otherObj.Data, false);
            else if (!otherObj)
                m_CacheObjectsCount[(m_Origin.z + position.z) % Constants.MapSizeZ]++;

            if (!!otherObj && otherObj.Type.IsFullBank || obj.Type.IsFullBank)
                CacheFullbank = false;

            return otherObj;
        }
        public Appearances.ObjectInstance ChangeObject(UnityEngine.Vector3Int position, int stackPos, Appearances.ObjectInstance obj) {
            AssertNullObject(obj, "WorldMapStorage.ChangeObject");
            Appearances.ObjectInstance otherObj = m_Fields[ToIndexInternal(position)].ChangeObject(obj, stackPos);
            if (!!otherObj && otherObj.ID == Appearances.AppearanceInstance.Creature &&
                !!obj && obj.ID == Appearances.AppearanceInstance.Creature && obj.Data != otherObj.Data)
                OpenTibiaUnity.CreatureStorage.MarkOpponentVisible(otherObj.Data, false);

            if (!!otherObj && otherObj.Type.IsFullBank || obj.Type.IsFullBank)
                CacheFullbank = false;

            return otherObj;
        }
        public Appearances.ObjectInstance DeleteObject(UnityEngine.Vector3Int position, int stackPos) {
            Appearances.ObjectInstance otherObj = m_Fields[ToIndexInternal(position)].DeleteObject(stackPos);
            if (!!otherObj && otherObj.ID == Appearances.AppearanceInstance.Creature)
                OpenTibiaUnity.CreatureStorage.MarkOpponentVisible(otherObj.Data, false);

            if (!!otherObj)
                m_CacheObjectsCount[(m_Origin.z + position.z) % Constants.MapSizeZ]--;

            if (!!otherObj && otherObj.Type.IsFullBank)
                CacheFullbank = false;

            return otherObj;
        }

        public void AppendEffect(UnityEngine.Vector3Int position, Appearances.AppearanceInstance effect) {
            UnityEngine.Vector3Int? mapPosition = ToMapInternal(position);

            int index = -1;
            Field field = null;
            if (mapPosition != null) {
                index = ToIndexInternal(mapPosition.Value);
                field = m_Fields[index];
            }

            if (!!field && effect is Appearances.TextualEffectInstance) {
                Appearances.TextualEffectInstance textualEffect = (Appearances.TextualEffectInstance)effect;
                for (int i = field.EffectsCount - 1; i > 0; i--) {
                    var otherEffect = field.Effects[i];
                    if (otherEffect is Appearances.TextualEffectInstance && textualEffect.Merge(otherEffect))
                        return;
                }
            }

            if (m_EffectsCount < Constants.NumEffects) {
                effect.MapField = index;
                if (!!field)
                    field.AppendEffect(effect);
                else
                    effect.MapData = 0;

                m_Effects[m_EffectsCount] = effect;
                m_EffectsCount++;
            }
        }
        public void MoveEffect(UnityEngine.Vector3Int position, int effectIndex) {
            if (effectIndex < 0 || effectIndex >= m_EffectsCount)
                return;

            UnityEngine.Vector3Int? mapPosition = ToMapInternal(position);
            int index = -1;
            if (mapPosition != null)
                index = ToIndexInternal(mapPosition.Value);

            var effect = m_Effects[effectIndex];
            if (effect.MapField == index)
                return;

            if (effect.MapData > -1) {
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

            if (effectIndex < this.m_EffectsCount)
                m_Effects[effectIndex] = m_Effects[m_EffectsCount];
            
            m_Effects[m_EffectsCount] = null;
        }

        public int GetTopLookObject(UnityEngine.Vector3Int position, out Appearances.ObjectInstance obj) {
            return GetField(position).GetTopLookObject(out obj);
        }
        public int GetTopLookObject(UnityEngine.Vector3Int position) {
            Appearances.ObjectInstance obj;
            return GetField(position).GetTopLookObject(out obj);
        }
        public int GetTopMultiUseObject(UnityEngine.Vector3Int position, out Appearances.ObjectInstance obj) {
            return GetField(position).GetTopMultiUseObject(out obj);
        }
        public int GetTopMultiUseObject(UnityEngine.Vector3Int position) {
            Appearances.ObjectInstance obj;
            return GetField(position).GetTopMultiUseObject(out obj);
        }
        public int GetTopUseObject(UnityEngine.Vector3Int position, out Appearances.ObjectInstance obj) {
            return GetField(position).GetTopUseObject(out obj);
        }
        public int GetTopUseObject(UnityEngine.Vector3Int position) {
            Appearances.ObjectInstance obj;
            return GetField(position).GetTopUseObject(out obj);
        }
        public int GetTopMoveObject(UnityEngine.Vector3Int position, out Appearances.ObjectInstance obj) {
            return GetField(position).GetTopMoveObject(out obj);
        }
        public int GetTopMoveObject(UnityEngine.Vector3Int position) {
            Appearances.ObjectInstance obj;
            return GetField(position).GetTopMoveObject(out obj);
        }
        public int GetTopCreatureObject(UnityEngine.Vector3Int position, out Appearances.ObjectInstance obj) {
            return GetField(position).GetTopCreatureObject(out obj);
        }
        public int GetTopCreatureObject(UnityEngine.Vector3Int position) {
            Appearances.ObjectInstance obj;
            return GetField(position).GetTopCreatureObject(out obj);
        }
        
        public Appearances.ObjectInstance GetObject(UnityEngine.Vector3Int position, int stackPos) {
            return GetField(position).GetObject(stackPos);
        }
        public Appearances.ObjectInstance GetObject(int x, int y, int z, int stackPos) {
            return m_Fields[ToIndexInternal(x, y, z)].GetObject(stackPos);
        }
        public int GetObjectPerLayer(int z) {
            if (z < 0 || z > Constants.MapSizeZ)
                throw new System.Exception("WorldMapStorage.GetObjectPerLayer: z=" + z + " is out of range");

            return m_CacheObjectsCount[(m_Origin.z + z) % Constants.MapSizeZ];
        }
        public int GetCreatureObjectForCreature(Creatures.Creature creature, out Appearances.ObjectInstance obj) {
            if (!!creature) {
                var mapPosition = ToMap(creature.Position);
                return GetField(mapPosition).GetCreatureObjectForCreatureID(creature.ID, out obj);
            }

            obj = null;
            return -1;
        }
        public int GetCreatureObjectForCreature(Creatures.Creature creature) {
            Appearances.ObjectInstance _;
            return GetCreatureObjectForCreature(creature, out _);
        }

        public OnscreenMessageBox AddOnscreenMessage(MessageModes mode, string text) {
            return AddOnscreenMessage(null, -1, null, 0, mode, text, uint.MaxValue);
        }

        public OnscreenMessageBox AddOnscreenMessage(UnityEngine.Vector3Int? position, int statementID, string speaker, int speakerLevel, MessageModes mode, string text) {
            return AddOnscreenMessage(position, statementID, speaker, speakerLevel, mode, text, uint.MaxValue);
        }

        public OnscreenMessageBox AddOnscreenMessage(UnityEngine.Vector3Int? position, int statementID, string speaker, int speakerLevel, MessageModes mode, Utility.UnionStrInt text, uint color) {
            Chat.MessageFilterSet messageFilterSet = Chat.MessageFilterSet.DefaultFilterSet;

            Chat.MessageMode messageMode = messageFilterSet.GetMessageMode(mode);
            if (messageMode == null || !messageMode.ShowOnScreen || messageMode.ScreenTarget == MessageScreenTargets.None)
                return null;

            // TODO: name filter (speaker)

            var screenTarget = messageMode.ScreenTarget;
            if (screenTarget == MessageScreenTargets.EffectCoordinate) {
                // color effect on a position
                // Tip: Here you can modify this "is int" and "as int" to add colored text and so on.

                if (!position.HasValue)
                    throw new System.Exception("WorldMapStorage.AddOnscreenMessage: Missing co-ordinate.");
                
                if (text.IsInt) {
                    //uint value = (uint)((int)text);
                    //var textualEffect = OpenTibiaUnity.AppearanceStorage.CreateTextualEffect(-1, color, value);
                    //AppendEffect(position.Value, textualEffect);
                }

                return null;
            }

            OnscreenMessageBox messageBox = null;
            OnscreenMessage message = null;
            if (screenTarget == MessageScreenTargets.BoxCoordinate) {
                if (!position.HasValue)
                    throw new System.Exception("WorldMapStorage.AddOnscreenMessage: Missing co-ordinate.");

                bool visible = true;

                for (int i = (int)screenTarget; i < MessageBoxes.Count; i++) {
                    var tmpMessageBox = MessageBoxes[i];
                    if (tmpMessageBox.Position == null || tmpMessageBox.Position == position.Value) {
                        if (tmpMessageBox.Speaker == speaker && tmpMessageBox.Mode == mode) {
                            messageBox = tmpMessageBox;
                            break;
                        }

                        visible = false;
                    }
                }

                if (messageBox == null) {
                    var textBox = UnityEngine.Object.Instantiate(m_TextBoxPrefab, OpenTibiaUnity.GameManager.OnscreenMessagesContainer);
                    messageBox = new OnscreenMessageBox(position, speaker, speakerLevel, mode, Constants.NumOnscreenMessages, textBox);
                    messageBox.Visible = visible;

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

        public void ExpireOldestMessages() {
            int minExpirationTime = int.MaxValue;
            int minExpirationTimeIndex = -1;

            for (int i = MessageBoxes.Count - 1; i >= 0; i--) {
                int currentMinExpirationTime = MessageBoxes[i].MinExpirationTime;
                if (currentMinExpirationTime < minExpirationTime) {
                    minExpirationTime = currentMinExpirationTime;
                    minExpirationTimeIndex = i;
                }
            }

            if (minExpirationTimeIndex > -1) {
                var messageBox = MessageBoxes[minExpirationTimeIndex];
                int count = messageBox.ExpireOldestMessage();
                if (count > 0)
                    LayoutOnscreenMessages = true;

                if (messageBox.Empty) {
                    if (minExpirationTimeIndex >= (int)MessageScreenTargets.BoxCoordinate) {
                        messageBox.RemoveMessages();
                        messageBox.DestroyTextMesh(); // remove text mesh associated with it.
                        MessageBoxes.RemoveAt(minExpirationTimeIndex);
                        LayoutOnscreenMessages = true;
                    } else {
                        // bottom -> top exist all the time, we just set their visiblity to false
                        // to avoid unnessecary calculations
                        messageBox.Visible = false;
                    }
                }
            }
        }

        private int ToIndexInternal(UnityEngine.Vector3Int position) {
            return ToIndexInternal(position.x, position.y, position.z);
        }

        private int ToIndexInternal(int x, int y, int z) {
            if (x < 0 || x >= Constants.MapSizeX || y < 0 || y >= Constants.MapSizeY || z < 0 || z >= Constants.MapSizeZ)
                throw new System.ArgumentException($"WorldMapStorage.ToIndexInternal: Input co-oridnate ({x}, {y}, {z}) is out of range.");

            return ((z + m_Origin.z) % Constants.MapSizeZ * Constants.MapSizeX + (x + m_Origin.x) % Constants.MapSizeX) * Constants.MapSizeY + (y + m_Origin.y) % Constants.MapSizeY;
        }

        public Field GetField(UnityEngine.Vector3Int position) {
            return GetField(position.x, position.y, position.z);
        }

        public Field GetField(int x, int y, int z) {
            return m_Fields[ToIndexInternal(x, y, z)];
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
            m_Position = UnityEngine.Vector3Int.zero;
            PlayerZPlane = 0;
            m_Origin = UnityEngine.Vector3Int.zero;

            for (int i = 0; i < m_Fields.Length; i++) {
                m_Fields[i].Reset();
            }

            m_Effects = new Appearances.AppearanceInstance[Constants.NumEffects];
            m_EffectsCount = 0;

            m_CacheObjectsCount.Clear();
            m_CacheObjectsCount.AddRange(Enumerable.Repeat(0, Constants.MapSizeZ));

            CacheFullbank = false;
            Valid = false;
            m_AmbientNextUpdate = 0;
            m_ObjectNextUpdate = 0;
        }

        public void ResetField(UnityEngine.Vector3Int position, bool resetCreatures = true, bool resetEffects = true) {
            int index = ToIndexInternal(position);
            Field field = m_Fields[index];
            m_CacheObjectsCount[(m_Origin.z + position.z) % Constants.MapSizeZ] = 
                m_CacheObjectsCount[(m_Origin.z + position.z) % Constants.MapSizeZ] - field.ObjectsCount;
            CacheFullbank = false;

            if (resetCreatures) {
                var creatureStorage = OpenTibiaUnity.CreatureStorage;
                for (int i = field.ObjectsCount - 1; i > 0; i--) {
                    var obj = field.ObjectsNetwork[i];
                    if (obj.ID == Appearances.AppearanceInstance.Creature) {
                        creatureStorage.MarkOpponentVisible(obj, false);
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
            for (int z = 0; z < Constants.MapSizeZ; z++) {
                if (GetObjectPerLayer(z) > 0) {
                    m_HelperCoordinate.Set(0, 0, z);
                    ToAbsolute(m_HelperCoordinate, out m_HelperCoordinate);
                }

                for (int x = 0; x < Constants.MapSizeX; x++) {
                    for (int y = 0; y < Constants.MapSizeY; y++) {
                        GetField(x, y, z).UpdateBitmapCache(m_HelperCoordinate);
                    }
                }
            }
        }

        public void InvalidateOnscreenMessages() {
            LayoutOnscreenMessages = true;
        }

        public void UpdateMiniMap(UnityEngine.Vector3Int position) {
            m_Fields[ToIndexInternal(position)].UpdateMiniMap();
        }

        public uint GetMiniMapColour(UnityEngine.Vector3Int position) {
            return m_Fields[ToIndexInternal(position)].MiniMapColor;
        }

        public int GetMiniMapCost(UnityEngine.Vector3Int position) {
            return m_Fields[ToIndexInternal(position)].MiniMapCost;
        }

        public int GetMiniMapCost(int x, int y, int z) {
            return m_Fields[ToIndexInternal(x, y, z)].MiniMapCost;
        }

        public void ScrollMap(int x, int y, int z = 0) {
            if (x < -Constants.MapSizeX || x > Constants.MapSizeX)
                throw new System.ArgumentException("WorldMapStorage.ScrollMap: X=" + x + " is out of range.");
            else if (y < -Constants.MapSizeY || y > Constants.MapSizeY)
                throw new System.ArgumentException("WorldMapStorage.ScrollMap: Y=" + y + " is out of range.");
            else if (z < -Constants.MapSizeZ || z > Constants.MapSizeZ)
                throw new System.ArgumentException("WorldMapStorage.ScrollMap: Z=" + z + " is out of range.");
            else if(x * y + y * z + x * z != 0)
                throw new System.ArgumentException("WorldMapStorage.ScrollMap: Only one of the agruments  may be != 0.");

            if (x != 0) {
                int startx;
                int endx;
                if (x > 0) {
                    startx = Constants.MapSizeX - x;
                    endx = Constants.MapSizeX;
                } else {
                    startx = 0;
                    endx = -x;
                }

                for (int tmpx = startx; tmpx < endx; tmpx++) {
                    for (int tmpy = 0; tmpy < Constants.MapSizeY; tmpy++) {
                        for (int tmpz = 0; tmpz < Constants.MapSizeZ; tmpz++) {
                            ResetField(tmpx, tmpy, tmpz);
                        }
                    }
                }

                m_Origin.x -= x;
                if (m_Origin.x < 0) {
                    m_Origin.x += Constants.MapSizeX;
                }

                m_Origin.x %= Constants.MapSizeX;
            }

            if (y != 0) {
                int starty;
                int endy;
                if (y > 0) {
                    starty = Constants.MapSizeY - y;
                    endy = Constants.MapSizeY;
                } else {
                    starty = 0;
                    endy = -y;
                }

                for (int tmpx = 0; tmpx < Constants.MapSizeX; tmpx++) {
                    for (int tmpy = starty; tmpy < endy; tmpy++) {
                        for (int tmpz = 0; tmpz < Constants.MapSizeZ; tmpz++) {
                            ResetField(tmpx, tmpy, tmpz);
                        }
                    }
                }

                m_Origin.y -= y;
                if (m_Origin.y < 0) {
                    m_Origin.y += Constants.MapSizeY;
                }

                m_Origin.y %= Constants.MapSizeY;
            }

            if (z != 0) {
                int startz;
                int endz;
                if (y > 0) {
                    startz = Constants.MapSizeZ - z;
                    endz = Constants.MapSizeZ;
                } else {
                    startz = 0;
                    endz = -z;
                }

                for (int tmpx = 0; tmpx < Constants.MapSizeX; tmpx++) {
                    for (int tmpy = 0; tmpy < Constants.MapSizeY; tmpy++) {
                        for (int tmpz = startz; tmpz < endz; tmpz++) {
                            ResetField(tmpx, tmpy, tmpz);
                        }
                    }
                }

                m_Origin.z -= z;
                if (m_Origin.z < 0) {
                    m_Origin.z += Constants.MapSizeZ;
                }

                m_Origin.z %= Constants.MapSizeZ;
                if (z > 0) {
                    for (int tmpx = 0; tmpx < Constants.MapSizeX; tmpx++) {
                        for (int tmpy = 0; tmpy < Constants.MapSizeY; tmpy++) {
                            for (int tmpz = Constants.MapSizeZ - Constants.UndergroundLayer - 1; tmpz < Constants.MapSizeZ; tmpz++) {
                                ResetField(tmpx, tmpy, tmpz);
                            }
                        }
                    }
                }
            }
        }

        public UnityEngine.Vector3Int ToMap(UnityEngine.Vector3Int position) {
            UnityEngine.Vector3Int? mapPosition = ToMapInternal(position);
            if (mapPosition == null) {
                throw new System.ArgumentException("WorldMapStorage.ToMap: Input co-ordinate " + position + " is out of range (m=" + m_Position + ").");
            }

            return mapPosition.Value;
        }

        public void ToMap(UnityEngine.Vector3Int position, out UnityEngine.Vector3Int outPosition) {
            outPosition = ToMap(position);
        }

        public UnityEngine.Vector3Int ToMapClosest(UnityEngine.Vector3Int absolutePosition) {
            int dZ = m_Position.z - absolutePosition.z;
            int x = UnityEngine.Mathf.Max(0, UnityEngine.Mathf.Min(absolutePosition.x - (m_Position.x - Constants.PlayerOffsetX) - dZ, Constants.MapSizeX - 1));
            int y = UnityEngine.Mathf.Max(0, UnityEngine.Mathf.Min(absolutePosition.y - (m_Position.y - Constants.PlayerOffsetY) - dZ, Constants.MapSizeY - 1));
            int z = UnityEngine.Mathf.Max(0, UnityEngine.Mathf.Min(PlayerZPlane + dZ, Constants.MapSizeZ - 1));

            return new UnityEngine.Vector3Int(x, y, z);
        }

        public void ToMapClosest(UnityEngine.Vector3Int position, out UnityEngine.Vector3Int outPosition) {
            outPosition = ToMapClosest(position);
        }

        private UnityEngine.Vector3Int? ToMapInternal(UnityEngine.Vector3Int position) {
            return ToMapInternal(position.x, position.y, position.z);
        }

        private UnityEngine.Vector3Int? ToMapInternal(int x, int y, int z) {
            int dZ = m_Position.z - z;
            x = x - (m_Position.x - Constants.PlayerOffsetX) - dZ;
            y = y - (m_Position.y - Constants.PlayerOffsetY) - dZ;
            z = PlayerZPlane + dZ;

            if (x < 0 || x >= Constants.MapSizeX || y < 0 || y >= Constants.MapSizeY || z < 0 || z >= Constants.MapSizeZ)
                return null;

            return new UnityEngine.Vector3Int(x, y, z);
        }

        public UnityEngine.Vector3Int ToAbsolute(UnityEngine.Vector3Int position) {
            return ToAbsolute(position.x, position.y, position.z);
        }

        public UnityEngine.Vector3Int ToAbsolute(int x, int y, int z) {
            if (x < 0 || x >= Constants.MapSizeX || y < 0 || y >= Constants.MapSizeY || z < 0 || z >= Constants.MapSizeZ) {
                throw new System.ArgumentException($"WorldMapStorage.ToAbsolute: Input co-oridnate ({x}, {y}, {z}) is out of range.");
            }

            int diffz = z - PlayerZPlane;

            return new UnityEngine.Vector3Int() {
                x = x + (m_Position.x - Constants.PlayerOffsetX) + diffz,
                y = y + (m_Position.y - Constants.PlayerOffsetY) + diffz,
                z = m_Position.z - diffz
            };
        }

        public void ToAbsolute(UnityEngine.Vector3Int position, out UnityEngine.Vector3Int absolutePosition) {
            absolutePosition = ToAbsolute(position);
        }

        public void ToAbsolute(int x, int y, int z, out UnityEngine.Vector3Int absolutePosition) {
            absolutePosition = ToAbsolute(x, y, z);
        }

        public bool IsVisible(UnityEngine.Vector3Int position, bool param4) {
            UnityEngine.Vector3Int? mapPosition = ToMapInternal(position);
            return mapPosition.HasValue && (param4 || m_Position.z == position.z);
        }

        public bool IsVisible(int x, int y, int z, bool param4) {
            UnityEngine.Vector3Int? mapPosition = ToMapInternal(x, y, z);
            return mapPosition.HasValue && (param4 || m_Position.z == z);
        }

        public bool IsLookPossible(int x, int y, int z) {
            return !m_Fields[ToIndexInternal(x, y, z)].CacheUnsight;
        }

        public bool IsTranslucent(int x, int y, int z) {
            return m_Fields[ToIndexInternal(x, y, z)].CacheTranslucent;
        }

        public void ProtocolGameMessageProcessingFinished(bool miniMap = true) {
            RefreshFields();
            if (miniMap)
                OpenTibiaUnity.MiniMapStorage.RefreshSectors();
            OpenTibiaUnity.CreatureStorage.RefreshOpponents();
        }

        public void Animate() {
            var ticks = OpenTibiaUnity.TicksMillis;
            if (ticks < m_ObjectNextUpdate)
                return;

            for (int i = m_EffectsCount - 1; i >=0; i--) {
                var effect = m_Effects[i];
                if (!effect.Animate(ticks)) {
                    DeleteEffect(i);
                } else if (effect is Appearances.MissileInstance) {
                    var missle = effect as Appearances.MissileInstance;
                    MoveEffect(missle.Position, i);
                }
            }

            for (int i = Constants.NumFields - 1; i >=0; i--) {
                var field = m_Fields[i];
                for (int j = field.ObjectsCount - 1; j >= 0; j--) {
                    field.ObjectsNetwork[j].Animate(ticks);
                }

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
            
            m_ObjectNextUpdate = ticks + Constants.ObjectsUpdateInterval;
        }

        public int GetFieldHeight(int x, int y, int z) {
            int height = 0;
            var field = GetField(x, y, z);
            for (int i = field.ObjectsCount -1; i >= 0; i--) {
                var obj = field.ObjectsNetwork[i];
                if (obj.Type.Elevation != 0)
                    height++;
            }

            return height;
        }

        public uint GetEnterPossibleFlag(int x, int y, int z, bool param4) {
            var field = GetField(x, y, z);
            if (param4 && x < Constants.MapSizeX - 1 && y < Constants.MapSizeY - 1 && z > 0 && field.ObjectsCount > 0 && !field.ObjectsNetwork[0].Type.IsBank && GetFieldHeight(x + 1, y + 1, z - 1) > 2)
                return Constants.FieldEnterPossible;

            if (param4 && GetFieldHeight(Constants.PlayerOffsetX, Constants.PlayerOffsetY, PlayerZPlane) > 2)
                return Constants.FieldEnterPossible;

            Creatures.Creature creature = null;
            for (int i = 0; i < field.ObjectsCount; i++) {
                var obj = field.ObjectsNetwork[i];
                if (obj.ID == Appearances.AppearanceInstance.Creature && creature == null) {
                    creature = OpenTibiaUnity.CreatureStorage.GetCreature(obj.Data);
                    if (!creature.Trapper && creature.Unpassable)
                        return !!creature.IsHuman ? Constants.FieldEnterPossibleNoAnimation : Constants.FieldEnterPossible;
                } else {
                    if (obj.Type.IsUnpassable)
                        return Constants.FieldEnterNotPossible;
                    else if (obj.Type.IsNoMoveAnimation)
                        return Constants.FieldEnterPossibleNoAnimation;
                }
            }

            return Constants.FieldEnterPossible;
        }

        public List<bool> GetLightBlockingTilesForZLayer(int z) {
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
                    var obj = field.GetObject(0);

                    if (obj == null || !obj.Type.IsBank)
                        list[index] = true;
                    else
                        list[index] = false;
                }
            }

            return list;
        }
    }
}
