using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RexSimulator.Hardware.Wramp;
using RexSimulator.Hardware;

namespace COMP200Marker.TestScripts
{
    class LabMarker_4_2 : LabMarker_4
    {
        public override string Version { get { return "19A.1.0"; } }

        protected override uint ExpectedCCTRL
        {
            get { return 0x4a; }
        }

        protected virtual int TEST_DURATION_SECONDS
        {
            get { return 100; }
        }

        protected override bool DoSubTest(RexBoard mBoard)
        {
            //Ensure timer is set up right
            if (mBoard.Timer.Control != 3)
            {
                mMessage += string.Format("Timer control register is incorrect. Was 0x{0:X8}.\r\n", mBoard.Timer.Control);
                return false;
            }
            if (mBoard.Timer.Load != 2400)
            {
                mMessage += string.Format("Timer load register is incorrect. Was 0x{0:X8}.\r\n", mBoard.Timer.Load);
                return false;
            }

            //Test for 100 seconds

            //Sneaky speedup to 10 interrupts per second, to save time.
            mBoard.Timer.Count = 0;
            mBoard.Timer.Load = 240;

            List<uint> ssdValues = new List<uint>();

            long ticksAtStart = mBoard.TickCounter;
            int tickLimit = (int)((6.25e6 * TEST_DURATION_SECONDS + ticksAtStart) / 10);
            for (int i = 0; i < tickLimit; i++)
            //while(mBoard.TickCounter < 4e6 * TEST_DURATION_SECONDS / 10)
            {
                mBoard.Tick();

                //Record SSD values at each interrupt
                if (mBoard.CPU.PC == mBoard.CPU.mSpRegisters[RegisterFile.SpRegister.evec] + 1)
                {
                    if (ssdValues.Count == 0 || ssdValues.Last() != (mBoard.Parallel.LeftSSD & 0xf) * 10 + (mBoard.Parallel.RightSSD & 0xf))
                        ssdValues.Add((mBoard.Parallel.LeftSSD & 0xf) * 10 + (mBoard.Parallel.RightSSD & 0xf));
                    Console.Write("{0:D2}%\r", (int)(100.0 * i / tickLimit));
                }
            }

            //Check for the correct sequence
            bool correctSequence = true;
            if (ssdValues.Count == TEST_DURATION_SECONDS)
            {
                for (uint i = 0; i < TEST_DURATION_SECONDS; i++)
                {
                    if (ssdValues[(int)i] != i)
                    {
                        correctSequence = false;
                        break;
                    }
                }
            }
            else
            {
                correctSequence = false;
            }

            if (!correctSequence)
            {
                mMessage += "Ran your program for 100 (simulated) seconds and observed:\r\n";
                foreach (uint i in ssdValues)
                    mMessage += string.Format("{0:D2}, ", i);

                mMessage += "\r\n\r\nExpected to see a sequence from \"00\" to \"99\" inclusive, in the correct order.\r\n";
                    return false;
            }

            return true;
        }
    }
}
