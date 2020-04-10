using OpenTibiaUnity.Core.Appearances;
using OpenTibiaUnity.Core.Creatures;
using OpenTibiaUnity.Core.Game;
using System.Collections.Generic;
using UnityEngine;
using OpenTibiaUnity.Core.Input.GameAction;

namespace OpenTibiaUnity.Modules.Battle
{
    public class BattleWidget : UI.Legacy.SidebarWidget, IUseWidget, IMoveWidget
    {
        // serialized fields
        [SerializeField]
        private RectTransform _filtersPanel = null;
        [SerializeField]
        private UI.Legacy.Button _showSortTypesButton = null;
        [SerializeField]
        private UI.Legacy.Toggle _filtersPanelToggle = null;
        [SerializeField]
        private UI.Legacy.Toggle _filterPlayersToggle = null;
        [SerializeField]
        private UI.Legacy.Toggle _filterNPCsToggle = null;
        [SerializeField]
        private UI.Legacy.Toggle _filterMonstersToggle = null;
        [SerializeField]
        private UI.Legacy.Toggle _filterNonSkulledToggle = null;
        [SerializeField]
        private UI.Legacy.Toggle _filterPartyToggle = null;
        [SerializeField]
        private UI.Legacy.Toggle _filterSummonsToggle = null;
        [SerializeField]
        private RectTransform _battleList = null;

        // non-serialized fields
        [System.NonSerialized]
        public BattleCreature activeWidget = null;

        // fields
        private Dictionary<uint, BattleCreature> battleCreatures = new Dictionary<uint, BattleCreature>();
        private Dictionary<uint, int> creatureAges = new Dictionary<uint, int>();

        protected override void Awake() {
            base.Awake();

            // setup events
            Creature.onVisbilityChange.AddListener(OnCreatureVisibilityChange);
            Creature.onSkillChange.AddListener(OnCreatureSkillChange);
            Creature.onPositionChange.AddListener(OnCreaturePositionChange);
            Creature.onPkFlagChange.AddListener(OnCreaturePkFlagChange);
            Creature.onGuildFlagChange.AddListener(OnCreatureGuildFlagChange);
            Creature.onMarksChange.AddListener(OnCreatureMarksChange);

            _filtersPanelToggle.onValueChanged.AddListener(OnToggleFilters);
            _filterPlayersToggle.onValueChanged.AddListener((value) => ToggleOpponentsFilter(OpponentFilters.Players, value));
            _filterNPCsToggle.onValueChanged.AddListener((value) => ToggleOpponentsFilter(OpponentFilters.NPCs, value));
            _filterMonstersToggle.onValueChanged.AddListener((value) => ToggleOpponentsFilter(OpponentFilters.Monsters, value));
            _filterNonSkulledToggle.onValueChanged.AddListener((value) => ToggleOpponentsFilter(OpponentFilters.NonSkulled, value));
            _filterPartyToggle.onValueChanged.AddListener((value) => ToggleOpponentsFilter(OpponentFilters.Party, value));
            _filterSummonsToggle.onValueChanged.AddListener((value) => ToggleOpponentsFilter(OpponentFilters.Summons, value));
        }

        protected override void Start() {
            base.Start();

            CheckCreatures();
        }

        protected override void OnEnable() {
            base.OnEnable();

            OpenTibiaUnity.InputHandler.AddMouseUpListener(Core.Utils.EventImplPriority.Default, OnMouseUp);
        }

        protected override void OnDisable() {
            base.OnDisable();

            if (OpenTibiaUnity.InputHandler != null)
                OpenTibiaUnity.InputHandler.RemoveMouseUpListener(OnMouseUp);
        }

        public void OnMouseUp(Event e, MouseButton mouseButton, bool repeat) {
            if (InternalStartMouseAction(e.mousePosition, mouseButton, true, false))
                e.Use();
        }

        private void OnCreatureVisibilityChange(Creature creature, bool value) {
            if (value && IsInitiallyFit(creature))
                AddCreature(creature);
            else if (!value)
                RemoveCreature(creature);
        }

