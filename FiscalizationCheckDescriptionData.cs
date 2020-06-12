
using Newtonsoft.Json;
using System.Collections.Generic;

namespace AAP.OTAFinance.Integration.Uniteller.FiscalizationPayment
{
    /// <summary>
    /// вид платежного средства
    /// </summary>
    public enum PaymentsKind
    {
        /// <summary>
        /// Оплата банковской картой
        /// </summary>
        CreditCardPayment = 1,

        /// <summary>
        /// Оплата электронной валютой
        /// </summary>
        PaymentByElectronicCurrency = 2,

        /// <summary>
        /// Оплата с помощью кредитной организации
        /// </summary>
        PaymentWithCreditInstitution = 3,

        /// <summary>
        /// Оплата дополнительным платежным средством 
        /// </summary>
        PaymentByAdditionalMeansPayment = 4
    }

    public enum PaymentsType
    {
        /// <summary>
        /// Подарочные карты Мерчанта
        /// </summary>
        MerchantGiftCards = 1,

        /// <summary>
        /// Бонусы - авансы Мерчанта
        /// </summary>
        BonusesMerchantAdvances = 2,

        /// <summary>
        /// Прямой аванс Мерчанта
        /// </summary>
        DirectAdvanceMerchant = 3,

        /// <summary>
        /// Использование авансов/билетов
        /// </summary>
        UsingAdvancesOrTickets = 4,

        /// <summary>
        /// Платеж через кредитную организацию (банкомат) 
        /// </summary>
        PaymentThroughCreditInstitutionATM = 5,

        /// <summary>
        /// Платеж через кредитную организацию (online) 
        /// </summary>
        PaymentThroughCreditInstitutionOnline = 6,

        /// <summary>
        /// Безналичное перечисление через банк
        /// </summary>
        CashlessPaymentViaBank = 7,

        /// <summary>
        /// Оплата онлайн кредитом
        /// </summary>
        PaymentOnlineLoan = 8,

        /// <summary>
        /// Оплата по СМС 
        /// </summary>
        SMSpayment = 9,

        /// <summary>
        /// Эквайринг внешний
        /// </summary>
        ExternalAcquiring = 10,

        /// <summary>
        /// Платеж через терминал электронными
        /// </summary>
        ElectronPaymentViaTerminal = 11,

        /// <summary>
        /// Платеж через терминал наличными
        /// </summary>
        CashPaymentViaTerminal = 12,

        /// <summary>
        /// Наличные
        /// </summary>
        Cash = 13,

        /// <summary>
        /// Продажа в кредит
        /// </summary>
        SaleOnCredit = 14
    }

    /// <summary>
    /// код системы налогообложения
    /// </summary>
    public enum Taxmode
    {
        /// <summary>
        /// Общая система налогообложения
        /// </summary>
        GeneralTaxationSystem = 0,
 
        /// <summary>
        /// Упрощённая система налогообложения (Доход)
        /// </summary>
        SimplifiedTaxSystemIncome = 1,

        /// <summary>
        /// Упрощённая СН (Доход минус Расход)
        /// </summary>
        SimplifiedTaxSystemIncomeMinusExpense = 2,

        /// <summary>
        /// Единый налог на вмененный доход
        /// </summary>
        SingleTaxImputedIncome = 3,

        /// <summary>
        /// Единый сельскохозяйственный налог
        /// </summary>
        SingleAgriculturalTax = 4,

        /// <summary>
        /// Патентная система налогообложения
        /// </summary>
        PatentTaxSystem = 5
    }

    /// <summary>
    /// Список допустимых значений ставки НДС
    /// </summary>
    public enum VAT
    {
        NotVAT = -1,
        Taxed0 = 0,
        Taxed10 = 10,
        Taxed20 = 20,
        //110 облагается НДС по ставке 10/110
        //120 облагается НДС по ставке 20/120
    };

    public enum PayAttribute
    {
        /// <summary>
        /// Полная предварительная оплата до момента передачи предмета расчёта
        /// </summary>
        FullAdvancePayment = 1,

        /// <summary>
        /// Частичная предварительная оплата до момента передачи предмета расчёта
        /// </summary>
        PartialPrepayment = 2,

        /// <summary>
        /// Аванс
        /// </summary>
        PrepaidExpense = 3,

        /// <summary>
        /// Полная оплата, в том числе с учётом аванса (предварительной оплаты) в момент передачи предмета расчёта
        /// </summary>
        FullPaymentIncludingAdvancePayment = 4,

        /// <summary>
        /// Частичная оплата предмета расчёта в момент его передачи с по-следующей оплатой в кредит
        /// </summary>
        PartialPaymentSubjectAtTimeTransfer = 5,

        /// <summary>
        /// Передача предмета расчёта без его оплаты в момент его передачи с последующей оплатой в кредит
        /// </summary>
        TransferSubjectWithoutPayment = 6,

