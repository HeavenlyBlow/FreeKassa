# AtolDriver
Библиотеки для работы с кассами с драйвером АТОЛ

<B>Начало работы</B>

1. Соберите проект
2. Добавьте собранный dll и драйвер Atol.Drivers10.Fptr (Данный можно найти в проекте или папке Program Files\ATOL\Drivers10\KKT\langs\netcore) В ПРОЕКТЕ ИСПОЛЬЗУЕТСЯ ДРАЙВЕР 10.9.4.0

Пример простейшей программы
```csharp
static void Main(string[] args)
        {
            //Прописываем скорость и порт ККТ
            int port = 3;
            int baudRate = 115200;
            
            string operatorName = "Анна";
            string operatorInn = null;
            
            //Инициализируем подключение
            var kkt = new Interface(port, baudRate);
            kkt.OpenConnection();
            // Получаем статус смены
            kkt.GetShiftStatus();
            //Устанавливаем оператора
            kkt.SetOperator(operatorName, operatorInn);
            //Открываем смену
            kkt.OpenShift();
            //Открываем новый чек
            kkt.OpenReceipt(true ,TypeReceipt.Sell, TaxationTypeEnum.Osn);
            //Добавляем товар в чек
            kkt.AddPosition("Норка", 15, 1,MeasurementUnitEnum.Piece, PaymentObjectEnum.Commodity, TaxTypeEnum.Vat20);
            //Добавляем товар в чек
            kkt.AddPosition("Ручка", 20, 1, MeasurementUnitEnum.Piece, PaymentObjectEnum.Commodity, TaxTypeEnum.Vat20);
            //Регистрируем оплату
            kkt.Pay(PaymentTypeEnum.cash, 35);
            //Закрываем чек
            kkt.CloseReceipt();
            //Получаем документ по его номеру в ФН
            var strin = printer.GetDocument(69);
            // Закрываем смену
            kkt.CloseShift();
        }
```
<B>В данном примере не обработаны ошибки!</B>

Каждый метод возвращает результат выполнения 0 - ошибок нет, -1 - была найдена ошибка.

Для получения детальной информации о последней ошибке используте метод ReadError()

<B>Получение документа из ккт</B>

1. Получите номер последнего документа из ФН GetLastDocumentNumber()
2. При работе с ним обращайте внимание что документами считаютя чеки и отчеты об открытии и закрытии смены
3. Запросите документ GetDocument()