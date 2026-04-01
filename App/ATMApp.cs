namespace ATMApp.UI
{
    public class ATMApp
    {
        public static void Main()
        {
           AppScreen.WelcomeSreen();
           Utility.PressEnter();

         if (OperatingSystem.IsWindows())
         {
            Console.WriteLine("youre using a windows computer . upgrade to macOS you broke nigga");
         }
         if(!OperatingSystem.IsAndroid())
            {
                System.Console.WriteLine("youre wise enough not to use an android device . thank your God , i would have blown up your computer ");
            }
           
        }
    }
}