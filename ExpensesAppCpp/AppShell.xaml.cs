namespace ExpensesAppCpp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(ExpensesAppCpp.ExpenditureCollection), typeof(ExpensesAppCpp.ExpenditureCollection));
        }
    }
}
