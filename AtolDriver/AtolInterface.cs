using System.Globalization;
using Atol.Drivers10.Fptr;
using Newtonsoft.Json;
using AtolDriver.Models;
using System.Text;
using AtolDriver.BaseClass;
using AtolDriver.Models.AnswerModel;
using AtolDriver.Models.RequestModel;
using AtolDriver.Utils;

namespace AtolDriver
{
    public class AtolInterface
    {

        #region Header

        readonly IFptr _fptr;
        private object _locker = new();
        Operator _cashier;
        ReceiptBase _receipt;
        private readonly bool _isMarked; 

        public AtolInterface(int port, int speed, bool isMarked)
        {
            _isMarked = isMarked;
            _fptr = new Fptr();
            _fptr.setSingleSetting(Constants.LIBFPTR_SETTING_MODEL, Constants.LIBFPTR_MODEL_ATOL_AUTO.ToString());
            _fptr.setSingleSetting(Constants.LIBFPTR_SETTING_PORT, Constants.LIBFPTR_PORT_COM.ToString());
            _fptr.setSingleSetting(Constants.LIBFPTR_SETTING_COM_FILE, "COM" + port);
            _fptr.setSingleSetting(Constants.LIBFPTR_SETTING_BAUDRATE, speed.ToString());
            _fptr.applySingleSettings();
        }

        #endregion

        #region Function

        #region Base

        private void SendJson<T>(T request, out Answer answer)
        {
            lock (_locker)
            {
                _fptr.setParam(Constants.LIBFPTR_PARAM_JSON_DATA, JsonConvert.SerializeObject(request));
                answer = new Answer()
                {
                    Code = _fptr.processJson(),
                    Json = _fptr.getParamString(Constants.LIBFPTR_PARAM_JSON_DATA)
                };
            }
        }
        /// <summary>
        /// Открыть соединение
        /// </summary>
        /// <returns></returns>
        public int OpenConnection()
        {
            return _fptr.open();
        }
        
        /// <summary>
        /// Закрыть соединение
        /// </summary>
        /// <returns></returns>
        public int CloseConnection()
        {
            return _fptr.close();
        }
        
        /// <summary>
        /// Возвращает последнюю ошибку
        /// </summary>
        /// <returns></returns>
        public string ReadError()
        {
            _fptr.errorCode();
            return _fptr.errorDescription();
        }
        
        /// <summary>
        /// Установка времени ккт
        /// </summary>
        public void SetDateTime()
        {
            _fptr.setParam(Constants.LIBFPTR_PARAM_DATE_TIME, DateTime.Now);
            _fptr.writeDateTime();
        }
        

        #endregion

        #region Receipt

        /// <summary>
        /// Открыть чек
        /// </summary>
        /// <param name="isElectronicReceipt">Это электронный чек?</param>
        /// <param name="typeReceiptEnum">Тип чека</param>
        /// <param name="taxationTypeEnum">Тип налогообложения</param>
        /// <param name="client">Информация о клиенте </param>
        /// <param name="isMarked">Маркированный чек</param>
        public void OpenReceipt(bool isElectronicReceipt ,TypeReceipt typeReceiptEnum ,
            TaxationTypeEnum taxationTypeEnum, ClientInfo? client = null)
        {
            _receipt = new ReceiptBase()
            {
                Type = GetTypeReceipt(typeReceiptEnum),
                TaxationType = GetTaxType(taxationTypeEnum),
                Operator = _cashier,
                Client = client,
                Items = new List<Item>(),
                Payments = new List<Payments>(),
                Electronic = isElectronicReceipt
            };

            if (_isMarked)
            {
                _receipt.ValidateMarkingCodes = true;
            }
        }

