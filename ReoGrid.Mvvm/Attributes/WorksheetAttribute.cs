using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReoGrid.Mvvm.Attributes
{
    /// <summary>
    /// Worksheet Attribute
    /// </summary>
    public class WorksheetAttribute: Attribute
    {
        /// <summary>
        /// worksheet title, default is the name of model class 
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// count of rows you might want to use, default is 100
        /// </summary>
        public int RowCount { get; set; }

        /// <summary>
        /// Unique id of worksheet
        /// </summary>
        public string Id { get; private set; }

        public WorksheetAttribute()
        {
            Id = Guid.NewGuid().ToString();
            Title = string.Empty;
            RowCount = 100;
        }
    }
}
