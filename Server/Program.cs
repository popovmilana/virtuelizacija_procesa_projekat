using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class Program
    {
        static void Main(string[] args)
        {

            ServiceHost host = new ServiceHost(typeof(SensorService));
            try
            {
                host.Open();
                Console.WriteLine("Server je pokrenut. Pritisni enter za zaustavljanje...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Greska prilikom pokretanja servera: " + ex.Message);
            }
            finally
            {
                host.Close();
            }
        }
    }
}
