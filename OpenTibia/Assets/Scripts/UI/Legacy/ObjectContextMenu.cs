using OpenTibiaUnity.Core.Appearances;
using OpenTibiaUnity.Core.Creatures;
using OpenTibiaUnity.Core.Input.GameAction;
using OpenTibiaUnity.Core.Input.StaticAction;
using UnityEngine;

namespace OpenTibiaUnity.UI.Legacy
{
    public class ObjectContextMenu : ContextMenuBase 
    {
        private Vector3Int _absolute;

        private ObjectInstance _lookObject;
        private ObjectInstance _useObject;
        private Creature _creature;

        private int _lookObjectStackPos;
        private int _useObjectStackPos;

        public void Set(Vector3Int absolutePosition, ObjectInstance lookObject, int lookObjectIndex, ObjectInstance useObject, int useObjectIndex, Creature creature) {
            _absolute = absolutePosition;
            _lookObject = lookObject;
            _lookObjectStackPos = lookObjectIndex;

            _useObject = useObject;
            _useObjectStackPos = useObjectIndex;

            _creature = creature;
        }

        public override void InitialiseOptions() {
            var gameManager = OpenTibiaUnity.GameManager;
            var optionStorage = OpenTibiaUnity.OptionStorage;
            var player = OpenTibiaUnity.Player;
            bool isClassic = optionStorage.MousePreset == MousePresets.Classic;
            bool isRegular = optionStorage.MousePreset == MousePresets.Regular;
            bool isLeftSmartClick = optionStorage.MousePreset == MousePresets.LeftSmartClick;

            if (!!_lookObject) {
                CreateTextItem(TextResources.CTX_OBJECT_LOOK, "Shift", () => {
                    if (!!_lookObject)
                        new LookActionImpl(_absolute, _lookObject, _lookObjectStackPos).Perform();
                });
            }

            // Tibia 11 (CTX_OBJECT_INSPECT_OBJECT)

            if (!!_useObject && (_useObject.Type.IsContainer || _useObject.Type.DefaultAction == Protobuf.Shared.PlayerAction.Open)) {
                if (_absolute.x == 65535 && _absolute.y >= 64) {
                    CreateTextItem(TextResources.CTX_OBJECT_OPEN, () => {
                        if (!!_useObject)
                            new UseActionImpl(_absolute, _useObject, _useObjectStackPos, Vector3Int.zero, null, 0, UseActionTarget.Auto).Perform();
                    });

                    CreateTextItem(TextResources.CTX_OBJECT_OPEN_NEW_WINDOW, () => {
                        if (!!_useObject)
                            new UseActionImpl(_absolute, _useObject, _useObjectStackPos, Vector3Int.zero, null, 0, UseActionTarget.NewWindow).Perform();
                    });
                } else {
                    CreateTextItem(TextResources.CTX_OBJECT_OPEN, () => {
                        if (!!_useObject)
                            new UseActionImpl(_absolute, _useObject, _useObjectStackPos, Vector3Int.zero, null, 0, UseActionTarget.NewWindow).Perform();
                    });
                }
            }

            if (!!_useObject && !_useObject.Type.IsContainer) {
                string text, shortcut;
                if (_useObject.Type.IsMultiUse) {
                    text = TextResources.CTX_OBJECT_MULTIUSE;
                    shortcut = isClassic ? "Alt" : (isRegular ? "Ctrl" : null);
                } else {
                    text = TextResources.CTX_OBJECT_USE;
                    shortcut = isRegular ? "Ctrl" : null;
                }

                CreateTextItem(text, shortcut, () => {
                    if (!!_useObject) {
                        if (!_useObject.Type.IsMultiUse)
                            new UseActionImpl(_absolute, _useObject, _useObjectStackPos, Vector3Int.zero, null, 0, UseActionTarget.Auto).Perform();
                    }
                });
            }

            if (!!_useObject && _useObject.Type.IsWrappable) {
                CreateTextItem(TextResources.CTX_OBJECT_WRAP, () => {
                    if (!!_useObject)
                        new ToggleWrapStateActionImpl(_absolute, _useObject, _useObjectStackPos).Perform();
                });
            }

            if (!!_useObject && _useObject.Type.IsUnwrappable) {
                CreateTextItem(TextResources.CTX_OBJECT_UNWRAP, () => {
                    if (!!_useObject)
                        new ToggleWrapStateActionImpl(_absolute, _useObject, _useObjectStackPos).Perform();
                });
            }

            if (!!_useObject && _useObject.Type.IsRotateable) {
                CreateTextItem(TextResources.CTX_OBJECT_TURN, () => {
                    if (!!_useObject)
                        new TurnActionImpl(_absolute, _useObject, _useObjectStackPos).Perform();
                });
            }
            
            if (gameManager.ClientVersion >= 984 && _absolute.x != 65535) {
                var worldMapStorage = OpenTibiaUnity.WorldMapStorage;
                var mapPosition = worldMapStorage.ToMap(_absolute);
                var @object = worldMapStorage.GetObject(mapPosition, 0);
                if (!!@object && @object.Type.IsGround) {
                    CreateTextItem(TextResources.CTX_OBJECT_BROWSE_FIELD, () => new BrowseFieldActionImpl(_absolute).Perform());
                }
            }

            CreateSeparatorItem();

            if (_absolute.x != 65535 && OpenTibiaUnity.GameManager.ProtocolGame.BugReportsAllowed) {
                CreateTextItem(TextResources.CTX_OBJECT_REPORT_FIELD, () => {
                    // TODO: Report Coordinate
                });

                CreateSeparatorItem();
            }

            if (!!_lookObject && _absolute.x == 65535 && _absolute.y >= 64 && OpenTibiaUnity.ContainerStorage.GetContainerView(_absolute.y - 64).IsSubContainer) {
                CreateTextItem(TextResources.CTX_OBJECT_MOVE_UP, () => {
                    if (!!_lookObject)
                        new MoveActionImpl(_absolute, _lookObject, _lookObjectStackPos,
                            new Vector3Int(_absolute.x, _absolute.y, 254), MoveActionImpl.MoveAll).Perform();
                });
            }

            if (!!_lookObject && !_lookObject.IsCreature && !_lookObject.Type.IsUnmovable && _lookObject.Type.IsPickupable) {
                CreateTextItem(TextResources.CTX_OBJECT_TRADE, () => {
                    // TODO
                    //if (!!_lookObject)
                    //    new SafeTradeActionImpl(_absolute, _lookObject, _lookObjectStackPos).Perform();
                });
            }

            if (!!_lookObject && !_lookObject.IsCreature && _lookObject.Type.IsMarket && player.IsInDepot) {
                CreateTextItem(TextResources.CTX_OBJECT_SHOW_IN_MARKET, () => {
                    // TODO, show in market
                });
            }

            if (gameManager.GetFeature(GameFeature.GameQuickLoot) && !!_lookObject && !_lookObject.IsCreature && !_lookObject.Type.IsUnmovable && _lookObject.Type.IsPickupable) {
                CreateSeparatorItem();

                if (_lookObject.Type.IsContainer && _absolute.x == 65535) {
                    CreateTextItem(TextResources.CTX_OBJECT_MANAGE_LOOT_CONTAINERS, () => {
                        // TODO, manage loot containers
                    });
                } else {
                    CreateTextItem(TextResources.CTX_OBJECT_QUICK_LOOT, () => {
                        // TODO, quick loot
                    });
                }
            }
            
            if (gameManager.GetFeature(GameFeature.GameStash) && !!_lookObject && !_lookObject.IsCreature && !_lookObject.Type.IsUnmovable && _lookObject.Type.IsPickupable && (_lookObject.Type.IsStackable || _lookObject.Type.IsContainer)) {
                CreateSeparatorItem();

                if (_lookObject.Type.IsStackable) {
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
            if (!!_creature && _creature.Id != player.Id) {
                if (_creature.IsNPC) {
                    CreateTextItem(TextResources.CTX_CREATURE_TALK, () => {
                        new GreetAction(_creature).Perform();
                    });
                } else {
                    var attackTarget = creatureStorage.AttackTarget;
                    var text = (!attackTarget || attackTarget.Id != _creature.Id) ? TextResources.CTX_CREATURE_ATTACK_START : TextResources.CTX_CREATURE_ATTACK_STOP;
                    var shortcut = (isClassic || isRegular) ? "Alt" : null;

                    CreateTextItem(text, shortcut, () => {
                        new ToggleAttackTargetActionImpl(_creature, true).Perform();
                    });
                }

                var followTarget = creatureStorage.FollowTarget;
                CreateTextItem((!followTarget || followTarget.Id != _creature.Id) ? TextResources.CTX_CREATURE_FOLLOW_START : TextResources.CTX_CREATURE_FOLLOW_STOP, () => {
                    creatureStorage.ToggleFollowTarget(_creature, true);
                });

                if (_creature.IsConfirmedPartyMember) {
                    CreateTextItem(string.Format(TextResources.CTX_PARTY_JOIN_AGGRESSION, _creature.Name), () => {
                        new PartyActionImpl(PartyActionType.JoinAggression, _creature).Perform();
                    });
                }

                if (_creature.IsHuman) {
                    CreateTextItem(string.Format(TextResources.CTX_PLAYER_CHAT_MESSAGE, _creature.Name), () => {
                        new PrivateChatActionImpl(PrivateChatActionType.OpenMessageChannel, PrivateChatActionImpl.ChatChannelNoChannel, _creature.Name).Perform();
                    });

                    var chatStorage = OpenTibiaUnity.ChatStorage;
                    if (chatStorage.HasOwnPrivateChannel) {
                        CreateTextItem(TextResources.CTX_PLAYER_CHAT_INVITE, () => {
                            new PrivateChatActionImpl(PrivateChatActionType.ChatChannelInvite, chatStorage.OwnPrivateChannelId, _creature.Name).Perform();
                        });
                    }

                    //if (!BuddylistActionImpl.IsBuddy(this._creature._id)) {
                    //    CreateTextItem(TextResources.CTX_PLAYER_ADD_BUDDY, () => {
                    //        if (!!_creature)
                    //            new BuddylistActionImpl(BuddylistActionImpl.ADD_BY_NAME, _creature.name).Perform();
                    //    });
                    //}

                    //if (NameFilterActionImpl.s_IsBlacklisted(this._creatureTarget.name)) {
                    //    CreateTextItem(string.Format(TextResources.CTX_PLAYER_UNIGNORE, _creature.Name), () => {
                    //        if (!!_creature)
                    //            new NameFilterActionImpl(NameFilterActionImpl.UNIGNORE, _creature.Name).Perform();
                    //    });
                    //} else {
                    //    CreateTextItem(string.Format(TextResources.CTX_PLAYER_IGNORE, _creature.Name), () => {
                    //        if (!!_creature)
                    //            new NameFilterActionImpl(NameFilterActionImpl.IGNORE, _creatureTarget.name).Perform();
                    //    });
                    //}

                    switch (player.PartyFlag) {
                        case PartyFlag.Leader:
                            CreateTextItem(TextResources.CTX_PARTY_EXCLUDE, () => new PartyActionImpl(PartyActionType.Exclude, _creature).Perform());
                            break;

                        case PartyFlag.Leader_SharedXP_Active:
                        case PartyFlag.Leader_SharedXP_Inactive_Guilty:
                        case PartyFlag.Leader_SharedXP_Inactive_Innocent:
                        case PartyFlag.Leader_SharedXP_Off:
                            switch (_creature.PartyFlag) {
                                case PartyFlag.Member:
                                    CreateTextItem(string.Format(TextResources.CTX_PARTY_EXCLUDE, _creature.Name), () => new PartyActionImpl(PartyActionType.Exclude, _creature).Perform());
                                    break;

                                case PartyFlag.Member_SharedXP_Active:
                                case PartyFlag.Member_SharedXP_Inactive_Guilty:
                                case PartyFlag.Member_SharedXP_Inactive_Innocent:
                                case PartyFlag.Member_SharedXP_Off:
                                    CreateTextItem(string.Format(TextResources.CTX_PARTY_PASS_LEADERSHIP, _creature.Name), () => new PartyActionImpl(PartyActionType.PassLeadership, _creature).Perform());
                                    break;

                                default:
                                    CreateTextItem(string.Format(TextResources.CTX_PARTY_INVITE, _creature.Name), () => new PartyActionImpl(PartyActionType.Invite, _creature).Perform());
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
                            if (_creature.PartyFlag == PartyFlag.Leader)
                                CreateTextItem(string.Format(TextResources.CTX_PARTY_JOIN, _creature.Name), () => new PartyActionImpl(PartyActionType.Join, _creature).Perform());
                            else if (_creature.PartyFlag != PartyFlag.Other)
                                CreateTextItem(string.Format(TextResources.CTX_PARTY_INVITE, _creature.Name), () => new PartyActionImpl(PartyActionType.Invite, _creature).Perform());
                            break;
                    }

                    // Tibia 11 (CTX_INSPECT_CHARACTER)
                }

                CreateSeparatorItem();

                if (_creature.IsReportTypeAllowed(ReportTypes.Name)) {
                    CreateTextItem(TextResources.CTX_PLAYER_REPORT_NAME, () => {
                        // TODO
                        //if (!!_creature)
                        //    new ReportWidget(ReportTypes.Name, _creature).show();
                    });
                }

                if (_creature.IsReportTypeAllowed(ReportTypes.Bot)) {
                    CreateTextItem(TextResources.CTX_PLAYER_REPORT_BOT, () => {
                        // TODO
                        //if (!!_creature)
                        //    new ReportWidget(ReportTypes.Bot, _creature).show();
                    });
                }
            }

            CreateSeparatorItem();

            if (!!_creature && _creature.Id == player.Id) {
                CreateTextItem(TextResources.CTX_PLAYER_SET_OUTFIT, () => StaticActionList.MiscShowOutfit.Perform());
                CreateTextItem(!!player.MountOutfit ? TextResources.CTX_PLAYER_DISMOUNT : TextResources.CTX_PLAYER_MOUNT, () => StaticActionList.PlayerMount.Perform());
                //CreateTextItem(TextResources.CTX_PLAYER_OPEN_PREY_DIALOG, () => StaticActionList.MiscShowPreyDialog.Perform());

                if (player.IsPartyLeader && !player.IsFighting) {
                    if (player.IsPartySharedExperienceActive)
                        CreateTextItem(string.Format(TextResources.CTX_PARTY_DISABLE_SHARED_EXPERIENCE, _creature.Name), () => new PartyActionImpl(PartyActionType.DisableSharedExperience, _creature).Perform());
                    else
                        CreateTextItem(string.Format(TextResources.CTX_PARTY_ENABLE_SHARED_EXPERIENCE, _creature.Name), () => new PartyActionImpl(PartyActionType.EnableSharedExperience, _creature).Perform());
                }


                if (player.IsPartyMember && !player.IsFighting) {
                    CreateTextItem(TextResources.CTX_PARTY_LEAVE, () => new PartyActionImpl(PartyActionType.Leave, _creature).Perform());
                }

                // Tibia 11 (CTX_INSPECT_CHARACTER)
            }

            CreateSeparatorItem();

            if (!!_creature) {
                CreateTextItem(TextResources.CTX_CREATURE_COPY_NAME, () => {
                    if (!!_creature)
                        GUIUtility.systemCopyBuffer = _creature.Name;
                });
            }
        }
    }
}
