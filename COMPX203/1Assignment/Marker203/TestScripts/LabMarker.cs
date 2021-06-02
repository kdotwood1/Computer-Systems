using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RexSimulator.Hardware;

namespace COMP200Marker.TestScripts
{
    abstract public class LabMarker
    {
        /// <summary>
        /// Version number for lab marker scripts.
        /// These take the format "YYS.M.P",
        /// where YYS is the three-character year-semester code that the scripts are designed to target,
        /// M is the minor version, and P is the patch version.
        /// For example, the version built to mark the assignments for COMPX203 in A semester of 2019 would
        /// have the version string "19A.x.x", with sequentially-incrementing minor and patch versions
        /// similar to SemVer. It's a property rather than a field so we can override it later.
        /// </summary>
        public virtual string Version { get { return "19A.0.0"; } }

        protected string mMessage = "";

        /// <summary>
        /// The message saying why the test failed.
        /// </summary>
        public string Message { get { return mMessage; } }

        /// <summary>
        /// Marks the program.
        /// </summary>
        /// <param name="mBoard">The REX board, pre-loaded with the program to test.</param>
        /// <returns>True if the program passes, false otherwise.</returns>
        public abstract bool Mark(RexBoard mBoard);

        #region Handy Shared Functions

        private const int CLOCK_CYCLE_LIMIT = 4000000;

        protected string mSP1RecvBuf;
        protected string mSP2RecvBuf;

        internal bool Verbose = false;

        /// <summary>
        /// Executes a test case.
        /// </summary>
        /// <param name="sendSP1">String to send to the serial port SP1</param>
        /// <param name="sendSP2">String to send to the serial port SP2</param>
        /// <param name="expectSP1">String expected to be received from SP1</param>
        /// <param name="expectSP2">String expected to be received from SP2</param>
        /// <param name="board">The board to run the tests on.</param>
        /// <returns>True if the test passes.</returns>
        protected bool RunSerialTestCase(string sendSP1, string sendSP2, string expectSP1, string expectSP2, RexBoard board)
        {
            if (Verbose)
            {
                Console.WriteLine("Test Case: ");
                if (sendSP1.Length > 0)
                    Console.WriteLine("-Sending '{0}' to SP1", sendSP1);
                if (sendSP2.Length > 0)
                    Console.WriteLine("-Sending '{0}' to SP2", sendSP2);
                if (expectSP1.Length > 0)
                    Console.WriteLine("-Expecting '{0}' from SP1", expectSP1);
                if (expectSP2.Length > 0)
                    Console.WriteLine("-Expecting '{0}' from SP2", expectSP2);
            }

            // We hook into the SerialIO devices' event handlers to correctly receive characters
            mSP1RecvBuf = mSP2RecvBuf = "";
            EventHandler<RexSimulator.Hardware.Rex.SerialIO.SerialEventArgs> serialHandler1 = new EventHandler<RexSimulator.Hardware.Rex.SerialIO.SerialEventArgs>(Serial1_SerialDataTransmitted);
            EventHandler<RexSimulator.Hardware.Rex.SerialIO.SerialEventArgs> serialHandler2 = new EventHandler<RexSimulator.Hardware.Rex.SerialIO.SerialEventArgs>(Serial2_SerialDataTransmitted);
            board.Serial1.SerialDataTransmitted += serialHandler1;
            board.Serial2.SerialDataTransmitted += serialHandler2;

            int clocks = CLOCK_CYCLE_LIMIT;

            //Transmit the data, and exit early if the received data happens sooner than we hoped
            string toSend1 = sendSP1;
            string toSend2 = sendSP2;
            while (clocks-- > 0)
            {
                board.Tick();
                if (toSend1.Length != 0 && board.Serial1.SendAsync(toSend1[0]))
                    toSend1 = toSend1.Substring(1);

                if (toSend2.Length != 0 && board.Serial2.SendAsync(toSend2[0]))
                    toSend2 = toSend2.Substring(1);

                if (expectSP1.Length != 0 && mSP1RecvBuf == expectSP1) break; 
                if (expectSP2.Length != 0 && mSP2RecvBuf == expectSP2) break;
            }

            board.Serial1.SerialDataTransmitted -= serialHandler1;
            board.Serial2.SerialDataTransmitted -= serialHandler2;

            //Make sure the correct message was received
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

        void Serial1_SerialDataTransmitted(object sender, RexSimulator.Hardware.Rex.SerialIO.SerialEventArgs e)
        {
            mSP1RecvBuf += (char)e.Data;
        }

        void Serial2_SerialDataTransmitted(object sender, RexSimulator.Hardware.Rex.SerialIO.SerialEventArgs e)
        {
            mSP2RecvBuf += (char)e.Data;
        }

        #endregion
    }
}
