using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReoGrid.Mvvm;
using ReoGrid.Mvvm.Attributes;
using ReoGrid.Mvvm.Interfaces;
using unvell.ReoGrid.DataFormat;

namespace ReoGrid.Mvvm.Demo.Models
{
    [WorksheetAttribute(Title = "Books")]
    public class Book: IRecordModel
    {
        [ColumnHeader(Index = 10, IsVisible = false)]
        public int Id { get; set; }

        [ColumnHeader(Index = 20, Text = "Name", Width = 150)]
        public string Title { get; set; }

        [ColumnHeader(Index = 30)]
        public string Author { get; set; }

        [ColumnHeader(Index = 35, Text = "Type")]
        public BindingType BindingType { get; set; }

        [ColumnHeader(Index = 36, Text = "OnSale")]
        public bool IsOnSale { get; set; }

        [NumberFormat(DecimalPlaces = 2, NegativeStyle = NumberDataFormatter.NumberNegativeStyle.Prefix_Sankaku, UseSeparator = true)]
        [ColumnHeader(Index = 40)]
        public decimal Price { get; set; }

        [DateTimeFormat( CultureName = "en-US")]
        [ColumnHeader(Index = 45, Text = "Publish Date", Width = 200)]
        public DateTime Pubdate { get; set; }

        public int RowIndex { get; set; }
    }

    public enum BindingType
    {
        Hardcover,
        Hardback
    }
}