        /// <summary>
        /// Добавление товаров в чек
        /// </summary>
        /// <param name="name">Имя товара</param>
        /// <param name="price">Прайс</param>
        /// <param name="quantity">Количество</param>
        /// <param name="measurementUnitEnum">Единицы измерения кол-ва товара</param>
        /// <param name="paymentObjectEnum">Признак предмета расчета</param>
        /// <param name="taxationTypeEnum">Процент налога</param>
        /// <param name="ims">Код маркировки</param>
        public void AddPosition(string name, double price, double quantity, MeasurementUnitEnum measurementUnitEnum,
            PaymentObjectEnum paymentObjectEnum, TaxTypeEnum taxationTypeEnum)
        {
            var item = new Item()
            {
                Type = "position",
                Name = name,
                Price = price,
                Quantity = quantity,
                MeasurementUnit = GetMeasurementUnitEnum(measurementUnitEnum),
                PaymentObject = GetPaymentObjectEnum(paymentObjectEnum),
                Amount = price * quantity,
                Tax = new Tax { Type = GetTaxTypeEnum(taxationTypeEnum) }
            };
            
            _receipt.Items.Add(item);
        }

        public Marks? ValidateMarks(List<string> marksList)
        {
            var model = new ValidateMarksRequest()
            {
                Timeout = 40000,
                Type = "validateMarks",
                Params = new List<Param>()

            };

            foreach (var marked in marksList)
            {
                model.Params.Add(new Param()
                {
                    Imc = marked,
                    ImcType = "auto",
                    ItemEstimatedStatus = "itemPieceSold",
                    ImcModeProcessing = 0
                });
            }
            
            SendJson(model, out var answer);
            
            if (answer.Code == -1) return null;
            
            var jobj = JsonConvert.DeserializeObject<Marks>(answer.Json);
            
            return jobj ?? null;
        }
        
        /// <summary>
        /// Зарегестрировать оплату
        /// </summary>
        /// <param name="paymentTypeEnum">Тип оплаты</param>
        /// <param name="sum">Сумма</param>
        public void Pay(PaymentTypeEnum paymentTypeEnum , double sum)
        {
            _receipt.Payments.Add(new Payments
            {
                Type = GetPaymentTypeEnum(paymentTypeEnum),
                Sum = sum
            });
        }

        /// <summary>
        /// Закрыть чек
        /// </summary>
        /// <returns>Код ошибки</returns>
        public ChequeInfo? CloseReceipt()
        {
            SendJson(_receipt, out var answer);
            if (answer.Code == -1) { return null; }

            object? jobj;

            jobj = JsonConvert.DeserializeObject<ChequeInfo>(answer.Json);

            if (jobj == null) return null;
            
            return (ChequeInfo)jobj;
        }
        
        /// <summary>
        /// Отмена чека
        /// </summary>
        /// <returns></returns>
        public int CanselReceipt()
        {
            return _fptr.cancelReceipt();
        }
        
        
        #endregion

        #region Shift

        /// <summary>
        /// Открыть смену
        /// </summary>
        /// <returns>код ошибки</returns>
        public OpenShiftInfo? OpenShift()
        {
            SendJson(new OpenShift
            {
                Operator = _cashier,
            }, out var answer);
            
            if (answer.Code == -1) return null;
            var jobj = DeserializeHelper.Deserialize(answer.Json, model: new OpenShiftInfo(), token: "fiscalParams");
            if (jobj == null) return null;
            return (OpenShiftInfo)jobj;
        }
        
        /// <summary>
        /// Утсновка оператора на смену
        /// </summary>
        /// <param name="operatorName">Имя опреатора</param>
        /// <param name="operatorInn">Инн оператора</param>
        public void SetOperator(string operatorName, string operatorInn)
        {
            _cashier = new Operator
            {
                Name = operatorName,
                Vatin = operatorInn
            };
        }
        