        private void OnCreatureSkillChange(Creature creature, SkillType skillType, Skill skill) {
            if (!creature.Visible || creature == OpenTibiaUnity.Player || skillType != SkillType.HealthPercent)
                return;

            if (battleCreatures.TryGetValue(creature.Id, out BattleCreature battleCreature)) {
                var sortType = OpenTibiaUnity.OptionStorage.OpponentSort;
                if (sortType == OpponentSortType.SortHitpointsAsc || sortType == OpponentSortType.SortHitpointsDesc) {
                    RemoveCreature(creature);
                    AddCreature(creature);
                    return;
                }

                battleCreature.healthPercent = (int)skill.Level;
            }
        }

        private void OnCreaturePositionChange(Creature creature, Vector3Int newPosition, Vector3Int oldPosition) {
            var sortType = OpenTibiaUnity.OptionStorage.OpponentSort;
            if (creature == OpenTibiaUnity.Player) {
                if (newPosition.z != oldPosition.z) {
                    CheckCreatures();
                } else {
                    if (sortType == OpponentSortType.SortDistanceAsc || sortType == OpponentSortType.SortDistanceDesc) {
                        var distanceList = new List<Creature>(battleCreatures.Count);
                        foreach (var pair in battleCreatures)
                            distanceList.Add(pair.Value.creature);

                        distanceList.Sort(OpponentComparator);

                        for (int i = 0; i < distanceList.Count; i++) {
                            var other = distanceList[i];
                            var battleCreature = battleCreatures[other.Id];
                            battleCreature.transform.SetSiblingIndex(i);
                        }
                    }

                    foreach (var p in battleCreatures)
                        AddCreature(p.Value.creature);
                }
            } else {
                bool exists = HasCreature(creature.Id);
                bool isOpponent = OpenTibiaUnity.CreatureStorage.IsOpponent(creature);
                if (exists && !isOpponent) {
                    RemoveCreature(creature);
                } else if (isOpponent) {
                    if (exists && (sortType == OpponentSortType.SortDistanceAsc || sortType == OpponentSortType.SortDistanceDesc))
                        RemoveCreature(creature);

                    AddCreature(creature);
                }
            }
        }

        private void OnCreaturePkFlagChange(Creature creature, PkFlag newFlag, PkFlag oldFlag) {

        }

        private void OnCreatureGuildFlagChange(Creature creature, GuildFlag newFlag, GuildFlag oldFlag) {

        }

        private void OnCreatureMarksChange(Creature creature, Marks marks) {
            if (!HasCreature(creature.Id))
                return;

            bool set = false;
            uint color = 0;
            if (marks.CurrentMarks.TryGetValue(MarkType.ClientBattleList, out var mark)) {
                set = mark.IsSet;
                color = mark.MarkColor;
            }

            battleCreatures[creature.Id].UpdateMark(set, color);
        }

        private void OnToggleFilters(bool value) {
            _filtersPanel.gameObject.SetActive(value);
        }

        protected override void OnClientVersionChange(int _, int newVersion) {
            bool isTibia11 = newVersion >= 1100;

            _filtersPanelToggle.gameObject.SetActive(isTibia11);
            _showSortTypesButton.gameObject.SetActive(isTibia11);
            _filtersPanel.gameObject.SetActive(isTibia11 && _filtersPanelToggle.isOn);
        }

        private bool InternalStartMouseAction(Vector3 mousePosition, MouseButton mouseButton, bool applyAction = false, bool updateCursor = false) {
            var gameManager = OpenTibiaUnity.GameManager;
            if (!activeWidget || !gameManager.GameCanvas.gameObject.activeSelf || gameManager.GamePanelBlocker.gameObject.activeSelf)
                return false;

            var eventModifiers = OpenTibiaUnity.InputHandler.GetRawEventModifiers();
            var action = DetermineAction(mousePosition, mouseButton, eventModifiers, applyAction, updateCursor);
            return action != AppearanceActions.None;
        }

