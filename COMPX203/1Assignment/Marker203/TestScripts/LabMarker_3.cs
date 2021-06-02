using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RexSimulator.Hardware;
using System.IO;

namespace COMP200Marker.TestScripts
{
    abstract class LabMarker_3 : LabMarker
    {
        public override string Version { get { return "19A.0.0"; } }

        private const int CLOCK_CYCLE_LIMIT = 4000000;
        private static readonly int NUM_TICKS = 1000000;

        new protected string mSP1RecvBuf;
        new protected string mSP2RecvBuf;

        internal bool SupressReset = false; //set true if you don't want to reset the board when calling .Mark()
        
        /// <summary>
        /// Executes a test case.
        /// </summary>
        /// <param name="sendSP1">String to send to the serial port SP1</param>
        /// <param name="sendSP2">String to send to the serial port SP2</param>
        /// <param name="expectSP1">String expected to be received from SP1</param>
        /// <param name="expectSP2">String expected to be received from SP2</param>
        /// <param name="board">The board to run the tests on.</param>
        /// <returns>True if the test passes.</returns>
        new protected bool RunSerialTestCase(string sendSP1, string sendSP2, string expectSP1, string expectSP2, RexBoard board)
        {
            /*Console.WriteLine("Test Case: ");
            if (sendSP1.Length > 0)
                Console.WriteLine("-Sending '{0}' to SP1", sendSP1);
            if (sendSP2.Length > 0)
                Console.WriteLine("-Sending '{0}' to SP2", sendSP2);
            if (expectSP1.Length > 0)
                Console.WriteLine("-Expecting '{0}' from SP1", expectSP1);
            if (expectSP2.Length > 0)
                Console.WriteLine("-Expecting '{0}' from SP2", expectSP2);*/

            mSP1RecvBuf = mSP2RecvBuf = "";
            EventHandler<RexSimulator.Hardware.Rex.SerialIO.SerialEventArgs> serialHandler1 = new EventHandler<RexSimulator.Hardware.Rex.SerialIO.SerialEventArgs>(Serial1_SerialDataTransmitted);
            EventHandler<RexSimulator.Hardware.Rex.SerialIO.SerialEventArgs> serialHandler2 = new EventHandler<RexSimulator.Hardware.Rex.SerialIO.SerialEventArgs>(Serial2_SerialDataTransmitted);
            board.Serial1.SerialDataTransmitted += serialHandler1;
            board.Serial2.SerialDataTransmitted += serialHandler2;

            //uint pc = board.CPU.PC;
            //board.Reset(true);
            //board.CPU.PC = pc;
            //board.CPU.mGpRegisters[RexSimulator.Hardware.Wramp.RegisterFile.GpRegister.sp] = 0x07b7f; //sensible value for the stack pointer
            //board.CPU.mGpRegisters[RexSimulator.Hardware.Wramp.RegisterFile.GpRegister.ra] = 0x80000; //when the CPU tries to jump here, the test is over.
            //board.CPU.mSpRegisters[RexSimulator.Hardware.Wramp.RegisterFile.SpRegister.evec] = 0x80000; //when the CPU tries to jump here, the test is over (should jal to exit, which is syscall).

            int clocks = CLOCK_CYCLE_LIMIT;

            //Transmit the data
            string toSend1 = sendSP1;
            string toSend2 = sendSP2;
            while (clocks-- > 0)
            {
                board.Tick();
                if (toSend1.Length != 0 && board.Serial1.SendAsync(toSend1[0]))
                    toSend1 = toSend1.Substring(1);

                if (toSend2.Length != 0 && board.Serial2.SendAsync(toSend2[0]))
                    toSend2 = toSend2.Substring(1);
            }

            //board.CPU.PC = pc;
            board.Serial1.SerialDataTransmitted -= serialHandler1;
            board.Serial2.SerialDataTransmitted -= serialHandler2;

            //Make sure the Done message was received
            if (mSP1RecvBuf != expectSP1)
            {
                if (sendSP1.Length != 0)
                    mMessage += string.Format("Sent \"{0}\" to SP1\r\n", sendSP1);
                if (sendSP2.Length != 0)
                    mMessage += string.Format("Sent \"{0}\" to SP2\r\n", sendSP2);

                mMessage += string.Format("Received \"{0}\" from SP1, expected \"{1}\"\r\n", mSP1RecvBuf, expectSP1);
                return false;
            }

            if (mSP2RecvBuf != expectSP2)
            {
                if (sendSP1.Length != 0)
                    mMessage += string.Format("Sent \"{0}\" to SP1\r\n", sendSP1);
                if (sendSP2.Length != 0)
                    mMessage += string.Format("Sent \"{0}\" to SP2\r\n", sendSP2);

                mMessage += string.Format("Received \"{0}\" from SP2, expected \"{1}\"\r\n", mSP2RecvBuf, expectSP2);
                return false;
            }

            return true;
        }

