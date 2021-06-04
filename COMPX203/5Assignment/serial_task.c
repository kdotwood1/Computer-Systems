#include "wramp.h"

int counter;

void serial_main(){
   while (1){
      WrampSp2->Tx = counter;
      int format = WrampSp2->Rx;
      switch (format):
         case 1:
            writeMinSec();
            break;
         case 2:
            writeSec();
            break;
         case 3:
            writeTimerTicks();
            break;
         case 'q':
            return;
   }
}

void writeMinSec(){
   
}

void writeSec(){}

void writeTimerTicks(){}
