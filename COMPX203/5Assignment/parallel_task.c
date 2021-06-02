#include "wramp.h"

void parallel_main(){
   while (1){    
      unsigned int swit = (unsigned) WrampParallel->Switches;
      unsigned int butt = (unsigned) WrampParallel->Buttons;
      switch (butt){
         case 1:
            writeHex(swit);
            break;
         case 2:
            writeDec(swit);
            break;
         case 4:
            return;
      }
   }
}

static void writeHex(unsigned int swNum){
   WrampParallel->LowerRightSSD = swNum;
   swNum = swNum >> 4;
   WrampParallel->LowerLeftSSD = swNum;
   swNum = swNum >> 4;
   WrampParallel->UpperRightSSD = swNum;
   swNum = swNum >> 4;
   WrampParallel->UpperLeftSSD = swNum;
   return;
}

static void writeDec(unsigned int swiNum){
   unsigned int temp = swiNum << 12;
   temp = temp >> 12;
   if (temp > 9) { temp = 9; }
   WrampParallel->LowerRightSSD = temp;
   temp = swiNum << 8;
   temp = temp >> 12;
   if (temp > 9) { temp = 9; }
   WrampParallel->LowerLeftSSD = temp;
   temp = swiNum << 4;
   temp = temp >> 12;
   if (temp > 9) { temp = 9; }
   WrampParallel->UpperRightSSD = temp;
   temp = swiNum >> 12;
   if (temp > 9) { temp = 9; }
   WrampParallel->UpperLeftSSD = temp;
   return;
}
