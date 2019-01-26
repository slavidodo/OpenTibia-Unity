using System.Collections.Generic;

namespace OpenTibiaUnity.Core.WorldMap
{
    public class Field
    {
        private static int s_CacheCount = 0;
        private UnityEngine.Rect m_CacheRectangle;

        public List<Appearances.AppearanceInstance> Effects { get; } = new List<Appearances.AppearanceInstance>();
        public Appearances.ObjectInstance[] ObjectsRenderer { get; } = new Appearances.ObjectInstance[Constants.MapSizeW];
        public Appearances.ObjectInstance[] ObjectsNetwork { get; } = new Appearances.ObjectInstance[Constants.MapSizeW];
        public bool CacheLyingObject { get; private set; } = false;
        public bool CacheTranslucent { get; private set; } = false;
        public bool CacheObjectsDirty { get; private set; } = false;
        public bool CacheUnsight { get; private set; } = false;
        public bool MiniMapDirty { get; private set; } = false;
        public bool CacheBitmapDirty { get; private set; } = false;

        public uint MiniMapColor { get; private set; } = 0;
        public int EffectsCount { get; private set; } = 0;
        public int ObjectsCount { get; private set; } = 0;
        public int CacheObjectsCount { get; private set; } = 0;
        public int MiniMapCost { get; private set; } = int.MaxValue;

        public Appearances.ObjectInstance EnvironmentalEffect { get; set; } = null;

        public Field() {
            Field.AllocateCache(this);
        }

        public static void AllocateCache(Field field) {
            if (Field.s_CacheCount > Constants.NumFields) {
                throw new System.Exception("Field.AllocateCache: Allocation limit exceeded (max=" + Field.s_CacheCount + ").");
            }

            int rX = Field.s_CacheCount % Constants.MapSizeX;
            int rY = Field.s_CacheCount / Constants.MapSizeX;
            
            field.m_CacheRectangle = new UnityEngine.Rect(rX * Constants.FieldCacheSize,
                rY * Constants.FieldCacheSize, Constants.FieldCacheSize, Constants.FieldCacheSize);
            Field.s_CacheCount++;
        }

        public int GetObjectPriority(Appearances.ObjectInstance objectInstance) {
            Appearances.AppearanceType appearanceType = objectInstance.Type;
            if (appearanceType.IsGround) {
                return 0;
            } else if (appearanceType.IsGroundBorder) {
                return 1;
            } else if (appearanceType.IsBottom) {
                return 2;
            } else if (appearanceType.IsTop) {
                return 3;
            } else if (objectInstance.ID == Appearances.AppearanceInstance.Creature) {
                return 4;
            } else {
                return 5;
            }
        }

        public Appearances.AppearanceInstance GetEffect(int stackPos) {
            return Effects[stackPos];
        }
        public void AppendEffect(Appearances.AppearanceInstance effect) {
            if (!effect || !effect.Type)
                throw new System.ArgumentNullException("Field.AppendEffect: Invalid effect.");

            if (effect.Type.IsTopEffect) {
                Effects.Insert(0, effect);
                for (int i = 0; i < Effects.Count; i++) {
                    Effects[i].MapData = i;
                }
            } else {
                Effects.Add(effect);
                effect.MapData = EffectsCount;
            }

            EffectsCount++;
        }
        public void DeleteEffect(int effectIndex) {
            if (effectIndex < 0 || effectIndex >= EffectsCount) // TODO: this has been troublesome for a while, find why..
                return; //throw new System.ArgumentException("Field.DeleteEffect: index " + effectIndex + " is out of range");
            
            Effects[effectIndex].MapData = -1;
            Effects.RemoveAt(effectIndex);
            EffectsCount--;

            for (; effectIndex < EffectsCount; effectIndex++) {
                Effects[effectIndex].MapData = effectIndex;
            }
        }

        public Appearances.ObjectInstance PutObject(Appearances.ObjectInstance objectInstance, int stackPos) {
            if (!objectInstance)
                return null;
            
            if (stackPos < 0) {
                stackPos = 0;
                int priority = GetObjectPriority(objectInstance);
                while (stackPos < ObjectsCount) {
                    int tmpPriority = GetObjectPriority(ObjectsNetwork[stackPos]);
                    if (tmpPriority > priority || (tmpPriority == priority && priority == 5))
                        break;
                    stackPos++;
                }

                if (stackPos >= Constants.MapSizeW)
                    return null;
            } else if (stackPos <= ObjectsCount || stackPos == Constants.MapSizeW) {
                stackPos = System.Math.Min(System.Math.Min(stackPos, ObjectsCount), Constants.MapSizeW - 1);
            } else {
                return null;
            }

            Appearances.ObjectInstance otherObject = null;
            if (ObjectsCount >= Constants.MapSizeW) {
                ObjectsCount = Constants.MapSizeW;
                otherObject = ObjectsNetwork[Constants.MapSizeW - 1];
            } else {
                ObjectsCount++;
            }

            int newIndex = ObjectsCount - 1;
            while (newIndex > stackPos) {
                ObjectsNetwork[newIndex] = ObjectsNetwork[newIndex - 1];
                newIndex--;
            }

            objectInstance.MapData = stackPos;
            ObjectsNetwork[stackPos] = objectInstance;
            CacheObjectsDirty = true;
            CacheBitmapDirty = true;
            MiniMapDirty = true;
            return otherObject;
        }
        public Appearances.ObjectInstance ChangeObject(Appearances.ObjectInstance objectInstance, int stackPos) {
            if (!objectInstance || stackPos < 0 || stackPos >= ObjectsCount) {
                return null;
            }

            Appearances.ObjectInstance myObject = ObjectsNetwork[stackPos];
            ObjectsNetwork[stackPos] = objectInstance;
            CacheObjectsDirty = true;
            CacheBitmapDirty = true;
            MiniMapDirty = true;
            return myObject;
        }

