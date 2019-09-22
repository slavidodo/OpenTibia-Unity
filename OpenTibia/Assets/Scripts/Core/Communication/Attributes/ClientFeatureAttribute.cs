using System;

namespace OpenTibiaUnity.Assets.Scripts.Core.Communication.Attributes
{
    class ClientFeatureAttribute : Attribute
    {
        private GameFeature _feature;

        public ClientFeatureAttribute(GameFeature feature) {
            _feature = feature;
        }

        public GameFeature Feature { get => _feature; }
    }
}
