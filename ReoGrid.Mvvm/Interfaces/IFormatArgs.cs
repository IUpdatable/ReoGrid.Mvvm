using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using unvell.ReoGrid.DataFormat;

namespace ReoGrid.Mvvm.Interfaces
{
    public interface IFormatArgs
    {
        CellDataFormatFlag CellDataFormatFlag { get; }
    }
}
