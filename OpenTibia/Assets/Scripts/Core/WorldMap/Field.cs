using System.Collections.Generic;
using UnityEngine;

namespace OpenTibiaUnity.Core.WorldMap
{
    public class Field
    {
        public List<Appearances.AppearanceInstance> Effects { get; } = new List<Appearances.AppearanceInstance>();
        public Appearances.ObjectInstance[] ObjectsRenderer { get; } = new Appearances.ObjectInstance[Constants.MapSizeW];
        public Appearances.ObjectInstance[] ObjectsNetwork { get; } = new Appearances.ObjectInstance[Constants.MapSizeW];
        public bool CacheTranslucent { get; private set; } = false;
        public bool CacheObjectsDirty { get; private set; } = false;
        public bool CacheUnsight { get; private set; } = false;
        public bool MiniMapDirty { get; private set; } = false;

        public uint MiniMapColor { get; private set; } = 0;
        public int EffectsCount { get; private set; } = 0;
        public int ObjectsCount { get; private set; } = 0;
        public int MiniMapCost { get; private set; } = int.MaxValue;

        public Appearances.ObjectInstance EnvironmentalEffect { get; set; } = null;
        
        public int GetObjectPriority(Appearances.ObjectInstance objectInstance) {
            Appearances.AppearanceType appearanceType = objectInstance.Type;
            if (appearanceType.IsGround)
                return 0;
            else if (appearanceType.IsGroundBorder)
                return 1;
            else if (appearanceType.IsBottom)
                return 2;
            else if (appearanceType.IsTop)
                return 3;
            else if (objectInstance.Id == Appearances.AppearanceInstance.Creature)
                return 4;
            else
                return 5;
        }

