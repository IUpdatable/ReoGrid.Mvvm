using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using unvell.ReoGrid;
using unvell.ReoGrid.Events;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using unvell.ReoGrid.DataFormat;
using ReoGrid.Mvvm.Attributes;
using ReoGrid.Mvvm.Interfaces;
using System.Windows.Forms.Integration;
using unvell.ReoGrid.Actions;

namespace ReoGrid.Mvvm
{
    public class WorksheetModel
    {

        #region [Fields]

        private ReoGridControl _ReoGridControl;
        private unvell.ReoGrid.Worksheet _Worksheet;
        private Type _RecordModelType;
        private ObservableCollection<IRecordModel> _Records;
        private List<int> _ColumnWidthList;
        private List<int> _RowHeightList;

        #endregion

        #region [Properties]
        /// <summary>
        /// IRecordModel Collection
        /// </summary>
        public ObservableCollection<IRecordModel> Records
        {
            get
            {
                return _Records;
            }
            set
            {
                _Records = value;
            }
        }

        public delegate bool? BeforeChangeRecordEventHander(IRecordModel record, PropertyInfo propertyInfo, object newProperyValue);
        public event BeforeChangeRecordEventHander OnBeforeChangeRecord;


        public delegate bool? BeforeDeleteRecordEventHander(IRecordModel record, int row);
        public event BeforeDeleteRecordEventHander OnBeforeDeleteRecord;

        public delegate void BeforeCellEditEventHander(object sender, unvell.ReoGrid.Events.CellBeforeEditEventArgs e);
        public event BeforeCellEditEventHander OnBeforeCellEdit;
        #endregion

        #region [Constructor]

        public WorksheetModel(ReoGridControl reoGridControl, Type recordModelType)
        {
            _ReoGridControl = reoGridControl;
            _Worksheet = reoGridControl.CurrentWorksheet;
            _RecordModelType = recordModelType;
            _Records = new ObservableCollection<IRecordModel>();

            InitWorksheet();
        }


        public WorksheetModel(ReoGridControl reoGridControl, Type recordModelType, ObservableCollection<IRecordModel> records)
        {
            _ReoGridControl = reoGridControl;
            _Worksheet = reoGridControl.CurrentWorksheet;
            _Records = records;
            _RecordModelType = recordModelType;
            _Records.CollectionChanged += Records_CollectionChanged;

            InitWorksheet();
        }
        #endregion

        #region [Private Methods]