        /// <summary>
        /// Оплата предмета расчёта после его передачи с оплатой в кредит (оплата кредита)
        /// </summary>
        PaymentSettlementAfterTransferWithPaymentOnCredit = 7
    };


    public class Receipt
    {
        [JsonProperty(PropertyName = "customer", NullValueHandling = NullValueHandling.Ignore)]
        public Сustomer customer;

        [JsonProperty(PropertyName = "cashier", NullValueHandling = NullValueHandling.Ignore)]
        public Cashier cashier;

        /// <summary>
        /// код системы налогообложения
        /// </summary>
        [JsonProperty(PropertyName = "taxmode", NullValueHandling = NullValueHandling.Ignore)]
        public Taxmode taxmode;

        [JsonProperty(PropertyName = "lines", NullValueHandling = NullValueHandling.Ignore)]
        public List<Lines> lines;

        /// <summary>
        /// Данные мерчанта.
        /// Опциональный параметр с произвольными данными от мерчанта
        /// Транслируется в неизменном виде во всех фискализированных
        /// чеках, созданных в процессе оплаты по данному чеку.
        /// Формат: json-объект произвольной внутренней структуры
        /// </summary>
        [JsonProperty(PropertyName = "optional", NullValueHandling = NullValueHandling.Ignore)]
        public string Optional;

        [JsonProperty(PropertyName = "params", NullValueHandling = NullValueHandling.Ignore)]
        public CheckParams сheckParams;

        /// <summary>
        /// Информации об оплате дополнительными платежными средствами.
        /// Обязательный блок
        /// </summary>
        [JsonProperty(PropertyName = "payments", NullValueHandling = NullValueHandling.Ignore)]
        public List<Payments> payments;

        /// <summary>
        /// итоговая сумма чека
        /// </summary>
        [JsonProperty(PropertyName = "total", NullValueHandling = NullValueHandling.Ignore)]
        public float total;
    }

    /// <summary>
    /// Контакты плательщика для отправки текста фискального чека.
    /// Этот блок может отсутствовать целиком,
    /// или в нем могут отсутствовать какие то элементы.
    /// </summary>
    public class Сustomer
    {
        /// <summary>
        /// номер телефона плательщика
        /// </summary>
        [JsonProperty(PropertyName = "phone", NullValueHandling = NullValueHandling.Ignore)]
        public string phone;

        /// <summary>
        /// адрес электронной почты плательщика
        /// </summary>
        [JsonProperty(PropertyName = "email", NullValueHandling = NullValueHandling.Ignore)]
        public string email;

        /// <summary>
        /// идентификатор плательщика, присвоенный мерчантом
        /// </summary>
        [JsonProperty(PropertyName = "id", NullValueHandling = NullValueHandling.Ignore)]
        public int id;

        /// <summary>
        /// Название покупателя
        /// </summary>
        [JsonProperty(PropertyName = "name", NullValueHandling = NullValueHandling.Ignore)]
        public string name;

        /// <summary>
        /// ИНН покупателя
        /// </summary>
        [JsonProperty(PropertyName = "inn", NullValueHandling = NullValueHandling.Ignore)]
        public string inn;
    }

    public class Cashier
    {
        /// <summary>
        /// Ф. И. О. кассира
        /// </summary>
        [JsonProperty(PropertyName = "name", NullValueHandling = NullValueHandling.Ignore)]
        public string name;

        /// <summary>
        /// ИНН кассира
        /// </summary>
        [JsonProperty(PropertyName = "inn", NullValueHandling = NullValueHandling.Ignore)]
        public string inn;
    }

    /// <summary>
    /// Опциональный блок
    /// дополнительные сведения о продукте
    /// </summary>
    public class Product
    {
        /// <summary>
        /// Код товара
        /// </summary>
        [JsonProperty(PropertyName = "kt", NullValueHandling = NullValueHandling.Ignore)]
        public int kt;

        /// <summary>
        /// Акциз
        /// </summary>
        [JsonProperty(PropertyName = "exc", NullValueHandling = NullValueHandling.Ignore)]
        public int exc;

        /// <summary>
        /// Код страны происхождения товара
        /// </summary>
        [JsonProperty(PropertyName = "coc")]
        public int coc;

        /// <summary>
        /// Номер таможенной декларации
        /// </summary>
        [JsonProperty(PropertyName = "ncd")]
        public int ncd;
    }

    /// <summary>
    /// Опциональный блок данных агента
    /// </summary>
    public class Agent
    {
        /// <summary>
        /// Признак агента
        /// </summary>
        [JsonProperty(PropertyName = "agentattr")]
        public int agentattr;

        /// <summary>
        /// Телефон платежного агента
        /// </summary>
        [JsonProperty(PropertyName = "agentphone")]
        public int agentphone;

        /// <summary>
        /// Телефон оператора по приему платежей
        /// </summary>
        [JsonProperty(PropertyName = "accopphone")]
        public int accopphone;