        /// <summary>
        /// Состояние смены
        /// </summary>
        /// <returns>0 - закрыта, 1 - октрыта</returns>
        public int GetShiftStatus()
        {
            _fptr.setParam(Constants.LIBFPTR_PARAM_DATA_TYPE, Constants.LIBFPTR_DT_STATUS);
            _fptr.queryData();
            return (int)_fptr.getParamInt(Constants.LIBFPTR_PARAM_SHIFT_STATE);
        }
        
        /// <summary>
        /// Закрыть смену
        /// </summary>
        /// <returns>Код ошибки</returns>
        public CloseShiftsInfo? CloseShift()
        {
            SendJson(new CloseShift
            {
                Type = "closeShift",
                Operator = _cashier
            }, out var answer);
        
            if (answer.Code == -1) return null;
            var jobj = DeserializeHelper.Deserialize(answer.Json, model: new CloseShiftsInfo());
            if (jobj == null) return null;
            return (CloseShiftsInfo)jobj;
        }

        #endregion

        #region Info

        
        /// <summary>
        /// Проверка связи с сервером ИМС
        /// </summary>
        
        public ImsStatus PingMarkingServer()
        {
            _fptr.pingMarkingServer();

            // Ожидание результатов проверки связи с сервером ИСМ
            
            while (true) {
                _fptr.getMarkingServerStatus();
                if (_fptr.getParamBool(Constants.LIBFPTR_PARAM_CHECK_MARKING_SERVER_READY))
                    break;
            }

            return new ImsStatus()
            {
                Description = _fptr.getParamString(Constants.LIBFPTR_PARAM_MARKING_SERVER_ERROR_DESCRIPTION),
                ErrorCode = _fptr.getParamInt(Constants.LIBFPTR_PARAM_MARKING_SERVER_ERROR_CODE),
                ResponseTime = _fptr.getParamInt(Constants.LIBFPTR_PARAM_MARKING_SERVER_RESPONSE_TIME)
            };
            
        }

        /// <summary>
        /// Получить номер последнего документа в ФН
        /// </summary>
        /// <returns></returns>
        public string GetLastDocumentNumber()
        {
            _fptr.setParam(Constants.LIBFPTR_PARAM_DATA_TYPE, Constants.LIBFPTR_DT_STATUS);
            _fptr.queryData();
            return $"{_fptr.getParamInt(Constants.LIBFPTR_PARAM_DOCUMENT_NUMBER)}";
        }
        
        /// <summary>
        /// Получить версию ффд
        /// </summary>
        /// <returns></returns>
        public string GetFfdVersion()
        {
            _fptr.setParam(Constants.LIBFPTR_PARAM_FN_DATA_TYPE, Constants.LIBFPTR_FNDT_FFD_VERSIONS);
            _fptr.fnQueryData();
            return $"{_fptr.getParamInt(Constants.LIBFPTR_PARAM_FFD_VERSION)}";
        }
        
        /// <summary>
        /// Получить серийный номер ккт
        /// </summary>
        /// <returns>Серийный номер</returns>
        public string GetSerialNumber()
        {
            _fptr.setParam(Constants.LIBFPTR_PARAM_DATA_TYPE, Constants.LIBFPTR_DT_STATUS);
            _fptr.queryData();
            return $"{_fptr.getParamInt(Constants.LIBFPTR_PARAM_SERIAL_NUMBER)}";
        }
        /// <summary>
        /// Сменные итоги
        /// </summary>
        /// <returns></returns>
        public CountdownStatusInfo? CountdownStatus()
        {
            SendJson(new Countdown
            {
                Type = "reportOfdExchangeStatus",
                Operator = _cashier
            }, out var answer);
            
            if (answer.Code == -1) return null;
            var jobj = DeserializeHelper.Deserialize(answer.Json, model: new CountdownStatusInfo());
            if (jobj == null) return null;
            return (CountdownStatusInfo)jobj;
        }
        /// <summary>
        /// Информация о компании
        /// </summary>
        /// <returns></returns>
        public CompanyInfo? GetCompanyInfo()
        {
            SendJson(new Request()
            {
                Type = "getRegistrationInfo",
            }, out var answer);

            if (answer.Code == -1) return null;
            var jobj = DeserializeHelper.Deserialize(answer.Json, model: new CompanyInfo(), token: "organization");
            if (jobj == null) return null;
            return (CompanyInfo)jobj;
        }
        /// <summary>
        /// Инфомация о фискальном накопителе
        /// </summary>
        /// <returns></returns>
        public FnStatistic? GetFnStatus()
        {
            SendJson(new Request()
            {
                Type = "getFnStatus",
            }, out var answer);

            if (answer.Code == -1) return null;
            var jobj = JsonConvert.DeserializeObject<FnStatistic>(answer.Json);
            return jobj ?? null;
        }
        /// <summary>
        /// Информация о смене
        /// </summary>
        /// <returns></returns>
        public Shifts? GetShiftsTotal()
        {
            SendJson(new Request()
            {
                Type = "getShiftTotals",
            }, out var answer);

            if (answer.Code == -1) return null;
            var jobj = JsonConvert.DeserializeObject<Shifts>(answer.Json);
            return jobj ?? null;
        }
        
