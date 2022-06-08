﻿using System;
using TableAPI;
using System.Diagnostics;


namespace IEM_verticalizer_controller
{
    public class TablePosition
    {
        private double tableAngleValue = 0.0;

        public TablePosition()
        {
            tableAngleValue = 0.0;
        }

        // sample function, remove from prod
        public void PrintPosition()
        {
            Console.WriteLine(string.Format("Current table position x: {0}", tableAngleValue));
        }

        public void CalculatePosition(double someValue, bool flag)
        {
            if (flag)
            {
                this.tableAngleValue -= someValue;
            }
            else
            {
                this.tableAngleValue += someValue;
            }
        }

        public double GetCurrentPosition()
        {
            return tableAngleValue;
        }
    }

    public class TableAngle
    {
        private double angleValue = 0.0;

        public TableAngle()
        {
            angleValue = 0.0;
        }

        public void RotateClockwise(double value)
        {
            angleValue += value;
        }

        public void RotateCounterClockwise(double value)
        {
            angleValue -= value;
        }

        public double GetCurrentAngle()
        {
            return angleValue;
        }
    }

    internal class TableControl
    {
        // calculate required one-directional rotation period -> seconds
        public int calculateRotationPeriod(double requiredAngle, float speed)
        {
            return (int)(requiredAngle / speed);
        }
    }

    internal class Program
    {
        static void EmergencyStop(ref Table tableInstance)
        {
            // Intercept Ctrl+C from the user input during execution and return table into horizontal position
            Console.Write("Emergency stop called\n");
            StopEngine(ref tableInstance);
            ResetEngine(ref tableInstance);
            Environment.Exit(0);
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
            Console.WriteLine("Connection established");
            return;
        }

        static void SetDirection(ref Table tableInstance, bool direction)
        {
            bool directionFlag = tableInstance.SetDirection(direction);
            //System.Threading.Thread.Sleep(100);
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
            //System.Threading.Thread.Sleep(100);
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
            //System.Threading.Thread.Sleep(100);
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
            //System.Threading.Thread.Sleep(100);
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
            //System.Threading.Thread.Sleep(100);
            if (!flag)
            {
                Console.WriteLine("Engine was reset");
            }
            return;
        }

        // minor chance this function won't be needed
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

        static int CalculateRotationPeriod(double requiredAngle, float speed)
        {
            //return (int)(requiredAngle / speed);
            return (int)(requiredAngle / speed);
        }

        static void ManualMode(ref Table tableInstance)
        {
            Console.Write("MANUAL mode engaged...\n");
            while (true)
            {
                Console.Write("Input speed: ");
                float speed = float.Parse(Console.ReadLine());

                Console.Write("Input direction to clockwise true - from me, false - to me: ");
                bool directionFlag = bool.Parse(Console.ReadLine());

                Console.Write("Input time interval (in seconds): ");
                int rotationInterval = int.Parse(Console.ReadLine());
                Console.Write("Rotation interval: ");
                Console.WriteLine(rotationInterval);

                // Set speed of the rotation of the table
                SetSpeed(ref tableInstance, speed);
                // Setting direction to clockwise true - from me, false - to me
                SetDirection(ref tableInstance, directionFlag);

                // Start engine clockwise
                StartEngine(ref tableInstance);
                System.Threading.Thread.Sleep(rotationInterval*1000);
                StopEngine(ref tableInstance);

                Console.Write("Continue? y/n\n");
                string exitFlag = Console.ReadLine();
                if (exitFlag == "y")
                {
                    continue;
                }
                break;
            }

            // Stop and reset engine
            StopEngine(ref tableInstance);
            ResetEngine(ref tableInstance);
        }

        // this function can be simplified without interval calculations
        static void NormalizeHorizontalPosition(ref Table tableInstance, ref TablePosition tablePosition, float tableSpeed, int resetTimeInterval)
        {
            double endTablePosition = tablePosition.GetCurrentPosition();
            Console.WriteLine("Resetting to horizontal position...");

            // distance divided by speed
            Console.Write(string.Format("resetTimeInterval: {0}\n", resetTimeInterval));

            // do something with this "directionFlag"
            bool directionFlag = endTablePosition > 0;
            SetSpeed(ref tableInstance, tableSpeed);
            SetDirection(ref tableInstance, directionFlag);
            StartEngine(ref tableInstance);

            System.Threading.Thread.Sleep(resetTimeInterval);
        }

        static void AutomaticMode(ref Table tableInstance, ref TablePosition tablePosition)
        {
            Console.WriteLine("AUTOMATIC mode engaged...");

            Console.Write("Input speed: ");
            float tableSpeed = float.Parse(Console.ReadLine());
            Console.Write("Input rotation interval (in seconds): ");
            int timeInterval = int.Parse(Console.ReadLine()) * 1000;
            Console.Write("Input overall time interval (in seconds): ");
            int totalTimeInterval = int.Parse(Console.ReadLine());

            bool rotationDirection = true;
            Stopwatch Timer = new Stopwatch();
            Timer.Start();

            // Set speed of the rotation of the table
            SetSpeed(ref tableInstance, tableSpeed);

            SetDirection(ref tableInstance, rotationDirection);
            StartEngine(ref tableInstance);
            System.Threading.Thread.Sleep(timeInterval);
            StopEngine(ref tableInstance);
            tablePosition.CalculatePosition(timeInterval * tableSpeed, rotationDirection);
            tablePosition.PrintPosition();
            timeInterval *= 2;

            while (Timer.Elapsed.TotalSeconds < totalTimeInterval)
            {
                // Invert rotation direction
                rotationDirection = !rotationDirection;

                // Setting direction to clockwise
                SetDirection(ref tableInstance, rotationDirection);

                // Start engine
                StartEngine(ref tableInstance);
                System.Threading.Thread.Sleep(timeInterval);
                // Stopping engine
                StopEngine(ref tableInstance);
                tablePosition.CalculatePosition(timeInterval * tableSpeed, rotationDirection);
                tablePosition.PrintPosition();

                Console.WriteLine();
            }
            Timer.Stop();
            Console.WriteLine("Main timer loop exceeded");
            // TODO: here must be a chunk of code that resets engine in [0, 0] coordinates
            NormalizeHorizontalPosition(ref tableInstance, ref tablePosition, tableSpeed, timeInterval/2);

            // Stop and reset engine
            StopEngine(ref tableInstance);
            ResetEngine(ref tableInstance);
        }

        static void Main(string[] args)
        {
            // Essensial instance for engine control
            Table tableInstance = new Table();

            // Initiate connection with the engine
            InitiateConnection(ref tableInstance);

            //Console.WriteLine("Do you wish to start in manual mode? y/n");
            //string userInputMode = Console.ReadLine();
            //while (userInputMode != "n" && userInputMode != "y")
            //{
            //    Console.WriteLine("Please answer with 'y' or 'n'");
            //    userInputMode = Console.ReadLine();
            //}

            Console.WriteLine("What mode do you wish to engage? manual(m)/auto(a)");
            string userInputMode = Console.ReadLine();
            while (userInputMode != "m" && userInputMode != "a")
            {
                Console.WriteLine("Please answer with 'm' or 'a'");
                userInputMode = Console.ReadLine();
            }

            switch(userInputMode)
            {
                case "m":
                    ManualMode(ref tableInstance);
                    break;
                case "a":
                    TablePosition tablePosition = new TablePosition();
                    AutomaticMode(ref tableInstance, ref tablePosition);
                    break;
            }

            Environment.Exit(0);
        }
    }
}