using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Appearances
{
    public class AppearanceStorage
    {
        private static List<ObjectInstance> EnvironmentalEffects = new List<ObjectInstance>();

        Protobuf.Appearances.Appearances _protoAppearances;
        SpritesProvider _spritesProvider;
        private readonly AppearanceType _creatureAppearanceType = new AppearanceType(AppearanceInstance.Creature, null, AppearanceCategory.Outfit);
        private AppearanceType _invisibleOutfitType;

        private List<AppearanceType> _objectTypes;
        private List<AppearanceType> _effectTypes;
        private List<AppearanceType> _missileTypes;
        private List<AppearanceType> _outfitTypes;

        private List<AppearanceType> _marketObjectTypes;
        private List<AppearanceTypeInfo> _objectTypeInfoCache;
        
        public void SetProtoAppearances(Protobuf.Appearances.Appearances appearances) {
            _protoAppearances = appearances;
            
            _objectTypes = new List<AppearanceType>(_protoAppearances.Objects.Count);
            _marketObjectTypes = new List<AppearanceType>();
            foreach (var appearance in _protoAppearances.Objects) {
                var type = new AppearanceType(appearance.ID, appearance, AppearanceCategory.Object);
                _objectTypes.Add(type);
                if (type.IsMarket)
                    _marketObjectTypes.Add(type);
            }

            _objectTypes.Sort((a, b) => {
                return a.Id.CompareTo(b.Id);
            });
            
            _effectTypes = new List<AppearanceType>(_protoAppearances.Effects.Count);
            foreach (var appearance in _protoAppearances.Effects)
                _effectTypes.Add(new AppearanceType(appearance.ID, appearance, AppearanceCategory.Effect));

            _missileTypes = new List<AppearanceType>(_protoAppearances.Missles.Count);
            foreach (var appearance in _protoAppearances.Missles)
                _missileTypes.Add(new AppearanceType(appearance.ID, appearance, AppearanceCategory.Missile));

            _outfitTypes = new List<AppearanceType>(_protoAppearances.Outfits.Count);
            foreach (var appearance in _protoAppearances.Outfits)
                _outfitTypes.Add(new AppearanceType(appearance.ID, appearance, AppearanceCategory.Outfit));
            
            _invisibleOutfitType = _effectTypes[13 - 1];
        }

        public void SetSpriteProvider(SpritesProvider spriteProvider) => _spritesProvider = spriteProvider;
        public void UnloadSpriteProvider() => _spritesProvider?.Unload();
        public SpriteLoadingStatus GetSprite(uint spriteId, out CachedSprite cachedSprite) {
            return _spritesProvider.GetSprite(spriteId, out cachedSprite);
        }

        public void Unload() {
            UnloadSpriteProvider();
            _spritesProvider = null;

            _objectTypes?.Clear();
            _effectTypes?.Clear();
            _missileTypes?.Clear();
            _outfitTypes?.Clear();
            _protoAppearances = null;
        }

        public AppearanceType GetObjectType(uint id) {
            if (_objectTypes == null)
                throw new System.Exception("AppearanceStorage.CreateObjectInstance: proto appearances not loaded.");

            if (id == AppearanceInstance.Creature)
                return _creatureAppearanceType;
            else if (id >= 100 && (id - 100) < _objectTypes.Count)
                return FindAppearanceType(_objectTypes, id);

            return null;
        }

        public ObjectInstance CreateObjectInstance(uint id, uint data) {
            var type = GetObjectType(id);
            if (type != null)
                return new ObjectInstance(id, type, data);
            return null;
        }

        public ObjectInstance CreateObjectInstance(uint id, int data) => CreateObjectInstance(id, (uint)data);

        public EffectInstance CreateEffectInstance(uint id) {
            if (_effectTypes == null)
                throw new System.Exception("AppearanceStorage.CreateEffectInstance: proto appearances not loaded.");

            if (id >= 1 || id <= _effectTypes.Count)
                return new EffectInstance(id, FindAppearanceType(_effectTypes, id));
            return null;
        }

        public MissileInstance CreateMissileInstance(uint id, UnityEngine.Vector3Int fromPosition, UnityEngine.Vector3Int toPosition) {
            if (_missileTypes == null)
                throw new System.Exception("AppearanceStorage.CreateMissileInstance: proto appearances not loaded.");

            if (id >= 1 || id <= _missileTypes.Count)
                return new MissileInstance(id, FindAppearanceType(_missileTypes, id), fromPosition, toPosition);
            return null;
        }

        public OutfitInstance CreateOutfitInstance(uint id, int head, int body, int legs, int feet, int addons) {
            if (_outfitTypes == null)
                throw new System.Exception("AppearanceStorage.CreateOutfitInstance: proto appearances not loaded.");

            if (id == OutfitInstance.InvisibleOutfitId) {
                return new OutfitInstance(id, _invisibleOutfitType, head, body, legs, feet, addons);
            } else if (id >= 1 && id <= _outfitTypes.Count) {
                return new OutfitInstance(id, FindAppearanceType(_outfitTypes, id), head, body, legs, feet, addons);
            }
                
            return null;
        }

        private AppearanceType FindAppearanceType(List<AppearanceType> list, uint id) {
            int lastIndex = list.Count - 1;
            int index = 0;
            while (index <= lastIndex) {
                int tmpIndex = index + lastIndex >> 1;
                var appearanceType = list[tmpIndex];
                if (appearanceType.Id < id)
                    index = tmpIndex + 1;
                else if (appearanceType.Id > id)
                    lastIndex = tmpIndex - 1;
                else
                    return appearanceType;
            }

            return null;
        }
        
        public TextualEffectInstance CreateTextualEffect(int color, string text, int value = -1) {
            return new TextualEffectInstance(color, text, value);
        }

        public ObjectInstance CreateEnvironmentalEffect(uint id) {
            // TODO(priority=low):
            // this code will actually return null all the time
            // since environmental_effects is always empty..

            int i = 0;
            var count = EnvironmentalEffects.Count - 1;
            while (i <= count) {
                int index = (i + count) / 2;
                var objectInstance = EnvironmentalEffects[index];
                if (objectInstance.Id < id) {
                    i = index + 1;
                    continue;
                } else if (objectInstance.Id > id) {
                    count = 0;
                    continue;
                }

                var type = objectInstance.Type;
                //uint data = !!objectInstance.Atmospheric ? 1 : 0;
                return new ObjectInstance(type.Id, type, 0);
            }

            return null;
        }
    }
}
