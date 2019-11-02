
[中文版说明](https://github.com/IUpdatable/ReoGrid.Mvvm/blob/master/README-CN.md)

Here is demo for book information to demonstrate the use of `ReoGrid.Mvvm`. The complete source code is in the project `ReoGrid.Mvvm.Demo`.

![all.gif](https://github.com/IUpdatable/ReoGrid.Mvvm/blob/master/all.gif)

#### 1. create a new WPF project.
#### 2. insatll ReoGrid.Mvvm

```
Install-Package ReoGrid.Mvvm
```
#### 3. create a book model

```cs
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

    [NumberFormat(DecimalPlaces = 2)]
    [ColumnHeader(Index = 40)]
    public decimal Price { get; set; }

    [DateTimeFormat( CultureName = "en-US")]
    [ColumnHeader(Index = 45, Text = "Publish Date", Width = 200)]
    public DateTime Pubdate { get; set; }

    public int RowIndex { get; set; }
}

```

**(1) Model must implement the interface `IRecordModel`.**

`IRecordModel` has only one property `RowIndex`, You don't need to do anything with it.

**(2) `WorksheetAttribute` is used to specify the worksheet name.**

It's optional, class name would be set as the worksheet name if it's not set.

**(3) in `ColumnHeader` Attribute, `Index` must be specified. others are optional.**

**(4) `DateTimeFormat` `DateTimeFormat` are not recommended for use for now.**

These features are not well implemented in `ReoGrid`.OR I have some misunderstand.

#### 4. in ViewModel file:

##### 4.1 create two fields
```cs
private ObservableCollection<IRecordModel> _Books;
private WorksheetModel _WorksheetModel;
```
##### 4.2 init them
```cs
_Books = new ObservableCollection<IRecordModel>();
for (int i = 0; i < 10; i++)
{
    Book book = new Book();
    book.Id = i;
    book.Title = string.Format("Title {0}", i);
    book.Author = string.Format("Author {0}", i);
    book.BindingType = BindingType.Hardback;
    book.IsOnSale = true;
    book.Price = (decimal)(i * 10.1);
    book.Pubdate = DateTime.Now;
    _Books.Add(book);
}
// reoGridControl is the ReoGridControl control instance
_WorksheetModel = new WorksheetModel(reoGridControl, typeof(Book), _Books);
//If you want to check the validity of the input variables, you can implement the function.
_WorksheetModel.OnBeforeChangeRecord += OnBeforeChangeRecord;
```
##### 4.3 implement the `OnBeforeChangeRecord`

```cs
private bool? OnBeforeChangeRecord(IRecordModel record, PropertyInfo propertyInfo, object newValue)
{
    if (propertyInfo.Name.Equals("Price"))
    {
        decimal price = Convert.ToDecimal(newValue);
        if (price > 100m) //assume the max price is 100
        {
            MessageBox.Show("Max price is 100.", "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
            return true; // cancel the change
        }
    }
    return null;
}
```
##### 4.4 add delete move or edit the model

```cs
// add a book
int count = _Books.Count;
Book book = new Book();
book.Id = count;
book.Title = string.Format("Title {0}", count);
book.Author = string.Format("Author {0}", count);
book.BindingType = BindingType.Hardback;
book.IsOnSale = true;
book.Price = (decimal)(count * 10.11) > 100m ? 100m :(decimal)(count * 10.11);
book.Pubdate = DateTime.Now;
_Books.Add(book);

// remove a book
if (_Books.Count > 0)
{
    _Books.RemoveAt(_Books.Count - 1);
}

// move a book
if (_Books.Count > 2)
{
    _Books.Move(0, _Books.Count - 1);
}

// edit a book
(_Books[0] as Book).Price = new Random(DateTime.Now.Millisecond).Next(1,100);
// invoke UpadteRecord after editing one record.
_WorksheetModel.UpadteRecord(_Books[0]); 
```
