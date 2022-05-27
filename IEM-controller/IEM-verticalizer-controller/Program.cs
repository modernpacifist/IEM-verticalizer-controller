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
                Console.WriteLine("Speed was not set");
                Environment.Exit(1);
            }
            if (direction) {
                Console.WriteLine("Speed was set clockwise");
            } else {
                Console.WriteLine("Speed was set counter-clockwise");
            }
            return;
        }

        static void SetSpeed(ref Table tableInstance, float speedValue) {
            if (0.05f > speedValue || speedValue > 0.5f) {
                Console.WriteLine("Speed value must be in the interval of [0.05, 0.5]");
                Environment.Exit(1);
                return;
            }
            bool speedFlag = tableInstance.SetSpeed(speedValue);
            if (!speedFlag) {
                Console.WriteLine("Speed was not set due to internal error");
                return;
            }
            Console.WriteLine(String.Format("Speed was successfully set to: {0}", speedValue));
            return;
        }

        static void Main(string[] args) {
            Table tableInstance = new Table();

            //// Initiate connection with the table
            //InitiateConnection(ref tableInstance);

            // Set direction 

            // Set speed of the rotation of the table
            float sampleSpeed = 0.05f;
            SetSpeed(ref tableInstance, sampleSpeed);

            Console.WriteLine("ff");
        }
    }
}