using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Store
{
    public class StoreProduct
    {
        string _name;
        string _description;
        List<StoreVisualisation> _visualisations = new List<StoreVisualisation>();

        public StoreProduct(string name, string description) {
            _name = name;
            _description = description;
        }

        public StoreProduct(string name, string description, StoreVisualisation visualisation) : this(name, description) {
            AddVisualisation(visualisation);
        }

        public StoreProduct(string name, string description, IEnumerable<StoreVisualisation> visualisations) : this(name, description) {
            _visualisations.AddRange(visualisations);
        }

        public void AddVisualisation(StoreVisualisation visualisation) {
            _visualisations.Add(visualisation);
        }
    }
}
