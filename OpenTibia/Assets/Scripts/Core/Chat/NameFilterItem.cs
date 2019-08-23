using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTibiaUnity.Core.Chat
{
    public struct NameFilterItem
    {
        private string _pattern;
        private bool _permenant;

        public string Pattern {
            get => _pattern;
            set {
                if (value != null)
                    value = value.Trim();

                if (value != null && value.Length > 0)
                    _pattern = value;
                else
                    _pattern = null;
            }
        }

        public bool Permenant { get => _permenant; }

        public NameFilterItem(string pattern, bool permenant) {
            _pattern = pattern;
            _permenant = permenant;
        }

        public NameFilterItem Clone() {
            return new NameFilterItem(_pattern, _permenant);
        }

    }
}
