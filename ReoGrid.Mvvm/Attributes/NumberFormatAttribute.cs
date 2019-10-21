using ReoGrid.Mvvm.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using unvell.ReoGrid.DataFormat;
using static unvell.ReoGrid.DataFormat.NumberDataFormatter;

namespace ReoGrid.Mvvm.Attributes
{
    public class NumberFormatAttribute: FormatAttributeBase
    {
        public override CellDataFormatFlag CellDataFormatFlag {
            get{
                return CellDataFormatFlag.Number;
            }
        }

        /// <summary>
        /// Get or set the digis places for number.
        /// </summary>
        public short DecimalPlaces { get; set; }

        /// <summary>
        /// Get or set the negative number styles.
        /// </summary>
        public NumberNegativeStyle NegativeStyle { get; set; }

        /// <summary>
        /// Determines that whether or not show the separators in numbers.
        /// </summary>
        public bool UseSeparator { get; set; }

        /// <summary>
        /// Prefix symbol before negative numbers. (Requires that NegativeStyle set to Custom)
        /// </summary>
        public string CustomNegativePrefix { get; set; }

        /// <summary>
        /// Postfix symbol after negative numbers. (Requires that NegativeStyle set to Custom)
        /// </summary>
        public string CustomNegativePostfix { get; set; }

        public NumberFormatAttribute()
        {
            DecimalPlaces = short.MaxValue;
            NegativeStyle = NumberNegativeStyle.Default;
            UseSeparator = false;
            CustomNegativePrefix = null;
            CustomNegativePostfix = null;
        }
    }
}
