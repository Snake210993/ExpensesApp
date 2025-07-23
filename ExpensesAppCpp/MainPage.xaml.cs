using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Storage;
using Microsoft.Maui.Controls.PlatformConfiguration;
using SkiaSharp;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using ExpensesAppCpp.Helper;
using ExpensesAppCpp.PreProcessor;
using ExpensesAppCpp.Tesseract;
using ExpensesAppCpp.ViewModel;
using ExpensesAppCpp.Models;




#if ANDROID
using Com.Googlecode.Tesseract.Android;
using Android.Graphics;
#endif

namespace ExpensesAppCpp
{
    public partial class MainPage : ContentPage
    {

        public MainPage(MainPageViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }

    }

}
