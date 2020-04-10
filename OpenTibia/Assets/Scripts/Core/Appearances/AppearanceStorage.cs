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


        private uint _minimumObjectId = 0;
        private uint _maximumObjectId = 0;
        private uint _minimumEffectId = 0;
        private uint _maximumEffectId = 0;
        private uint _minimumMissileId = 0;
        private uint _maximumMissileId = 0;
        private uint _minimumOutfitId = 0;
        private uint _maximumOutfitId = 0;

        private Protobuf.Appearances.SpecialMeaningAppearanceIds SpecialMeaningAppearanceIds {
            get => _protoAppearances != null ? _protoAppearances.SpecialMeaningAppearanceIDs : null;
        }
        public uint GoldCoinId { get => SpecialMeaningAppearanceIds != null ? SpecialMeaningAppearanceIds.GoldCoinId : 0; }
        public uint PlatinumCoinId { get => SpecialMeaningAppearanceIds != null ? SpecialMeaningAppearanceIds.PlatinumCoinId : 0; }
        public uint CrystalCoinId { get => SpecialMeaningAppearanceIds != null ? SpecialMeaningAppearanceIds.CrystalCoinId : 0; }

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

            _effectTypes = new List<AppearanceType>(_protoAppearances.Effects.Count);
            foreach (var appearance in _protoAppearances.Effects)
                _effectTypes.Add(new AppearanceType(appearance.ID, appearance, AppearanceCategory.Effect));

            _missileTypes = new List<AppearanceType>(_protoAppearances.Missles.Count);
            foreach (var appearance in _protoAppearances.Missles)
                _missileTypes.Add(new AppearanceType(appearance.ID, appearance, AppearanceCategory.Missile));

            _outfitTypes = new List<AppearanceType>(_protoAppearances.Outfits.Count);
            foreach (var appearance in _protoAppearances.Outfits)
                _outfitTypes.Add(new AppearanceType(appearance.ID, appearance, AppearanceCategory.Outfit));

            _objectTypes.Sort((a, b) => a.Id.CompareTo(b.Id));
            _effectTypes.Sort((a, b) => a.Id.CompareTo(b.Id));
            _missileTypes.Sort((a, b) => a.Id.CompareTo(b.Id));
            _outfitTypes.Sort((a, b) => a.Id.CompareTo(b.Id));

            _minimumObjectId = _objectTypes[0].Id;
            _maximumObjectId = _objectTypes[_objectTypes.Count - 1].Id;
            _minimumEffectId = _effectTypes[0].Id;
            _maximumEffectId = _effectTypes[_effectTypes.Count - 1].Id;
            _minimumMissileId = _missileTypes[0].Id;
            _maximumMissileId = _missileTypes[_missileTypes.Count - 1].Id;
            _minimumOutfitId = _outfitTypes[0].Id;
            _maximumOutfitId = _outfitTypes[_outfitTypes.Count - 1].Id;
            
            _invisibleOutfitType = _effectTypes[13 - 1];
        }

        public void SetSpriteProvider(SpritesProvider spriteProvider) => _spritesProvider = spriteProvider;
        public void UnloadSpriteProvider() => _spritesProvider?.Dispose();
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
            else if (id >= _minimumObjectId && id <= _maximumObjectId)
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

            if (id >= _minimumEffectId && id <= _maximumEffectId)
                return new EffectInstance(id, FindAppearanceType(_effectTypes, id));
            return null;
        }

        public MissileInstance CreateMissileInstance(uint id, UnityEngine.Vector3Int fromPosition, UnityEngine.Vector3Int toPosition) {
            if (_missileTypes == null)
                throw new System.Exception("AppearanceStorage.CreateMissileInstance: proto appearances not loaded.");

            if (id >= _minimumMissileId && id <= _maximumMissileId)
                return new MissileInstance(id, FindAppearanceType(_missileTypes, id), fromPosition, toPosition);
            return null;
        }

        public EffectInstance CreateInvisibleEffect() {
            return new EffectInstance(_invisibleOutfitType.Id, _invisibleOutfitType);
        }

        public OutfitInstance CreateOutfitInstance(uint id, int head, int body, int legs, int feet, int addons) {
            if (_outfitTypes == null)
                throw new System.Exception("AppearanceStorage.CreateOutfitInstance: proto appearances not loaded.");

            if (id >= _minimumOutfitId && id <= _maximumOutfitId) {
                return new OutfitInstance(id, FindAppearanceType(_outfitTypes, id), head, body, legs, feet, addons);
            }
            return null;
        }

        private AppearanceType FindAppearanceType(List<AppearanceType> list, uint id) {
            int l = 0, r = list.Count - 1;
            while (l <= r) {
                int tmpIndex = l + (r - l) / 2;
                var appearanceType = list[tmpIndex];
                if (appearanceType.Id < id)
                    l = tmpIndex + 1;
                else if (appearanceType.Id > id)
                    r = tmpIndex - 1;
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
