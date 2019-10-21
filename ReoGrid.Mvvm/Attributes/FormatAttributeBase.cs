using ReoGrid.Mvvm.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using unvell.ReoGrid.DataFormat;

namespace ReoGrid.Mvvm.Attributes
{
    public abstract class FormatAttributeBase : Attribute, IFormatArgs
    {
        public abstract CellDataFormatFlag CellDataFormatFlag { get; }
    }
}