        #region [InitWorksheet] Init Worksheet
        /// <summary>
        /// Init Worksheet
        /// </summary>
        private void InitWorksheet()
        {
            

            WorksheetAttribute classAttribue = _RecordModelType.GetCustomAttribute(typeof(WorksheetAttribute)) as WorksheetAttribute;
            if (classAttribue != null)
            {
                _Worksheet.Name = classAttribue.Title;
            }
            else
            {
                _Worksheet.Name = _RecordModelType.Name;
            }

            PropertyInfo[] properties = _RecordModelType.GetProperties();
            Dictionary<PropertyInfo, ColumnHeaderAttribute> ColHeaderAttributeDict = new Dictionary<PropertyInfo, ColumnHeaderAttribute>();
            foreach (PropertyInfo property in properties)
            {
                ColumnHeaderAttribute headerAttribute = property.GetCustomAttribute(typeof(ColumnHeaderAttribute)) as ColumnHeaderAttribute;
                if (headerAttribute != null && headerAttribute.IsVisible) //filter invisible item
                {
                    ColHeaderAttributeDict.Add(property, headerAttribute);
                }
            }
            if (ColHeaderAttributeDict.Count < 1)
            {
#if DEBUG
                Console.WriteLine("InitWorksheet Failed: HeaderAttributes.Count is 0.");
#endif
                return;
            }
            ColHeaderAttributeDict = ColHeaderAttributeDict.OrderBy(one => one.Value.Index).ToDictionary(one => one.Key, one => one.Value); // order by index
            // Re-Set Index
            for (int i = 0; i < ColHeaderAttributeDict.Keys.Count; i++)
            {
                var key = ColHeaderAttributeDict.Keys.ElementAt(i);
                ColHeaderAttributeDict[key].Index = i;
            }

           
            _Worksheet.Columns = ColHeaderAttributeDict.Count;
           

            RangePosition rangePosition = new RangePosition();

            rangePosition.Cols = 1;
            rangePosition.Row = 0;
            rangePosition.Rows = _Worksheet.RowCount;

            for (int i = 0; i < properties.Count(); i++)
            {
                PropertyInfo property = properties[i];
                var attribute = property.GetCustomAttribute(typeof(FormatAttributeBase));
                if (attribute == null)
                {
                    continue;
                }
                IFormatArgs formatArgs = attribute as IFormatArgs;
                if (formatArgs != null)
                {
                    ColumnHeaderAttribute headerAttribute = (from key in ColHeaderAttributeDict.Keys
                             where key.Equals(property)
                             select ColHeaderAttributeDict[property]).FirstOrDefault();

                    if (headerAttribute != null && headerAttribute.IsVisible)
                    {
                        rangePosition.Col = headerAttribute.Index;
                        //not work correctly 
                        switch (formatArgs.CellDataFormatFlag)
                        {
                            case CellDataFormatFlag.General:
                                break;
                            case CellDataFormatFlag.Number:
                                {
                                    NumberFormatAttribute numberFormatAttribute = formatArgs as NumberFormatAttribute;
                                    NumberDataFormatter.NumberFormatArgs numberFormatter = new NumberDataFormatter.NumberFormatArgs();
                                    if (numberFormatAttribute.DecimalPlaces != short.MaxValue)
                                    {
                                        numberFormatter.DecimalPlaces = numberFormatAttribute.DecimalPlaces;
                                    }
                                    numberFormatter.NegativeStyle = numberFormatAttribute.NegativeStyle;
                                    numberFormatter.UseSeparator = numberFormatAttribute.UseSeparator;
                                    numberFormatter.CustomNegativePrefix = numberFormatAttribute.CustomNegativePrefix;
                                    numberFormatter.CustomNegativePostfix = numberFormatAttribute.CustomNegativePostfix;
                                    _Worksheet.SetRangeDataFormat(rangePosition, CellDataFormatFlag.Number, numberFormatter);
                                    //_ReoGridControl.DoAction(new SetRangeDataFormatAction(rangePosition, CellDataFormatFlag.Number, numberFormatter));
                                    break;
                                }
                            case CellDataFormatFlag.DateTime:
                                {
                                    DateTimeFormatAttribute dateTimeFormatAttribute = formatArgs as DateTimeFormatAttribute;
                                    DateTimeDataFormatter.DateTimeFormatArgs dateTimeFormatArgs = new DateTimeDataFormatter.DateTimeFormatArgs();
                                    dateTimeFormatArgs.Format = dateTimeFormatAttribute.Format;
                                    dateTimeFormatArgs.CultureName = dateTimeFormatAttribute.CultureName;
                                    _Worksheet.SetRangeDataFormat(rangePosition, CellDataFormatFlag.DateTime, dateTimeFormatArgs);
                                    break;
                                }
                            case CellDataFormatFlag.Percent:
                                break;
                            case CellDataFormatFlag.Currency:
                                break;
                            case CellDataFormatFlag.Text:
                                break;
                            case CellDataFormatFlag.Custom:
                                break;
                            default:
                                break;
                        }
                    }
                }

                
            }

            _ColumnWidthList = new List<int>();
            _RowHeightList = new List<int>();

            for (int i = 0; i < ColHeaderAttributeDict.Count; i++)
            {
                ColumnHeaderAttribute headerAttribute = ColHeaderAttributeDict.ElementAt(i).Value;

                if (string.IsNullOrEmpty(headerAttribute.Text))
                {
                    _Worksheet.ColumnHeaders[i].Text = ColHeaderAttributeDict.ElementAt(i).Key.Name;
                }
                else
                {
                    _Worksheet.ColumnHeaders[i].Text = headerAttribute.Text;
                }
                if (headerAttribute.Width <= 0)
                {
                    _Worksheet.ColumnHeaders[i].IsAutoWidth = true;
                    
                }
                else
                {
                    _Worksheet.ColumnHeaders[i].Width = (ushort)headerAttribute.Width;
                }
                _Worksheet.ColumnHeaders[i].IsVisible = true;
                _Worksheet.ColumnHeaders[i].Tag = ColHeaderAttributeDict.ElementAt(i).Key; // Tag stores PropertyInfo of Model
                _ColumnWidthList.Add(_Worksheet.ColumnHeaders[i].Width); // store column width
            }

            //stroe row height
            for (int i = 0; i < _Worksheet.Rows; i++)
            {
                _RowHeightList.Add(_Worksheet.RowHeaders[i].Height);
            }

            // load data
            LoadRecords();


            _Worksheet.BeforeCellEdit += Worksheet_BeforeCellEdit;
            _Worksheet.CellDataChanged += Worksheet_CellDataChanged;
            _Worksheet.RangeDataChanged += Worksheet_RangeDataChanged;
            _Worksheet.AfterPaste += Worksheet_AfterPaste;

            _Worksheet.RowsHeightChanged += Worksheet_RowsHeightChanged;
            _Worksheet.ColumnsWidthChanged += Worksheet_ColumnsWidthChanged;
        }
        #endregion

