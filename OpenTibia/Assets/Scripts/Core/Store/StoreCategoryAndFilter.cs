namespace OpenTibiaUnity.Core.Store
{
    public class StoreCategoryAndFilter
    {
        string _category;
        string _filter;

        public string Category { get => _category; }
        public string Filter { get => _filter; }

        public StoreCategoryAndFilter(string category, string filter) {
            _category = category;
            _filter = filter;
        }
    }
}
