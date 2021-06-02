using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace COMP200Marker.TestScripts
{
    class LabMarker_kernel : LabMarker
    {
        public override string Version { get { return "19A.0.0"; } }

        public override bool Mark(RexSimulator.Hardware.RexBoard mBoard)
        {
            //TODO: kernel marking script
            mMessage = "Kernel submitted. This is not confirmation that it works as required - it will need to be manually verified by a marker on the due date.";
            return true;
        }
    }
}