        public string GetStatus()
        {
            _fptr.setParam(Constants.LIBFPTR_PARAM_DATA_TYPE, Constants.LIBFPTR_DT_STATUS);
            _fptr.queryData();
            var status = $"---------Status-----------/n" +
                         $" operatorID {_fptr.getParamInt(Constants.LIBFPTR_PARAM_OPERATOR_ID)}/n" +
                         $" logicalNumber {_fptr.getParamInt(Constants.LIBFPTR_PARAM_LOGICAL_NUMBER)}" +
                         $" shiftState {_fptr.getParamInt(Constants.LIBFPTR_PARAM_SHIFT_STATE)}" +
                         $" model = {_fptr.getParamInt(Constants.LIBFPTR_PARAM_MODEL)}" +
                         $" mode = {_fptr.getParamInt(Constants.LIBFPTR_PARAM_MODEL)}" +
                         $" submode = {_fptr.getParamInt(Constants.LIBFPTR_PARAM_SUBMODE)}" +
                         $" receiptNumber = {_fptr.getParamInt(Constants.LIBFPTR_PARAM_RECEIPT_NUMBER)}" +
                         $" documentNumber = {_fptr.getParamInt(Constants.LIBFPTR_PARAM_RECEIPT_NUMBER)}" +
                         $" shiftNumber = {_fptr.getParamInt(Constants.LIBFPTR_PARAM_SHIFT_NUMBER)}" +
                         $" receiptType = {_fptr.getParamInt(Constants.LIBFPTR_PARAM_RECEIPT_TYPE)}" +
                         $" documentType = {_fptr.getParamInt(Constants.LIBFPTR_PARAM_DOCUMENT_TYPE)}" +
                         $" lineLength = {_fptr.getParamInt(Constants.LIBFPTR_PARAM_RECEIPT_LINE_LENGTH)}" +
                         $" lineLengthPix = {_fptr.getParamInt(Constants.LIBFPTR_PARAM_RECEIPT_LINE_LENGTH_PIX)}" +
                         $" receiptSum = {_fptr.getParamDouble(Constants.LIBFPTR_PARAM_RECEIPT_SUM)}" +
                         $" isFiscalDevice ={_fptr.getParamBool(Constants.LIBFPTR_PARAM_FISCAL)}" +
                         $" isFiscalFN = {_fptr.getParamBool(Constants.LIBFPTR_PARAM_FN_FISCAL)}" +
                         $" isFNPresent = {_fptr.getParamBool(Constants.LIBFPTR_PARAM_FN_PRESENT)}" +
                         $" isCashDrawerOpened = {_fptr.getParamBool(Constants.LIBFPTR_PARAM_CASHDRAWER_OPENED)}" +
                         $" isPaperPresent = {_fptr.getParamBool(Constants.LIBFPTR_PARAM_RECEIPT_PAPER_PRESENT)}" +
                         $" isPaperNearEnd = {_fptr.getParamBool(Constants.LIBFPTR_PARAM_PAPER_NEAR_END)}" +
                         $" isCoverOpened = {_fptr.getParamBool(Constants.LIBFPTR_PARAM_COVER_OPENED)}" +
                         $" isPrinterConnectionLost = {_fptr.getParamBool(Constants.LIBFPTR_PARAM_PRINTER_CONNECTION_LOST)}" +
                         $" isPrinterError = {_fptr.getParamBool(Constants.LIBFPTR_PARAM_PRINTER_ERROR)}" +
                         $" isCutError = {_fptr.getParamBool(Constants.LIBFPTR_PARAM_CUT_ERROR)}" +
                         $" isPrinterOverheat = {_fptr.getParamBool(Constants.LIBFPTR_PARAM_PRINTER_OVERHEAT)}" +
                         $" isDeviceBlocked = {_fptr.getParamBool(Constants.LIBFPTR_PARAM_BLOCKED)}" +
                         $" dateTime = {_fptr.getParamDateTime(Constants.LIBFPTR_PARAM_DATE_TIME)}" +
                         $" serialNumber = {_fptr.getParamString(Constants.LIBFPTR_PARAM_SERIAL_NUMBER)}" +
                         $" modelName = {_fptr.getParamString(Constants.LIBFPTR_PARAM_MODEL_NAME)}" +
                         $" firmwareVersion = {_fptr.getParamString(Constants.LIBFPTR_PARAM_UNIT_VERSION)}" +
                         $"--------------------------";

            return status;
        }
        
