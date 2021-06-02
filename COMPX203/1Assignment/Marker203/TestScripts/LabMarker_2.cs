using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RexSimulator.Hardware;
using RexSimulator.Hardware.Wramp;

namespace COMP200Marker.TestScripts
{
    abstract class LabMarker_2 : LabMarker
    {
        public override string Version { get { return "19A.1.0"; } }

        #region Shared for all three

        protected uint originalPC;
        internal bool SupressReset = false; //set true if you don't want to reset the board when calling .ResetBoard()

        /// <summary>
        /// Set up the state we want to restore when ResetBoard is called.false
        /// Note that this pair of functions won't preserve pre-initialised memory!
        /// </summary>
        protected void Initialise(RexBoard mBoard)
        {
            originalPC = mBoard.CPU.PC;
        }

        /// <summary>
        /// Restore the state we set up in Initialise, plus some other stuff.
        /// </summary>
        protected void ResetBoard(RexBoard mBoard)
        {
            if (SupressReset)
                return;

            mBoard.CPU.PC = originalPC;

            mBoard.CPU.mGpRegisters[RexSimulator.Hardware.Wramp.RegisterFile.GpRegister.sp] = 0x03b7f; //sensible value for the stack pointer
            mBoard.CPU.mGpRegisters[RexSimulator.Hardware.Wramp.RegisterFile.GpRegister.ra] = 0xFFFFF; //when the CPU tries to jump here, the test is over.
            mBoard.CPU.mSpRegisters[RexSimulator.Hardware.Wramp.RegisterFile.SpRegister.evec] = 0x80000; //when the CPU tries to jump here, the test is over (should jal to exit, which is syscall).
        }

        #endregion
        
        #region Shared between questions 2 and 3

        protected const int TEST_DURATION_SECONDS = 1;
        protected int TEST_DURATION_TICKS
        {
            get { return (int)(6.25e6 * TEST_DURATION_SECONDS); }
        }

        protected readonly uint[] TEST_CASES = new uint[]
        {
            0x000F, 0x00F0, 0x0F00, 0xF000, 0x00FF, 0xFF00,
            0x0000, 0xFFFF, 0x0F0F, 0xF0F0, 0xA5A5, 0x5A5A,
            0xdead, 0xbeef, 0xf00d, 0x1234, 0xF080, 0x80F0
        };

        /// <summary>
        /// Converts a decimal number on the SSDs to its value.
        /// </summary>
        /// <param name="ssd">The value of the seven-segment displays</param>
        protected uint GetSSDNumber(uint ssd)
        {
            uint ones = ssd & 0xF;
            uint tens = (ssd >> 4) & 0xF;
            uint hundreds = (ssd >> 8) & 0xF;
            uint thousands = (ssd >> 12) & 0xF;

            return (thousands * 1000) + (hundreds * 100) + (tens * 10) + ones;
        }

        /// <summary>
        /// Returns the contents up to the array, up to a certain character limit.
        /// Similar to string.Join().
        /// </summary>
        protected string GetCappedLengthList(List<uint> ssdValues, int charLimit = 800)
        {
            string msg = "";
            int i = 0;
            while (msg.Length < charLimit && i < ssdValues.Count)
            {
                msg += $"{ssdValues[i++]:X2}";
                msg += ", ";
            }
            if (i == ssdValues.Count)
            {
                return msg.Substring(0, msg.Length - 2);
            }
            return msg + "and a lot more...";
        }