        public AppearanceActions DetermineAction(Vector3 mousePosition, MouseButton mouseButton, EventModifiers eventModifiers, bool applyAction = false, bool updateCursor = false) {
            if (!activeWidget || !activeWidget.creature)
                return AppearanceActions.None;

            var inputHandler = OpenTibiaUnity.InputHandler;
            if (inputHandler.IsMouseButtonDragged(MouseButton.Left) || inputHandler.IsMouseButtonDragged(MouseButton.Right))
                return AppearanceActions.None;

            var action = AppearanceActions.None;
            var optionStorage = OpenTibiaUnity.OptionStorage;

            var creature = activeWidget.creature;
            updateCursor &= OpenTibiaUnity.GameManager.ClientVersion >= 1100;

            switch (optionStorage.MousePreset) {
                case MousePresets.Classic: {
                    if (mouseButton == MouseButton.Left) {
                        if (eventModifiers == EventModifiers.Shift)
                            action = AppearanceActions.Look;
                        else if (eventModifiers == EventModifiers.Control)
                            action = AppearanceActions.ContextMenu;
                        else
                            action = AppearanceActions.Attack;
                    } else if (mouseButton == MouseButton.Right) {
                        if (eventModifiers == EventModifiers.None || eventModifiers == EventModifiers.Control)
                            action = AppearanceActions.ContextMenu;
                        else if (eventModifiers == EventModifiers.Shift)
                            action = AppearanceActions.Look;
                        else
                            action = AppearanceActions.Attack;
                    }

                    break;
                }

                case MousePresets.Regular: {
                    // TODO

                    break;
                }

                case MousePresets.LeftSmartClick: {
                    // TODO

                    break;
                }
            }

            if (updateCursor)
                OpenTibiaUnity.GameManager.CursorController.SetCursorState(action, CursorPriority.Medium);

            if (applyAction) {
                switch (action) {
                    case AppearanceActions.Attack:
                        OpenTibiaUnity.CreatureStorage.ToggleAttackTarget(creature, true);
                        break;

                    case AppearanceActions.ContextMenu:
                        CreateBattleCreatureContextMenu(creature).Display(mousePosition);
                        break;

                    case AppearanceActions.Look:
                        var protocolGame = OpenTibiaUnity.ProtocolGame;
                        if (!!protocolGame && protocolGame.IsGameRunning)
                            protocolGame.SendLookAtCreature(creature.Id);
                        break;

                    case AppearanceActions.Talk:
                        new GreetAction(activeWidget.creature);
                        break;
                }
            }

            return action;
        }

        private BattleCreatureContextMenu CreateBattleCreatureContextMenu(Creature creature) {
            var gameManager = OpenTibiaUnity.GameManager;
            var canvas = gameManager.ActiveCanvas;
            var gameObject = Instantiate(gameManager.ContextMenuBasePrefab, canvas.transform);

            var channelMessageContextMenu = gameObject.AddComponent<BattleCreatureContextMenu>();
            channelMessageContextMenu.Set(creature);
            return channelMessageContextMenu;
        }

        protected void ToggleOpponentsFilter(OpponentFilters filter, bool value) {
            var optionStorage = OpenTibiaUnity.OptionStorage;
            if (value)
                optionStorage.OpponentFilter |= filter;
            else
                optionStorage.OpponentFilter &= ~filter;
        }

        private bool IsInitiallyFit(Creature creature) {
            var player = OpenTibiaUnity.Player;
            if (creature == player)
                return false;


            if (creature.Position.z != player.Position.z)
                return false;

            return true;
        }

        private void AddCreature(Creature creature) {
            battleCreatures.TryGetValue(creature.Id, out BattleCreature battleCreature);

            int age = GetCreatureAge(creature.Id);
            if (age == -1)
                AddCreatureAge(creature);

            if (!battleCreature) {
                if (creature == OpenTibiaUnity.CreatureStorage.AttackTarget)
                    OnAttack(creature);
                
                if (creature == OpenTibiaUnity.CreatureStorage.FollowTarget)
                    OnFollow(creature);

                var creatures = new Creature[_battleList.childCount];
                int index = 0;
                foreach (Transform child in _battleList)
                    creatures[index++] = child.GetComponent<BattleCreature>().creature;

                index = System.Array.BinarySearch(creatures, creature, Comparer<Creature>.Create(OpponentComparator));
                if (index == -1)
                    index = 0;

                battleCreature = Instantiate(ModulesManager.Instance.BattleCreaturePrefab, _battleList);
                battleCreature.name = $"BattleCreature_{creature.Name}";
                battleCreature.transform.SetSiblingIndex(index);
                battleCreature.creature = creature;

                battleCreatures.Add(creature.Id, battleCreature);
            } else {
                battleCreature.healthPercent = creature.HealthPercent;
            }

            var player = OpenTibiaUnity.Player;
            battleCreature.gameObject.SetActive(creature != player && OpenTibiaUnity.CreatureStorage.IsOpponent(creature, true));
        }

