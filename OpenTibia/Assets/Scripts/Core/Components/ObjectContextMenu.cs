using OpenTibiaUnity.Core.Appearances;
using OpenTibiaUnity.Core.Input.GameAction;
using OpenTibiaUnity.Core.Input.StaticAction;
using UnityEngine;

namespace OpenTibiaUnity.Core.Components
{
    internal class ObjectContextMenu : ContextMenuBase 
    {
        private Vector3Int m_Absolute;

        private ObjectInstance m_LookObject;
        private ObjectInstance m_UseObject;
        private Creatures.Creature m_Creature;

        private int m_LookObjectStackPos;
        private int m_UseObjectStackPos;

        internal void Set(Vector3Int absolutePosition, ObjectInstance lookObject, int lookObjectIndex, ObjectInstance useObject, int useObjectIndex, Creatures.Creature creature) {
            m_Absolute = absolutePosition;
            m_LookObject = lookObject;
            m_LookObjectStackPos = lookObjectIndex;

            m_UseObject = useObject;
            m_UseObjectStackPos = useObjectIndex;

            m_Creature = creature;
        }

        internal override void InitialiseOptions() {
            var gameManager = OpenTibiaUnity.GameManager;
            var optionStorage = OpenTibiaUnity.OptionStorage;
            var player = OpenTibiaUnity.Player;
            bool isClassic = optionStorage.MousePreset == MousePresets.Classic;
            bool isRegular = optionStorage.MousePreset == MousePresets.Regular;
            bool isLeftSmartClick = optionStorage.MousePreset == MousePresets.LeftSmartClick;

            if (!!m_LookObject) {
                CreateTextItem(TextResources.CTX_OBJECT_LOOK, "Shift", () => {
                    if (!!m_LookObject)
                        new LookActionImpl(m_Absolute, m_LookObject, m_LookObjectStackPos).Perform();
                });
            }

            // Tibia 11 (CTX_OBJECT_INSPECT_OBJECT)

            if (!!m_UseObject && (m_UseObject.Type.IsContainer || m_UseObject.Type.DefaultAction == Protobuf.Shared.PlayerAction.Open)) {
                if (m_Absolute.x == 65535 && m_Absolute.y >= 64) {
                    CreateTextItem(TextResources.CTX_OBJECT_OPEN, () => {
                        if (!!m_UseObject)
                            GameActionFactory.CreateUseAction(m_Absolute, m_UseObject, m_UseObjectStackPos, Vector3Int.zero, null, 0, UseActionTarget.Auto).Perform();
                    });

                    CreateTextItem(TextResources.CTX_OBJECT_OPEN_NEW_WINDOW, () => {
                        if (!!m_UseObject)
                            GameActionFactory.CreateUseAction(m_Absolute, m_UseObject, m_UseObjectStackPos, Vector3Int.zero, null, 0, UseActionTarget.NewWindow).Perform();
                    });
                } else {
                    CreateTextItem(TextResources.CTX_OBJECT_OPEN, () => {
                        if (!!m_UseObject)
                            GameActionFactory.CreateUseAction(m_Absolute, m_UseObject, m_UseObjectStackPos, Vector3Int.zero, null, 0, UseActionTarget.NewWindow).Perform();
                    });
                }
            }

            if (!!m_UseObject && !m_UseObject.Type.IsContainer) {
                string text, shortcut;
                if (m_UseObject.Type.IsMultiUse) {
                    text = TextResources.CTX_OBJECT_MULTIUSE;
                    shortcut = isClassic ? "Alt" : (isRegular ? "Ctrl" : null);
                } else {
                    text = TextResources.CTX_OBJECT_USE;
                    shortcut = isRegular ? "Ctrl" : null;
                }

                CreateTextItem(text, shortcut, () => {
                    //if (!!m_UseObject)
                    //    GameActionFactory.CreateUseAction(m_Absolute, m_UseObject, m_UseObjectStackPos, UseActionTarget.Auto).Perform();
                });
            }

            if (!!m_UseObject && m_UseObject.Type.IsWrappable) {
                CreateTextItem(TextResources.CTX_OBJECT_WRAP, () => {
                    if (!!m_UseObject)
                        new ToggleWrapStateActionImpl(m_Absolute, m_UseObject, m_UseObjectStackPos).Perform();
                });
            }

            if (!!m_UseObject && m_UseObject.Type.IsUnwrappable) {
                CreateTextItem(TextResources.CTX_OBJECT_UNWRAP, () => {
                    if (!!m_UseObject)
                        new ToggleWrapStateActionImpl(m_Absolute, m_UseObject, m_UseObjectStackPos).Perform();
                });
            }

            if (!!m_UseObject && m_UseObject.Type.IsRotateable) {
                CreateTextItem(TextResources.CTX_OBJECT_TURN, () => {
                    if (!!m_UseObject)
                        new TurnActionImpl(m_Absolute, m_UseObject, m_UseObjectStackPos).Perform();
                });
            }
            
            if (gameManager.ClientVersion >= 984 && m_Absolute.x != 65535) {
                var worldMapStorage = OpenTibiaUnity.WorldMapStorage;
                var mapPosition = worldMapStorage.ToMap(m_Absolute);
                var @object = worldMapStorage.GetObject(mapPosition, 0);
                if (!!@object && @object.Type.IsGround) {
                    CreateTextItem(TextResources.CTX_OBJECT_BROWSE_FIELD, () => new BrowseFieldActionImpl(m_Absolute).Perform());
                }
            }

            CreateSeparatorItem();

            if (m_Absolute.x != 65535 && OpenTibiaUnity.GameManager.ProtocolGame.BugReportsAllowed) {
                CreateTextItem(TextResources.CTX_OBJECT_REPORT_FIELD, () => {
                    // TODO: Report Coordinate
                });

                CreateSeparatorItem();
            }

            if (!!m_LookObject && m_Absolute.x == 65535 && m_Absolute.y >= 64 && OpenTibiaUnity.ContainerStorage.GetContainerView(m_Absolute.y - 64).IsSubContainer) {
                CreateTextItem(TextResources.CTX_OBJECT_MOVE_UP, () => {
                    if (!!m_LookObject)
                        GameActionFactory.CreateMoveAction(m_Absolute, m_LookObject, m_LookObjectStackPos,
                            new Vector3Int(m_Absolute.x, m_Absolute.y, 254), MoveActionImpl.MoveAll).Perform();
                });
            }

            if (!!m_LookObject && !m_LookObject.IsCreature && !m_LookObject.Type.IsUnmovable && m_LookObject.Type.IsPickupable) {
                CreateTextItem(TextResources.CTX_OBJECT_TRADE, () => {
                    // TODO
                    //if (!!m_LookObject)
                    //    new SafeTradeActionImpl(m_Absolute, m_LookObject, m_LookObjectStackPos).Perform();
                });
            }

            if (!!m_LookObject && !m_LookObject.IsCreature && m_LookObject.Type.IsMarket && player.IsInDepot) {
                CreateTextItem(TextResources.CTX_OBJECT_SHOW_IN_MARKET, () => {
                    // TODO, show in market
                });
            }

            if (gameManager.GetFeature(GameFeature.GameQuickLoot) && !!m_LookObject && !m_LookObject.IsCreature && !m_LookObject.Type.IsUnmovable && m_LookObject.Type.IsPickupable) {
                CreateSeparatorItem();

                if (m_LookObject.Type.IsContainer && m_Absolute.x == 65535) {
                    CreateTextItem(TextResources.CTX_OBJECT_MANAGE_LOOT_CONTAINERS, () => {
                        // TODO, manage loot containers
                    });
                } else {
                    CreateTextItem(TextResources.CTX_OBJECT_QUICK_LOOT, () => {
                        // TODO, quick loot
                    });
                }
            }
            
            if (gameManager.GetFeature(GameFeature.GameStash) && !!m_LookObject && !m_LookObject.IsCreature && !m_LookObject.Type.IsUnmovable && m_LookObject.Type.IsPickupable && (m_LookObject.Type.IsStackable || m_LookObject.Type.IsContainer)) {
                CreateSeparatorItem();

                if (m_LookObject.Type.IsStackable) {
                    CreateTextItem(TextResources.CTX_OBJECT_STOW, () => {
                        // TODO, stow
                    });

                    CreateTextItem(TextResources.CTX_OBJECT_STOW_ALL, () => {
                        // TODO, stow all
                    });
                } else {
                    CreateTextItem(TextResources.CTX_OBJECT_STOW_CONTENT, () => {
                        // TODO, stow all
                    });
                }
            }

            var creatureStorage = OpenTibiaUnity.CreatureStorage;
            if (!!m_Creature && m_Creature.ID != player.ID) {
                if (m_Creature.IsNPC) {
                    CreateTextItem(TextResources.CTX_CREATURE_TALK, () => {
                        GameActionFactory.CreateGreetAction(m_Creature).Perform();
                    });
                } else {
                    var attackTarget = creatureStorage.AttackTarget;
                    var text = (!attackTarget || attackTarget.ID != m_Creature.ID) ? TextResources.CTX_CREATURE_ATTACK_START : TextResources.CTX_CREATURE_ATTACK_STOP;
                    var shortcut = (isClassic || isRegular) ? "Alt" : null;

                    CreateTextItem(text, shortcut, () => {
                        GameActionFactory.CreateToggleAttackTargetAction(m_Creature, true).Perform();
                    });
                }

                var followTarget = creatureStorage.FollowTarget;
                CreateTextItem((!followTarget || followTarget.ID != m_Creature.ID) ? TextResources.CTX_CREATURE_FOLLOW_START : TextResources.CTX_CREATURE_FOLLOW_STOP, () => {
                    creatureStorage.ToggleFollowTarget(m_Creature, true);
                });

                if (m_Creature.IsConfirmedPartyMember) {
                    CreateTextItem(string.Format(TextResources.CTX_PARTY_JOIN_AGGRESSION, m_Creature.Name), () => {
                        new PartyActionImpl(PartyActionType.JoinAggression, m_Creature).Perform();
                    });
                }

                if (m_Creature.IsHuman) {
                    CreateTextItem(string.Format(TextResources.CTX_PLAYER_CHAT_MESSAGE, m_Creature.Name), () => {
                        new PrivateChatActionImpl(PrivateChatActionType.OpenMessageChannel, PrivateChatActionImpl.ChatChannelNoChannel, m_Creature.Name).Perform();
                    });

                    var chatStorage = OpenTibiaUnity.ChatStorage;
                    if (chatStorage.HasOwnPrivateChannel) {
                        CreateTextItem(TextResources.CTX_PLAYER_CHAT_INVITE, () => {
                            new PrivateChatActionImpl(PrivateChatActionType.ChatChannelInvite, chatStorage.OwnPrivateChannelID, m_Creature.Name).Perform();
                        });
                    }

                    //if (!BuddylistActionImpl.IsBuddy(this.m_Creature.ID)) {
                    //    CreateTextItem(TextResources.CTX_PLAYER_ADD_BUDDY, () => {
                    //        if (!!m_Creature)
                    //            new BuddylistActionImpl(BuddylistActionImpl.ADD_BY_NAME, m_Creature.name).Perform();
                    //    });
                    //}

                    //if (NameFilterActionImpl.s_IsBlacklisted(this.m_CreatureTarget.name)) {
                    //    CreateTextItem(string.Format(TextResources.CTX_PLAYER_UNIGNORE, m_Creature.Name), () => {
                    //        if (!!m_Creature)
                    //            new NameFilterActionImpl(NameFilterActionImpl.UNIGNORE, m_Creature.Name).Perform();
                    //    });
                    //} else {
                    //    CreateTextItem(string.Format(TextResources.CTX_PLAYER_IGNORE, m_Creature.Name), () => {
                    //        if (!!m_Creature)
                    //            new NameFilterActionImpl(NameFilterActionImpl.IGNORE, m_CreatureTarget.name).Perform();
                    //    });
                    //}

                    switch (player.PartyFlag) {
                        case PartyFlag.Leader:
                            CreateTextItem(TextResources.CTX_PARTY_EXCLUDE, () => new PartyActionImpl(PartyActionType.Exclude, m_Creature).Perform());
                            break;

                        case PartyFlag.Leader_SharedXP_Active:
                        case PartyFlag.Leader_SharedXP_Inactive_Guilty:
                        case PartyFlag.Leader_SharedXP_Inactive_Innocent:
                        case PartyFlag.Leader_SharedXP_Off:
                            switch (m_Creature.PartyFlag) {
                                case PartyFlag.Member:
                                    CreateTextItem(string.Format(TextResources.CTX_PARTY_EXCLUDE, m_Creature.Name), () => new PartyActionImpl(PartyActionType.Exclude, m_Creature).Perform());
                                    break;

                                case PartyFlag.Member_SharedXP_Active:
                                case PartyFlag.Member_SharedXP_Inactive_Guilty:
                                case PartyFlag.Member_SharedXP_Inactive_Innocent:
                                case PartyFlag.Member_SharedXP_Off:
                                    CreateTextItem(string.Format(TextResources.CTX_PARTY_PASS_LEADERSHIP, m_Creature.Name), () => new PartyActionImpl(PartyActionType.PassLeadership, m_Creature).Perform());
                                    break;

                                default:
                                    CreateTextItem(string.Format(TextResources.CTX_PARTY_INVITE, m_Creature.Name), () => new PartyActionImpl(PartyActionType.Invite, m_Creature).Perform());
                                    break;
                            }

                            break;

                        case PartyFlag.Member_SharedXP_Active:
                        case PartyFlag.Member_SharedXP_Inactive_Guilty:
                        case PartyFlag.Member_SharedXP_Inactive_Innocent:
                        case PartyFlag.Member_SharedXP_Off:
                            break;

                        case PartyFlag.None:
                        case PartyFlag.Member:
                            if (m_Creature.PartyFlag == PartyFlag.Leader)
                                CreateTextItem(string.Format(TextResources.CTX_PARTY_JOIN, m_Creature.Name), () => new PartyActionImpl(PartyActionType.Join, m_Creature).Perform());
                            else if (m_Creature.PartyFlag != PartyFlag.Other)
                                CreateTextItem(string.Format(TextResources.CTX_PARTY_INVITE, m_Creature.Name), () => new PartyActionImpl(PartyActionType.Invite, m_Creature).Perform());
                            break;
                    }

                    // Tibia 11 (CTX_INSPECT_CHARACTER)
                }

                CreateSeparatorItem();

                if (m_Creature.IsReportTypeAllowed(ReportTypes.Name)) {
                    CreateTextItem(TextResources.CTX_PLAYER_REPORT_NAME, () => {
                        // TODO
                        //if (!!m_Creature)
                        //    new ReportWidget(ReportTypes.Name, m_Creature).show();
                    });
                }

                if (m_Creature.IsReportTypeAllowed(ReportTypes.Bot)) {
                    CreateTextItem(TextResources.CTX_PLAYER_REPORT_BOT, () => {
                        // TODO
                        //if (!!m_Creature)
                        //    new ReportWidget(ReportTypes.Bot, m_Creature).show();
                    });
                }
            }

            CreateSeparatorItem();

            if (!!m_Creature && m_Creature.ID == player.ID) {
                CreateTextItem(TextResources.CTX_PLAYER_SET_OUTFIT, () => StaticActionList.MiscShowOutfit.Perform());
                CreateTextItem(!!player.MountOutfit ? TextResources.CTX_PLAYER_DISMOUNT : TextResources.CTX_PLAYER_MOUNT, () => StaticActionList.PlayerMount.Perform());
                //CreateTextItem(TextResources.CTX_PLAYER_OPEN_PREY_DIALOG, () => StaticActionList.MiscShowPreyDialog.Perform());

                if (player.IsPartyLeader && !player.IsFighting) {
                    if (player.IsPartySharedExperienceActive)
                        CreateTextItem(string.Format(TextResources.CTX_PARTY_DISABLE_SHARED_EXPERIENCE, m_Creature.Name), () => new PartyActionImpl(PartyActionType.DisableSharedExperience, m_Creature).Perform());
                    else
                        CreateTextItem(string.Format(TextResources.CTX_PARTY_ENABLE_SHARED_EXPERIENCE, m_Creature.Name), () => new PartyActionImpl(PartyActionType.EnableSharedExperience, m_Creature).Perform());
                }


                if (player.IsPartyMember && !player.IsFighting) {
                    CreateTextItem(TextResources.CTX_PARTY_LEAVE, () => new PartyActionImpl(PartyActionType.Leave, m_Creature).Perform());
                }

                // Tibia 11 (CTX_INSPECT_CHARACTER)
            }

            CreateSeparatorItem();

            if (!!m_Creature) {
                CreateTextItem(TextResources.CTX_CREATURE_COPY_NAME, () => {
                    if (!!m_Creature)
                        GUIUtility.systemCopyBuffer = m_Creature.Name;
                });
            }
        }
    }
}
