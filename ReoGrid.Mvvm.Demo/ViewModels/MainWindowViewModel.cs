using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using ReoGrid.Mvvm.Demo.Models;
using ReoGrid.Mvvm.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Unity;
using unvell.ReoGrid;

namespace ReoGrid.Mvvm.Demo.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        #region [Fields]
        private string _title = "ReoGrid.Mvvm.Demo";

        private ObservableCollection<IRecordModel> _Books;
        private WorksheetModel _WorksheetModel;
        #endregion

        #region [Properties]

        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public DelegateCommand<ReoGridControl> LoadedCommand { get; set; }
        public DelegateCommand AddRecordCommand    { get; set; }
        public DelegateCommand DeleteRecordCommand { get; set; }
        public DelegateCommand MoveRecordCommand   { get; set; }
        public DelegateCommand EditRecordCommand   { get; set; }
        public DelegateCommand GetFromUiCommand    { get; set; }

        #endregion


        public MainWindowViewModel(IRegionManager regionManager)
        {
            InitBooks();
            InitCommands();
        }

        private void InitBooks()
        {
            _Books = new ObservableCollection<IRecordModel>();
            for (int i = 0; i < 10; i++)
            {
                Book book = new Book();
                book.Id = i;
                book.Title = string.Format("Title {0}", i);
                book.Author = string.Format("Author {0}", i);
                book.BindingType = BindingType.Hardback;
                book.Price = (decimal)(i * 10.1);
                book.Pubdate = DateTime.Now;
                _Books.Add(book);
            }
        }

        private void InitCommands()
        {
            LoadedCommand = new DelegateCommand<ReoGridControl>(OnLoadedCommand);
            AddRecordCommand = new DelegateCommand(OnAddRecordCommand);
            DeleteRecordCommand = new DelegateCommand(OnDeleteRecordCommand);
            MoveRecordCommand = new DelegateCommand(OnMoveRecordCommand);
            EditRecordCommand = new DelegateCommand(OnEditRecordCommand);
            GetFromUiCommand = new DelegateCommand(OnGetFromUiCommand);
        }

        private void OnLoadedCommand(ReoGridControl reoGridControl)
        {
            _WorksheetModel = new WorksheetModel(reoGridControl, typeof(Book), _Books);
        }

        private void OnAddRecordCommand()
        {
            int count = _Books.Count;
            Book book = new Book();
            book.Id = count;
            book.Title = string.Format("Title {0}", count);
            book.Author = string.Format("Author {0}", count);
            book.BindingType = BindingType.Hardback;
            book.Price = (decimal)(count * 10.11);
            book.Pubdate = DateTime.Now;
            _Books.Add(book);
        }

        private void OnDeleteRecordCommand()
        {
            if (_Books.Count > 0)
            {
                _Books.RemoveAt(_Books.Count - 1);
            }
        }

        private void OnMoveRecordCommand()
        {
            if (_Books.Count > 2)
            {
                _Books.Move(0, _Books.Count - 1);
            }
        }

        private void OnEditRecordCommand()
        {
            (_Books[0] as Book).Price += 3.33m;
            _WorksheetModel.UpadteRecord(_Books[0]); // invoke UpadteRecord after editing one record.
        }

        private void OnGetFromUiCommand()
        {
            string result = string.Empty;
            foreach (Book book in _Books)
            {
                result += string.Format("Id:{0}\t Title:{1}\t Author:{2}\t BindingType:{3}\t Price:{4}\t Pubdate:{5}\t RowIndex:{6}\r\n",
                    book.Id, book.Title, book.Author, book.BindingType, book.Price, book.Pubdate, book.RowIndex);
            }

            Window window = new Window();
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.WindowStyle = WindowStyle.ToolWindow;
            window.SizeToContent = SizeToContent.WidthAndHeight;
            TextBlock textBlock = new TextBlock();
            textBlock.Margin = new Thickness(10);
            textBlock.Text = result;
            window.Content = textBlock;
            window.ShowDialog();
        }
    }
}
