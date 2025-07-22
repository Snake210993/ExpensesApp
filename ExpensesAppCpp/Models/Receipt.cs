using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpensesAppCpp.Models
{
    public struct Receipt
    {
        public DateTime Date { get; set; }
        public string StoreName { get; set; }
        public decimal Amount { get; set; }
    }
}
