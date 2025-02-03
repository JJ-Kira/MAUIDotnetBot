using TestDotnetBot;

namespace TestDotnetBot.MAUI
{
    public partial class MainPage : ContentPage
    {
        private readonly MyApplication evergineApplication;
        private int count = 0;

        public MainPage()
        {
            InitializeComponent();
            this.evergineApplication = new MyApplication();

            // Links the Evergine application to the EvergineView defined in XAML
            this.evergineView.Application = this.evergineApplication;
        }

        private void OnRotateLeftClicked(object sender, EventArgs e)
        {
            this.evergineApplication.RotateLeft();
        }

        private void OnRotateRightClicked(object sender, EventArgs e)
        {
            this.evergineApplication.RotateRight();
        }
    }
}