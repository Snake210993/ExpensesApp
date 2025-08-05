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
        private TaskCompletionSource<bool>? _resultSource;

        public PopUpViewModel(string message, bool isConfirmation = false)
        {
            CustomErrorMessage = message;
            CloseButtonText = isConfirmation ? "Cancel" : "Close";
            ContinueButtonVisible = isConfirmation;

            if (isConfirmation)
                _resultSource = new TaskCompletionSource<bool>();
        }

        [ObservableProperty]
        public string customErrorMessage;
        [ObservableProperty]
        string closeButtonText;
        [ObservableProperty]
        bool continueButtonVisible;

        public Task<bool>? GetResultTask() => _resultSource?.Task;


        [RelayCommand]
        public void Close(Popup popup)
        {
            popup.CloseAsync();
            CustomErrorMessage = string.Empty; // Clear the message after closing
            _resultSource?.TrySetResult(false);

        }

        [RelayCommand]
        public void Continue(Popup popup)
        {
            popup.CloseAsync();
            CustomErrorMessage = string.Empty; // Clear the message after closing
            _resultSource?.TrySetResult(true);
        }
    }
}
