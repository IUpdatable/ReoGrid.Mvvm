using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReoGrid.Mvvm.Interfaces
{
    /// <summary>
    /// The interface of record model
    /// </summary>
    public interface IRecordModel
    {
        /// <summary>
        /// index of row
        /// </summary>
        int RowIndex { get; set; }
    }
}
