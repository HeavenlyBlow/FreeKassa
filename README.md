<h1 align="center">FreeKassa</h1>
<h2>Описание проекта</h2>
```
<p>
Проект был создан для упрощения интергрирования кассовых решений в ПО на платформе .NET

На данный момент в проекте реализована поддержка:
</p>

<ul>
    <li>ККТ на базе АТОЛ</li>
    <li>Принетров ESP/POS</li>
    <li>Купюроприемника CashCode</li>
    <li>Пинпадов Сбербанк | Inpas</li>
</ul>

<h2>Быстрый старт</h2>
<h3>1. Настройте файл configKassa.json

``` json
{
  "KKT": {
    "PrinterManagement": 0,
    "MarkedProducts": true,
    "SerialPort": 9,
    "BaundRate" : 115200,
    "OperatorName": "Кассир 1",
    "Inn" : "",
    "Shift": {
      "NonStopWork": {
        "On" : 1,
        "ShiftChangeTime" : "23:44"
      },
      "WorkKWithBreaks": {
        "On" : 0,
        "OpeningTime" : "10:58",
        "CloseTime" : "10:55"
      }
    }
  },
  "Printer": {
    "SerialPort": "COM7",
    "BaundRate" : 115200
  },
  "CashValidator": {
    "SerialPort": "COM7",
    "BaundRate" : 9600
  },
  "Sberbank": {
    "Directory": "C:\\WinSber"
  }
}
```

<h4>Описание полей:
<ul>
    <li>PrinterManagement - отвечает за переключение режимов работы с принетором где 0 - ручное управление, 1 - автоматическое управление принтером ККТ</li>
    <li>MarkedProducts - Будет ли в чеках маркированный товар </li>
    <li>SerialPort - номер поледовательного порта к которому подключен ККТ</li>
    <li>OperatorName - имя которое будет отображаться в чеках</li>
    <li>Shift - По поддерживает 2 режима работы со сменами. Первый безприровный, второй интервальный </li>
</ul>

___
<h3>2. Создайте экземпляр класса и подпишитесь на необходимые события</h3>

```csharp
var kassa = new KassaManager();
kassa.SuccessfullyReceipt += (sender, args) => Logger.Info("Фискализация прошла успешно");
kassa.Error += (sender, args) => Logger.Info("Ошибка фискализации");
kassa.StartKassa();
```
___
<h3>3. Запустите процесс оплаты предварительно подписавшись на его события (при необходимости) </h3>

```csharp
kassa.SuccessfullyPayment += (sender, args) => Logger.Info("Оплата получена");
kassa.ErrorPayment += (sender, args) => Logger.Info("Ошибка оплаты");
kassa.StartPayment(PaymentType.Sberbank, 1000);          
```
___
<h3>4.Запустите процесс фискализации чека </h3>

```csharp
          kassa.RegisterReceipt( new ReceiptModel()
                {
                    isElectron = true,
                    TaxationType = TaxationTypeEnum.Osn,
                    TypeReceipt = TypeReceipt.Sell
                }, 
                new List<BasketModel>() 
                { 
                    new BasketModel() 
                    {
                        Cost = 10,
                        MeasurementUnit = MeasurementUnitEnum.Piece,
                        Name = "Фотографии",
                        PaymentObject = PaymentObjectEnum.Service,
                        Quantity = 5,
                        TaxType = TaxTypeEnum.Vat10
                    }
                },
                new PayModel()
                {
                    PaymentType = PaymentTypeEnum.cash,
                    Sum = 50
                },
                new ClientInfo()
                {
                    EmailOrPhone = "+79911231088"
                }
            );
```
Если isElectron = false чек распечатается автоматически. Если вам не нужно печатать чек сразу после фискализации событие SuccessfullyReceipt возвращает вам модель чека для последующей печати

```
 ChequeFormModel model;
 
 kassa.Successfully += delegate(ChequeFormModel cheque)
    {
        model = cheque
    };
            
 kassa.PrintCheque(model)
```

Обратите внимание что для каждого типа настроект ККТ необходимы свои поля. Например для продаж в интернете необходимо указывать номер телефона. В остальных случаях не трубется по этому я рекоменуд перед установлкой на клиентские машины моделировать их насройки в тестовом ККТ и через тест драйвер атол точно выяснить какие параметры необходимо передавать для фискализации.
___
<h2>Настройка форм печати</h2>
В по реализованы формы фискальныех доументов, к ним относятся чеки открытия, закрытия и фискальные чеки. Данные формы используются для привязки моделей к полям. Вы можете самостоятельно изменять эти формы, добавляя наобходимы элементы.

``` csharp
public static byte[] GetOpenShiftsForm(EPSON e, OpenShiftsFormModel model)
        {
            return ByteSplicer.Combine(
                e.ResetLineSpacing(),
                e.CenterAlign(),
                e.SetStyles(PrintStyle.Bold),
                e.PrintLine("ОТЧЕТ ОБ ОТКРЫТИИ СМЕНЫ"),
                e.PrintLine(""),
                e.SetStyles(PrintStyle.FontB),
                e.LeftAlign(),
                e.PrintLine(IdentHelper.ArrangeWords("Кассир", $"{model.CashierName}", IdentHelper.Style.FontB)),
                e.PrintLine(model.CompanyName),
                e.PrintLine(IdentHelper.ArrangeWords("Место расчетов", $"{model.Address}", IdentHelper.Style.FontB)),
                e.PrintLine(model.DateTime),
                e.PrintLine(IdentHelper.ArrangeWords("Версия ККТ", $"{model.VersionKKT}", IdentHelper.Style.FontB)),
                e.PrintLine(IdentHelper.ArrangeWords("Смена", $"{model.ChangeNumber}", IdentHelper.Style.FontB)),
                e.PrintLine(IdentHelper.ArrangeWords("РН ККТ", $"{model.RegisterNumberKKT}", IdentHelper.Style.FontB)),
                e.PrintLine(IdentHelper.ArrangeWords("ИНН", $"{model.Inn}", IdentHelper.Style.FontB)),
                e.PrintLine(IdentHelper.ArrangeWords("ФН", $"{model.FiscalStorageRegisterNumber}",
                    IdentHelper.Style.FontB)),
                e.PrintLine(IdentHelper.ArrangeWords("ФД", $"{model.FiscalDocumentNumber}", IdentHelper.Style.FontB)),
                e.PrintLine(IdentHelper.ArrangeWords("ФП", $"{model.FiscalFeatureDocument}", IdentHelper.Style.FontB))
            );
        }
```
Для получения массива байт для печати применяется статический класс ByteSplicer.Combine из библиотеки EPSON

Данная библиотека имеет большой функционал для настройки шрифтов их расположения и отступов. Как вы могли заметить в коде присутсвует:

```csharp
public static string ArrangeWords(string leftString, string rightString, Style eStyle)
```

Данный метод сделан для того чтобы в одной строке по разным сторонам поместились 2 строки. Данный метод работает только с FontB. 
Для добавления другого шрифта необходимо узнать количество знаков которые поещаются на чековую ленту.

В проекте предусмотрена возможность печати пользовательских документов для этого был реализован интерфейс IForm и метод  PrintUsersDocument   


Спасибо Luke Paireepinart (https://github.com/lukevp/ESC-POS-.NET) и Xobnail (https://github.com/Xobnail/AtolDriver) 

