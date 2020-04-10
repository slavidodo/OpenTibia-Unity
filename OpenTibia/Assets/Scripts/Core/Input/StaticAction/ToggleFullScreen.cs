using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTibiaUnity.Core.Input.StaticAction
{
    public class ToggleFullScreen : StaticAction
    {
        public ToggleFullScreen(int id, string label, InputEvent eventMask) : base(id, label, eventMask, false) { }

        public override bool Perform(bool repeat = false) {
            var optionStorage = OpenTibiaUnity.OptionStorage;
            optionStorage.FullscreenMode = !optionStorage.FullscreenMode;

            optionStorage.UpdateFullscreenMode();
            return true;
        }

        public override IAction Clone() {
            return new ToggleFullScreen(_id, _label, _eventMask);
        }
    }
}
