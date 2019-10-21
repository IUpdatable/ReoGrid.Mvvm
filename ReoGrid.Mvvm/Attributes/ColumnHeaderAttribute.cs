using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReoGrid.Mvvm.Attributes
{
    /// <summary>
    /// attribute of column header
    /// </summary>
    public class ColumnHeaderAttribute: Attribute
    {
        /// <summary>
        /// Sorting in the column, not the exact index position
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// Header Text in column, default is property name.
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// Whether to display in the worksheet, default is true
        /// </summary>
        public bool IsVisible { get; set; }
        /// <summary>
        /// column width, default is AutoWidth
        /// if not set, the length of Text should shorter than model property.
        /// </summary>
        public int Width { get; set; }

        public ColumnHeaderAttribute()
        {
            IsVisible = true;
            Width = -1;
        }
    }
}
