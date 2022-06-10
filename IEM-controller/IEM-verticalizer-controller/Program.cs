using System;
using TableAPI;
using System.Diagnostics;
using System.Threading.Tasks;

namespace IEM_verticalizer_controller
{
    public class TableMovementConfig
    {
        // these must not be accessible
        public double tableOffset = 0.0;
        public readonly float rotationSpeed = 0.0f;
        public readonly int totalMoveTime = 0;
        public readonly int halfInterval = 0;
        public readonly int fullInterval = 0;
        public bool directionFlag = true;

        public TableMovementConfig(float rotationSpeed, int halfInterval, int totalTimeInterval)
        {
            this.rotationSpeed = rotationSpeed;
            // converting into milliseconds
            this.halfInterval = halfInterval * 1000;
            this.fullInterval = halfInterval * 1000 * 2;
            this.totalMoveTime = totalTimeInterval;
        }

        // this must be calculated by getting max offset value
        public void LegalAngleCheck()
        {
            // dividing by 1000 - calculates through seconds
            double maxAngle = this.halfInterval * this.rotationSpeed / 1000;
            if (maxAngle > 15)
            {
                Console.WriteLine("Max angle value exceeded");
                Environment.Exit(1);
            }
            return;
        }

        // sample function, remove from prod
        public void PrintPosition()
        {
            Console.WriteLine(string.Format("Current table offset: {0}", tableOffset));
        }

        public void ReverseDirection()
        {
            this.directionFlag = !this.directionFlag;
        }

        //public void CalculatePosition(double someValue, bool directionFlag)
        public void CalculatePosition(double someValue)
        {
            if (this.directionFlag)
            {
                this.tableOffset -= (int)(someValue * this.rotationSpeed);
            }
            else
            {
                this.tableOffset += (int)(someValue * this.rotationSpeed);
            }
        }
    }

    // chance this won't be needed
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
        static void EmergencyStop(ref Table tableInstance, ref TableMovementConfig tableMoveConfig)
        {
            Console.Write("Emergency stop called\n");
            NormalizeHorizontalPosition(ref tableInstance, ref tableMoveConfig);
            Environment.Exit(0);
            return;
        }

        // Terminate execution of a program with exit code if connection fails
        static void InitiateConnection(ref Table tableInstance)
        {
            bool successFlag = tableInstance.Connect();
            System.Threading.Thread.Sleep(100);
            if (!successFlag)
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
            System.Threading.Thread.Sleep(250);
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

        // this function is needed in case parameters are set through required angle
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
                bool userDirection = bool.Parse(Console.ReadLine());

                // Set speed of the rotation of the table
                SetSpeed(ref tableInstance, tableMoveConfig.rotationSpeed);
                // Setting direction to clockwise true - from me, false - to me
                SetDirection(ref tableInstance, userDirection);
                tableMoveConfig.directionFlag = userDirection;

                // Start engine clockwise
                StartEngine(ref tableInstance);
                System.Threading.Thread.Sleep(tableMoveConfig.halfInterval);
                StopEngine(ref tableInstance);
                tableMoveConfig.CalculatePosition(tableMoveConfig.halfInterval);
                tableMoveConfig.PrintPosition();

                Console.Write("Continue? y/n: ");
                string continueFlag = Console.ReadLine();
                if (continueFlag == "y")
                {
                    continue;
                }
                Console.Write("Move table into horizontal position? y/n: ");
                string normalizePositionFlag = Console.ReadLine();
                if (normalizePositionFlag == "y")
                {
                    NormalizeHorizontalPosition(ref tableInstance, ref tableMoveConfig);
                }
                break;
            }

            // Stop and reset engine
            StopEngine(ref tableInstance);
            ResetEngine(ref tableInstance);
        }

        static void NormalizeHorizontalPosition(ref Table tableInstance, ref TableMovementConfig tableMoveConfig)
        {
            double currentTableOffset = tableMoveConfig.tableOffset;
            if (currentTableOffset == 0)
            {
                return;
            }
            Console.WriteLine("Resetting to horizontal position...");
            // resetTimeInterval is based on the offset of the table
            int resetTimeInterval = (int)(Math.Abs(tableMoveConfig.tableOffset) / tableMoveConfig.rotationSpeed);
            Console.Write(string.Format("resetTimeInterval: {0}\n", resetTimeInterval));

            // determine which direction to turn to get into default position
            bool directionFlag = currentTableOffset > 0;
            SetDirection(ref tableInstance, directionFlag);
            StartEngine(ref tableInstance);
            System.Threading.Thread.Sleep(resetTimeInterval);
            StopEngine(ref tableInstance);
            ResetEngine(ref tableInstance);
        }

        static void AutomaticMode(ref Table tableInstance, ref TableMovementConfig tableMoveConfig)
        {
            Console.WriteLine("AUTOMATIC mode engaged...");

            Stopwatch Timer = new Stopwatch();
            Timer.Start();

            // Set speed of the rotation of the table
            SetSpeed(ref tableInstance, tableMoveConfig.rotationSpeed);

            SetDirection(ref tableInstance, tableMoveConfig.directionFlag);
            StartEngine(ref tableInstance);
            System.Threading.Thread.Sleep(tableMoveConfig.halfInterval);
            StopEngine(ref tableInstance);
            tableMoveConfig.CalculatePosition(tableMoveConfig.halfInterval);
            tableMoveConfig.PrintPosition();

            while (Timer.Elapsed.TotalSeconds < tableMoveConfig.totalMoveTime)
            {
                // Invert rotation direction
                tableMoveConfig.ReverseDirection();

                // Setting direction to clockwise
                SetDirection(ref tableInstance, tableMoveConfig.directionFlag);

                // Start engine
                StartEngine(ref tableInstance);
                System.Threading.Thread.Sleep(tableMoveConfig.fullInterval);

                // Stopping engine
                StopEngine(ref tableInstance);
                tableMoveConfig.CalculatePosition(tableMoveConfig.fullInterval);
                tableMoveConfig.PrintPosition();

                Console.WriteLine();
            }
            Timer.Stop();
            Console.WriteLine("Main timer loop exceeded");
            NormalizeHorizontalPosition(ref tableInstance, ref tableMoveConfig);
        }

        static void Main(string[] args)
        {
            // Essensial instance for engine control
            Table tableInstance = new Table();

            // Initiate connection with the engine
            InitiateConnection(ref tableInstance);

            Console.WriteLine("What mode do you wish to engage?");
            Console.Write("Type 'm' for manual or 'a' for automatic: ");
            string userInputMode = Console.ReadLine();
            while (userInputMode != "m" && userInputMode != "a")
            {
                Console.WriteLine("Please answer with 'm' or 'a'");
                userInputMode = Console.ReadLine();
            }

            Console.Write("Input speed, available values are [0.05 - 0.5]: ");
            float tableSpeed = float.Parse(Console.ReadLine());
            Console.Write("Input rotation interval (in seconds): ");
            int timeInterval = int.Parse(Console.ReadLine());
            Console.Write("Input overall time interval (in seconds): ");
            int totalTimeInterval = int.Parse(Console.ReadLine());

            TableMovementConfig movementConfig = new TableMovementConfig(tableSpeed, timeInterval, totalTimeInterval);
            movementConfig.LegalAngleCheck();

            // Intercept Ctrl+C from the user input during execution
            Console.CancelKeyPress += delegate
            {
                EmergencyStop(ref tableInstance, ref movementConfig);
            };

            switch (userInputMode)
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