using Atol.Drivers10.Fptr;
using System;
using Newtonsoft.Json;
using AtolDriver.models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AtolDriver
{
    public class Interface
    {
        IFptr fptr;
        Operator cashier;
        Receipt receipt;
        private bool _printOpenShift = true;
        private bool _printCloseShiftReceipt = true;
        private bool _isElectronicReceipt;

        public bool PrintOpenShift { set => _printOpenShift = value; }
        public bool PrintCloseShiftReceipt { set => _printCloseShiftReceipt = value; }
        

        public Interface(int port, int speed)
        {
            fptr = new Fptr();
            fptr.setSingleSetting(Constants.LIBFPTR_SETTING_MODEL, Constants.LIBFPTR_MODEL_ATOL_AUTO.ToString());
            fptr.setSingleSetting(Constants.LIBFPTR_SETTING_PORT, Constants.LIBFPTR_PORT_COM.ToString());
            fptr.setSingleSetting(Constants.LIBFPTR_SETTING_COM_FILE, "COM" + port);
            fptr.setSingleSetting(Constants.LIBFPTR_SETTING_BAUDRATE, speed.ToString());
            fptr.applySingleSettings();
        }

        private int SendJson<T>(T request)
        {
            fptr.setParam(Constants.LIBFPTR_PARAM_JSON_DATA, JsonConvert.SerializeObject(request));
            return fptr.processJson();
        }
        
        /// <summary>
        /// Возвращает последнюю ошибку
        /// </summary>
        /// <returns></returns>
        public string ReadError()
        {
            fptr.errorCode();
            return fptr.errorDescription();
        }
        
        /// <summary>
        /// Получить номер последнего документа в ФН
        /// </summary>
        /// <returns></returns>
        public string GetLastDocumentNumber()
        {
            fptr.setParam(Constants.LIBFPTR_PARAM_DATA_TYPE, Constants.LIBFPTR_DT_STATUS);
            fptr.queryData();
            return $"{fptr.getParamInt(Constants.LIBFPTR_PARAM_DOCUMENT_NUMBER)}";
        }
        

        // private int SendJson<T>(T request, out string answer)
        // {
        //     fptr.setParam(Constants.LIBFPTR_PARAM_DATA_TYPE, Constants.LIBFPTR_DT_STATUS);
        //     var code = fptr.queryData();
        //     answer = $"{fptr.getParamInt(Constants.LIBFPTR_PARAM_SHIFT_STATE)}";
        //     return code;
        // }

        public int OpenConnection()
        {
            return fptr.open();
        }
        
        /// <summary>
        /// Утсновка оператора на смену
        /// </summary>
        /// <param name="operatorName">Имя опреатора</param>
        /// <param name="operatorInn">Инн оператора</param>
        public void SetOperator(string operatorName, string operatorInn)
        {
            cashier = new Operator
            {
                Name = operatorName,
                Vatin = operatorInn
            };
        }
        
        /// <summary>
        /// Открыть смену
        /// </summary>
        /// <returns>код ошибки</returns>
        public int OpenShift()
        {
            return SendJson(new OpenShift
            {
                Operator = cashier,
            });
        }
        
        /// <summary>
        /// Открыть чек
        /// </summary>
        /// <param name="isElectronicReceipt">Это электронный чек?</param>
        /// <param name="typeReceiptEnum">Тип чека</param>
        /// <param name="taxationTypeEnum">Тип налогообложения</param>
        public void OpenReceipt(bool isElectronicReceipt,TypeReceipt typeReceiptEnum, TaxationTypeEnum taxationTypeEnum)
        {
            receipt = new Receipt
            {
                Type = GetTypeReceipt(typeReceiptEnum),
                TaxationType = GetTaxType(taxationTypeEnum),
                Operator = cashier,
                Items = new List<Item>(),
                Payments = new List<Payments>(),
                Electronic = isElectronicReceipt
            };
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
        public void AddPosition(string name, double price, double quantity, MeasurementUnitEnum measurementUnitEnum,
            PaymentObjectEnum paymentObjectEnum, TaxTypeEnum taxationTypeEnum)
        {
            receipt.Items.Add(new Item
            {
                Type = "position",
                Name = name,
                Price = price,
                Quantity = quantity,
                MeasurementUnit = GetMeasurementUnitEnum(measurementUnitEnum),
                PaymentObject = GetPaymentObjectEnum(paymentObjectEnum),
                Amount = price * quantity,
                Tax = new Tax { Type = GetTaxTypeEnum(taxationTypeEnum) }
            });
        }
        
        /// <summary>
        /// Зарегестрировать оплату
        /// </summary>
        /// <param name="paymentTypeEnum">Тип оплаты</param>
        /// <param name="sum">Сумма</param>
        public void Pay(PaymentTypeEnum paymentTypeEnum , double sum)
        {
            receipt.Payments.Add(new Payments
            {
                Type = GetPaymentTypeEnum(paymentTypeEnum),
                Sum = sum
            });
        }

        /// <summary>
        /// Закрыть чек
        /// </summary>
        /// <returns>Код ошибки</returns>
        public int CloseReceipt()
        {
            return SendJson(receipt);
        }
        
        /// <summary>
        /// Закрыть смену
        /// </summary>
        /// <returns>Код ошибки</returns>
        public int CloseShift()
        {
            return SendJson(new CloseShift
            {
                Type = "closeShift",
                Operator = cashier
            });
        }
        
        /// <summary>
        /// Состояние смены
        /// </summary>
        /// <returns>0 - закрыта, 1 - октрыта</returns>
        public string GetShiftStatus()
        {
            fptr.setParam(Constants.LIBFPTR_PARAM_DATA_TYPE, Constants.LIBFPTR_DT_STATUS);
            var code = fptr.queryData();
            return $"{fptr.getParamInt(Constants.LIBFPTR_PARAM_SHIFT_STATE)}";
        }
        
        /// <summary>
        /// Получить документ по номеру
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public string GetDocument(int number)
        {
            fptr.setParam(Constants.LIBFPTR_PARAM_RECORDS_TYPE, Constants.LIBFPTR_RT_FN_DOCUMENT_TLVS);
            fptr.setParam(Constants.LIBFPTR_PARAM_DOCUMENT_NUMBER, number);
            fptr.beginReadRecords();
            String recordsID = fptr.getParamString(Constants.LIBFPTR_PARAM_RECORDS_ID);

            StringBuilder result = new StringBuilder();
            while (readNextRecord(fptr, recordsID) == 0)
            {
                result.Append(parse(fptr, 0,
                    fptr.getParamString(Constants.LIBFPTR_PARAM_TAG_NAME),
                    fptr.getParamByteArray(Constants.LIBFPTR_PARAM_TAG_VALUE),
                    fptr.getParamInt(Constants.LIBFPTR_PARAM_TAG_NUMBER),
                    fptr.getParamInt(Constants.LIBFPTR_PARAM_TAG_TYPE)));
            }

            endReadRecords(fptr, recordsID);
            return result.ToString();
        }
        
        private static int readNextRecord(IFptr fptr, String recordsID)
        {
            fptr.setParam(Constants.LIBFPTR_PARAM_RECORDS_ID, recordsID);
            return fptr.readNextRecord();
        }

        private static int endReadRecords(IFptr fptr, String recordsID)
        {
            fptr.setParam(Constants.LIBFPTR_PARAM_RECORDS_ID, recordsID);
            return fptr.endReadRecords();
        }

        private static String printable(int offset, String tagName, uint tagNumber, String tagValue, bool newLineBeforeValue, bool newLineAfterValue)
        {
            return String.Format("{0}{1} ({2}): {3}{4}{5}", new StringBuilder().Insert(0, "  ", offset).ToString(),
                    tagName,
                    tagNumber,
                    newLineBeforeValue ? "\n" : "",
                    tagValue,
                    newLineAfterValue ? "\n" : "");
        }

        private static String parse(IFptr fptr, int printOffset, String tagName, byte[] tagValue, uint tagNumber, uint tagType)
        {
            switch ((int)tagType)
            {
                case Constants.LIBFPTR_TAG_TYPE_BITS:
                case Constants.LIBFPTR_TAG_TYPE_BYTE:
                case Constants.LIBFPTR_TAG_TYPE_UINT_16:
                case Constants.LIBFPTR_TAG_TYPE_UINT_32:
                    return printable(printOffset, tagName, tagNumber, fptr.getParamInt(Constants.LIBFPTR_PARAM_TAG_VALUE).ToString(), false, true);
                case Constants.LIBFPTR_TAG_TYPE_VLN:
                case Constants.LIBFPTR_TAG_TYPE_FVLN:
                    return printable(printOffset, tagName, tagNumber, fptr.getParamDouble(Constants.LIBFPTR_PARAM_TAG_VALUE).ToString(), false, true);
                case Constants.LIBFPTR_TAG_TYPE_BOOL:
                    return printable(printOffset, tagName, tagNumber, fptr.getParamBool(Constants.LIBFPTR_PARAM_TAG_VALUE).ToString(), false, true);
                case Constants.LIBFPTR_TAG_TYPE_ARRAY:
                    return printable(printOffset, tagName, tagNumber, BitConverter.ToString(fptr.getParamByteArray(Constants.LIBFPTR_PARAM_TAG_VALUE)), false, true);
                case Constants.LIBFPTR_TAG_TYPE_STRING:
                    return printable(printOffset, tagName, tagNumber, fptr.getParamString(Constants.LIBFPTR_PARAM_TAG_VALUE), false, true);
                case Constants.LIBFPTR_TAG_TYPE_UNIX_TIME:
                    return printable(printOffset, tagName, tagNumber, fptr.getParamDateTime(Constants.LIBFPTR_PARAM_TAG_VALUE).ToString(), false, true);
                case Constants.LIBFPTR_TAG_TYPE_STLV:
                    fptr.setParam(Constants.LIBFPTR_PARAM_RECORDS_TYPE, Constants.LIBFPTR_RT_PARSE_COMPLEX_ATTR);
                    fptr.setParam(Constants.LIBFPTR_PARAM_TAG_VALUE, tagValue);
                    fptr.beginReadRecords();
                    String recordsID = fptr.getParamString(Constants.LIBFPTR_PARAM_RECORDS_ID);

                    StringBuilder result = new StringBuilder();
                    while (readNextRecord(fptr, recordsID) == 0)
                    {
                        result.Append(parse(fptr, printOffset + 1,
                                fptr.getParamString(Constants.LIBFPTR_PARAM_TAG_NAME),
                                fptr.getParamByteArray(Constants.LIBFPTR_PARAM_TAG_VALUE),
                                fptr.getParamInt(Constants.LIBFPTR_PARAM_TAG_NUMBER),
                                fptr.getParamInt(Constants.LIBFPTR_PARAM_TAG_TYPE)));
                    }

                    endReadRecords(fptr, recordsID);
                    return printable(printOffset, tagName, tagNumber, result.ToString(), true, false);
                default:
                    return "";
            }
        }

        
        public string GetStatus()
        {
            fptr.setParam(Constants.LIBFPTR_PARAM_DATA_TYPE, Constants.LIBFPTR_DT_STATUS);
            fptr.queryData();
            var status = $"---------Status-----------/n" +
                         $" operatorID {fptr.getParamInt(Constants.LIBFPTR_PARAM_OPERATOR_ID)}/n" +
                         $" logicalNumber {fptr.getParamInt(Constants.LIBFPTR_PARAM_LOGICAL_NUMBER)}" +
                         $" shiftState {fptr.getParamInt(Constants.LIBFPTR_PARAM_SHIFT_STATE)}" +
                         $" model = {fptr.getParamInt(Constants.LIBFPTR_PARAM_MODEL)}" +
                         $" mode = {fptr.getParamInt(Constants.LIBFPTR_PARAM_MODEL)}" +
                         $" submode = {fptr.getParamInt(Constants.LIBFPTR_PARAM_SUBMODE)}" +
                         $" receiptNumber = {fptr.getParamInt(Constants.LIBFPTR_PARAM_RECEIPT_NUMBER)}" +
                         $" documentNumber = {fptr.getParamInt(Constants.LIBFPTR_PARAM_RECEIPT_NUMBER)}" +
                         $" shiftNumber = {fptr.getParamInt(Constants.LIBFPTR_PARAM_SHIFT_NUMBER)}" +
                         $" receiptType = {fptr.getParamInt(Constants.LIBFPTR_PARAM_RECEIPT_TYPE)}" +
                         $" documentType = {fptr.getParamInt(Constants.LIBFPTR_PARAM_DOCUMENT_TYPE)}" +
                         $" lineLength = {fptr.getParamInt(Constants.LIBFPTR_PARAM_RECEIPT_LINE_LENGTH)}" +
                         $" lineLengthPix = {fptr.getParamInt(Constants.LIBFPTR_PARAM_RECEIPT_LINE_LENGTH_PIX)}" +
                         $" receiptSum = {fptr.getParamDouble(Constants.LIBFPTR_PARAM_RECEIPT_SUM)}" +
                         $" isFiscalDevice ={fptr.getParamBool(Constants.LIBFPTR_PARAM_FISCAL)}" +
                         $" isFiscalFN = {fptr.getParamBool(Constants.LIBFPTR_PARAM_FN_FISCAL)}" +
                         $" isFNPresent = {fptr.getParamBool(Constants.LIBFPTR_PARAM_FN_PRESENT)}" +
                         $" isCashDrawerOpened = {fptr.getParamBool(Constants.LIBFPTR_PARAM_CASHDRAWER_OPENED)}" +
                         $" isPaperPresent = {fptr.getParamBool(Constants.LIBFPTR_PARAM_RECEIPT_PAPER_PRESENT)}" +
                         $" isPaperNearEnd = {fptr.getParamBool(Constants.LIBFPTR_PARAM_PAPER_NEAR_END)}" +
                         $" isCoverOpened = {fptr.getParamBool(Constants.LIBFPTR_PARAM_COVER_OPENED)}" +
                         $" isPrinterConnectionLost = {fptr.getParamBool(Constants.LIBFPTR_PARAM_PRINTER_CONNECTION_LOST)}" +
                         $" isPrinterError = {fptr.getParamBool(Constants.LIBFPTR_PARAM_PRINTER_ERROR)}" +
                         $" isCutError = {fptr.getParamBool(Constants.LIBFPTR_PARAM_CUT_ERROR)}" +
                         $" isPrinterOverheat = {fptr.getParamBool(Constants.LIBFPTR_PARAM_PRINTER_OVERHEAT)}" +
                         $" isDeviceBlocked = {fptr.getParamBool(Constants.LIBFPTR_PARAM_BLOCKED)}" +
                         $" dateTime = {fptr.getParamDateTime(Constants.LIBFPTR_PARAM_DATE_TIME)}" +
                         $" serialNumber = {fptr.getParamString(Constants.LIBFPTR_PARAM_SERIAL_NUMBER)}" +
                         $" modelName = {fptr.getParamString(Constants.LIBFPTR_PARAM_MODEL_NAME)}" +
                         $" firmwareVersion = {fptr.getParamString(Constants.LIBFPTR_PARAM_UNIT_VERSION)}" +
                         $"--------------------------";

            return status;
        }
        

        private static string GetTaxType(TaxationTypeEnum taxationTypeEnum) => taxationTypeEnum switch
        {
            TaxationTypeEnum.Osn => "osn",
            TaxationTypeEnum.TtEsn => "ttEsn",
            TaxationTypeEnum.TtPatent => "ttPatent",
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
            TaxTypeEnum.No => "no",
            TaxTypeEnum.Vat0 => "vat0",
            TaxTypeEnum.Vat10 => "vat10",
            TaxTypeEnum.Vat20 => "vat20",
            TaxTypeEnum.Vat110 => "vat110",
            TaxTypeEnum.Vat120 => "vat120",
            _ => ""
        };

        private static string GetPaymentTypeEnum(PaymentTypeEnum paymentTypeEnum) => paymentTypeEnum switch
        {
            PaymentTypeEnum.cash => "cash",
            PaymentTypeEnum.credit => "credit",
            PaymentTypeEnum.electronically => "electronically",
            PaymentTypeEnum.other => "other",
            PaymentTypeEnum.prepaid => "prepaid",
            _ => ""
        };


    }

    public enum PaymentTypeEnum
    {
        /// <summary>
        /// наличными
        /// </summary>
        cash,
        /// <summary>
        /// безналичными
        /// </summary>
        electronically,
        /// <summary>
        /// предварительная оплата (аванс)
        /// </summary>
        prepaid,
        /// <summary>
        /// последующая оплата (кредит)
        /// </summary>
        credit,
        /// <summary>
        ///  иная форма оплаты (встречное предоставление)
        /// </summary>
        other,
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
        No,
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
}
