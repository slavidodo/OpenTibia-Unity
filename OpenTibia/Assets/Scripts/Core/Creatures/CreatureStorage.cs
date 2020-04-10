using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Creatures
{
    public class CreatureStorage
    {
        public class OpponentsChangeEvent : UnityEngine.Events.UnityEvent<List<Creature>> { }

        private int _creatureCount = 0;
        private int _creatureIndex = 0;
        private List<Creature> _creatures = new List<Creature>();

        private Creature _aim = null;
        public Creature Aim {
            get => _aim;
            set {
                if (_aim != value) {
                    var old = _aim;
                    _aim = value;
                    UpdateExtendedMark(old);
                    UpdateExtendedMark(_aim);
                }
            }
        }

        public Creature AttackTarget { get; set; } = null;
        public Creature FollowTarget { get; set; } = null;
        List<Creature> Trappers { get; set; } = null;
        public Player Player { get; set; } = new Player();

        public Creature ReplaceCreature(Creature creature, uint id = 0) {
            if (!creature)
                throw new System.ArgumentException("CreatureStorage.replaceCreature: Invalid creature.");

            if (id != 0)
                RemoveCreature(id);

            if (_creatureCount >= Constants.MaxCreatureCount)
                throw new System.ArgumentException("CreatureStorage.replaceCreature: No space left to append " + creature.Id);

            int l = 0, r = _creatureCount - 1;
            while (l <= r) {
                int i = l + r >> 1;
                var other = _creatures[i];
                if (other.Id < creature.Id)
                    l = i + 1;
                else if (other.Id > creature.Id)
                    r = i - 1;
                else
                    return other;
            }

            creature.KnownSince = ++_creatureIndex;
            _creatures.Insert(l, creature);
            _creatureCount++;
            return creature;
        }

        public void RemoveCreature(uint id) {
            int i = 0, l = 0, r = _creatureCount - 1;
            while (l <= r) {
                var other = _creatures[i];
                if (other.Id < id)
                    l = i + 1;
                else if (other.Id > id)
                    r = i - 1;
                else
                    break;
            }

            Creature creature = null;
            if (i < 0 || i >= _creatureCount || (creature = _creatures[i]).Id != id)
                throw new System.ArgumentException("CreatureStorage.RemoveCreature: creature " + id + " not found");
            else if (creature == Player)
                throw new System.Exception("CreatureStorage.RemoveCreature: cannot remove player.");

            if (creature == Aim) {
                Aim = null;
                UpdateExtendedMark(creature);
            }

            if (creature == AttackTarget) {
                AttackTarget = null;
                UpdateExtendedMark(creature);
            }

            if (creature == FollowTarget) {
                FollowTarget = null;
                UpdateExtendedMark(creature);
            }

            if (Trappers != null) {
                int trapperIndex = Trappers.FindIndex((x) => x == creature);
                if (trapperIndex > 0) {
                    Trappers[trapperIndex].Trapper = false;
                    Trappers.RemoveAt(trapperIndex);
                }
            }

            creature.Reset();
            _creatures.RemoveAt(i);
            _creatureCount--;
        }

        public Creature GetCreatureByName(string name) {
            for (int i = 0; i < _creatureCount; i++) {
                if (_creatures[i].Name == name)
                    return _creatures[i];
            }

            return null;
        }

        public Creature GetCreatureById(uint id) {
            int l = 0, r = _creatureCount - 1;
            while (l <= r) {
                int i = l + r >> 1;
                var other = _creatures[i];
                if (other.Id < id)
                    l = i + 1;
                else if (other.Id > id)
                    r = i - 1;
                else
                    return other;
            }

            return null;
        }

        protected void UpdateExtendedMark(Creature creature) {
            if (!creature)
                return;

            // aim-attack: white outline & red border
            // aim-follow: white outline & green border
            // aim: white outline
            // attack: red border
            // follow: green border

            var marks = creature.Marks;
            if (creature == Aim) {
                if (creature == AttackTarget) {
                    marks.SetMark(MarkType.ClientMapWindow, Appearances.Marks.MarkAimAttack);
                    marks.SetMark(MarkType.ClientBattleList, Appearances.Marks.MarkAimAttack);
                } else if (creature == FollowTarget) {
                    marks.SetMark(MarkType.ClientMapWindow, Appearances.Marks.MarkAimFollow);
                    marks.SetMark(MarkType.ClientBattleList, Appearances.Marks.MarkAimFollow);
                } else {
                    marks.SetMark(MarkType.ClientMapWindow, Appearances.Marks.MarkAim);
                    marks.SetMark(MarkType.ClientBattleList, Appearances.Marks.MarkAim);
                }
            } else if (creature == AttackTarget) {
                marks.SetMark(MarkType.ClientMapWindow, Appearances.Marks.MarkAttack);
                marks.SetMark(MarkType.ClientBattleList, Appearances.Marks.MarkAttack);
            } else if (creature == FollowTarget) {
                marks.SetMark(MarkType.ClientMapWindow, Appearances.Marks.MarkFollow);
                marks.SetMark(MarkType.ClientBattleList, Appearances.Marks.MarkFollow);
            } else {
                marks.SetMark(MarkType.ClientMapWindow, Appearances.Marks.MarkUnmarked);
                marks.SetMark(MarkType.ClientBattleList, Appearances.Marks.MarkUnmarked);
            }
        }

        public void SetTrappers(IEnumerable<Creature> trappers) {
            int index = Trappers != null ? Trappers.Count : -1;
            while (index >= 0) {
                if (Trappers[index] != null)
                    Trappers[index].Trapper = false;
                index--;
            }

            Trappers = new List<Creature>(trappers);
            index = Trappers != null ? Trappers.Count : -1;
            while (index >= 0) {
                if (Trappers[index] != null)
                    Trappers[index].Trapper = true;
                index--;
            }
        }

        public void MarkOpponentVisible(object param, bool visible) {
            Creature creature;
            if (param is Creature) {
                creature = param as Creature;
            } else if (param is Appearances.ObjectInstance @object) {
                creature = GetCreatureById(@object.Data);
            } else if (param is uint || param is int) {
                creature = GetCreatureById((uint)param);
            } else {
                throw new System.ArgumentException("CreatureStorage.MarkOpponentVisible: Invalid overload.");
            }

            if (creature)
                creature.Visible = visible;
        }

        public void MarkAllOpponentsVisible(bool value) {
            for (int i = 0; i < _creatureCount; i++)
                _creatures[i].Visible = true;
        }

        public bool IsOpponent(Creature creature, bool deepCheck = false) {
            if (!creature || !creature.Visible || creature is Player)
                return false;

            var creaturePosition = creature.Position;
            var myPosition = Player.Position;

            if (creaturePosition.z != myPosition.z || System.Math.Abs(creaturePosition.x - myPosition.x) > Constants.MapWidth / 2 || System.Math.Abs(creaturePosition.y - myPosition.y) > Constants.MapHeight / 2)
                return false;

            var filter = OpenTibiaUnity.OptionStorage.OpponentFilter;
            if (!deepCheck || filter == OpponentFilters.None)
                return true;

            if ((filter & OpponentFilters.Players) > 0 && creature.Type == CreatureType.Player)
                return false;

            if ((filter & OpponentFilters.NPCs) > 0 && creature.Type == CreatureType.NPC)
                return false;

            if ((filter & OpponentFilters.Monsters) > 0 && creature.Type == CreatureType.Monster)
                return false;

            if ((filter & OpponentFilters.NonSkulled) > 0 && creature.Type == CreatureType.Player && creature.PkFlag == PkFlag.None)
                return false;

            if ((filter & OpponentFilters.Party) > 0 && creature.PartyFlag != PartyFlag.None)
                return false;

            if ((filter & OpponentFilters.Summons) > 0 && creature.SummonType != SummonType.None)
                return false;

            return true;
        }

        public List<Creature> GetOpponents() {
            var opponents = new List<Creature>();
            for (int i = 0; i < _creatureCount; i++) {
                var creature = _creatures[i];
                if (!!creature && IsOpponent(creature))
                    opponents.Add(creature);
            }

            return opponents;
        }

        public void Reset(bool resetPlayer = true) {
            if (resetPlayer)
                Player.Reset();

            _creatures.ForEach(creature => creature.Reset());

            _creatures.Clear();
            _creatureCount = 0;
            _creatureIndex = 0;

            if (!resetPlayer)
                ReplaceCreature(Player);

            Aim = null;
            AttackTarget = null;
            FollowTarget = null;
            Trappers = null;
        }

        public void Animate() {
            int ticks = OpenTibiaUnity.TicksMillis;

            foreach (var creature in _creatures) {
                if (creature) {
                    creature.AnimateMovement(ticks);
                    creature.AnimateOutfit(ticks);
                }
            }
        }

        public void ToggleAttackTarget(Creature attack, bool send) {
            if (attack == Player) {
                throw new System.ArgumentException("CreatureStorage.ToggleAttackTarget: Cannot attack player.");
            }

            var creature = AttackTarget;
            if (creature != attack)
                AttackTarget = attack;
            else
                AttackTarget = null;

            if (send) {
                var protocolGame = OpenTibiaUnity.ProtocolGame;
                if (protocolGame)
                    protocolGame.SendAttack(AttackTarget ? AttackTarget.Id : 0);
            }

            UpdateExtendedMark(creature);
            UpdateExtendedMark(AttackTarget);

            if (FollowTarget) {
                creature = FollowTarget;
                FollowTarget = null;
                UpdateExtendedMark(creature);
            }
        }

        public void ToggleFollowTarget(Creature follow, bool send) {
            if (follow == Player) {
                throw new System.ArgumentException("CreatureStorage.ToggleFollowTarget: Cannot follow player.");
            }

            var creature = FollowTarget;
            if (creature != follow)
                FollowTarget = follow;
            else
                FollowTarget = null;

            if (send) {
                var protocolGame = OpenTibiaUnity.ProtocolGame;
                if (protocolGame)
                    protocolGame.SendFollow(FollowTarget ? FollowTarget.Id : 0);
            }

            UpdateExtendedMark(creature);
            UpdateExtendedMark(FollowTarget);

            if (AttackTarget) {
                creature = AttackTarget;
                AttackTarget = null;
                UpdateExtendedMark(creature);
            }
        }

        public void SetAttackTarget(Creature attack, bool send) {
            if (attack == Player)
                throw new System.ArgumentException("CreatureStorage.SetAttackTarget: Cannot follow player.");

            var creature = AttackTarget;
            if (creature != attack) {
                AttackTarget = attack;

                if (send) {
                    var protocolGame = OpenTibiaUnity.ProtocolGame;
                    if (!!protocolGame && protocolGame.IsGameRunning)
                        protocolGame.SendAttack(AttackTarget ? AttackTarget.Id : 0);
                }

                UpdateExtendedMark(creature);
                UpdateExtendedMark(AttackTarget);
            }

            if (FollowTarget) {
                creature = FollowTarget;
                FollowTarget = null;
                UpdateExtendedMark(creature);
            }
        }

        public void SetFollowTarget(Creature follow, bool send) {
            if (follow == Player) {
                throw new System.ArgumentException("CreatureStorage.ToggleFollowTarget: Cannot follow player.");
            }

            Creature creature = FollowTarget;
            if (creature != follow) {
                FollowTarget = follow;

                if (send) {
                    var protocolGame = OpenTibiaUnity.ProtocolGame;
                    if (!!protocolGame && protocolGame.IsGameRunning)
                        protocolGame.SendFollow(FollowTarget ? FollowTarget.Id : 0);
                }

                UpdateExtendedMark(creature);
                UpdateExtendedMark(FollowTarget);
            }

            if (AttackTarget) {
                creature = AttackTarget;
                AttackTarget = null;
                UpdateExtendedMark(creature);
            }
        }

        public void ClearTargets() {
            var optionStorage = OpenTibiaUnity.OptionStorage;
            if (AttackTarget != null && optionStorage.AutoChaseOff && optionStorage.CombatChaseMode != CombatChaseModes.Off) {
                // trigger any listener that expects the chase mode not to be off yet
                OpenTibiaUnity.GameManager.onTacticsChange.Invoke(
                    optionStorage.CombatAttackMode,
                    CombatChaseModes.Off,
                    optionStorage.CombatSecureMode,
                    optionStorage.CombatPvPMode);

                // if no listeners, safely set it off
                // mostly this will never occur and this is just for safety
                optionStorage.CombatChaseMode = CombatChaseModes.Off;

                // send the change to the server, since invoking any game event
                // shouldn't send anything to the server
                var protocolGame = OpenTibiaUnity.ProtocolGame;
                if (protocolGame != null && protocolGame.IsGameRunning)
                    protocolGame.SendSetTactics();
            }

            if (FollowTarget != null)
                SetFollowTarget(null, true);
        }
    }
}