        /// <summary>
        /// Статус принтера
        /// </summary>
        /// <returns></returns>
        public PrinterStatus GetPrinterStatus()
        {
            _fptr.setParam(Constants.LIBFPTR_PARAM_DATA_TYPE, Constants.LIBFPTR_DT_STATUS);
            _fptr.queryData();
            return new PrinterStatus()
            {
                CutError = _fptr.getParamBool(Constants.LIBFPTR_PARAM_CUT_ERROR),
                PrinterOverheat = _fptr.getParamBool(Constants.LIBFPTR_PARAM_PRINTER_OVERHEAT),
                PrinterError = _fptr.getParamBool(Constants.LIBFPTR_PARAM_PRINTER_ERROR),
                PrinterConnectionLost = _fptr.getParamBool(Constants.LIBFPTR_PARAM_PRINTER_CONNECTION_LOST),
                PaperAvailability = _fptr.getParamBool(Constants.LIBFPTR_PARAM_RECEIPT_PAPER_PRESENT)
            };
        }

        #endregion

        #region Documents

        /// <summary>
        /// Получить документ по номеру
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public string GetDocument(int number)
        {
            _fptr.setParam(Constants.LIBFPTR_PARAM_RECORDS_TYPE, Constants.LIBFPTR_RT_FN_DOCUMENT_TLVS);
            _fptr.setParam(Constants.LIBFPTR_PARAM_DOCUMENT_NUMBER, number);
            _fptr.beginReadRecords();
            String recordsId = _fptr.getParamString(Constants.LIBFPTR_PARAM_RECORDS_ID);

            StringBuilder result = new StringBuilder();
            while (ReadNextRecord(_fptr, recordsId) == 0)
            {
                result.Append(Parse(_fptr, 0,
                    _fptr.getParamString(Constants.LIBFPTR_PARAM_TAG_NAME),
                    _fptr.getParamByteArray(Constants.LIBFPTR_PARAM_TAG_VALUE),
                    _fptr.getParamInt(Constants.LIBFPTR_PARAM_TAG_NUMBER),
                    _fptr.getParamInt(Constants.LIBFPTR_PARAM_TAG_TYPE)));
            }

            EndReadRecords(_fptr, recordsId);
            return result.ToString();
        }
        