        public void UpdateBitmapCache(UnityEngine.Vector3Int position) {
            UpdateObjectsCache();
            CacheObjectsCount = 0;

            int topLookIndex = GetTopLookObject();
            while (CacheObjectsCount < topLookIndex) {
                var obj = ObjectsRenderer[CacheObjectsCount];
                var type = obj.Type;

                if (!type.IsCachable) { // TODO: directly calculate those
                    break;
                }
                
                CacheLyingObject = CacheLyingObject || type.IsLyingObject;
                CacheObjectsCount++;
            }
        }
        
        public Appearances.ObjectInstance GetObject(int stackPos) {
            if (stackPos < 0 || stackPos >= ObjectsCount || (stackPos == 0 && ObjectsCount == 0))
                return null;
            return ObjectsNetwork[stackPos];
        }
        public int GetTopLookObject(out Appearances.ObjectInstance topLookObj) {
            topLookObj = null;
            int index = -1;

            for (int i = 0; i < ObjectsCount; i++) {
                var obj = ObjectsNetwork[i];
                if (topLookObj == null || obj.Type.IsIgnoreLook) {
                    index = i;
                    topLookObj = obj;

                    if (!obj.Type.IsBank && !obj.Type.IsClip && !obj.Type.IsBottom && !obj.Type.IsTop)
                        break;
                }
            }

            return index;
        }
        public int GetTopLookObject() {
            Appearances.ObjectInstance _;
            return GetTopLookObject(out _);
        }
        public int GetTopMultiUseObject(out Appearances.ObjectInstance topMultiUseObj) {
            if (ObjectsCount > 0) {
                int index = 0;
                Appearances.ObjectInstance obj;
                for (; index < ObjectsCount; index++) {
                    obj = ObjectsNetwork[index];
                    if (obj.Type.IsForceUse)
                        break;

                    if (!obj.Type.IsBank && !obj.Type.IsClip && !obj.Type.IsBottom && !obj.Type.IsTop)
                        break;
                }

                if (index > 0 && !(obj = ObjectsNetwork[index]).Type.IsForceUse && obj.Type.IsLiquidPool)
                    index--;

                topMultiUseObj = ObjectsNetwork[index];
                return index;
            }

            topMultiUseObj = null;
            return -1;
        }
        public int GetTopMultiUseObject() {
            Appearances.ObjectInstance _;
            return GetTopMultiUseObject(out _);
        }
        public int GetTopUseObject(out Appearances.ObjectInstance obj) {
            obj = null;
            int index = -1;

            for (int i = 0; i < ObjectsCount; i++) {
                obj = ObjectsNetwork[i];
                if (obj.Type.IsForceUse)
                    break;

                if (!obj.Type.IsBank && !obj.Type.IsClip && !obj.Type.IsBottom && !obj.Type.IsTop && obj.ID != Appearances.AppearanceInstance.Creature)
                    break;
            }

            while (index > 0 && ((obj = ObjectsNetwork[index]).ID == Appearances.AppearanceInstance.Creature || obj.Type.IsLiquidPool)) {
                index--;
            }

            if (index > 0)
                obj = ObjectsNetwork[index];
            else
                obj = null;

            return index;
        }
        public int GetTopUseObject() {
            Appearances.ObjectInstance _;
            return GetTopUseObject(out _);
        }
        public int GetTopMoveObject(out Appearances.ObjectInstance topMoveObj) {
            if (ObjectsCount > 0) {
                int index = 0;
                while (index < ObjectsCount) {
                    var obj = ObjectsNetwork[index];
                    if (!obj.Type.IsBank && !obj.Type.IsClip && !obj.Type.IsBottom && !obj.Type.IsTop && obj.Type.ID != Appearances.AppearanceInstance.Creature)
                        break;
                    index++;
                }

                if (index > 0 && ObjectsNetwork[index].Type.IsUnmoveable)
                    index--;

                topMoveObj = ObjectsNetwork[index];
                return index;
            }

            topMoveObj = null;
            return -1;
        }
        public int GetTopMoveObject() {
            Appearances.ObjectInstance _;
            return GetTopMoveObject(out _);
        }
        public int GetTopCreatureObject(out Appearances.ObjectInstance topCreatureObj) {
            int topLookIndex = GetTopLookObject(out topCreatureObj);
            if (!!topCreatureObj) {
                if (topCreatureObj.IsCreature) {
                    return topLookIndex;
                }

                topCreatureObj = null;
                return -1;
            }
            return -1;
        }
        public int GetTopCreatureObject() {
            Appearances.ObjectInstance _ = null;
            return GetTopCreatureObject(out _);
        }
        public int GetCreatureObjectForCreatureID(uint creatureID, out Appearances.ObjectInstance obj) {
            if (creatureID == 0 || ObjectsCount == 0) {
                obj = null;
                return -1;
            }

            for (int i = 0; i < ObjectsCount; i++) {
                var otherObj = ObjectsNetwork[i];
                var type = otherObj.Type;
                if (type.IsCreature && otherObj.Data == creatureID) {
                    obj = otherObj;
                    return i;
                }
            }

            obj = null;
            return -1;
        }

