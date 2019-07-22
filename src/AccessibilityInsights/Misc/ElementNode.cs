using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessibilityInsights.Misc
{
    public class ElementNode
    {
        public string Name { get; set; }
        public string ControlType { get; set; } // Localized, or fallback to control type
        public IEnumerable<ElementNode> Children { get; set; }
    }
}
