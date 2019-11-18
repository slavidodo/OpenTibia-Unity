using System;

namespace OpenTibiaUnity.Core.Communication.Attributes
{
    class ClientFeatureAttribute : Attribute
    {
        private GameFeature _feature;
        public GameFeature Feature { get => _feature; }

        public ClientFeatureAttribute(GameFeature feature) {
            _feature = feature;
        }
    }
}
