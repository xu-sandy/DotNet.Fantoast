using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FCake.Core.Common
{
    public class DropdownItem
    {
        public DropdownItem() { }
        public DropdownItem(string value, string text) {
            this.Value = value;
            this.Text = text;
        }
        public DropdownItem(string value, object text)
        {
            this.Value = value;
            if(text!=null)
                this.Text = text.ToString();
        }

        public string Value { get; set; }
        public string Text { get; set; }
        public double? Sorting { get; set; }
    }
}
