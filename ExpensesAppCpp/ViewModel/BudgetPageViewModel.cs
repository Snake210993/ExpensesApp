﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpensesAppCpp.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;


namespace ExpensesAppCpp.ViewModel
{
    public partial class BudgetPageViewModel : ObservableObject
    {

        private readonly BudgetData _budgetData;

        public BudgetPageViewModel(BudgetData budgetData)
        {
            Items = new ObservableCollection<string>();
            BudgetPeriods = new ObservableCollection<BudgetingPeriod>();
            Start = DateTime.Now;
            End = DateTime.Now.AddDays(30); // Default to 30 days from now
            _budgetData = budgetData;
            BudgetPeriods = _budgetData.BudgetPeriods;
        }

        [ObservableProperty]
        ObservableCollection<BudgetingPeriod> budgetPeriods;

        [ObservableProperty]
        ObservableCollection<string> items;

        [ObservableProperty]
        DateTime start;

        [ObservableProperty]
        DateTime end;

        [RelayCommand]
        public void ClearPeriods()
        {
            BudgetPeriods.Clear();
        }





        [RelayCommand]
        public void AddPeriod()
        {
            if (Start > End)
            {
                Debug.WriteLine("Start date cannot be after end date.");
                return;
            }
            if (Start == End)
            {
                Debug.WriteLine("Start date cannot be the same as end date.");
                return;
            }

            var receipts = new List<Receipt>
            {
                new Receipt { Date = Start, StoreName = "Test Store", Amount = 12.0m },
                new Receipt { Date = Start, StoreName = "adb Store", Amount = 19.0m },
                new Receipt { Date = Start, StoreName = "123 Store", Amount = 5.0m }
            };

            BudgetPeriods.Add(new BudgetingPeriod
            {
                StartDate = Start,
                EndDate = End,
                Receipts = receipts,
                TotalSpent = receipts.Sum(r => r.Amount)
            });

        }
        [RelayCommand]
        void AddReceipt(BudgetingPeriod period)
        {
            if (period == null)
            {
                Debug.WriteLine("Cannot add receipt to a null period.");
                return;
            }
            var newReceipt = new Receipt
            {
                Date = DateTime.Now,
                StoreName = "New Store",
                Amount = 10.0m // Default amount
            };
            period.AddReceipt(newReceipt);
            //recalculate total spent if needed
            var updatedPeriod = new BudgetingPeriod
            {
                StartDate = period.StartDate,
                EndDate = period.EndDate,
                Receipts = period.Receipts,
                TotalSpent = period.Receipts.Sum(r => r.Amount)
            };
            var index = BudgetPeriods.IndexOf(period);
            if (index >= 0)
            {
                BudgetPeriods[index] = updatedPeriod;
            }
            Trace.WriteLine($"Receipt Added! Total Number of Receipts: {period.Receipts.Count}");


        }
        [RelayCommand]
        public void RemovePeriod(BudgetingPeriod period)
        {
            if (BudgetPeriods.Contains(period))
            {
                BudgetPeriods.Remove(period);
            }
        }

        
    }
}
