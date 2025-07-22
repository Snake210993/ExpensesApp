using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ExpensesAppCpp.ViewModel
{
    public partial class PopUpViewModel : ObservableObject
    {

        public PopUpViewModel(string message)
        {
            CustomErrorMessage = message;
            CloseButtonText = "Close";
        }

        [ObservableProperty]
        public string customErrorMessage;
        [ObservableProperty]
        string closeButtonText;



        [RelayCommand]
        public void Close(Popup popup)
        {
            popup.CloseAsync();
            CustomErrorMessage = string.Empty; // Clear the message after closing
        }
    }
}
