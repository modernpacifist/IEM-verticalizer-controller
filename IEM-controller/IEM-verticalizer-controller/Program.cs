using System;
using TableAPI;
using System.Diagnostics;


namespace IEM_verticalizer_controller
{
    public class TablePosition
    {
        private double x = 0.0;
        private double y = 0.0;

        // default constructor
        public TablePosition()
        {
            x = 0.0;
            y = 0.0;
        }

        // sample function, remove from prod
        public void PrintPosition()
        {
            Console.WriteLine(String.Format("{0} {1}", x, y));
        }

        public void IncreaseX(double some_value)
        {
            this.x += some_value;
        }

        public void DecreaseX(double some_value)
        {
            this.x -= some_value;
        }

        public void IncreaseY(double some_value)
        {
            this.y += some_value;
        }

        public void DecreaseY(double some_value)
        {
            this.y -= some_value;
        }
    }

    internal class Program
    {
        static void EmergencyStop(ref Table tableInstance)
        {
            // Intercept Ctrl+C from the user input during exeuciton and return table into horizontal position
            return;
        }

        // Terminate execution of a program with exit code if connection fails
        static void InitiateConnection(ref Table tableInstance)
        {
            if (!tableInstance.Connect())
            {
                Console.WriteLine("Connection failed, check physical state");
                Environment.Exit(1);
            }
            Console.WriteLine("Connection was established");
            return;
        }

        static void SetDirection(ref Table tableInstance, bool direction)
        {
            bool directionFlag = tableInstance.SetDirection(direction);
            if (!directionFlag)
            {
                Console.WriteLine("Direction was not set");
                Environment.Exit(1);
            }

            if (direction)
            {
                Console.WriteLine("Direction set to CLOCKWISE");
            }
            else
            {
                Console.WriteLine("Direction set to COUNTER-CLOCKWISE");
            }
            return;
        }

        static void SetSpeed(ref Table tableInstance, float speedValue)
        {
            if (0.05f > speedValue || speedValue > 0.5f)
            {
                Console.WriteLine("Speed value must be in the interval of [0.05, 0.5]");
                Environment.Exit(1);
            }

            bool speedFlag = tableInstance.SetSpeed(speedValue);
            if (!speedFlag)
            {
                Console.WriteLine("Speed was not set due to internal error");
                return;
            }
            Console.WriteLine(String.Format("Speed was successfully set to: {0}", speedValue));
            return;
        }

        static void StartEngine(ref Table tableInstsance)
        {
            bool flag = tableInstsance.Start();
            if (!flag)
            {
                Console.WriteLine("Engine start failed");
                Environment.Exit(1);
            }
            Console.WriteLine("Engine movement started");
            return;
        }

        static void StopEngine(ref Table tableInstsance)
        {
            // add try/except block to stop engine definitely
            bool flag = tableInstsance.Stop();
            if (!flag)
            {
                Console.WriteLine("ENGINE STOP FAILED");
            }
            Console.WriteLine("Engine is stopping...");
            return;
        }

        static void ResetEngine(ref Table tableInstsance)
        {
            bool flag = tableInstsance.Reset();
            if (!flag)
            {
                Console.WriteLine("Engine was reset");
            }
            return;
        }

        // minor chance, this function won't be needed
        static void LegalAngleCheck(double timeInterval, double angleSpeed)
        {
            double maxAngle = timeInterval * angleSpeed;
            if (maxAngle > 15)
            {
                Console.WriteLine("Illegal max angle");
                Environment.Exit(1);
            }
            return;
        }

        static void Main(string[] args)
        {
            Table tableInstance = new Table();
            // This value is from prompt in minutes
            int timeInterval = 3 * 1000; // 3 seconds

            // Initiate connection with the table
            InitiateConnection(ref tableInstance);

            // Create position instance of a table
            TablePosition tablePosition = new TablePosition();
            tablePosition.PrintPosition();

            // Set speed of the rotation of the table
            float TableSpeed = 0.05f;
            SetSpeed(ref tableInstance, TableSpeed);

            int sampleSeconds = 60;
            Stopwatch Timer = new Stopwatch();
            Timer.Start();
            while (Timer.Elapsed.TotalSeconds < sampleSeconds)
            {
                // Setting direction to clockwise
                SetDirection(ref tableInstance, true);

                // Start engine clockwise
                StartEngine(ref tableInstance);
                System.Threading.Thread.Sleep(timeInterval);

                // Stopping engine
                StopEngine(ref tableInstance);

                // Setting direction to counter-clockwise
                SetDirection(ref tableInstance, false);

                // Start engine in counter-clockwise direction
                StartEngine(ref tableInstance);
                System.Threading.Thread.Sleep(timeInterval);
            }
            Timer.Stop();

            //// Setting direction to clockwise
            //SetDirection(ref tableInstance, true);

            //// Start engine clockwise
            //StartEngine(ref tableInstance);
            //System.Threading.Thread.Sleep(timeInterval);

            //// Stopping engine
            //StopEngine(ref tableInstance);

            //// Setting direction to counter-clockwise
            //SetDirection(ref tableInstance, false);

            //// Start engine in counter-clockwise direction
            //StartEngine(ref tableInstance);
            //System.Threading.Thread.Sleep(timeInterval);

            //// Stopping engine
            //StopEngine(ref tableInstance);

            //// Resetting engine
            //ResetEngine(ref tableInstance);
        }
    }
}