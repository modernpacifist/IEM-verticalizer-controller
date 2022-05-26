using System;
using TableAPI;


namespace IEM_verticalizer_controller
{
    internal class Program
    {
        static void InitiateConnection(ref Table table) {
            bool connectFlag = table.Connect();
            if (connectFlag) {
                Console.WriteLine("Connection was established");
            } else {
                Console.WriteLine("Connection failed, check physical state");
                Environment.Exit(1);
            }
        }

        static void Main(string[] args) {
            Table myTable = new Table();
            InitiateConnection(ref myTable);
        }
    }
}