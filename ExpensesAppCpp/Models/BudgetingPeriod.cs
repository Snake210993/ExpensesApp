using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpensesAppCpp.Models
{
    public partial class BudgetingPeriod : ObservableObject
    {
        [ObservableProperty]
        public DateTime startDate;

        [ObservableProperty]
        public DateTime endDate;

        [ObservableProperty]
        public ObservableCollection<Receipt> receipts = new();

        public string DisplayName => $"{StartDate:dd MMM} - {EndDate:dd MMM}";

        [ObservableProperty]
        public decimal totalSpent;

        [ObservableProperty]
        public bool isExpanded;


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
