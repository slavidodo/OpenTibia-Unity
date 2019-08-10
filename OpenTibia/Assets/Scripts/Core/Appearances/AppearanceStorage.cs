using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Appearances
{
    internal class AppearanceStorage
    {
        private static List<ObjectInstance> EnvironmentalEffects = new List<ObjectInstance>();

        Protobuf.Appearances.Appearances m_ProtoAppearances;
        SpritesProvider m_SpritesProvider;
        private readonly AppearanceType m_CreatureAppearanceType = new AppearanceType(AppearanceInstance.Creature, null, AppearanceCategory.Outfit);
        private AppearanceType m_InvisibleOutfitType;

        private List<AppearanceType> m_ObjectTypes;
        private List<AppearanceType> m_EffectTypes;
        private List<AppearanceType> m_MissileTypes;
        private List<AppearanceType> m_OutfitTypes;

        private List<AppearanceType> m_MarketObjectTypes;
        private List<AppearanceTypeInfo> m_ObjectTypeInfoCache;
        
        internal void SetProtoAppearances(Protobuf.Appearances.Appearances appearances) {
            m_ProtoAppearances = appearances;
            
            m_ObjectTypes = new List<AppearanceType>(m_ProtoAppearances.Objects.Count);
            m_MarketObjectTypes = new List<AppearanceType>();
            foreach (var appearance in m_ProtoAppearances.Objects) {
                var type = new AppearanceType(appearance.ID, appearance, AppearanceCategory.Object);
                m_ObjectTypes.Add(type);
                if (type.IsMarket)
                    m_MarketObjectTypes.Add(type);
            }

            m_ObjectTypes.Sort((a, b) => {
                return a.ID.CompareTo(b.ID);
            });
            
            m_EffectTypes = new List<AppearanceType>(m_ProtoAppearances.Effects.Count);
            foreach (var appearance in m_ProtoAppearances.Effects)
                m_EffectTypes.Add(new AppearanceType(appearance.ID, appearance, AppearanceCategory.Effect));

            m_MissileTypes = new List<AppearanceType>(m_ProtoAppearances.Missles.Count);
            foreach (var appearance in m_ProtoAppearances.Missles)
                m_MissileTypes.Add(new AppearanceType(appearance.ID, appearance, AppearanceCategory.Missile));

            m_OutfitTypes = new List<AppearanceType>(m_ProtoAppearances.Outfits.Count);
            foreach (var appearance in m_ProtoAppearances.Outfits)
                m_OutfitTypes.Add(new AppearanceType(appearance.ID, appearance, AppearanceCategory.Outfit));
            
            m_InvisibleOutfitType = m_EffectTypes[13 - 1];
        }

        internal void SetSpriteProvider(SpritesProvider spriteProvider) => m_SpritesProvider = spriteProvider;
        internal void UnloadSpriteProvider() => m_SpritesProvider?.Unload();
        internal CachedSpriteInformation GetSprite(uint spriteID) => m_SpritesProvider.GetSprite(spriteID);

        internal void Unload() {
            UnloadSpriteProvider();
            m_SpritesProvider = null;

            m_ObjectTypes?.Clear();
            m_EffectTypes?.Clear();
            m_MissileTypes?.Clear();
            m_OutfitTypes?.Clear();
            m_ProtoAppearances = null;
        }

        internal AppearanceType GetObjectType(uint id) {
            if (m_ObjectTypes == null)
                throw new System.Exception("AppearanceStorage.CreateObjectInstance: proto appearances not loaded.");

            if (id == AppearanceInstance.Creature)
                return m_CreatureAppearanceType;
            else if (id >= 100 && (id - 100) < m_ObjectTypes.Count)
                return FindAppearanceType(m_ObjectTypes, id);

            return null;
        }

        internal ObjectInstance CreateObjectInstance(uint id, uint data) {
            var type = GetObjectType(id);
            if (type != null)
                return new ObjectInstance(id, type, data);
            return null;
        }

        internal ObjectInstance CreateObjectInstance(uint id, int data) => CreateObjectInstance(id, (uint)data);

        internal EffectInstance CreateEffectInstance(uint id) {
            if (m_EffectTypes == null)
                throw new System.Exception("AppearanceStorage.CreateEffectInstance: proto appearances not loaded.");

            if (id >= 1 || id <= m_EffectTypes.Count)
                return new EffectInstance(id, FindAppearanceType(m_EffectTypes, id));
            return null;
        }

        internal MissileInstance CreateMissileInstance(uint id, UnityEngine.Vector3Int fromPosition, UnityEngine.Vector3Int toPosition) {
            if (m_MissileTypes == null)
                throw new System.Exception("AppearanceStorage.CreateMissileInstance: proto appearances not loaded.");

            if (id >= 1 || id <= m_MissileTypes.Count)
                return new MissileInstance(id, FindAppearanceType(m_MissileTypes, id), fromPosition, toPosition);
            return null;
        }

        internal OutfitInstance CreateOutfitInstance(uint id, int head, int body, int legs, int feet, int addons) {
            if (m_OutfitTypes == null)
                throw new System.Exception("AppearanceStorage.CreateOutfitInstance: proto appearances not loaded.");

            if (id == OutfitInstance.InvisibleOutfitID) {
                return new OutfitInstance(id, m_InvisibleOutfitType, head, body, legs, feet, addons);
            } else if (id >= 1 && id <= m_OutfitTypes.Count) {
                return new OutfitInstance(id, FindAppearanceType(m_OutfitTypes, id), head, body, legs, feet, addons);
            }
                
            return null;
        }

        private AppearanceType FindAppearanceType(List<AppearanceType> list, uint id) {
            int lastIndex = list.Count - 1;
            int index = 0;
            while (index <= lastIndex) {
                int tmpIndex = index + lastIndex >> 1;
                var appearanceType = list[tmpIndex];
                if (appearanceType.ID < id)
                    index = tmpIndex + 1;
                else if (appearanceType.ID > id)
                    lastIndex = tmpIndex - 1;
                else
                    return appearanceType;
            }

            return null;
        }
        
        internal TextualEffectInstance CreateTextualEffect(int color, string value, TMPro.TextMeshProUGUI textMesh) {
            return new TextualEffectInstance(color, value, textMesh);
        }

        internal ObjectInstance CreateEnvironmentalEffect(uint id) {
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
