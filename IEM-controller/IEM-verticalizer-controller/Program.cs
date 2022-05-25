namespace IEM_verticalizer_controller
{
    internal class Program
    {
        static void Main(string[] args)
        {
            TableAPI.Table myTable = new();
            myTable.Connect();
        }
    }
}