        public Appearances.AppearanceInstance GetEffect(int stackPos) {
            return Effects[stackPos];
        }
        public void AppendEffect(Appearances.AppearanceInstance effect) {
            if (!effect || (!(effect is Appearances.TextualEffectInstance) && !effect.Type))
                throw new System.ArgumentException("Field.AppendEffect: Invalid effect.");

            if (!!effect.Type && effect.Type.IsTopEffect) {
                Effects.Insert(0, effect);
                for (int i = 0; i < Effects.Count; i++)
                    Effects[i].MapData = i;
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

            int index = ObjectsCount - 1;
            while (index > stackPos) {
                ObjectsNetwork[index] = ObjectsNetwork[index - 1];
                if (!!ObjectsNetwork[index])
                    ObjectsNetwork[index].MapData = index;
                index--;
            }

            objectInstance.MapData = stackPos;
            ObjectsNetwork[stackPos] = objectInstance;
            CacheObjectsDirty = true;
            MiniMapDirty = true;
            return otherObject;
        }
        public Appearances.ObjectInstance ChangeObject(Appearances.ObjectInstance objectInstance, int stackPos) {
            if (!objectInstance || stackPos < 0 || stackPos >= ObjectsCount) {
                return null;
            }

            Appearances.ObjectInstance myObject = ObjectsNetwork[stackPos];
            ObjectsNetwork[stackPos] = objectInstance;
            objectInstance.MapData = stackPos;
            if (!!myObject) myObject.MapData = -1;

            CacheObjectsDirty = true;
            MiniMapDirty = true;
            return myObject;
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
                var @object = ObjectsNetwork[i];
                if (topLookObj == null || !@object.Type.IsIgnoreLook) {
                    index = i;
                    topLookObj = @object;

                    if (!@object.Type.IsGround && !@object.Type.IsGroundBorder && !@object.Type.IsBottom && !@object.Type.IsTop)
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
                Appearances.ObjectInstance @object;
                for (; index < ObjectsCount; index++) {
                    @object = ObjectsNetwork[index];
                    if (@object.Type.IsForceUse)
                        break;

                    if (!@object.Type.IsGround && !@object.Type.IsGroundBorder && !@object.Type.IsBottom && !@object.Type.IsTop)
                        break;
                }

                if (index > 0 && !(@object = ObjectsNetwork[index]).Type.IsForceUse && @object.Type.IsSplash)
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
        public int GetTopUseObject(out Appearances.ObjectInstance @object) {
            @object = null;
            if (ObjectsCount == 0)
                return -1;

            int index = 0;
            for (; index < ObjectsCount - 1; index++) {
                @object = ObjectsNetwork[index];
                if (@object.Type.IsForceUse || (!@object.Type.IsGround && !@object.Type.IsGroundBorder && !@object.Type.IsBottom && !@object.Type.IsTop && !@object.IsCreature))
                    break;
            }

            try {
                while (index > 0 && ((@object = ObjectsNetwork[index]).IsCreature || @object.Type.IsSplash))
                    index--;
            } catch (System.NullReferenceException) {
                throw new System.Exception("Field.GetTopUseObject: Invalid network of objects (index=" + index + ", count=" + ObjectsCount + ").");
            }

            @object = ObjectsNetwork[index];
            return index;
        }
        public int GetTopUseObject() {
            Appearances.ObjectInstance _;
            return GetTopUseObject(out _);
        }
        public int GetTopMoveObject(out Appearances.ObjectInstance topMoveObj) {
            if (ObjectsCount > 0) {
                int index = 0;
                for (; index < ObjectsCount - 1; index++) {
                    var @object = ObjectsNetwork[index];
                    if (!@object.Type.IsGround && !@object.Type.IsGroundBorder && !@object.Type.IsBottom && !@object.Type.IsTop && !@object.IsCreature)
                        break;
                }

                if (index > 0 && ObjectsNetwork[index].Type.IsUnmovable)
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
        public int GetCreatureObjectForCreatureId(uint creatureId, out Appearances.ObjectInstance @object) {
            if (creatureId == 0 || ObjectsCount == 0) {
                @object = null;
                return -1;
            }

            for (int i = 0; i < ObjectsCount; i++) {
                var otherObj = ObjectsNetwork[i];
                var type = otherObj.Type;
                if (type.IsCreature && otherObj.Data == creatureId) {
                    @object = otherObj;
                    return i;
                }
            }

            @object = null;
            return -1;
        }

        public void InvalidateObjectsTRS() {
            UpdateObjectsCache();

            for (int i = 0; i < ObjectsCount; i++) {
                var @object = ObjectsRenderer[i];
                if (@object.IsCreature) {
                    var creature = OpenTibiaUnity.CreatureStorage.GetCreatureById(@object.Data);
                    if (!!creature) {
                        if (!!creature.Outfit)
                            creature.Outfit.InvalidateTRS();
                        if (!!creature.MountOutfit)
                            creature.MountOutfit.InvalidateTRS();
                    }
                } else {
                    @object.InvalidateTRS();
                }
            }

            for (int i = 0; i < EffectsCount; i++)
                Effects[i].InvalidateTRS();
        }

        public void UpdateObjectsCache() {
            if (!CacheObjectsDirty)
                return;
            
            CacheObjectsDirty = false;

            int index = 0;
            if (ObjectsCount > 0) {
                // ground, borders, bottom items
                for (int i = 0; i < ObjectsCount; i++) {
                    var @object = ObjectsNetwork[i];
                    var type = @object.Type;
                    if (type.IsGround || type.IsGroundBorder || type.IsBottom)
                        ObjectsRenderer[index++] = @object;
                }

                // common items
                for (int i = ObjectsCount - 1; i >= 0; i--) {
                    var @object = ObjectsNetwork[i];
                    var type = @object.Type;
                    if (!type.IsGround && !type.IsGroundBorder && !type.IsBottom && !type.IsTop && !type.IsCreature)
                        ObjectsRenderer[index++] = @object;
                }

                // creatures
                for (int i = ObjectsCount - 1; i >= 0; i--) {
                    var @object = ObjectsNetwork[i];
                    var type = @object.Type;
                    if (@object.IsCreature)
                        ObjectsRenderer[index++] = @object;
                }

                // top items
                for (int i = 0; i < ObjectsCount; i++) {
                    var @object = ObjectsNetwork[i];
                    var type = @object.Type;
                    if (type.IsTop)
                        ObjectsRenderer[index++] = @object;
                }
            }
            
            while (index < Constants.MapSizeW)
                ObjectsRenderer[index++] = null;


            CacheTranslucent = false;
            CacheUnsight = false;
            
            Appearances.ObjectInstance hangableObject = null;
            Appearances.AppearanceType hookType = null;

            for (int i = 0; i < ObjectsCount; i++) {
                Appearances.AppearanceType tmpType = ObjectsNetwork[i].Type;
                CacheTranslucent = CacheTranslucent || tmpType.IsTranslucent;
                CacheUnsight = CacheUnsight || tmpType.IsUnsight;

                if (tmpType.IsHangable)
                    hangableObject = ObjectsNetwork[i];
                else if (tmpType.IsHookEast|| tmpType.IsHookSouth)
                    hookType = tmpType;
            }

            if (hangableObject) {
                if (!!hookType && hookType.IsHookEast)
                    hangableObject.Hang = Appearances.AppearanceInstance.HookEast;
                else if (hookType)
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
            CacheTranslucent = false;
            CacheUnsight = false;
            CacheObjectsDirty = false;
            MiniMapColor = 0;
            MiniMapCost = Constants.PathCostObstacle;
            MiniMapDirty = false;
        }

        public Appearances.ObjectInstance DeleteObject(int stackPos) {
            if (stackPos < 0 || stackPos > ObjectsCount)
                return null;

            Appearances.ObjectInstance @object = ObjectsNetwork[stackPos];
            ObjectsCount = System.Math.Max(ObjectsCount - 1, 0);
            while (stackPos < ObjectsCount) {
                ObjectsNetwork[stackPos] = ObjectsNetwork[stackPos + 1];
                stackPos++;
            }

            ObjectsNetwork[ObjectsCount] = null;
            CacheObjectsDirty = true;
            MiniMapDirty = true;
            return @object;
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

            bool obstacle = ObjectsCount == 0 || !ObjectsRenderer[0].Type.IsGround; // no objects or no ground
            for (int i = 0; i < ObjectsCount; i++) {
                Appearances.AppearanceType appearanceType = ObjectsRenderer[i].Type;
                if (!obstacle) {
                    if (appearanceType.IsBlockPath || appearanceType.IsUnpassable)
                        obstacle = true;
                    else if (appearanceType.IsGround)
                        MiniMapCost = (int)System.Math.Min(Constants.PathCostMax, appearanceType.GroundSpeed);
                }

                if (appearanceType.IsAutomap)
                    MiniMapColor = Colors.ARGBFrom8Bit(appearanceType.AutomapColor);
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
