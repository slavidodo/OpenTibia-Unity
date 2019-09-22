using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Store
{
    public class StoreStorage
    {
        List<StoreCategory> _categories = new List<StoreCategory>();
        StoreCategory _homeCategory = null;

        public List<StoreCategory> Categories { get => _categories; }
        public StoreCategory HomeCategory {
            get {
                if (_homeCategory == null)
                    _homeCategory = new StoreCategory(Constants.StoreHomeCategoryName, null, StoreHighlightState.None);
                return _homeCategory;
            }
        }
        
        public void ClearCategories() {
            _categories.Clear();
            _homeCategory = null;
        }

        public void AddCategory(StoreCategory category) {
            _categories.Add(category);
        }

        public StoreCategory FindCategory(string name) {
            if (OpenTibiaUnity.GameManager.ClientVersion >= 1180 && name == Constants.StoreHomeCategoryName)
                return HomeCategory;

            return _categories.Find((x) => x.Name == name);
        }
    }
}
