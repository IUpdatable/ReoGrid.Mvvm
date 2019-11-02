
下面以一个图书信息的简单项目演示如何使用 `ReoGrid.Mvvm`. 完整代码见项目 `ReoGrid.Mvvm.Demo`.

![all.gif](https://i.loli.net/2019/10/23/RnLb5wEFKOJsd4c.gif)
<!--more-->
#### 1. 创建一个WPF项目.
#### 2. NuGet安装 ReoGrid.Mvvm

```
Install-Package ReoGrid.Mvvm
```
#### 3. 创建一个图书的模型（Model）

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

**(1) Model 必须实现`IRecordModel`接口**

`IRecordModel` 只有一个 `RowIndex` 属性, 你完全不用管这个属性，这是 `ReoGrid.Mvvm` 内部用到的。

**(2) `WorksheetAttribute` 用来说明工作表的名字**

可选，不指定该特性，那么就用Model类的类名作为工作表名称。

**(3) `ColumnHeader`特性中, 必须指定 `Index` 属性，其他的是可选的。**

**(4) `DateTimeFormat` `DateTimeFormat` 目前不建议使用**

`ReoGrid`本身并没有完整实现这些特性。当然，也有可能我理解有误。

#### 4. 在ViewModel中需要的修改:

##### 4.1 创建两个成员变量
```cs
private ObservableCollection<IRecordModel> _Books;
private WorksheetModel _WorksheetModel;
```
##### 4.2 初始化
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
// 变量 reoGridControl 是 ReoGridControl 的控件元素实例
_WorksheetModel = new WorksheetModel(reoGridControl, typeof(Book), _Books);
//如果需要在输入值前检查变量的有效性，那么就实现该函数
_WorksheetModel.OnBeforeChangeRecord += WorksheetModel_OnBeforeChangeRecord;
```
##### 4.3 在 `WorksheetModel_OnBeforeChangeRecord` 函数中演示输入值有效性检查

```cs
private bool? WorksheetModel_OnBeforeChangeRecord(IRecordModel record, System.Reflection.PropertyInfo propertyInfo, object newProperyValue)
{
    if (propertyInfo.Name.Equals("Price"))
    {
        decimal price = Convert.ToDecimal(newProperyValue);
        if (price > 100m) //假设最大价格是100
        {
            MessageBox.Show("最大价格是 100， 请重新输入！.", "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
            return true; // 返回 true 则取消本次输入
        }
    }

    return null;
}
```
##### 4.4 增加、删除、移动、编辑 模型（Model）

```cs
// 增加一条书目信息
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

// 移除一条书目信息
if (_Books.Count > 0)
{
    _Books.RemoveAt(_Books.Count - 1);
}

// 移动一条书目信息
if (_Books.Count > 2)
{
    _Books.Move(0, _Books.Count - 1);
}

// 编辑一条书目信息
(_Books[0] as Book).Price = new Random(DateTime.Now.Millisecond).Next(1,100);
_WorksheetModel.UpadteRecord(_Books[0]); // 编辑完 模型（Model） 之后要调用 UpadteRecord 函数将模型（Model）的变化同步到视图（View）中
```