using OpenTibiaUnity.Core.Container;
using OpenTibiaUnity.Core.Trade;
using System.Collections.Generic;
using UnityEngine;

using UnityUI = UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Trade
{
    public class NPCTradeWindow : UI.Legacy.SidebarWidget
    {
        [SerializeField] private UI.Legacy.Toggle _buyToggle = null;
        [SerializeField] private UI.Legacy.Toggle _sellToggle = null;
        [SerializeField] private UI.Legacy.Slider _amountSlider = null;
        [SerializeField] private TMPro.TextMeshProUGUI _amountLabel = null;
        [SerializeField] private TMPro.TextMeshProUGUI _priceLabel = null;
        [SerializeField] private TMPro.TextMeshProUGUI _moneyLabel = null;
        [SerializeField] private UI.Legacy.Button _acceptButton = null;
        [SerializeField] private UnityUI.ToggleGroup _tradeItemsToggleGroup = null;

        [SerializeField] private NPCTradeItem _nPCTradeItemTemplate = null;

        private List<NPCTradeItem> _buyTradeItems = null;
        private List<NPCTradeItem> _sellTradeItems = null;
        private NPCTradeItem _currentTradeItem = null;
        private bool _changingCurrentOffers = false;
        private bool _forceDontUpdateTradeItems = false;

        private bool _ignoreCapacity = false;
        private bool _buyWithBackpack = false;
        private bool _ignoreEquipped = false;

        protected override void Awake() {
            base.Awake();

            _buyToggle.onValueChanged.AddListener((_) => ShowBuyOffers());
            _sellToggle.onValueChanged.AddListener((_) => ShowSellOffers());
            _acceptButton.onClick.AddListener(() => AcceptSelectedOffer());
            _amountSlider.onValueChanged.AddListener((v) => UpdateTradeAmount((int)v));

            var containerStorage = OpenTibiaUnity.ContainerStorage;
            containerStorage.onPlayerGoodsChange.AddListener(OnPlayerGoodsChange);
            containerStorage.onPlayerMoneyChange.AddListener(OnPlayerMoneyChange);

            OpenTibiaUnity.GameManager.onGameEnd.AddListener(OnGameEnd);
        }
        
        private void OnGameEnd() {
            var gameManager = OpenTibiaUnity.GameManager;
            gameManager.InvokeOnMainThread(() => gameManager.onGameEnd.RemoveListener(OnGameEnd));

            CloseWithoutNotifying();
        }

        private void OnTradeItemValueChanged(NPCTradeItem tradeItem, bool value) {
            if (!value)
                return;

            _currentTradeItem = tradeItem;
            var tradeObject = _currentTradeItem.tradeObject;
            if (tradeObject.Amount == 0) {
                _amountSlider.minValue = 0;
                _amountSlider.maxValue = 0;
            } else {
                _amountSlider.minValue = 1;
                _amountSlider.maxValue = tradeObject.Amount;
            }

            _amountSlider.handleRect.gameObject.SetActive(_amountSlider.maxValue != 0);
            UpdateTradeAmount((int)_amountSlider.value);
        }

        private void OnPlayerMoneyChange(long _, long newMoney) {
            _moneyLabel.text = newMoney.ToString();

            if (!_forceDontUpdateTradeItems)
                InternalUpdateOffers();
        }

        private void OnPlayerGoodsChange(List<InventoryTypeInfo> _) {
            if (!_forceDontUpdateTradeItems)
                InternalUpdateOffers();
        }
        
        public void Setup(string npcName, List<TradeObjectRef> buyList, List<TradeObjectRef> sellList) {
            // setup window title
            string title;
            if (npcName != null)
                title = string.Format(TextResources.WINDOWTITLE_NPCTRADE_WITH_NAME, npcName);
            else
                title = TextResources.WINDOWTITLE_NPCTRADE_NO_NAME;

            _title.text = title;

            _currentTradeItem = null;

            // setup objects
            _buyTradeItems = new List<NPCTradeItem>();
            foreach (var tradeObject in buyList) {
                var tradeItem = CreateTradeItem(tradeObject);
                tradeItem.gameObject.SetActive(true);
                _buyTradeItems.Add(tradeItem);
            }

            _sellTradeItems = new List<NPCTradeItem>();
            foreach (var tradeObject in sellList) {
                var tradeItem = CreateTradeItem(tradeObject);
                tradeItem.gameObject.SetActive(false);
                _sellTradeItems.Add(tradeItem);
            }

            _forceDontUpdateTradeItems = true;
            OnPlayerMoneyChange(0, OpenTibiaUnity.ContainerStorage.PlayerMoney);
            OnPlayerGoodsChange(OpenTibiaUnity.ContainerStorage.PlayerGoods);
            _forceDontUpdateTradeItems = false;

            InternalUpdateOffers();
            ShowBuyOffers();
        }

        private void UpdateTradeAmount(int amount) {
            _amountLabel.text = amount.ToString();
            _priceLabel.text = (_currentTradeItem.tradeObject.Price * amount).ToString();
        }

        private void InternalUpdateOffers() {
            long playerCapacity = OpenTibiaUnity.Player.GetSkillValue(SkillType.Capacity);
            long playerMoney = OpenTibiaUnity.ContainerStorage.PlayerMoney;
            var playerGoods = OpenTibiaUnity.ContainerStorage.PlayerGoods;

            if (playerMoney == 0)
                return;

            foreach (var tradeItem in _buyTradeItems) {
                var tradeObject = tradeItem.tradeObject;
                
                tradeObject.Amount = (uint)Mathf.Min(playerMoney / tradeObject.Price, playerCapacity / tradeObject.Weight, 100);
            }

            foreach (var tradeItem in _sellTradeItems) {
                var tradeObject = tradeItem.tradeObject;

                var inventoryObject = playerGoods != null ? playerGoods.Find((m) => m.Id == tradeObject.Id) : null;
                tradeObject.Amount = inventoryObject == null ? 0 : (uint)Mathf.Min(100, inventoryObject.Count);
            }
        }

        public override void Close() {
            base.Close();

            var protocolGame = OpenTibiaUnity.ProtocolGame;
            if (!!protocolGame && protocolGame.IsGameRunning)
                protocolGame.SendCloseNPCTrade();
        }

        public void CloseWithoutNotifying() {
            base.Close();
        }

        private NPCTradeItem CreateTradeItem(TradeObjectRef tradeObject) {
            var tradeItem = Instantiate(_nPCTradeItemTemplate, _content);
            tradeItem.tradeObject = tradeObject;
            tradeItem.toggle.group = _tradeItemsToggleGroup;
            tradeItem.toggle.onValueChanged.AddListener((value) => OnTradeItemValueChanged(tradeItem, value));
            tradeItem.text = string.Format("{0}: {1} gold", tradeObject.Name, tradeObject.Price);
            return tradeItem;
        }

        private void ShowBuyOffers() {
            if (_changingCurrentOffers || _buyTradeItems == null || _sellTradeItems == null)
                return;

            _changingCurrentOffers = true;
            _buyToggle.isOn = true;

            foreach (var tradeItem in _buyTradeItems) {
                if (!tradeItem.gameObject.activeSelf)
                    tradeItem.gameObject.SetActive(true);
            }
            
            foreach (var tradeItem in _sellTradeItems) {
                if (tradeItem.gameObject.activeSelf)
                    tradeItem.gameObject.SetActive(false);
            }

            if (_buyTradeItems.Count > 0)
                _buyTradeItems[0].toggle.isOn = true;
            else
                _currentTradeItem = null;

            _changingCurrentOffers = false;
        }

        private void ShowSellOffers() {
            if (_changingCurrentOffers || _buyTradeItems == null || _sellTradeItems == null)
                return;

            _changingCurrentOffers = true;
            _sellToggle.isOn = true;

            foreach (var tradeItem in _buyTradeItems) {
                if (tradeItem.gameObject.activeSelf)
                    tradeItem.gameObject.SetActive(false);
            }

            foreach (var tradeItem in _sellTradeItems) {
                if (!tradeItem.gameObject.activeSelf)
                    tradeItem.gameObject.SetActive(true);
            }

            if (_sellTradeItems.Count > 0)
                _sellTradeItems[0].toggle.isOn = true;
            else
                _currentTradeItem = null;

            _changingCurrentOffers = false;
        }

        private void AcceptSelectedOffer() {
            if (!_currentTradeItem)
                return;

            var protocolGame = OpenTibiaUnity.ProtocolGame;
            if (!protocolGame || !protocolGame.IsGameRunning)
                return;

            var tradeObject = _currentTradeItem.tradeObject;
            int amount = (int)_amountSlider.value;
            if (_buyToggle.isOn)
                protocolGame.SendBuyObject(tradeObject.Id, tradeObject.Data, amount, _ignoreCapacity, _buyWithBackpack);
            else
                protocolGame.SendSellObject(tradeObject.Id, tradeObject.Data, amount, _ignoreEquipped);
        }
    }
}