        /// <summary>
        /// Executes a test case for questions 2 and 3. Initialise() should be called before this function.
        /// </summary>
        /// <param name="board">The RexBoard to use.</param>
        /// <param name="switches">The value to set the switches to for this case.</param>
        /// <param name="questionNumber">Either 2 or 3 depending on the question.</param>
        protected bool TryTestCase(RexBoard board, uint switches, bool startFirst)
        {
            ResetBoard(board);
            board.Parallel.Switches = switches;
            List<uint> ssdValues = new List<uint> {  };

            uint start, end;
            if (startFirst)
            {
                start = (switches >> 8) & 0xFF;
                end = switches & 0xFF;
            }
            else
            {
                end = (switches >> 8) & 0xFF;
                start = switches & 0xFF;
            }

            // We start the SSDs at a value different from the one they should start at when
            // the program begins - this value is ignored, but a change is detected unless
            // the student writes the same number to the same register.
            // Of course, this number is pretty nonsensical so it's unlikely.
            board.Parallel.RightSSD = 0xdead1ed;
            uint oldLRSSD = board.Parallel.RightSSD;

            for (int i = 0; i < TEST_DURATION_TICKS; i++)
            {
                // Hacky way to detect when the program is in delay() and skip the loop
                // Speeds up marking by around a hundred thousand times, but relies on
                // the delay() function allocating its specialNumber local variable to
                // a particular register - Perfect for wcc!
                if (board.CPU.mGpRegisters[RegisterFile.GpRegister.r13] == 0xdeadf00d)
                {
                    // We use 5 to preserve the stack rather than jumping straight out of the
                    // function, and change the register so this doesn't happen several times.
                    board.CPU.PC += 5;
                    board.CPU.mGpRegisters[RegisterFile.GpRegister.r13] = 0xbeeff00d;
                }
                
                board.Tick();

                uint ssd = GetSSDNumber(board.Parallel.SSD);

                // We check the rightmost SSD since it's guaranteed to change on any counting operation
                if (board.Parallel.RightSSD != oldLRSSD)
                {
                    // Uncomment these if you want some more verbose output during the test.
                    // It happens so fast with the delay skip that it's not really worth keeping around.
                    /*
                    try
                    {
                        Console.Write($"{new string(' ', Console.WindowWidth)}\r");
                        Console.Write($"Test case {c+1}/{TEST_CASES.Length} (0x{switches:X4}): {ssd}\r");
                    }
                    catch (System.IO.IOException)
                    {
                        // Git bash doesn't support the WindowWidth call.
                    }
                    */

                    ssdValues.Add(ssd);

                    // Don't exit early if there should be no counting, just to make sure they don't count.
                    if (start != end && ssd == end) break;
                    oldLRSSD = board.Parallel.RightSSD;
                }
            }
            try
            {
                Console.Write($"{new string(' ', Console.WindowWidth)}\r");
            }
            catch (System.IO.IOException)
            {
                // Git bash doesn't support the WindowWidth call.
            }

            // If there should be counting, but there isn't, complain.
            if (ssdValues.Count == 0)
            {
                if (start == end)
                {
                    return true;
                }
                else
                {
                    mMessage = $"I didn't see any counting. The switches were {switches:X4}.";
                    return false;
                }
            }
            // If there shouldn't be counting, but there is, complain.
            if (start == end)
            {
                if (ssdValues.Count == 1 && ssdValues[0] == start)
                {
                    return true;
                }
                else
                {
                    mMessage = $"You counted when you shouldn't have! The switches were {switches:X4}.";
                    mMessage += Environment.NewLine + "Got numbers: " + GetCappedLengthList(ssdValues);
                }
            }

            // If they started or finished counting at the wrong place, complain.
            if (ssdValues[0] != start)
            {
                mMessage = $"Counting started at {ssdValues[0]}, not {start} when the switches were 0x{switches:X4}.";
                mMessage += Environment.NewLine + "Got numbers: " + GetCappedLengthList(ssdValues);
                return false;
            }
            if (ssdValues[ssdValues.Count - 1] != end)
            {
                mMessage = $"Counting ended at {ssdValues[ssdValues.Count - 1]}, not {end} when the switches were 0x{switches:X4}.";
                mMessage += Environment.NewLine + "Got numbers: " + GetCappedLengthList(ssdValues);
                return false;
            }

            // If they counted in a weird order, complain.
            // Of course it's good code when I'm duplicating everything except the direction!
            if (start < end)
            {
                uint previousValue = uint.MinValue;
                foreach (uint value in ssdValues)
                {
                    if (value < previousValue)
                    {
                        if (value == previousValue - 1)
                        {
                            mMessage = $"Counting was backwards when the switches were {switches:X4}.";
                        }
                        else
                        {
                            mMessage = $"Counting did not happen in the correct order when the switches were {switches:X4}.";
                        }
                        mMessage += Environment.NewLine + "Got numbers: " + GetCappedLengthList(ssdValues);
                        return false;
                    }
                    if (previousValue != uint.MinValue && value != previousValue + 1)
                    {
                        mMessage = $"It's not counting when you go from {previousValue} to {value}! The switches were {switches:X4}";
                        mMessage += Environment.NewLine + "Got numbers: " + GetCappedLengthList(ssdValues);
                        return false;
                    }
                }
            }
            if (start > end)
            {
                uint previousValue = uint.MaxValue;
                foreach (uint value in ssdValues)
                {
                    if (value > previousValue)
                    {
                        if (value == previousValue + 1)
                        {
                            mMessage = $"Counting was backwards when the switches were {switches:X4}.";
                        }
                        else
                        {
                            mMessage = $"Counting did not happen in the correct order when the switches were {switches:X4}.";
                        }
                        mMessage += Environment.NewLine + "Got numbers: " + GetCappedLengthList(ssdValues);
                        return false;
                    }
                    if (previousValue != uint.MaxValue && value != previousValue - 1)
                    {
                        mMessage = $"It's not counting when you go from {previousValue} to {value}! The switches were {switches:X4}";
                        mMessage += Environment.NewLine + "Got numbers: " + GetCappedLengthList(ssdValues);
                        return false;
                    }
                }
            }
            return true;
        }

        #endregion
    }
}