using OpenTibiaUnity.Core.Appearances;
using OpenTibiaUnity.Core.Components.Base;
using OpenTibiaUnity.Core.Creatures;
using OpenTibiaUnity.Core.Game;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Battle
{
    public class BattleWindow : MiniWindow, IUseWidget, IMoveWidget
    {
        [SerializeField] private RectTransform _filtersPanel = null;
        [SerializeField] private RectTransform _battleList = null;
        [SerializeField] private BattleCreature _battleCreaturePrefab = null;

        [SerializeField] private Button _showSortTypesButton = null;
        [SerializeField] private Toggle _filtersPanelToggle = null;
        [SerializeField] private Toggle _filterPlayersToggle = null;
        [SerializeField] private Toggle _filterNPCsToggle = null;
        [SerializeField] private Toggle _filterMonstersToggle = null;
        [SerializeField] private Toggle _filterNonSkulledToggle = null;
        [SerializeField] private Toggle _filterPartyToggle = null;
        [SerializeField] private Toggle _filterSummonsToggle = null;

        protected override void Start() {
            base.Start();

            OpenTibiaUnity.CreatureStorage.onOpponentsRefreshed.AddListener(OnOpponentsRefreshed);
            OpenTibiaUnity.CreatureStorage.onOpponentsRebuilt.AddListener(OnOpponentsRebuilt);

            _filtersPanelToggle.onValueChanged.AddListener(OnToggleFilters);
            _filterPlayersToggle.onValueChanged.AddListener((value) => ToggleOpponentsFilter(OpponentFilters.Players, value));
            _filterNPCsToggle.onValueChanged.AddListener((value) => ToggleOpponentsFilter(OpponentFilters.NPCs, value));
            _filterMonstersToggle.onValueChanged.AddListener((value) => ToggleOpponentsFilter(OpponentFilters.Monsters, value));
            _filterNonSkulledToggle.onValueChanged.AddListener((value) => ToggleOpponentsFilter(OpponentFilters.NonSkulled, value));
            _filterPartyToggle.onValueChanged.AddListener((value) => ToggleOpponentsFilter(OpponentFilters.Party, value));
            _filterSummonsToggle.onValueChanged.AddListener((value) => ToggleOpponentsFilter(OpponentFilters.Summons, value));
        }

        protected void OnOpponentsRefreshed(List<Creature> opponents) {
            var copy = new List<Creature>(opponents);

            for (int i = 0; i < _battleList.childCount; i++) {
                var child = _battleList.GetChild(i).GetComponent<BattleCreature>();

                int index = opponents.IndexOf(child.Creature);
                if (index == -1) {
                    Destroy(child.gameObject);
                    continue;
                }
                
                child.transform.SetSiblingIndex(index);
            }
        }

        protected void OnOpponentsRebuilt(List<Creature> opponents) {
            Dictionary<Creature, BattleCreature> existingOpponents = new Dictionary<Creature, BattleCreature>();
            for (int i = 0; i < _battleList.childCount; i++) {
                var child = _battleList.GetChild(i).GetComponent<BattleCreature>();
                if (opponents.Contains(child.Creature))
                    existingOpponents.Add(child.Creature, child);
                else
                    Destroy(child.gameObject);
            }

            int index = 0;
            foreach (var creature in opponents) {
                if (existingOpponents.TryGetValue(creature, out BattleCreature battleCreature))
                    battleCreature.transform.SetSiblingIndex(index);
                else if (battleCreature = Instantiate(_battleCreaturePrefab, _battleList))
                    battleCreature.UpdateDetails(creature);
                
                index++;
            }
        }

        protected void OnToggleFilters(bool value) {
            _filtersPanel.gameObject.SetActive(value);
        }

        protected override void OnClientVersionChange(int _, int newVersion) {
            bool isTibia11 = newVersion >= 1100;

            _filtersPanelToggle.gameObject.SetActive(isTibia11);
            _showSortTypesButton.gameObject.SetActive(isTibia11);
            _filtersPanel.gameObject.SetActive(isTibia11 && _filtersPanelToggle.isOn);
        }

        protected void ToggleOpponentsFilter(OpponentFilters filter, bool value) {
            var optionStorage = OpenTibiaUnity.OptionStorage;
            if (value)
                optionStorage.OpponentFilter |= filter;
            else
                optionStorage.OpponentFilter &= ~filter;

            OpenTibiaUnity.CreatureStorage.InvalidateOpponents();
            OpenTibiaUnity.CreatureStorage.RefreshOpponents();
        }
        
        public int GetTopObjectUnderPoint(Vector3 mousePosition, out ObjectInstance @object) {
            @object = null;
            return -1;
        }

        public int GetMoveObjectUnderPoint(Vector3 mousePosition, out ObjectInstance @object) {
            return GetUseObjectUnderPoint(mousePosition, out @object);
        }

        public int GetUseObjectUnderPoint(Vector3 mousePosition, out ObjectInstance @object) {
            @object = null;
            return -1;
        }

        public int GetMultiUseObjectUnderPoint(Vector3 mousePosition, out ObjectInstance @object) {
            return GetUseObjectUnderPoint(mousePosition, out @object);
        }
    }
}
