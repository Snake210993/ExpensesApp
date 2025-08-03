using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ExpensesAppCpp.Models
{
    public class BudgetData
    {
        public ObservableCollection<BudgetingPeriod> BudgetPeriods { get; } = new();

        private static readonly string SaveFilePath = Path.Combine(FileSystem.AppDataDirectory, "budget_periods.json");

        public async Task SaveAsync()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(BudgetPeriods, options);
            await File.WriteAllTextAsync(SaveFilePath, json);
        }

        public async Task LoadAsync()
        {
            if (!File.Exists(SaveFilePath))
                return;

            var json = await File.ReadAllTextAsync(SaveFilePath);
            var loaded = JsonSerializer.Deserialize<ObservableCollection<BudgetingPeriod>>(json);

            if (loaded != null)
            {
                BudgetPeriods.Clear();
                foreach (var period in loaded)
                    BudgetPeriods.Add(period);
            }
        }
    }
}