        #region [Worksheet_ColumnsWidthChanged] Worksheet Columns Width Changed
        /// <summary>
        /// Worksheet Columns Width Changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Worksheet_ColumnsWidthChanged(object sender, ColumnsWidthChangedEventArgs e)
        {
            // Exception: Process is terminated due to StackOverflowException.
            // _Worksheet.ColumnHeaders[e.Index].Width = (ushort)e.Width;

            if (e.Index < _ColumnWidthList.Count)
            {
                _ColumnWidthList[e.Index] = e.Width;
            }
            else
            {
                _ColumnWidthList.Add(e.Width);
            }
        }
        #endregion

        #region [Worksheet_RowsHeightChanged] Worksheet Rows Height Changed
        /// <summary>
        /// Worksheet Rows Height Changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Worksheet_RowsHeightChanged(object sender, RowsHeightChangedEventArgs e)
        {
            // Exception: Process is terminated due to StackOverflowException.
            // _Worksheet.RowHeaders[e.Index].Height = (ushort)e.Height;

            if (e.Index < _RowHeightList.Count)
            {
                _RowHeightList[e.Index] = e.Height;
            }
            else
            {
                _RowHeightList.Add(e.Height);
            }
        }
        #endregion

        #region [LoadRecords] Load Records
        /// <summary>
        /// Load Records
        /// </summary>
        private void LoadRecords()
        {
            if (_Records.Count > 0)
            {
                for (int rowIndex = 0; rowIndex < _Records.Count; rowIndex++)
                {
                    IRecordModel record = _Records.ElementAt(rowIndex);
                    AddOrUpdateOneFromRecord(rowIndex, record);
                }
            }
        }
        #endregion

