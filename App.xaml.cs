using System;

namespace LizardButton
{
    /// <summary>
    /// Application bootstrap class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// </summary>
        public App()
        {
            try
            {
                InitializeComponent();
                MainPage = new MainPage();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Application initialization failed: {ex.Message}");
                throw;
            }
        }
    }
}
