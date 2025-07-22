using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpensesAppCpp.Models
{
    public class BudgetData
    {
        public ObservableCollection<BudgetingPeriod> BudgetPeriods { get; } = new();
    }
}
