using CommunityToolkit.Maui.Converters;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpensesAppCpp.Models;
using ExpensesAppCpp.Helper;
using ExpensesAppCpp.ErrorHandling;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
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
        public async Task AddPeriod()
        {
            if (Start > End)
            {
                await ErrorHandlingHelper.ShowPopup("Start date cannot be after end date.");
                return;
            }
            if (Start == End)
            {
                await ErrorHandlingHelper.ShowPopup("Start date cannot be the same as end date.");
                return;
            }

            var receipts = new ObservableCollection<Receipt>
            {

            };

            BudgetPeriods.Add(new BudgetingPeriod
            {
                StartDate = Start,
                EndDate = End,
                Receipts = receipts,
                TotalSpent = receipts.Sum(r => r.Amount)
            });

            //save state
            await _budgetData.SaveAsync();
        }
        [RelayCommand]
        async Task AddReceipt(BudgetingPeriod period)
        {
            if (period == null) return;

            //either open camera/laod from gallery or manually add receipt


            period.TotalSpent = period.Receipts.Sum(r => r.Amount);

            Trace.WriteLine($"Receipt Added! Total: {period.TotalSpent}");

            await _budgetData.SaveAsync();
        }
        [RelayCommand]
        public async Task RemovePeriod(BudgetingPeriod period)
        {
            if (BudgetPeriods.Contains(period))
            {
                if (!await ErrorHandlingHelper.ShowConfirmationPopup("Are you sure you want to delete this period?"))
                {
                    return; // User cancelled the deletion
                }
                BudgetPeriods.Remove(period);
                await _budgetData.SaveAsync();
            }
            
        }

        [RelayCommand]
        public void ToggleExpand(BudgetingPeriod period)
        {
            if (period == null) return;
            period.IsExpanded = !period.IsExpanded;

            // Notify UI by refreshing the collection
            var index = BudgetPeriods.IndexOf(period);
            if (index >= 0)
            {
                BudgetPeriods[index] = period;
            }
        }

        [RelayCommand]
        public async Task DeleteReceipt(Receipt receipt)
        {
            var period = BudgetPeriods.FirstOrDefault(p => p.Receipts.Contains(receipt));
            if (period == null) return;


            if ( !await ErrorHandlingHelper.ShowConfirmationPopup("Are you sure you want to delete this receipt?"))
            {
                return; // User cancelled the deletion
            }

            period.RemoveReceipt(receipt); // This updates TotalSpent too
            period.TotalSpent = period.Receipts.Sum(r => r.Amount);

            await _budgetData.SaveAsync();
        }

        [RelayCommand]
        public async Task EditReceipt()
        {
            await ErrorHandlingHelper.ShowPopup("Edit Receipt functionality is not implemented yet.");
        }


    }
}
