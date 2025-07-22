using CommunityToolkit.Maui.Views;
using ExpensesAppCpp.ViewModel;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ExpensesAppCpp.Models;


namespace ExpensesAppCpp.PopUp;

public partial class CustomPopUp : Popup
{
	public CustomPopUp(PopUpViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
    }
}