        private static int ReadNextRecord(IFptr fptr, String recordsId)
        {
            fptr.setParam(Constants.LIBFPTR_PARAM_RECORDS_ID, recordsId);
            return fptr.readNextRecord();
        }

        private static int EndReadRecords(IFptr fptr, String recordsId)
        {
            fptr.setParam(Constants.LIBFPTR_PARAM_RECORDS_ID, recordsId);
            return fptr.endReadRecords();
        }

        private static string Printable(int offset, String tagName, uint tagNumber, String tagValue, bool newLineBeforeValue, bool newLineAfterValue)
        {
            return  $"{new StringBuilder().Insert(0, "  ", offset).ToString()}{tagName} ({tagNumber}): {(newLineBeforeValue ? "\n" : "")}{tagValue}{(newLineAfterValue ? "\n" : "")}";
        }

        private static string Parse(IFptr fptr, int printOffset, String tagName, byte[] tagValue, uint tagNumber, uint tagType)
        {
            switch ((int)tagType)
            {
                case Constants.LIBFPTR_TAG_TYPE_BITS:
                case Constants.LIBFPTR_TAG_TYPE_BYTE:
                case Constants.LIBFPTR_TAG_TYPE_UINT_16:
                case Constants.LIBFPTR_TAG_TYPE_UINT_32:
                    return Printable(printOffset, tagName, tagNumber, fptr.getParamInt(Constants.LIBFPTR_PARAM_TAG_VALUE).ToString(), false, true);
                case Constants.LIBFPTR_TAG_TYPE_VLN:
                case Constants.LIBFPTR_TAG_TYPE_FVLN:
                    return Printable(printOffset, tagName, tagNumber, fptr.getParamDouble(Constants.LIBFPTR_PARAM_TAG_VALUE).ToString(CultureInfo.InvariantCulture), false, true);
                case Constants.LIBFPTR_TAG_TYPE_BOOL:
                    return Printable(printOffset, tagName, tagNumber, fptr.getParamBool(Constants.LIBFPTR_PARAM_TAG_VALUE).ToString(), false, true);
                case Constants.LIBFPTR_TAG_TYPE_ARRAY:
                    return Printable(printOffset, tagName, tagNumber, BitConverter.ToString(fptr.getParamByteArray(Constants.LIBFPTR_PARAM_TAG_VALUE)), false, true);
                case Constants.LIBFPTR_TAG_TYPE_STRING:
                    return Printable(printOffset, tagName, tagNumber, fptr.getParamString(Constants.LIBFPTR_PARAM_TAG_VALUE), false, true);
                case Constants.LIBFPTR_TAG_TYPE_UNIX_TIME:
                    return Printable(printOffset, tagName, tagNumber, fptr.getParamDateTime(Constants.LIBFPTR_PARAM_TAG_VALUE).ToString(CultureInfo.InvariantCulture), false, true);
                case Constants.LIBFPTR_TAG_TYPE_STLV:
                    fptr.setParam(Constants.LIBFPTR_PARAM_RECORDS_TYPE, Constants.LIBFPTR_RT_PARSE_COMPLEX_ATTR);
                    fptr.setParam(Constants.LIBFPTR_PARAM_TAG_VALUE, tagValue);
                    fptr.beginReadRecords();
                    var recordsId = fptr.getParamString(Constants.LIBFPTR_PARAM_RECORDS_ID);

                    var result = new StringBuilder();
                    while (ReadNextRecord(fptr, recordsId) == 0)
                    {
                        result.Append(Parse(fptr, printOffset + 1,
                                fptr.getParamString(Constants.LIBFPTR_PARAM_TAG_NAME),
                                fptr.getParamByteArray(Constants.LIBFPTR_PARAM_TAG_VALUE),
                                fptr.getParamInt(Constants.LIBFPTR_PARAM_TAG_NUMBER),
                                fptr.getParamInt(Constants.LIBFPTR_PARAM_TAG_TYPE)));
                    }

                    EndReadRecords(fptr, recordsId);
                    return Printable(printOffset, tagName, tagNumber, result.ToString(), true, false);
                default:
                    return "";
            }
        }

