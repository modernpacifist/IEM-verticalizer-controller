using System;
using TableAPI;
using System.Diagnostics;
using System.Threading.Tasks;

namespace IEM_verticalizer_controller
{
    public class TableMovementConfig
    {
        public double tableOffset = 0.0;
        public readonly float tableSpeed = 0.0f;
        public readonly int unaryMoveInterval = 0;
        public readonly int totalTimeInterval = 0;

        public TableMovementConfig(float tableSpeed, int unaryMoveInterval, int totalTimeInterval)
        {
            this.tableSpeed = tableSpeed;
            this.unaryMoveInterval = unaryMoveInterval;
            this.totalTimeInterval = totalTimeInterval;
        }

        // sample function, remove from prod
        public void PrintPosition()
        {
            Console.WriteLine(string.Format("Current table offset: {0}", tableOffset));
        }

        public void CalculatePosition(double someValue, bool flag)
        {
            if (flag)
            {
                this.tableOffset -= someValue;
            }
            else
            {
                this.tableOffset += someValue;
            }
        }
    }

    internal class TableControl: Table
    {
        // calculate required one-directional rotation period -> seconds
        public int calculateRotationPeriod(double requiredAngle, float speed)
        {
            return (int)(requiredAngle / speed);
        }
    }

    internal class Program
    {
        static void EmergencyStop(ref Table tableInstance, ref TableMovementConfig tableConfig)
        {
            // Intercept Ctrl+C from the user input during execution and return table into horizontal position
            Console.Write("Emergency stop called\n");
            NormalizeHorizontalPosition(ref tableInstance, ref tableConfig);
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
            System.Threading.Thread.Sleep(100);
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
            System.Threading.Thread.Sleep(100);
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
            System.Threading.Thread.Sleep(100);
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
            System.Threading.Thread.Sleep(100);
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
            System.Threading.Thread.Sleep(100);
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
            return (int)(requiredAngle / speed);
        }

        static void ManualMode(ref Table tableInstance, ref TableMovementConfig tableMoveConfig)
        {
            Console.Write("MANUAL mode engaged...\n");
            while (true)
            {
                Console.Write("Input direction to clockwise true - from me, false - to me: ");
                bool directionFlag = bool.Parse(Console.ReadLine());

                // Set speed of the rotation of the table
                SetSpeed(ref tableInstance, tableMoveConfig.tableSpeed);
                // Setting direction to clockwise true - from me, false - to me
                SetDirection(ref tableInstance, directionFlag);

                // Start engine clockwise
                StartEngine(ref tableInstance);
                System.Threading.Thread.Sleep(tableMoveConfig.unaryMoveInterval);
                StopEngine(ref tableInstance);

                Console.Write("Continue? y/n\n");
                string continueFlag = Console.ReadLine();
                if (continueFlag == "y")
                {
                    continue;
                }
                break;
            }

            // Stop and reset engine
            StopEngine(ref tableInstance);
            ResetEngine(ref tableInstance);
        }

        static void NormalizeHorizontalPosition(ref Table tableInstance, ref TableMovementConfig tableConfig)
        {
            double currentTableOffset = tableConfig.tableOffset;
            if (currentTableOffset == 0)
            {
                return;
            }
            Console.WriteLine("Resetting to horizontal position...");

            Console.Write(string.Format("resetTimeInterval: {0}\n", tableConfig.unaryMoveInterval));

            // determine which direction to turn to get into default position
            bool directionFlag = currentTableOffset > 0;
            SetSpeed(ref tableInstance, tableConfig.tableSpeed);
            SetDirection(ref tableInstance, directionFlag);
            StartEngine(ref tableInstance);
            System.Threading.Thread.Sleep(tableConfig.unaryMoveInterval);
            StopEngine(ref tableInstance);
            ResetEngine(ref tableInstance);
        }

        static void AutomaticMode(ref Table tableInstance, ref TableMovementConfig moveConfig)
        {
            Console.WriteLine("AUTOMATIC mode engaged...");

            bool rotationDirection = true;
            Stopwatch Timer = new Stopwatch();
            Timer.Start();

            // Set speed of the rotation of the table
            SetSpeed(ref tableInstance, moveConfig.tableSpeed);

            SetDirection(ref tableInstance, rotationDirection);
            StartEngine(ref tableInstance);
            System.Threading.Thread.Sleep(moveConfig.unaryMoveInterval);
            StopEngine(ref tableInstance);
            moveConfig.CalculatePosition(moveConfig.unaryMoveInterval * moveConfig.tableSpeed, rotationDirection);
            moveConfig.PrintPosition();

            int doubledTimeInterval = moveConfig.unaryMoveInterval * 2;
            while (Timer.Elapsed.TotalSeconds < moveConfig.totalTimeInterval)
            {
                // Invert rotation direction
                rotationDirection = !rotationDirection;

                // Setting direction to clockwise
                SetDirection(ref tableInstance, rotationDirection);

                // Start engine
                StartEngine(ref tableInstance);
                System.Threading.Thread.Sleep(doubledTimeInterval);
                // Stopping engine
                StopEngine(ref tableInstance);
                moveConfig.CalculatePosition(doubledTimeInterval * moveConfig.tableSpeed, rotationDirection);
                moveConfig.PrintPosition();

                Console.WriteLine();
            }
            Timer.Stop();
            Console.WriteLine("Main timer loop exceeded");
            NormalizeHorizontalPosition(ref tableInstance, ref moveConfig);

            // Stop and reset engine
            //StopEngine(ref tableInstance);
            //ResetEngine(ref tableInstance);
        }

        static void Main(string[] args)
        {
            // Essensial instance for engine control
            Table tableInstance = new Table();

            // Initiate connection with the engine
            InitiateConnection(ref tableInstance);

            Console.WriteLine("What mode do you wish to engage? manual(m)/auto(a)");
            string userInputMode = Console.ReadLine();
            while (userInputMode != "m" && userInputMode != "a")
            {
                Console.WriteLine("Please answer with 'm' or 'a'");
                userInputMode = Console.ReadLine();
            }

            Console.Write("Input speed: ");
            float tableSpeed = float.Parse(Console.ReadLine());
            Console.Write("Input rotation interval (in seconds): ");
            int timeInterval = int.Parse(Console.ReadLine()) * 1000;
            Console.Write("Input overall time interval (in seconds): ");
            int totalTimeInterval = int.Parse(Console.ReadLine());

            TableMovementConfig movementConfig = new TableMovementConfig(tableSpeed, timeInterval, totalTimeInterval);

            Console.CancelKeyPress += delegate
            {
                EmergencyStop(ref tableInstance, ref movementConfig);
            };

            switch(userInputMode)
            {
                case "m":
                    ManualMode(ref tableInstance, ref movementConfig);
                    break;
                case "a":
                    AutomaticMode(ref tableInstance, ref movementConfig);
                    break;
            }

            Environment.Exit(0);
        }
    }
}