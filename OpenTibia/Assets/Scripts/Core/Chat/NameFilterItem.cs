using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTibiaUnity.Core.Chat
{
    public struct NameFilterItem
    {
        private string m_Pattern;
        private bool m_Permenant;

        public string Pattern {
            get => m_Pattern;
            set {
                if (value != null)
                    value = value.Trim();

                if (value != null && value.Length > 0)
                    m_Pattern = value;
                else
                    m_Pattern = null;
            }
        }

        public bool Permenant { get => m_Permenant; }

        public NameFilterItem(string pattern, bool permenant) {
            m_Pattern = pattern;
            m_Permenant = permenant;
        }

        public NameFilterItem Clone() {
            return new NameFilterItem(m_Pattern, m_Permenant);
        }

    }
}
