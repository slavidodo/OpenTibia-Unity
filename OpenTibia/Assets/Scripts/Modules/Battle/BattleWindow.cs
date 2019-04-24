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
#pragma warning disable CS0649 // never assigned to
        [SerializeField] private RectTransform m_FiltersPanel;
        [SerializeField] private RectTransform m_BattleList;
        [SerializeField] private BattleCreature m_BattleCreaturePrefab;

        [SerializeField] private Button m_ShowSortTypesPanel;
        [SerializeField] private Toggle m_FiltersPanelToggle;
        [SerializeField] private Toggle m_FilterPlayersToggle;
        [SerializeField] private Toggle m_FilterNPCsToggle;
        [SerializeField] private Toggle m_FilterMonstersToggle;
        [SerializeField] private Toggle m_FilterNonSkulledToggle;
        [SerializeField] private Toggle m_FilterPartyToggle;
        [SerializeField] private Toggle m_FilterSummonsToggle;
#pragma warning restore CS0649 // never assigned to

        protected override void Start() {
            base.Start();

            OpenTibiaUnity.CreatureStorage.onOpponentsRefreshed.AddListener(OnOpponentsRefreshed);
            OpenTibiaUnity.CreatureStorage.onOpponentsRebuilt.AddListener(OnOpponentsRebuilt);

            m_FiltersPanelToggle.onValueChanged.AddListener(OnToggleFilters);
            m_FilterPlayersToggle.onValueChanged.AddListener((value) => ToggleOpponentsFilter(OpponentFilters.Players, value));
            m_FilterNPCsToggle.onValueChanged.AddListener((value) => ToggleOpponentsFilter(OpponentFilters.NPCs, value));
            m_FilterMonstersToggle.onValueChanged.AddListener((value) => ToggleOpponentsFilter(OpponentFilters.Monsters, value));
            m_FilterNonSkulledToggle.onValueChanged.AddListener((value) => ToggleOpponentsFilter(OpponentFilters.NonSkulled, value));
            m_FilterPartyToggle.onValueChanged.AddListener((value) => ToggleOpponentsFilter(OpponentFilters.Party, value));
            m_FilterSummonsToggle.onValueChanged.AddListener((value) => ToggleOpponentsFilter(OpponentFilters.Summons, value));
        }

        protected void OnOpponentsRefreshed(List<Creature> opponents) {
            var copy = new List<Creature>(opponents);

            for (int i = 0; i < m_BattleList.childCount; i++) {
                var child = m_BattleList.GetChild(i).GetComponent<BattleCreature>();

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
            for (int i = 0; i < m_BattleList.childCount; i++) {
                var child = m_BattleList.GetChild(i).GetComponent<BattleCreature>();
                if (opponents.Contains(child.Creature))
                    existingOpponents.Add(child.Creature, child);
                else
                    Destroy(child.gameObject);
            }

            int index = 0;
            foreach (var creature in opponents) {
                if (existingOpponents.TryGetValue(creature, out BattleCreature battleCreature))
                    battleCreature.transform.SetSiblingIndex(index);
                else if (battleCreature = Instantiate(m_BattleCreaturePrefab, m_BattleList))
                    battleCreature.UpdateDetails(creature);
                
                index++;
            }
        }

        protected void OnToggleFilters(bool value) {
            m_FiltersPanel.gameObject.SetActive(value);
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

        public int GetMoveObjectUnderPoint(Vector3 mousePosition, out ObjectInstance obj) {
            return GetUseObjectUnderPoint(mousePosition, out obj);
        }

        public int GetUseObjectUnderPoint(Vector3 mousePosition, out ObjectInstance obj) {
            obj = null;
            return -1;
        }

        public int GetMultiUseObjectUnderPoint(Vector3 mousePosition, out ObjectInstance obj) {
            return GetUseObjectUnderPoint(mousePosition, out obj);
        }
    }
}
