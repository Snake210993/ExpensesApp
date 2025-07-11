namespace ExpensesAppCpp
{
    public partial class ExpenditureCollection : ContentPage
    {
        private async void OnBack(object? sender, EventArgs e)
        {
            // Navigate back to the previous page
            await Navigation.PopAsync();
        }
    }
}
