using System;
using TableAPI;
using System.Diagnostics;


namespace IEM_verticalizer_controller
{
    public class TableMovementConfig
    {
        // these must not be accessible
        public double tableOffset = 0.0;
        public readonly float rotationSpeed = 0.0f;
        // totalTimeInterval value is in milliseconds
        public int totalTimeInterval = 0;
        public readonly int halfInterval = 0;
        public readonly int fullInterval = 0;
        public bool directionFlag = true;

        public TableMovementConfig(float rotationSpeed, int halfInterval, int totalTimeInterval)
        {
            this.rotationSpeed = rotationSpeed;
            // converting into milliseconds
            this.halfInterval = halfInterval * 1000;
            this.fullInterval = halfInterval * 1000 * 2;
            //this.totalTimeInterval = totalTimeInterval;
            this.totalTimeInterval = totalTimeInterval * 1000;
        }

        public void ExpandTotalTimeInterval(int millisecondsToAdd)
        {
            this.totalTimeInterval += millisecondsToAdd;
        }

        public void LegalOffsetCheck()
        {
            //if (tableMoveConfig.directionFlag == false && tableMoveConfig.tableOffset > 3600)
            if (this.directionFlag == false && true)
            {
                Console.WriteLine("Max counter-clockwise table offset exceeded");
                //StopEngine(ref tableInstance);
                //tableMoveConfig.ReverseDirection();
                Environment.Exit(1);
            }
            return;
        }

        public void PrintCurrentOffset()
        {
            Console.WriteLine(string.Format("Current table offset: {0}\n", this.tableOffset));
        }

        public void ReverseDirection()
        {
            this.directionFlag = !this.directionFlag;
        }

        public void CalculatePosition(double timeValue)
        {
            if (this.directionFlag)
            {
                this.tableOffset -= (int)(timeValue * this.rotationSpeed);
            }
            else
            {
                this.tableOffset += (int)(timeValue * this.rotationSpeed);
            }
        }
    }

    internal class EngineControl
    {
        static public void InitiateConnection(ref Table tableInstance)
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

        static public void RotateEngine(ref Table tableInstance, ref TableMovementConfig tableMoveConfig)
        {
            //Console.WriteLine("Starting to rotate engine");
        }
    }

    internal class Program
    {
        static void EmergencyStop(ref Table tableInstance, ref TableMovementConfig tableMoveConfig)
        {
            StopEngine(ref tableInstance);
            Console.Write("Emergency stop called\n");
            NormalizeHorizontalPosition(ref tableInstance, ref tableMoveConfig);
            Environment.Exit(0);
            return;
        }

        static void SetDirection(ref Table tableInstance, bool direction)
        {
            bool directionFlag = tableInstance.SetDirection(direction);
            //System.Threading.Thread.Sleep(100);
            System.Threading.Thread.Sleep(250);
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
            System.Threading.Thread.Sleep(250);
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
            System.Threading.Thread.Sleep(250);
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
            System.Threading.Thread.Sleep(400);
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

        static void ManualMode(ref Table tableInstance, ref TableMovementConfig tableMoveConfig)
        {
            Console.Write("MANUAL mode engaged...\n");
            while (true)
            {
                Console.Write("Input direction to clockwise true - from me, false - to me: ");
                bool userDirection = bool.Parse(Console.ReadLine());

                Console.Write("Input unary rotation interval in seconds: ");
                int userUnaryInterval = int.Parse(Console.ReadLine()) * 1000;

                // Set speed of the rotation of the table
                SetSpeed(ref tableInstance, tableMoveConfig.rotationSpeed);
                // Setting direction to clockwise true - from me, false - to me
                SetDirection(ref tableInstance, userDirection);
                tableMoveConfig.directionFlag = userDirection;

                // Start engine clockwise
                StartEngine(ref tableInstance);
                tableMoveConfig.CalculatePosition(userUnaryInterval);
                System.Threading.Thread.Sleep(userUnaryInterval);
                StopEngine(ref tableInstance);
                tableMoveConfig.PrintCurrentOffset();

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
            int resetTimeInterval = (int)(Math.Abs(currentTableOffset) / tableMoveConfig.rotationSpeed);
            Console.Write(string.Format("resetTimeInterval: {0}\n", resetTimeInterval));

            // determine which direction to turn to get into default position
            bool directionFlag = currentTableOffset > 0;
            SetDirection(ref tableInstance, directionFlag);
            StartEngine(ref tableInstance);
            System.Threading.Thread.Sleep(resetTimeInterval + 500);
            StopEngine(ref tableInstance);
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
            tableMoveConfig.PrintCurrentOffset();

            //while (Timer.Elapsed.TotalSeconds < tableMoveConfig.totalTimeInterval)
            while (Timer.ElapsedMilliseconds < tableMoveConfig.totalTimeInterval)
            {
                // Invert rotation direction
                tableMoveConfig.ReverseDirection();

                // Setting direction to clockwise
                SetDirection(ref tableInstance, tableMoveConfig.directionFlag);
                StartEngine(ref tableInstance);
                System.Threading.Thread.Sleep(tableMoveConfig.fullInterval);
                StopEngine(ref tableInstance);
                tableMoveConfig.CalculatePosition(tableMoveConfig.fullInterval);
                tableMoveConfig.PrintCurrentOffset();
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
            //InitiateConnection(ref tableInstance);
            EngineControl.InitiateConnection(ref tableInstance);

            Console.WriteLine("What mode do you wish to engage?");
            Console.Write("Type 'm' for manual or 'a' for automatic: ");
            string userInputMode = Console.ReadLine();
            while (userInputMode != "m" && userInputMode != "a")
            {
                Console.WriteLine("Please answer with 'm' or 'a'");
                userInputMode = Console.ReadLine();
            }

            // max offset is 2500

            Console.Write("Input speed, available values are [0.05 - 0.5]: ");
            float tableSpeed = float.Parse(Console.ReadLine());
            Console.Write("Input rotation interval (in seconds): ");
            int timeInterval = int.Parse(Console.ReadLine());
            Console.Write("Input overall time interval (in seconds): ");
            int totalTimeInterval = int.Parse(Console.ReadLine());

            TableMovementConfig movementConfig = new TableMovementConfig(tableSpeed, timeInterval, totalTimeInterval);

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