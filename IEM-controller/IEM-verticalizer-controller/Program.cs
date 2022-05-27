using System;
using TableAPI;


namespace IEM_verticalizer_controller
{
    internal class Program
    {
        // Terminate execution of a program with exit code if connection fails
        static void InitiateConnection(ref Table tableInstance) {
            if (!tableInstance.Connect()) {
                Console.WriteLine("Connection failed, check physical state");
                Environment.Exit(1);
            }
            Console.WriteLine("Connection was established");
            return;
        }

        static void SetDirection(ref Table tableInstance, bool direction) {
            bool directionFlag = tableInstance.SetDirection(direction);
            if (!directionFlag) {
                Console.WriteLine("Direction was not set");
                Environment.Exit(1);
            }
            if (direction) {
                Console.WriteLine("Direction was set clockwise");
            } else {
                Console.WriteLine("Direction was set counter-clockwise");
            }
            return;
        }

        static void SetSpeed(ref Table tableInstance, float speedValue) {
            if (0.05f > speedValue || speedValue > 0.5f) {
                Console.WriteLine("Speed value must be in the interval of [0.05, 0.5]");
                Environment.Exit(1);
            }
            bool speedFlag = tableInstance.SetSpeed(speedValue);
            if (!speedFlag) {
                Console.WriteLine("Speed was not set due to internal error");
                return;
            }
            Console.WriteLine(String.Format("Speed was successfully set to: {0}", speedValue));
            return;
        }

        static void StartEngine(ref Table tableInstsance) {
            bool flag = tableInstsance.Start();
            if (!flag) {
                Console.WriteLine("Engine start failed");
                Environment.Exit(1);
            }
            Console.WriteLine("Engine successfully started");
            return;
        }

        static void StopEngine(ref Table tableInstsance) {
            bool flag = tableInstsance.Stop();
            if (!flag) {
                Console.WriteLine("ENGINE STOP FAILED");
            }
            Console.WriteLine("Engine is stopping...");
            return;
        }

        static void ResetEngine(ref Table tableInstsance) {
            bool flag = tableInstsance.Reset();
            if (!flag) {
                Console.WriteLine("Engine was successfully reset");
            }
            return;
        }

        static void Main(string[] args) {
            Table tableInstance = new Table();

            // Initiate connection with the table
            InitiateConnection(ref tableInstance);

            // Setting direction to clockwise
            SetDirection(ref tableInstance, true);

            // Set speed of the rotation of the table
            float sampleSpeed = 0.05f;
            SetSpeed(ref tableInstance, sampleSpeed);

            // Start engine in clockwise direction
            StartEngine(ref tableInstance);
            System.Threading.Thread.Sleep(2000);

            // Stopping engine
            StopEngine(ref tableInstance);

            // Setting direction to counter-clockwise
            SetDirection(ref tableInstance, false);

            StartEngine(ref tableInstance);
            System.Threading.Thread.Sleep(2000);

            ResetEngine(ref tableInstance);
        }
    }
}