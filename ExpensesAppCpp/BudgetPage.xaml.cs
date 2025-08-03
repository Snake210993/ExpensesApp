using ExpensesAppCpp.Models;
using ExpensesAppCpp.ViewModel;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ExpensesAppCpp;

public partial class BudgetPage : ContentPage
{

    public BudgetPage(BudgetPageViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
}