        private void InternalRemoveCreature(Creature creature) {
            // todo last buttons witched

            // todo hide static square
            Destroy(battleCreatures[creature.Id]);
        }

        private void RemoveCreature(Creature creature) {
            if (HasCreature(creature.Id)) {
                InternalRemoveCreature(creature);
                var widget = battleCreatures[creature.Id];
                battleCreatures.Remove(creature.Id);
                Destroy(widget.gameObject);
            }
        }

        private void RemoveAllCreatures() {
            foreach (var battleCreature in battleCreatures)
                InternalRemoveCreature(battleCreature.Value.creature);

            battleCreatures.Clear();
        }

        private bool HasCreature(uint creatureId) {
            return battleCreatures.ContainsKey(creatureId);
        }

        private void CheckCreatures() {
            RemoveAllCreatures();

            if (!OpenTibiaUnity.GameManager.IsGameRunning)
                return;

            var opponents = OpenTibiaUnity.CreatureStorage.GetOpponents();
            foreach (var creature in opponents)
                AddCreature(creature);
        }

        private int GetCreatureAge(uint creatureId) {
            if (creatureAges.TryGetValue(creatureId, out int age))
                return age;
            return -1;
        }

        private void AddCreatureAge(Creature creature) {
            creatureAges.Add(creature.Id, OpenTibiaUnity.TicksMillis);
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

        private static int OpponentComparator(Creature a, Creature b) {
            return OpponentComparator(a, b, OpenTibiaUnity.OptionStorage.OpponentSort);
        }

        private static int OpponentComparator(Creature a, Creature b, OpponentSortType sortType) {
            if (a == null || b == null)
                return 0;

            var pos = 0;
            bool desc = false;
            if (sortType == OpponentSortType.SortDistanceDesc || sortType == OpponentSortType.SortHitpointsDesc || sortType == OpponentSortType.SortKnownSinceDesc || sortType == OpponentSortType.SortNameDesc)
                desc = true;

            switch (sortType) {
                case OpponentSortType.SortDistanceAsc:
                case OpponentSortType.SortDistanceDesc:
                    var myPosition = OpenTibiaUnity.Player.Position;
                    var d1 = Core.Utils.Utility.ManhattanDistance(myPosition, a.Position);
                    var d2 = Core.Utils.Utility.ManhattanDistance(myPosition, a.Position);
                    if (d1 < d2)
                        pos = -1;
                    else if (d1 > d2)
                        pos = 1;

                    break;

                case OpponentSortType.SortHitpointsAsc:
                case OpponentSortType.SortHitpointsDesc:
                    if (a.HealthPercent < b.HealthPercent)
                        pos = -1;
                    else if (a.HealthPercent > b.HealthPercent)
                        pos = 1;

                    break;

                case OpponentSortType.SortNameAsc:
                case OpponentSortType.SortNameDesc:
                    pos = a.Name.CompareTo(b.Name);
                    break;

                case OpponentSortType.SortKnownSinceAsc:
                case OpponentSortType.SortKnownSinceDesc:
                    break;
            }

            if (pos == 0) {
                if (a.KnownSince < b.KnownSince)
                    pos = -1;
                else if (a.KnownSince > b.KnownSince)
                    pos = 1;
                else
                    return 0;
            }

            return pos * (desc ? -1 : 1);
        }

        private void OnAttack(Creature creature) {

        }

        private void OnFollow(Creature creature) {

        }
    }
}