        protected void Tick(RexBoard mBoard)
        {
            for (int i = 0; i < NUM_TICKS; i++)
                mBoard.Tick();
        }

        protected bool RunParallelTestCase(RexBoard mBoard, uint switches)
        {
            //Set switches
            mBoard.Parallel.Switches = switches;

            //Check that the SSDs don't change without waiting for a button press
            uint ssdOld = mBoard.Parallel.SSD & 0xFFFF;
            Tick(mBoard);
            if (ssdOld != (mBoard.Parallel.SSD & 0xFFFF))
            {
                mMessage += "SSDs changed value without waiting for a button press.\r\n";
                return false;
            }

            //Press button 0
            mBoard.Parallel.Buttons = 1;
            Tick(mBoard);
            if (mBoard.Parallel.SSD != switches)
            {
                mMessage += string.Format("SSDs did not show the correct value after pressing button 0 (observed: {0:X4}, switches: {1:X4})\r\n", mBoard.Parallel.SSD, switches);
                return false;
            }
            if ((switches & 0xFFFF) % 4 == 0)
            {
                if ((mBoard.Parallel.Leds & 0xFFFF) != 0xFFFF)
                {
                    mMessage += string.Format("LEDs were not high after pressing button 0 (observed: {0:X4}, switches {1:X4})\r\n", mBoard.Parallel.Leds, switches);
                    return false;
                }
            }
            else
            {
                if ((mBoard.Parallel.Leds & 0xFFFF) != 0x0000)
                {
                    mMessage += string.Format("LEDs were not low after pressing button 0 (observed: {0:X4}, switches {1:X4})\r\n", mBoard.Parallel.Leds, switches);
                    return false;
                }
            }

            //Press button 1
            mBoard.Parallel.Buttons = 2;
            Tick(mBoard);
            if ((mBoard.Parallel.SSD & 0xFFFF) != (~switches & 0xFFFF))
            {
                mMessage += string.Format("SSDs did not show the correct value after pressing button 1 (observed: {0:X4}, switches: {1:X4})\r\n", mBoard.Parallel.SSD, switches);
                return false;
            }
            if ((~switches & 0xFFFF) % 4 == 0)
            {
                if ((mBoard.Parallel.Leds & 0xFFFF) != 0xFFFF)
                {
                    mMessage += string.Format("LEDs were not high after pressing button 1 (observed: {0:X4}, switches {1:X4})\r\n", mBoard.Parallel.Leds, switches);
                    return false;
                }
            }
            else
            {
                if ((mBoard.Parallel.Leds & 0xFFFF) != 0x0000)
                {
                    mMessage += string.Format("LEDs were not low after pressing button 1 (observed: {0:X4}, switches {1:X4})\r\n", mBoard.Parallel.Leds, switches);
                    return false;
                }
            }

            //Release buttons
            mBoard.Parallel.Buttons = 0;
            Tick(mBoard);
            return true;
        }

        void Serial1_SerialDataTransmitted(object sender, RexSimulator.Hardware.Rex.SerialIO.SerialEventArgs e)
        {
            mSP1RecvBuf += (char)e.Data;
        }

        void Serial2_SerialDataTransmitted(object sender, RexSimulator.Hardware.Rex.SerialIO.SerialEventArgs e)
        {
            mSP2RecvBuf += (char)e.Data;
        }

        public override bool Mark(RexSimulator.Hardware.RexBoard mBoard)
        {
            if (SupressReset)
                return true;

            mBoard.CPU.mGpRegisters[RexSimulator.Hardware.Wramp.RegisterFile.GpRegister.sp] = 0x03b7f; //sensible value for the stack pointer
            mBoard.CPU.mGpRegisters[RexSimulator.Hardware.Wramp.RegisterFile.GpRegister.ra] = 0x80000; //when the CPU tries to jump here, the test is over.
            mBoard.CPU.mSpRegisters[RexSimulator.Hardware.Wramp.RegisterFile.SpRegister.evec] = 0x80000; //when the CPU tries to jump here, the test is over (should jal to exit, which is syscall).
            return true;
        }
    }
}