        #endregion

        #region Enum

                private static string GetTaxType(TaxationTypeEnum taxationTypeEnum) => taxationTypeEnum switch
        {
            TaxationTypeEnum.Osn => "osn",
            TaxationTypeEnum.TtEsn => "esn",
            TaxationTypeEnum.TtPatent => "patent",
            TaxationTypeEnum.UsnIncome => "usnIncome",
            TaxationTypeEnum.UsnIncomeOutcome => "usnIncomeOutcome",
            _ => ""
        };

        private static string GetTypeReceipt(TypeReceipt typeReceiptEnum) => typeReceiptEnum switch
        {
            TypeReceipt.Buy => "buy",
            TypeReceipt.Sell => "sell",
            TypeReceipt.BuyReturn => "buyReturn",
            TypeReceipt.SellReturn => "sellReturn",
            TypeReceipt.BuyCorrection => "buyCorrection",
            TypeReceipt.SellCorrection => "sellCorrection",
            TypeReceipt.BuyReturnCorrection => "buyReturnCorrection",
            TypeReceipt.SellReturnCorrection => "sellReturnCorrection",
            _ => ""
        };

        private static string GetMeasurementUnitEnum(MeasurementUnitEnum measurementUnitEnum) => measurementUnitEnum switch
        {
            MeasurementUnitEnum.Centimeter => "centimeter",
            MeasurementUnitEnum.Decimeter => "decimeter",
            MeasurementUnitEnum.Gram => "gram",
            MeasurementUnitEnum.Kilogram => "kilogram",
            MeasurementUnitEnum.Liter => "liter",
            MeasurementUnitEnum.Meter => "meter",
            MeasurementUnitEnum.Milliliter => "milliliter",
            MeasurementUnitEnum.Piece => "piece",
            MeasurementUnitEnum.Ton => "ton",
            MeasurementUnitEnum.SquareCentimeter => "squareCentimeter",
            MeasurementUnitEnum.SquareMeter => "squareMeter",
            _ => ""
        };

        private static string GetPaymentObjectEnum(PaymentObjectEnum paymentObjectEnum) => paymentObjectEnum switch
        {
            PaymentObjectEnum.Another => "another",
            PaymentObjectEnum.Commodity => "commodity",
            PaymentObjectEnum.Excise => "excise",
            PaymentObjectEnum.Job => "job",
            PaymentObjectEnum.Payment => "payment",
            PaymentObjectEnum.Service => "service",
            PaymentObjectEnum.CommodityWithMarking => "commodityWithMarking",
            PaymentObjectEnum.CommodityWithoutMarking => "commodityWithoutMarking",
            PaymentObjectEnum.ExciseWithMarking => "exciseWithMarking",
            PaymentObjectEnum.ExciseWithoutMarking => "exciseWithoutMarking",
            _ => ""
        };

        private static string GetTaxTypeEnum(TaxTypeEnum taxTypeEnum) => taxTypeEnum switch
        {
            TaxTypeEnum.None => "none",
            TaxTypeEnum.Vat0 => "vat0",
            TaxTypeEnum.Vat10 => "vat10",
            TaxTypeEnum.Vat20 => "vat20",
            TaxTypeEnum.Vat110 => "vat110",
            TaxTypeEnum.Vat120 => "vat120",
            _ => ""
        };

        private static string GetPaymentTypeEnum(PaymentTypeEnum paymentTypeEnum) => paymentTypeEnum switch
        {
            PaymentTypeEnum.Cash => "cash",
            PaymentTypeEnum.Credit => "credit",
            PaymentTypeEnum.Electronically => "electronically",
            PaymentTypeEnum.Other => "other",
            PaymentTypeEnum.Prepaid => "prepaid",
            _ => ""
        };

