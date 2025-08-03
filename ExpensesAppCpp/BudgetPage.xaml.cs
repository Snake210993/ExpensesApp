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

    private double _swipeOffset;

    private void OnSwipeChanging(object sender, SwipeChangingEventArgs e)
    {
        _swipeOffset = e.Offset;
    }

    private void OnSwipeEnded(object sender, SwipeEndedEventArgs e)
    {
        if (Math.Abs(_swipeOffset) < 60)
        {
            (sender as SwipeView)?.Close(); // Close if swipe too short
        }
    }
}