using Json2TableV2.ViewModel;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace Json2TableV2
{
    public static class Program
    {
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool FreeConsole();



        [STAThread]
        public static int Main(string[] args)
        {
            MainWindowViewModel vm = new MainWindowViewModel();


            if (args != null && args.Length > 0)
            {
                string path = args[0];
                string content = "";
                dynamic root;

                // Open and read file
                try
                {
                    content = File.ReadAllText(path);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: " + e.Message);
                    return -1;
                }



                // Parse File to JSON
                try
                {
                    root = JsonConvert.DeserializeObject<JToken>(content);
                }
                catch (JsonReaderException e)
                {
                    Console.WriteLine("Here is the problem");
                    return -1;
                }
                catch (IOException ex)
                {
                    Console.WriteLine("No, here is the problem");
                    return -1;
                }


                Console.Clear();


                string convertionType;
                // Convert
                if (args.Length < 2)
                {
                    Console.WriteLine("What do you want to convert to?");
                    convertionType = Console.ReadLine();
                }
                else
                {
                    convertionType = args[1];
                }

                if (root != null)
                {
                    switch (convertionType.ToLower())
                    {
                        case "dbml":
                            Console.WriteLine("");
                            Console.WriteLine(vm.ConvertJsonToDbml(root));
                            break;
                        case "mysql":
                            Console.WriteLine(vm.ConvertJsonToSql(root));
                            break;
                        case "beautified json":
                            Console.WriteLine(vm.BeautifiedJson(root));
                            break;
                        default:
                            Console.WriteLine("No Conversion");
                            break;
                    }

                }


                // ObservableCollection<dynamic> items = vm.ParseFileToJson(content);

                Console.WriteLine("Press anything to exit...");
                Console.ReadLine();
                return 0;
            }
            else
            {
                FreeConsole();
                var app = new App();
                return app.Run();
            }
        }
    }
}
