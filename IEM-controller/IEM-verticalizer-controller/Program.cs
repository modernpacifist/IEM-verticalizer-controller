using System;
using TableAPI;


namespace IEM_verticalizer_controller
{
    internal class Program
    {
        // Terminate execution of a program with exit code if connection fails
        static void InitiateConnection(ref Table table) {
            if (!table.Connect()) {
                Console.WriteLine("Connection failed, check physical state");
                Environment.Exit(1);
            }
            Console.WriteLine("Connection was established");
            return;
        }

        static void Main(string[] args) {
            Table myTable = new Table();
            InitiateConnection(ref myTable);
        }
    }
}