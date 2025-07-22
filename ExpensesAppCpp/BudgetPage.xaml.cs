using System.Collections.ObjectModel;
using System.Windows.Input;
using ExpensesAppCpp.Models;
using ExpensesAppCpp.ViewModel;

namespace ExpensesAppCpp;

public partial class BudgetPage : ContentPage
{

    public BudgetPage(BudgetPageViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }




}