        #region [Records_CollectionChanged] Data records collection changed
        /// <summary>
        /// Data records collection changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Records_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Changed event, _Records has got action result
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    {
                        foreach (IRecordModel item in e.NewItems)
                        {
                            AddOrUpdateOneFromRecord(_Records.Count - 1, item);
                        }
                        break;
                    }
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    {
                        foreach (IRecordModel item in e.OldItems)
                        {
                            int rowIndex = item.RowIndex;
                            _Worksheet.DeleteRows(rowIndex, 1);

                            for (int i = rowIndex; i < _Records.Count; i++)
                            {
                                _Records.ElementAt(rowIndex).RowIndex = rowIndex;
                            }
                        }
                        break;
                    }
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    {
                        for (int i = 0; i < e.NewItems.Count; i++)
                        {
                            IRecordModel item = e.NewItems[i] as IRecordModel;
                            int rowIndex = (e.OldItems[i] as IRecordModel).RowIndex;
                            AddOrUpdateOneFromRecord(rowIndex, item);
                        }
                        break;
                    }
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    {
                        for (int i = 0; i < _Records.Count; i++)
                        {
                            IRecordModel item = _Records.ElementAt(i);
                            int rowIndex = item.RowIndex;
                            if (rowIndex != i)
                            {
                                AddOrUpdateOneFromRecord(i, item);
                            }
                        }
                        break;
                    }
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    {
                        _Worksheet.DeleteRows(0, _Worksheet.UsedRange.Rows);
                        break;
                    }
                default:
                    break;
            }
        }
        #endregion

        #region [Worksheet_BeforeCellEdit] Before editing cell data
        /// <summary>
        /// Before editing cell data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Worksheet_BeforeCellEdit(object sender, unvell.ReoGrid.Events.CellBeforeEditEventArgs e)
        {

            int currentColIndex = e.Cell.Column;
            int currentRowIndex = e.Cell.Row;
            
            if (_Worksheet.ColumnHeaders[currentColIndex] != null)
            {
                PropertyInfo propertyInfo = (PropertyInfo)_Worksheet.ColumnHeaders[currentColIndex].Tag;
                if (propertyInfo.PropertyType.BaseType == typeof(Enum))
                {
                    // get enum values
                    List<object> enumValues = new List<object>();
                    foreach (var item in System.Enum.GetValues(propertyInfo.PropertyType))
                    {
                        enumValues.Add(item);
                    }
                    SimulateComboBox(enumValues, e);
                }
                else if (propertyInfo.PropertyType == typeof(bool))
                {
                    List<object> values = new List<object>() { Boolean.TrueString, Boolean.FalseString };
                    SimulateComboBox(values, e);
                }
            }
            if (OnBeforeCellEdit != null)
            {
                OnBeforeCellEdit(sender, e);
            }
        }
        #endregion

        #region [Worksheet_CellDataChanged] Worksheet Cell Data Changed
        /// <summary>
        /// Worksheet Cell Data Changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Worksheet_CellDataChanged(object sender, unvell.ReoGrid.Events.CellEventArgs e)
        {
            if (e.Cell != null)
            {
                int row = e.Cell.Row;
                int col = e.Cell.Column;
                
                AddOrUpdateOneFromUi(row, col, col);
            }
        }
        #endregion

        #region [Worksheet_AfterPaste] Paste Data
        /// <summary>
        /// Paste Data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Worksheet_AfterPaste(object sender, unvell.ReoGrid.Events.RangeEventArgs e)
        {
            RangePosition range = e.Range;
            AddOrUpdateRecords(range);
        }
        #endregion

        #region [AddOrUpdateRecords] Add or Update mulit records
        /// <summary>
        /// Add or Update mulit records
        /// </summary>
        /// <param name="range">edit range</param>
        private void AddOrUpdateRecords(RangePosition range)
        {
            for (int rowIndex = range.StartPos.Row; rowIndex <= range.EndRow; rowIndex++)
            {
                AddOrUpdateOneFromUi(rowIndex, range.StartPos.Col, range.EndPos.Col);
            }
        }
        #endregion

        #region [AddOrUpdateOneFromUi] Add or Update one record
        /// <summary>
        /// Add or Update one record
        /// </summary>
        /// <param name="rowIndex">current row index</param>
        /// <param name="startCol">start column index</param>
        /// <param name="endCol">end column index</param>
        private void AddOrUpdateOneFromUi(int rowIndex, int startCol, int endCol)
        {
            IRecordModel record;

            if (rowIndex < _Records.Count) // update record
            {
                record = _Records.ElementAt(rowIndex);
            }
            else // insert record
            {
                record = Activator.CreateInstance(_RecordModelType) as IRecordModel;
                record.RowIndex = rowIndex;
            }

            for (int colIndex = startCol; colIndex <= endCol; colIndex++)
            {
                PropertyInfo propertyInfo = (PropertyInfo)_Worksheet.ColumnHeaders[colIndex].Tag;
                object cellData = _Worksheet.GetCellData(rowIndex, colIndex);
                object value = null;
                if (cellData != null)
                {
                    if (propertyInfo.PropertyType.BaseType == typeof(Enum))
                    {
                        try
                        {
                            value = Convert.ChangeType(Enum.Parse(propertyInfo.PropertyType, cellData.ToString()), propertyInfo.PropertyType);
                        }
                        catch (Exception ex)
                        {
#if DEBUG
                            Console.WriteLine(ex.Message);
                            break;
#endif
                        }
                    }
                    else
                    {
                        try
                        {
                            value = Convert.ChangeType(cellData, propertyInfo.PropertyType);
                        }
                        catch (Exception ex)
                        {
#if DEBUG
                            Console.WriteLine(ex.Message);
                            break;
#endif
                        }

                    }
                }

                if (OnBeforeChangeRecord != null)
                {
                    bool? isCancel = OnBeforeChangeRecord(record, propertyInfo, value);
                    if (isCancel.HasValue && isCancel.Value) // if has value and cancel is true, then undo the change
                    {
                        _Worksheet.SetCellData(rowIndex, colIndex, propertyInfo.GetValue(record));
                        //if (rowIndex < _Records.Count)
                        //{
                        //    _Worksheet.SetCellData(rowIndex, colIndex, propertyInfo.GetValue(record));
                        //}
                        //else
                        //{

                        //}
                        continue;
                    }
                }
                propertyInfo.SetValue(record, value);
            }
            if (rowIndex >= _Records.Count)
            {
                _Records.Add(record);
            }
        }
        #endregion

        #region [AddOrUpdateOneFromRecord] Add or update one IRecordModel object into Worksheet
        /// <summary>
        /// Add or update one IRecordModel object into Worksheet
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <param name="record"></param>
        private void AddOrUpdateOneFromRecord(int rowIndex, IRecordModel record)
        {
            _Worksheet.SuspendDataChangedEvents();
            record.RowIndex = rowIndex; // set row index
            for (int colIndex = 0; colIndex < _Worksheet.Columns; colIndex++)
            {
                CellPosition currentCellPos = new CellPosition(rowIndex, colIndex);
                if (_Worksheet.ColumnHeaders[colIndex].Tag != null)
                {
                    PropertyInfo propertyInfo = (PropertyInfo)_Worksheet.ColumnHeaders[colIndex].Tag;
                    object data = propertyInfo.GetValue(record);
                    _Worksheet.SetCellData(currentCellPos, data);
                }
                else
                {
#if DEBUG
                    Console.WriteLine("LoadRecords Error: index of columns[{0}] is null!", colIndex);
#endif
                }
            }
            _Worksheet.ResumeDataChangedEvents();
        }
        #endregion

        #region [Worksheet_RangeDataChanged] Worksheet Range Data Changed
        /// <summary>
        /// Worksheet Range Data Changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Worksheet_RangeDataChanged(object sender, unvell.ReoGrid.Events.RangeEventArgs e)
        {
            bool isDeleteRows = false;
            if (e.Range.Cols >= _Worksheet.UsedRange.Cols) //delete whole rows
            {
                isDeleteRows = true;
                for (int rowIndex = e.Range.StartPos.Row; rowIndex <= e.Range.EndRow; rowIndex++)
                {
                    IRecordModel recordModel = (from one in _Records where one.RowIndex == rowIndex select one).FirstOrDefault();
                    if (recordModel != null)
                    {
                        _Records.CollectionChanged -= Records_CollectionChanged;
                        _Records.Remove(recordModel);
                        _Records.CollectionChanged += Records_CollectionChanged;
                    }
                }
                if (isDeleteRows)
                {
                    _Worksheet.DeleteRows(e.Range.StartPos.Row, e.Range.Rows);

                    for (int i = e.Range.StartPos.Row; i < _Records.Count; i++)
                    {
                        _Records.ElementAt(i).RowIndex = i;
                    }
                }
            }
            else //delete parts of rows
            {
                for (int rowIndex = e.Range.StartPos.Row; rowIndex <= e.Range.EndRow; rowIndex++)
                {
                    AddOrUpdateOneFromUi(rowIndex, e.Range.StartPos.Col, e.Range.EndCol);
                }
            }
        }
        #endregion

        #region [SimulateComboBox] Simulate ComboBox
        private void SimulateComboBox(List<object> list, CellBeforeEditEventArgs e)
        {
            int currentColIndex = e.Cell.Column;
            int currentRowIndex = e.Cell.Row;
            
            Window window = new Window();
            WrapPanel wrapPanel = new WrapPanel();
            ListBox listBox = new ListBox();
            listBox.SetValue( ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Hidden);
            listBox.SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Hidden);
            listBox.Width = _ColumnWidthList.ElementAt(e.Cell.Column);

            for (int i = 0; i < list.Count; i++)
            {
                var item = list.ElementAt(i);
                ListBoxItem listBoxItem = new ListBoxItem();
                listBoxItem.Content = item;
                listBox.Items.Add(listBoxItem);
            }
            listBox.RenderTransform = new ScaleTransform(_Worksheet.ScaleFactor, _Worksheet.ScaleFactor);

            listBox.MouseDoubleClick += (object obj, MouseButtonEventArgs eventArgs) => {
                e.EditText = (listBox.SelectedValue as ListBoxItem).Content.ToString();
                window.DialogResult = true;
            };
            wrapPanel.Children.Add(listBox);
            Point point = new Point();
            for (int rowIndex = 0; rowIndex <= currentRowIndex + 1; rowIndex++)
            {
                point.Y += (int)(_RowHeightList.ElementAt(rowIndex) * _Worksheet.ScaleFactor);
            }
            for (int colIndex = 0; colIndex < currentColIndex; colIndex++)
            {
                point.X += (int)(_ColumnWidthList.ElementAt(colIndex) * _Worksheet.ScaleFactor);
            }
            point.X += (int)(_Worksheet.RowHeaderWidth * _Worksheet.ScaleFactor);
            Point screenPoint = _ReoGridControl.PointToScreen(point);

            window.Width = _Worksheet.ColumnHeaders[e.Cell.Column].Width;
            window.WindowStyle = WindowStyle.None;
            window.ResizeMode = ResizeMode.NoResize;
            window.BorderThickness = new Thickness(0);
            window.Content = wrapPanel;
            window.SizeToContent = SizeToContent.WidthAndHeight;
            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.Left = screenPoint.X;
            window.Top = screenPoint.Y;
            window.Loaded += (object win, RoutedEventArgs routedEventArgs) =>
            {
                wrapPanel.Width = listBox.RenderSize.Width * _Worksheet.ScaleFactor;
                wrapPanel.Height = listBox.RenderSize.Height * _Worksheet.ScaleFactor;
            };
            window.ShowDialog();
        }
        #endregion

        #endregion

        #region [Public Methods]

        #region [UpadteRecord] Update one record
        /// <summary>
        /// Update one record
        /// </summary>
        /// <param name="recordModel"></param>
        public void UpadteRecord(IRecordModel recordModel)
        {
            AddOrUpdateOneFromRecord(recordModel.RowIndex, recordModel);
        }
        #endregion

        #endregion
    }
}