        #endregion

        #endregion
        
    }

    #region Enum

     public enum PaymentTypeEnum
    {
        /// <summary>
        /// наличными
        /// </summary>
        Cash,
        /// <summary>
        /// безналичными
        /// </summary>
        Electronically,
        /// <summary>
        /// предварительная оплата (аванс)
        /// </summary>
        Prepaid,
        /// <summary>
        /// последующая оплата (кредит)
        /// </summary>
        Credit,
        /// <summary>
        ///  иная форма оплаты (встречное предоставление)
        /// </summary>
        Other,
    }
    public enum TaxTypeEnum
    {
        /// <summary>
        /// НДС 10%
        /// </summary>
        Vat10,
        /// <summary>
        ///  НДС рассчитанный 10/110;
        /// </summary>
        Vat110,
        /// <summary>
        /// НДС 0%
        /// </summary>
        Vat0,
        /// <summary>
        /// не облагается
        /// </summary>
        None,
        /// <summary>
        /// НДС 20%
        /// </summary>
        Vat20,
        /// <summary>
        /// НДС рассчитанный 20/120
        /// </summary>
        Vat120
    }
    public enum PaymentObjectEnum
    {
        /// <summary>
        /// товар
        /// </summary>
        Commodity,
        /// <summary>
        /// подакцизный товар
        /// </summary>
        Excise,
        /// <summary>
        /// работа
        /// </summary>
        Job,
        /// <summary>
        /// услуга
        /// </summary>
        Service,
        /// <summary>
        /// платеж
        /// </summary>
        Payment,
        /// <summary>
        /// иной предмет расчета
        /// </summary>
        Another,
        /// <summary>
        /// подакцизный товар, не имеющий код маркировки
        /// </summary>
        ExciseWithoutMarking,
        /// <summary>
        /// подакцизный товар, имеющий код маркировки
        /// </summary>
        ExciseWithMarking,
        /// <summary>
        /// товар, не имеющий код маркировки
        /// </summary>
        CommodityWithoutMarking,
        /// <summary>
        /// товар, имеющий код маркировки
        /// </summary>
        CommodityWithMarking,
    }
    public enum MeasurementUnitEnum
    {
        Piece,
        Gram,
        Kilogram,
        Ton,
        Centimeter,
        Decimeter,
        Meter,
        SquareCentimeter,
        SquareMeter,
        Milliliter,
        Liter,
    }
    public enum TaxationTypeEnum
    {
        /// <summary>
        /// общая
        /// </summary>
        Osn,
        /// <summary>
        /// упрощенная доход
        /// </summary>
        UsnIncome,
        /// <summary>
        /// упрощенная доход минус расход
        /// </summary>
        UsnIncomeOutcome,
        /// <summary>
        /// единый сельскохозяйственный доход
        /// </summary>
        TtEsn,
        /// <summary>
        /// патентная система налогообложения
        /// </summary>
        TtPatent
        
    }
    public enum TypeReceipt
    {
        /// <summary>
        /// чек прихода (продажи)
        /// </summary>
        Sell,
        /// <summary>
        /// чек возврата прихода (продажи)
        /// </summary>
        SellReturn,
        /// <summary>
        /// чек коррекции прихода
        /// </summary>
        SellCorrection,
        /// <summary>
        /// чек коррекции возврата прихода
        /// </summary>
        SellReturnCorrection,
        /// <summary>
        /// чек расхода (покупки
        /// </summary>
        Buy,
        /// <summary>
        /// чек возврата расхода (покупки)
        /// </summary>
        BuyReturn,
        /// <summary>
        /// чек коррекции расхода
        /// </summary>
        BuyCorrection,
        /// <summary>
        /// чек коррекции возврата расхода
        /// </summary>
        BuyReturnCorrection
    }

    #endregion

   
}
