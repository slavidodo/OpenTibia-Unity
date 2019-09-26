using UnityEngine;

namespace OpenTibiaUnity.Core.Input.GameAction
{
    public class LootActionImpl : IActionImpl
    {
        Vector3Int _absolutePosition;
        Appearances.AppearanceType _appearanceType;
        int _stackPos;

        public LootActionImpl(Vector3Int absolutePosition, Appearances.ObjectInstance objectInstance, int stackPos) {
            Init(absolutePosition, objectInstance?.Type, stackPos);
        }

        public LootActionImpl(Vector3Int absolutePosition, Appearances.AppearanceType appearnceType, int stackPos) {
            Init(absolutePosition, appearnceType, stackPos);
        }

        public LootActionImpl(Vector3Int absolutePosition, uint objectId, int stackPos) {
            var appearnceType = OpenTibiaUnity.AppearanceStorage.GetObjectType(objectId);
            Init(absolutePosition, appearnceType, stackPos);
        }

        protected void Init(Vector3Int absolutePosition, Appearances.AppearanceType appearanceType, int stackPos) {
            _appearanceType = appearanceType;
            if (!_appearanceType)
                throw new System.ArgumentException("LookActionImpl.LookActionImpl: Invalid type: " + appearanceType);

            _absolutePosition = absolutePosition;
            _stackPos = stackPos;
        }

        public void Perform(bool repeat = false) {
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            if (!!protocolGame && protocolGame.IsGameRunning)
                protocolGame.SendQuickLoot(_absolutePosition, (ushort)_appearanceType.Id, _stackPos);
        }
    }
}