        /// <summary>
        /// Телефон оператора перевода
        /// </summary>
        [JsonProperty(PropertyName = "opphone")]
        public int opphone;

        /// <summary>
        /// наименование оператора перевода
        /// </summary>
        [JsonProperty(PropertyName = "opname")]
        public string opname;

        /// <summary>
        /// ИНН оператора перевода
        /// </summary>
        [JsonProperty(PropertyName = "opinn")]
        public string opinn;

        /// <summary>
        /// адрес оператора перевода
        /// </summary>
        [JsonProperty(PropertyName = "opaddress")]
        public string opaddress;

        /// <summary>
        /// операция платежного агента
        /// </summary>
        [JsonProperty(PropertyName = "operation")]
        public string operation;

        /// <summary>
        /// наименование поставщика
        /// </summary>
        [JsonProperty(PropertyName = "suppliername")]
        public string suppliername;

        /// <summary>
        /// ИНН поставщика
        /// </summary>
        [JsonProperty(PropertyName = "supplierinn")]
        public string supplierinn;

        /// <summary>
        /// телефон поставщика
        /// </summary>
        [JsonProperty(PropertyName = "supplierphone")]
        public string supplierphone;
    }

    /// <summary>
    /// Описание позиции.
    /// Все поля обязательны и не могут быть пустыми,
    /// поле qty не может иметь нулевое значение
    /// </summary>
    public class Lines
    {
        /// <summary>
        /// наименование позиции
        /// </summary>
        [JsonProperty(PropertyName = "name", NullValueHandling = NullValueHandling.Ignore)]
        public string name;

        /// <summary>
        /// цена за единицу измерения
        /// </summary>
        [JsonProperty(PropertyName = "price", NullValueHandling = NullValueHandling.Ignore)]
        public float price;

        /// <summary>
        /// количество
        /// </summary>
        [JsonProperty(PropertyName = "qty", NullValueHandling = NullValueHandling.Ignore)]
        public int qty;

        /// <summary>
        /// сумма
        /// </summary>
        [JsonProperty(PropertyName = "sum", NullValueHandling = NullValueHandling.Ignore)]
        public float sum;

        /// <summary>
        /// код ставки налогообложения
        /// </summary>
        [JsonProperty(PropertyName = "vat", NullValueHandling = NullValueHandling.Ignore)]
        public VAT vat;

        /// <summary>
        /// Признак способа расчета
        /// </summary>
        [JsonProperty(PropertyName = "payattr", NullValueHandling = NullValueHandling.Ignore)]
        public PayAttribute payattr;

        /// <summary>
        /// Признак предмета расчета
        /// </summary>
        [JsonProperty(PropertyName = "lineattr", NullValueHandling = NullValueHandling.Ignore)]
        public int lineattr;

        /// <summary>
        /// Опциональный блок
        /// дополнительные сведения о продукте
        /// </summary>
        [JsonProperty(PropertyName = "product", NullValueHandling = NullValueHandling.Ignore)]
        public Product product;

        /// <summary>
        /// Опциональный блок данных агента
        /// </summary>
        [JsonProperty(PropertyName = "agent", NullValueHandling = NullValueHandling.Ignore)]
        public Agent agent;

    }

    /// <summary>
    /// Дополнительные параметры платежа.
    /// Этот блок может отсутствовать целиком,
    /// или в нем могут отсутствовать какие-то элементы.
    /// </summary>
    public class CheckParams
    {
        /// <summary>
        /// Место расчета.
        /// в параметре place можно указать url одного из сайтов,
        /// перечисленных в Личном кабинете налоговой мерчанта.
        /// </summary>
        [JsonProperty(PropertyName = "place", NullValueHandling = NullValueHandling.Ignore)]
        public string place;
    }

    /// <summary>
    /// Обязательный блок информации об оплате дополнительными платежными средствами
    /// </summary>
    public class Payments
    {
        /// <summary>
        /// вид платежного средства
        /// </summary>
        [JsonProperty(PropertyName = "kind", NullValueHandling = NullValueHandling.Ignore)]
        public PaymentsKind kind;

        /// <summary>
        /// тип платежного средства       
        /// </summary>
        [JsonProperty(PropertyName = "type", NullValueHandling = NullValueHandling.Ignore)]
        public PaymentsType type;

        /// <summary>
        /// идентификатор платежного средства
        /// опциональный параметр – идентификатор платежного средства
        /// например, номер бонусной карты
        /// номер подарочного сертификата, лицевого счета
        /// </summary>
        [JsonProperty(PropertyName = "id", NullValueHandling = NullValueHandling.Ignore)]
        public string id;

        /// <summary>
        /// сумма оплаты платежным средством
        /// </summary>
        [JsonProperty(PropertyName = "amount", NullValueHandling = NullValueHandling.Ignore)]
        public float amount;
    }

}
