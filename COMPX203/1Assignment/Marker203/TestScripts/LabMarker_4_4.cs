using RexSimulator.Hardware;
using RexSimulator.Hardware.Wramp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace COMP200Marker.TestScripts
{
    class LabMarker_4_4 : LabMarker_4
    {
        public override string Version { get { return "19A.1.1"; } }

        protected override uint ExpectedCCTRL
        {
            get { return 0xca; }
        }

        protected virtual int TEST_DURATION_SECONDS
        {
            get { return 100; }
        }

        protected virtual int SUBTEST_DURATION_SECONDS
        {
            get { return 20; }
        }

        protected List<uint> RunTestCase(RexBoard mBoard, int tickLimit)
        {
            List<uint> ssdValues = new List<uint>();

            for (int i = 0; i < tickLimit; i++)
            {
                mBoard.Tick();

                //Record SSD values at each interrupt
                if (mBoard.CPU.PC == mBoard.CPU.mSpRegisters[RegisterFile.SpRegister.evec] + 1)
                {
                    uint currentSsdValue = (mBoard.Parallel.LeftSSD & 0xf) * 10 + (mBoard.Parallel.RightSSD & 0xf);

                    if (ssdValues.Count == 0 || ssdValues.Last() != currentSsdValue)
                        ssdValues.Add(currentSsdValue);
                    Console.Write("SSDs: {0:D2}\r", currentSsdValue);
                }
            }

            return ssdValues;
        }

        protected bool CheckSequence(List<uint> ssdValues, int limit, int tolerance)
        {
            //Remove duplicates
            for (int i = ssdValues.Count - 1; i >= 1; i--)
            {
                if (ssdValues[i] == ssdValues[i - 1])
                    ssdValues.RemoveAt(i);
            }

            if (Math.Abs(ssdValues.Count - limit) <= tolerance)
            {
                for (uint i = 0; i < ssdValues.Count; i++)
                {
                    if (ssdValues[(int)i] != i)
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        protected void PrintSsdSequence(List<uint> ssdValues)
        {
            foreach (uint i in ssdValues)
                mMessage += string.Format("{0:D2}, ", i);

            mMessage = mMessage.Substring(0, mMessage.Length - 2);
        }

        protected override bool DoSubTest(RexBoard mBoard)
        {
            //Ensure timer is set up right
            if (mBoard.Timer.Control != 2)
            {
                mMessage += string.Format("Timer control register is incorrect. Was 0x{0:X8}.\r\n", mBoard.Timer.Control);
                return false;
            }
            if (mBoard.Timer.Load != 24)
            {
                mMessage += string.Format("Timer load register is incorrect. Was 0x{0:X8}.\r\n", mBoard.Timer.Load);
                return false;
            }

            //Test for 100 seconds

            List<uint> ssdValues;

            //10 seconds of nothing
            Console.WriteLine("Running program...");
            int tickLimit = (int)((6.25e6 * 10));
            ssdValues = RunTestCase(mBoard, tickLimit);

            if (ssdValues.Count != 0)
            {
                mMessage += "After running the program for 10 seconds without pressing any buttons, the following output was observed (expected nothing):\r\n";
                PrintSsdSequence(ssdValues);
                mMessage += "\r\nNote: SSD output is sampled when handling an exception. The timer should be disabled, so no exceptions should be taking place.\r\n";
                return false;
            }

            //Press start, and make sure it starts counting
            Console.WriteLine("Pressed start; expecting SSDs to begin counting...");
            tickLimit = (int)((6.25e6 * SUBTEST_DURATION_SECONDS));
            mBoard.Parallel.Buttons = (uint)Buttons.Start;
            Wag(mBoard);
            mBoard.Parallel.Buttons = (uint)Buttons.None;

            ssdValues = RunTestCase(mBoard, tickLimit);
            if (!CheckSequence(ssdValues, SUBTEST_DURATION_SECONDS, 2))
            {
                mMessage += "Ran your program for 20 (simulated) seconds after pressing the start button, and observed:\r\n";
                PrintSsdSequence(ssdValues);
                mMessage += "\r\nExpected to see a sequence from \"00\" to \"19\" inclusive, in the correct order.\r\n";
                return false;
            }
            if (mBoard.Parallel.InterruptAck != 0)
            {
                mMessage += "Parallel Interrupts were not acknowledged.";
                return false;
            }

            //Press reset, and make sure it continues counting
            Console.WriteLine("Pressed the lap button (at approx. 20 seconds); should have no effect on counting...");
            tickLimit = (int)((6.25e6 * SUBTEST_DURATION_SECONDS));
            mBoard.Parallel.Buttons = (uint)Buttons.Reset;
            Wag(mBoard);
            mBoard.Parallel.Buttons = (uint)Buttons.None;
            ssdValues.AddRange(RunTestCase(mBoard, tickLimit));
            if (!CheckSequence(ssdValues, SUBTEST_DURATION_SECONDS * 2, 4))
            {
                mMessage += "Ran your program for 40 (simulated) seconds; pressed the reset button at 20 seconds. Observed:\r\n";
                PrintSsdSequence(ssdValues);
                mMessage += "\r\nExpected to see a sequence from \"00\" to \"39\" inclusive, in the correct order. The reset button should have no effect while the timer is running.\r\n";
                return false;
            }
            if (mBoard.Parallel.InterruptAck != 0)
            {
                mMessage += "Parallel Interrupts were not acknowledged.";
                return false;
            }

            //Check lap output
            if (mSerialBuf.Length == 0)
            {
                mMessage += "Did not observe the lap time printed to serial port 1\r\n";
                return false;
            }
            else
            {
                double lapTime = 0.0;
                string rawLapTime = mSerialBuf;//.Trim();
                mSerialBuf = "";

                if (Regex.Match(rawLapTime, @"\r\n\d\d\.\d\d").Success)
                {
                    //check numeric value
                    double.TryParse(rawLapTime.Trim(), out lapTime); //will succeed, because of rexex match above.
                    if (Math.Abs(20 - lapTime) > 0.200)
                    {
                        mMessage += "Expected to receive a lap time of approximately 20 seconds. Observed: \"" + rawLapTime.Trim() + "\"\r\n";
                        return false;
                    }
                }
                else
                {
                    mMessage += "Lap output string was not in the correct format. Observed: \"" + rawLapTime.Trim() + "\". If this looks OK, make sure that you're correctly sending \\r and \\n before the numbers.";
                    return false;
                }

                Console.WriteLine("Lap Time Received: {0}", rawLapTime.Trim());
            }
            
            //Change switches, and make sure it continues counting
            Console.WriteLine("Changed switch value; should have no effect...");
            tickLimit = (int)((6.25e6 * SUBTEST_DURATION_SECONDS));
            mBoard.Parallel.Switches = 0x42;
            ssdValues.AddRange(RunTestCase(mBoard, tickLimit));
            if (!CheckSequence(ssdValues, SUBTEST_DURATION_SECONDS * 3, 6))
            {
                mMessage += "Ran your program for 60 (simulated) seconds; pressed the reset button at 20 seconds, and changed the switch value at 40 seconds. Observed:\r\n";
                PrintSsdSequence(ssdValues);
                mMessage += "\r\nExpected to see a sequence from \"00\" to \"59\" inclusive, in the correct order. Changing switches should have no effect.\r\n";
                return false;
            }
            if (mBoard.Parallel.InterruptAck != 0)
            {
                mMessage += "Parallel Interrupts were not acknowledged.";
                return false;
            }

            //Press stop, and make sure it stops counting
            Console.WriteLine("Pressed stop; expecting the value shown on the SSDs to stop counting...");
            tickLimit = (int)((6.25e6 * SUBTEST_DURATION_SECONDS));
            mBoard.Parallel.Buttons = (uint)Buttons.Start;
            Wag(mBoard);
            mBoard.Parallel.Buttons = (uint)Buttons.None;
            ssdValues.AddRange(RunTestCase(mBoard, tickLimit));
            if (!CheckSequence(ssdValues, SUBTEST_DURATION_SECONDS * 3, 6))
            {
                mMessage += "Ran your program for 80 (simulated) seconds; pressed the reset button at 20 seconds, changed the switch value at 40 seconds, and pressed 'stop' at 60 seconds. Observed:\r\n";
                PrintSsdSequence(ssdValues);
                mMessage += "\r\nExpected to see a sequence from \"00\" to \"59\" inclusive, in the correct order.\r\n";
                return false;
            }
            if (mBoard.Parallel.InterruptAck != 0)
            {
                mMessage += "Parallel Interrupts were not acknowledged.";
                return false;
            }

            //Press start again, and make sure it continues counting
            Console.WriteLine("Pressed start; expecting the value shown on the SSDs to resume counting...");
            tickLimit = (int)((6.25e6 * SUBTEST_DURATION_SECONDS));
            mBoard.Parallel.Buttons = (uint)Buttons.Start;
            Wag(mBoard);
            mBoard.Parallel.Buttons = (uint)Buttons.None;
            ssdValues.AddRange(RunTestCase(mBoard, tickLimit));
            if (!CheckSequence(ssdValues, SUBTEST_DURATION_SECONDS * 4, 6))
            {
                mMessage += "Ran your program for 100 (simulated) seconds; pressed the reset button at 20 seconds, changed the switch value at 40 seconds, pressed 'stop' at 60 seconds, and 'start' at 80 seconds. Observed:\r\n";
                PrintSsdSequence(ssdValues);
                mMessage += "\r\nExpected to see a sequence from \"00\" to \"79\" inclusive, in the correct order.\r\n";
                return false;
            }
            if (mBoard.Parallel.InterruptAck != 0)
            {
                mMessage += "Parallel Interrupts were not acknowledged.";
                return false;
            }

            //Hit the lap button again
            Console.WriteLine("Pressed the lap button again (at approx. 80 seconds); expecting counting to continue as usual...");
            mBoard.Parallel.Buttons = (uint)Buttons.Reset;
            Wag(mBoard);
            mBoard.Parallel.Buttons = (uint)Buttons.None;
            Wag(mBoard, 1000000);

            //Check lap output
            if (mSerialBuf.Length == 0)
            {
                mMessage += "Did not observe the lap time printed to serial port 1\r\n";
                return false;
            }
            else
            {
                double lapTime = 0.0;
                string rawLapTime = mSerialBuf;//.Trim();
                mSerialBuf = "";

                if (Regex.Match(rawLapTime, @"\r\n\d\d\.\d\d").Success)
                {
                    //check numeric value
                    double.TryParse(rawLapTime.Trim(), out lapTime); //will succeed, because of rexex match above.
                    if (Math.Abs(80 - lapTime) > 0.500)
                    {
                        mMessage += "Expected to receive a lap time of approximately 80 seconds. Observed: \"" + rawLapTime.Trim() + "\"\r\n";
                        return false;
                    }
                }
                else
                {
                    mMessage += "Lap output string was not in the correct format. Observed: \"" + rawLapTime.Trim() + "\". If this looks OK, make sure that you're correctly sending \\r and \\n before the numbers.";
                    return false;
                }

                Console.WriteLine("Lap Time Received: {0}", rawLapTime.Trim());
            }
            if (mBoard.Parallel.InterruptAck != 0)
            {
                mMessage += "Parallel Interrupts were not acknowledged.";
                return false;
            }

            //Press stop, then reset, and make sure it goes back to zero
            Console.WriteLine("Pressed stop, then reset; expecting the timer to be disabled, and the value on the SSDs to be reset to zero...");
            tickLimit = (int)((6.25e6 * SUBTEST_DURATION_SECONDS));
            mBoard.Parallel.Buttons = (uint)Buttons.Start;
            Wag(mBoard);
            mBoard.Parallel.Buttons = (uint)Buttons.Reset;
            Wag(mBoard);
            ssdValues = RunTestCase(mBoard, tickLimit);
            if (!CheckSequence(ssdValues, 0, 0))
            {
                mMessage += "After pressing the reset button (button 1) while the timer is stopped, the SSDs are expected to be set to zero and the timer is supposed to remain stopped. Observed:\r\n";
                PrintSsdSequence(ssdValues);
                return false;
            }
            if (mBoard.Parallel.InterruptAck != 0)
            {
                mMessage += "Parallel Interrupts were not acknowledged.";
                return false;
            }

            //Press start, and make sure it counts again
            Console.WriteLine("Pressed start again; expecting SSDs to start counting from zero...");
            tickLimit = (int)((6.25e6 * SUBTEST_DURATION_SECONDS));
            mBoard.Parallel.Buttons = (uint)Buttons.Start;
            Wag(mBoard);
            mBoard.Parallel.Buttons = (uint)Buttons.None;

            ssdValues = RunTestCase(mBoard, tickLimit);
            if (!CheckSequence(ssdValues, SUBTEST_DURATION_SECONDS, 2))
            {
                mMessage += "Ran your program for 20 (simulated) seconds after pressing the start button, and observed:\r\n";
                PrintSsdSequence(ssdValues);
                mMessage += "\r\nExpected to see a sequence from \"00\" to \"19\" inclusive, in the correct order.\r\n";
                return false;
            }
            if (mBoard.Parallel.InterruptAck != 0)
            {
                mMessage += "Parallel Interrupts were not acknowledged.";
                return false;
            }

            return true;
        }
    }
}