        public void UpdateObjectsCache() {
            int index = 0;
            int objectsCount = ObjectsCount;
            Appearances.AppearanceType tmpType = null;

            while (index < ObjectsCount && ((tmpType = ObjectsNetwork[index].Type).IsBank || tmpType.IsClip || tmpType.IsBottom)) {
                ObjectsRenderer[index] = ObjectsNetwork[index];
                index++;
            }

            while (index < ObjectsCount) {
                ObjectsRenderer[index] = ObjectsNetwork[--objectsCount];
                index++;
            }

            while (index < Constants.MapSizeW) {
                ObjectsRenderer[index] = null;
                index++;
            }

            Appearances.ObjectInstance hangableObject = null;
            Appearances.AppearanceType hookType = null;

            CacheTranslucent = false;
            CacheUnsight = false;

            for (int i = 0; i < ObjectsCount; i++) {
                tmpType = ObjectsNetwork[i].Type;
                CacheTranslucent = CacheTranslucent || tmpType.IsTranslucent;
                CacheUnsight = CacheUnsight || tmpType.IsUnsight;

                if (tmpType.IsHangable)
                    hangableObject = ObjectsNetwork[i];
                else if (tmpType.IsHookEast|| tmpType.IsHookSouth)
                    hookType = tmpType;
            }

            if (hangableObject) {
                if (!!tmpType && tmpType.IsHookEast)
                    hangableObject.Hang = Appearances.AppearanceInstance.HookEast;
                else if (tmpType)
                    hangableObject.Hang = Appearances.AppearanceInstance.HookSouth;
                else
                    hangableObject.Hang = 0;
            }
        }

        public void ResetObjects() {
            for (int i = 0; i < Constants.MapSizeW; i++) {
                ObjectsNetwork[i] = null;
                ObjectsRenderer[i] = null;
            }

            ObjectsCount = 0;
            CacheObjectsCount = 0;
            CacheLyingObject = false;
            CacheTranslucent = false;
            CacheUnsight = false;
            CacheObjectsDirty = false;
            CacheBitmapDirty = false;
            MiniMapColor = 0;
            MiniMapCost = Constants.PathCostObstacle;
            MiniMapDirty = false;
        }

        public Appearances.ObjectInstance DeleteObject(int stackPos) {
            if (stackPos < 0 || stackPos > ObjectsCount)
                return null;

            Appearances.ObjectInstance obj = ObjectsNetwork[stackPos];
            ObjectsCount = System.Math.Max(ObjectsCount - 1, 0);
            while (stackPos < ObjectsCount) {
                ObjectsNetwork[stackPos] = ObjectsNetwork[stackPos + 1];
                stackPos++;
            }

            ObjectsNetwork[ObjectsCount] = null;
            CacheObjectsDirty = true;
            CacheBitmapDirty = true;
            MiniMapDirty = true;
            return obj;
        }
        
        public void Reset() {
            ResetObjects();
            ResetEffects();
        }

        private void ConsistencyCheck() {

        }

        public void ResetEffects() {
            Effects.Clear();
            EffectsCount = 0;
            EnvironmentalEffect = null;
        }
        
        public void UpdateMiniMap() {
            if (!MiniMapDirty)
                return;

            MiniMapDirty = false;
            UpdateObjectsCache();
            MiniMapColor = 0;
            MiniMapCost = Constants.PathCostMax;

            bool obstacle = ObjectsCount == 0 || !ObjectsRenderer[0].Type.IsGround; // no objects or very bottom item is not ground
            for (int i = 0; i < ObjectsCount; i++) {
                Appearances.AppearanceType appearanceType = ObjectsRenderer[i].Type;
                if (appearanceType.Ground != 0)
                    MiniMapCost = (int)System.Math.Min(Constants.PathCostMax, appearanceType.Waypoints);

                if (appearanceType.IsAutoMap)
                    MiniMapColor = Colors.ARGBFrom8Bit(appearanceType.AutoMapColor);

                if (appearanceType.IsAvoid || appearanceType.IsUnpassable)
                    obstacle = true;
            }

            if (obstacle)
                MiniMapCost = Constants.PathCostObstacle;
        }

        public static bool operator !(Field instance) {
            return instance == null;
        }

        public static bool operator true(Field instance) {
            return !!instance;
        }

        public static bool operator false(Field instance) {
            return !instance;
        }
    }
}
