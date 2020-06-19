using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundJob
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Main method execution has been started");
            Console.WriteLine("======================================");
            UserRegistration.RegisterUser();
            OrderProcessing.ProcessOrder();
            Console.WriteLine("======================================");
            Console.WriteLine("Main method execution has been completed");

        }
    }
}
