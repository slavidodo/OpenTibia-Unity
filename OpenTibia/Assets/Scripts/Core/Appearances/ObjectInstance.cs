using UnityEngine;

using CommandBuffer = UnityEngine.Rendering.CommandBuffer;

namespace OpenTibiaUnity.Core.Appearances
{
    public class ObjectInstance : AppearanceInstance
    {
        protected int _hang = 0;
        protected int _specialPatternX = 0;
        protected int _specialPatternY = 0;
        protected Marks _marks = new Marks();
        protected uint _data;
        protected bool _hasSpecialPattern = false;

        public uint Data {
            get { return _data; }
            set { if (_data != value) { _data = value; UpdateSpecialPattern(); } }
        }

        public int Hang {
            get { return _hang; }
            set { if (_hang != value) { _hang = value; UpdateSpecialPattern(); } }
        }

        public Marks Marks {
            get { return _marks; }
        }

        public bool HasMark {
            get { return !!Marks && Marks.IsMarkSet(MarkType.Permenant); }
        }

        public bool IsCreature {
            get { return !!Type ? Type.IsCreature : false; }
        }

        public ObjectInstance(uint id, AppearanceType type, uint data) : base(id, type) {
            _data = data;
            UpdateSpecialPattern();
        }

        public override int GetSpriteIndex(int layer, int patternX, int patternY, int patternZ) {
            patternX = _specialPatternX > 0 ? _specialPatternX : patternX;
            patternY = _specialPatternY > 0 ? _specialPatternY : patternY;
            return base.GetSpriteIndex(layer, patternX, patternY, patternZ);
        }

        public override void Draw(CommandBuffer commandBuffer, Vector2Int screenPosition, int patternX, int patternY, int patternZ, bool highlighted = false, float highlightOpacity = 0) {
            if (_hasSpecialPattern) {
                patternX = -1;
                patternY = -1;
            }
            
            base.Draw(commandBuffer, screenPosition, patternX, patternY, patternZ, highlighted, highlightOpacity);
        }

        public override AppearanceInstance Clone() {
            return new ObjectInstance(Id, Type, Data);
        }

        protected void UpdateSpecialPattern() {
            _hasSpecialPattern = false;
            if (!_type || _type.IsCreature)
                return;

            if (_type.IsStackable) {
                _hasSpecialPattern = true;
                if (_data < 2) {
                    _specialPatternX = 0;
                    _specialPatternY = 0;
                } else if (_data == 2) {
                    _specialPatternX = 1;
                    _specialPatternY = 0;
                } else if (_data == 3) {
                    _specialPatternX = 2;
                    _specialPatternY = 0;
                } else if (_data == 4) {
                    _specialPatternX = 3;
                    _specialPatternY = 0;
                } else if (_data < 10) {
                    _specialPatternX = 0;
                    _specialPatternY = 1;
                } else if (_data < 25) {
                    _specialPatternX = 1;
                    _specialPatternY = 1;
                } else if (_data < 50) {
                    _specialPatternX = 2;
                    _specialPatternY = 1;
                } else {
                    _specialPatternX = 3;
                    _specialPatternY = 1;
                }

                _specialPatternX %= (int)_type.FrameGroups[(int)Protobuf.Shared.FrameGroupType.Idle].SpriteInfo.PatternWidth;
                _specialPatternY %= (int)_type.FrameGroups[(int)Protobuf.Shared.FrameGroupType.Idle].SpriteInfo.PatternHeight;
            } else if (_type.IsSplash || _type.IsFluidContainer) {
                _hasSpecialPattern = true;
                FluidColor color;
                if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameNewFluids)) {
                    switch ((FluidType)_data) {
                        case FluidType.None:
                            color = FluidColor.Transparent;
                            break;
                        case FluidType.Water:
                            color = FluidColor.Blue;
                            break;
                        case FluidType.Mana:
                            color = FluidColor.Purple;
                            break;
                        case FluidType.Beer:
                            color = FluidColor.Brown;
                            break;
                        case FluidType.Oil:
                            color = FluidColor.Brown;
                            break;
                        case FluidType.Blood:
                            color = FluidColor.Red;
                            break;
                        case FluidType.Slime:
                            color = FluidColor.Green;
                            break;
                        case FluidType.Mud:
                            color = FluidColor.Brown;
                            break;
                        case FluidType.Lemonade:
                            color = FluidColor.Yellow;
                            break;
                        case FluidType.Milk:
                            color = FluidColor.White;
                            break;
                        case FluidType.Wine:
                            color = FluidColor.Purple;
                            break;
                        case FluidType.Health:
                            color = FluidColor.Red;
                            break;
                        case FluidType.Urine:
                            color = FluidColor.Yellow;
                            break;
                        case FluidType.Rum:
                            color = FluidColor.Brown;
                            break;
                        case FluidType.FruidJuice:
                            color = FluidColor.Yellow;
                            break;
                        case FluidType.CoconutMilk:
                            color = FluidColor.White;
                            break;
                        case FluidType.Tea:
                            color = FluidColor.Brown;
                            break;
                        case FluidType.Mead:
                            color = FluidColor.Brown;
                            break;
                        default:
                            color = FluidColor.Blue;
                            break;
                    }
                } else {
                    color = (FluidColor)_data;
                }
                
                _specialPatternX = ((int)color & 3) % (int)_type.FrameGroups[(int)Protobuf.Shared.FrameGroupType.Idle].SpriteInfo.PatternWidth;
                _specialPatternY = ((int)color >> 2) % (int)_type.FrameGroups[(int)Protobuf.Shared.FrameGroupType.Idle].SpriteInfo.PatternHeight;
            } else if (_type.IsHangable) {
                _hasSpecialPattern = true;
                if (_hang == AppearanceInstance.HookSouth) {
                    _specialPatternX = _type.FrameGroups[(int)Protobuf.Shared.FrameGroupType.Idle].SpriteInfo.PatternWidth >= 2 ? 1 : 0;
                    _specialPatternY = 0;
                } else if (_hang == AppearanceInstance.HookEast) {
                    _specialPatternX = _type.FrameGroups[(int)Protobuf.Shared.FrameGroupType.Idle].SpriteInfo.PatternWidth >= 3 ? 2 : 0;
                    _specialPatternY = 0;
                } else {
                    _specialPatternX = 0;
                    _specialPatternY = 0;
                }
            }
        }
    }
}
