namespace OpenTibiaUnity.Core.Store.OpenParameters
{
    class StoreCategoryAndFilterOpenParamater : IStoreOpenParamater
    {
        StoreCategoryAndFilter _categoryAndFilter;

        public StoreCategoryAndFilter CategoryAndFilter { get => _categoryAndFilter; }

        public StoreCategoryAndFilterOpenParamater(StoreCategoryAndFilter categoryAndFilter) {
            _categoryAndFilter = categoryAndFilter;
        }

        public void WriteToMessage(Communication.Internal.ByteArray message) {
            message.WriteString(_categoryAndFilter.Category);
            message.WriteString(_categoryAndFilter.Filter);
        }
    }
}
