using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Appearances
{
    public class AppearanceStorage
    {
        private static List<ObjectInstance> EnvironmentalEffects = new List<ObjectInstance>();

        Proto.Appearances.Appearances m_ProtoAppearances;
        SpritesProvider m_SpritesProvider;
        private readonly AppearanceType m_CreatureAppearanceType = new AppearanceType(AppearanceInstance.Creature, null, AppearanceCategory.Outfit);
        private AppearanceType m_InvisibleOutfitType;

        private List<AppearanceType> m_ObjectTypes;
        private List<AppearanceType> m_EffectTypes;
        private List<AppearanceType> m_MissileTypes;
        private List<AppearanceType> m_OutfitTypes;

        private List<AppearanceType> m_MarketObjectTypes;
        private List<AppearanceTypeInfo> m_ObjectTypeInfoCache;
        
        public void SetProtoAppearances(Proto.Appearances.Appearances appearances) {
            m_ProtoAppearances = appearances;
            
            m_ObjectTypes = new List<AppearanceType>(m_ProtoAppearances.Objects.Count);
            m_MarketObjectTypes = new List<AppearanceType>();
            foreach (var appearance in m_ProtoAppearances.Objects) {
                var type = new AppearanceType(appearance.Id, appearance, AppearanceCategory.Object);
                m_ObjectTypes.Add(type);
                if (type.IsMarket)
                    m_MarketObjectTypes.Add(type);
            }

            m_EffectTypes = new List<AppearanceType>(m_ProtoAppearances.Effects.Count);
            foreach (var appearance in m_ProtoAppearances.Effects)
                m_EffectTypes.Add(new AppearanceType(appearance.Id, appearance, AppearanceCategory.Effect));

            m_MissileTypes = new List<AppearanceType>(m_ProtoAppearances.Missles.Count);
            foreach (var appearance in m_ProtoAppearances.Missles)
                m_MissileTypes.Add(new AppearanceType(appearance.Id, appearance, AppearanceCategory.Missile));

            m_OutfitTypes = new List<AppearanceType>(m_ProtoAppearances.Outfits.Count);
            foreach (var appearance in m_ProtoAppearances.Outfits)
                m_OutfitTypes.Add(new AppearanceType(appearance.Id, appearance, AppearanceCategory.Outfit));
            
            m_InvisibleOutfitType = m_EffectTypes[13 - 1];
        }

        public void SetSpriteProvider(SpritesProvider spriteProvider) => m_SpritesProvider = spriteProvider;
        public void UnloadSpriteProvider() => m_SpritesProvider?.Unload();
        public CachedSpriteInformation GetSprite(uint spriteID) => m_SpritesProvider.GetSprite(spriteID);

        public void Unload() {
            UnloadSpriteProvider();
            m_SpritesProvider = null;

            m_ObjectTypes?.Clear();
            m_EffectTypes?.Clear();
            m_MissileTypes?.Clear();
            m_OutfitTypes?.Clear();
            m_ProtoAppearances = null;
        }

        public AppearanceType GetObjectType(uint id) {
            if (m_ObjectTypes == null)
                throw new System.Exception("AppearanceStorage.CreateObjectInstance: proto appearances not loaded.");

            if (id == AppearanceInstance.Creature)
                return m_CreatureAppearanceType;
            else if (id >= 100 && (id - 100) < m_ObjectTypes.Count)
                return m_ObjectTypes[(int)id - 100];

            return null;
        }

        public ObjectInstance CreateObjectInstance(uint id, uint data) {
            if (m_ObjectTypes == null)
                throw new System.Exception("AppearanceStorage.CreateObjectInstance: proto appearances not loaded.");

            if (id == AppearanceInstance.Creature)
                return new ObjectInstance(id, m_CreatureAppearanceType, data);
            else if (id >= 100 && (id - 100) < m_ObjectTypes.Count)
                return new ObjectInstance(id, m_ObjectTypes[(int)id - 100], data);
            
            return null;
        }

        public ObjectInstance CreateObjectInstance(uint id, int data) => CreateObjectInstance(id, (uint)data);

        public EffectInstance CreateEffectInstance(uint id) {
            if (m_EffectTypes == null)
                throw new System.Exception("AppearanceStorage.CreateEffectInstance: proto appearances not loaded.");

            if (id >= 1 || id <= m_EffectTypes.Count)
                return new EffectInstance(id, m_EffectTypes[(int)id - 1]);
            return null;
        }

        public MissileInstance CreateMissileInstance(uint id, UnityEngine.Vector3Int fromPosition, UnityEngine.Vector3Int toPosition) {
            if (m_MissileTypes == null)
                throw new System.Exception("AppearanceStorage.CreateMissileInstance: proto appearances not loaded.");

            if (id >= 1 || id <= m_MissileTypes.Count)
                return new MissileInstance(id, m_MissileTypes[(int)id - 1], fromPosition, toPosition);
            return null;
        }

        public OutfitInstance CreateOutfitInstance(uint id, int head, int body, int legs, int feet, int addons) {
            if (m_OutfitTypes == null)
                throw new System.Exception("AppearanceStorage.CreateOutfitInstance: proto appearances not loaded.");

            if (id == OutfitInstance.InvisibleOutfitID) {
                return new OutfitInstance(id, m_InvisibleOutfitType, head, body, legs, feet, addons);
            } else if (id >= 1 && id <= m_OutfitTypes.Count) {
                return new OutfitInstance(id, m_OutfitTypes[(int)id - 1], head, body, legs, feet, addons);
            }
                
            return null;
        }

        public TextualEffectInstance CreateTextualEffect(int color, int value) {
            return new TextualEffectInstance(color, value);
        }

        public TextualEffectInstance CreateTextualEffect(int color, string value) {
            return new TextualEffectInstance(color, value);
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
                if (objectInstance.ID < id) {
                    i = index + 1;
                    continue;
                } else if (objectInstance.ID > id) {
                    count = 0;
                    continue;
                }

                var type = objectInstance.Type;
                //uint data = !!objectInstance.Atmospheric ? 1 : 0;
                return new ObjectInstance(type.ID, type, 0);
            }

            return null;
        }
    }
}
