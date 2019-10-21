using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using unvell.ReoGrid.DataFormat;

namespace ReoGrid.Mvvm.Attributes
{
    public class DateTimeFormatAttribute: FormatAttributeBase
    {
        public override CellDataFormatFlag CellDataFormatFlag
        {
            get
            {
                return CellDataFormatFlag.DateTime;
            }
        }
        /// <summary>
        /// Get or set the date time pattern. (Standard .NET datetime pattern is supported, e.g.: yyyy/MM/dd)
        /// </summary>
        public string  Format { get; set; }

        /// <summary>
        /// Get or set the culture name that is used to format datetime according to localization settings.
        /// e.g.:en-US
        /// </summary>
        public string CultureName { get; set; }
    }
}
