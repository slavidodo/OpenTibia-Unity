using OpenTibiaUnity.Core.Chat;
using OpenTibiaUnity.Core.Creatures;
using OpenTibiaUnity.Core.Input.GameAction;
using UnityEngine;
using TR = OpenTibiaUnity.TextResources;

namespace OpenTibiaUnity.Modules.Battle
{
    public class BattleCreatureContextMenu : UI.Legacy.ContextMenuBase
    {
        private Creature _creature;

        public void Set(Creature creature) {
            _creature = creature;
        }

        public override void InitialiseOptions() {
            var gameManager = OpenTibiaUnity.GameManager;
            var creatureStorage = OpenTibiaUnity.CreatureStorage;
            var chatStorage = OpenTibiaUnity.ChatStorage;

            if (gameManager.ClientVersion >= 1100 && _creature.IsNPC) {
                CreateTextItem(TR.CTX_CREATURE_TALK, () => {
                    new GreetAction(_creature).Perform();
                });
            } else {
                bool isAttacked = creatureStorage.AttackTarget == _creature;
                CreateTextItem(!isAttacked ? TR.CTX_CREATURE_ATTACK_START : TR.CTX_CREATURE_ATTACK_STOP, () => {
                    creatureStorage.ToggleAttackTarget(_creature, true);
                });
            }

            bool isFollowed = creatureStorage.FollowTarget == _creature;
            CreateTextItem(!isFollowed ? TR.CTX_CREATURE_FOLLOW_START : TR.CTX_CREATURE_FOLLOW_STOP, () => {
                creatureStorage.ToggleFollowTarget(_creature, true);
            });

            if (_creature.IsConfirmedPartyMember) {
                // aggression, expert pvp
            }

            CreateSeparatorItem();

            if (_creature.IsHuman) {
                CreateTextItem(string.Format(TR.CTX_VIEW_PRIVATE_MESSAGE, _creature.Name), () => {
                    // todo, notify about expected channel :)
                    new PrivateChatActionImpl(PrivateChatActionType.OpenMessageChannel, PrivateChatActionImpl.ChatChannelNoChannel, _creature.Name).Perform();
                });

                if (chatStorage.HasOwnPrivateChannel) {
                    // todo, any way to check if he is there already?
                    // maybe channel participants are there for use
                    CreateTextItem(TR.CTX_VIEW_PRIVATE_INVITE, () => {
                        new PrivateChatActionImpl(PrivateChatActionType.ChatChannelInvite, chatStorage.OwnPrivateChannelId, _creature.Name).Perform();
                    });
                }

                // TODO, this should only appear if this player isn't already on the vip list
                if (true) {
                    CreateTextItem(string.Format(TextResources.CTX_VIEW_ADD_BUDDY, _creature.Name), "TODO", () => {
                        // TODO
                    });
                }

                var nameFilterSet = OpenTibiaUnity.OptionStorage.GetNameFilterSet(NameFilterSet.DefaultSet);
                if (nameFilterSet.IsBlacklisted(_creature.Name)) {
                    CreateTextItem(string.Format(TR.CTX_VIEW_PLAYER_UNIGNORE, _creature.Name), "TODO", () => {
                        // TODO
                    });
                } else {
                    CreateTextItem(string.Format(TR.CTX_VIEW_PLAYER_IGNORE, _creature.Name), "TODO", () => {
                        // TODO
                    });
                }

                // todo, party actions
                // todo inspect
                CreateSeparatorItem();
            }

            // todo report

            CreateTextItem(TR.CTX_VIEW_COPY_NAME, () => {
                GUIUtility.systemCopyBuffer = _creature.Name;
            });
        }
    }
}
