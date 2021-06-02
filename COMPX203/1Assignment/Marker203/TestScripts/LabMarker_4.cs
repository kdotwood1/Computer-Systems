using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RexSimulator.Hardware.Wramp;
using RexSimulator.Hardware;

namespace COMP200Marker.TestScripts
{
    abstract class LabMarker_4 : LabMarker
    {
        public override string Version { get { return "19A.1.0"; } }

        protected const int CLOCK_CYCLE_LIMIT = 4000000;
        protected string mSerialBuf = "";
        protected const uint WRAMPMON_ADDRESS = 0x80000;
        protected const uint TEST_EVEC = 0x81234;
        protected uint program_start;

        protected enum Buttons
        {
            None = 0, Start = 2, Reset = 1, Exit = 4
        }

        /// <summary>
        /// Wags the dog.
        /// </summary>
        /// <param name="board"></param>
        protected int Wag(RexBoard board, int count = 10000)
        {
            for (int i = 0; i < count; i++)
            {
                board.Tick();
            }
            return count;
        }

        /// <summary>
        /// The value that $CCTRL is expected to be.
        /// </summary>
        protected abstract uint ExpectedCCTRL { get; }

        protected abstract bool DoSubTest(RexBoard mBoard);

        public override bool Mark(RexSimulator.Hardware.RexBoard mBoard)
        {
            mBoard.Serial2.SerialDataTransmitted += new EventHandler<RexSimulator.Hardware.Rex.SerialIO.SerialEventArgs>(Serial2_SerialDataTransmitted);

            program_start = mBoard.CPU.PC;

            //Use a reasonable value for the old exception vector
            mBoard.CPU.mSpRegisters[RegisterFile.SpRegister.evec] = TEST_EVEC;

            Wag(mBoard);

            //Ensure the cctrl and evec are fine.
            if ((mBoard.CPU.mSpRegisters[RegisterFile.SpRegister.cctrl] & 0x000002E2) != (ExpectedCCTRL & 0x000002E2)) //don't care if the interrupts for SP1 or the unused stuff are on or off
            {
                mMessage += string.Format("$cctrl was not set to the correct value: was 0x{0:X8}\r\n", mBoard.CPU.mSpRegisters[RegisterFile.SpRegister.cctrl]);
                return false;
            }

            if (mBoard.CPU.mSpRegisters[RegisterFile.SpRegister.evec] >= WRAMPMON_ADDRESS)
            {
                mMessage += "$evec still points to the original exception handler.\r\n";
                return false;
            }

            //Perform question-specific tests
            if (!DoSubTest(mBoard))
                return false;

            mBoard.CPU.InterruptStatus |= 0x1000; //raise a GPF exception
            for (int i = 0; i < CLOCK_CYCLE_LIMIT; i++)
            {
                mBoard.Tick();
                if (mBoard.CPU.PC >= WRAMPMON_ADDRESS)
                {
                    break;
                }
            }
            if (mBoard.CPU.PC != TEST_EVEC && mBoard.CPU.PC != TEST_EVEC + 1)
            {
                mMessage += "Did not jump to the default exception handler to handle a GPF exception. Are you saving $evec correctly?\r\n";
                return false;
            }

            return true;
        }

        void Serial2_SerialDataTransmitted(object sender, RexSimulator.Hardware.Rex.SerialIO.SerialEventArgs e)
        {
            mSerialBuf += (char)e.Data;
        }
    }
}
