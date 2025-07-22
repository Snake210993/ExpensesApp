using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpensesAppCpp.Models
{
    public class BudgetingPeriod
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<Receipt> Receipts { get; set; } = new();
        public string DisplayName => $"{StartDate:dd MMM} - {EndDate:dd MMM}";
        public decimal TotalSpent { get; set; }


        public void AddReceipt(Receipt receipt)
        {
            Receipts.Add(receipt);
        }
        public void RemoveReceipt(Receipt receipt)
        {
            Receipts.Remove(receipt);
        }

    }
}
