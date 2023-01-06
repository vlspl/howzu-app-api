using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Howzu_API.Model
{
    public class PaymentSignature
    {
        public int BookingId { get; set; }
        public int LabId { get; set; }
        public string MerchantLogin { get; set; }
        public string MerchantPass { get; set; }
        public string MerchantDiscretionaryData { get; set; }
        public string ProductID { get; set; }
        public string ClientCode { get; set; }
        public string CustomerAccountNo { get; set; }
        public string TransactionType { get; set; }
        public string TransactionAmount { get; set; }
        public string TransactionCurrency { get; set; }
        public string TransactionServiceCharge { get; set; }
        public string TransactionID { get; set; }
        public string TransactionDateTime { get; set; }
        public string BankID { get; set; }       
        public string TestId { get; set; }
      //  public string TestPrice { get; set; }

    }
}
