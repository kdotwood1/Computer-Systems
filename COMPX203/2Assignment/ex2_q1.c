#include "/home/compx203/ex2/lib_ex2.h"
/**
* A method which counts from "start" to "end" and displays to user via the SSD
**/
void count(int start, int end){
   if (0 <= start <10000 && 0<= end <10000){ // First condition of both numbers being >0 and <10000
      if (start > end){                      // Condition to count down
         while (start != end){               // Stops the loop from counting past the end
            writessd(start);                 // Writes to the SSD with a delay afterward for ease of viewing
            delay();
            start--;                         // decrements start to get closer to end
         }
      } else {                               // Condition to count up
         while (start != end){               // Stops the loop from counting past the end
            writessd(start);                 // Writes to the SSD with a delay afterward for ease of viewing
            delay();
            start++;                         // increments start to get closer to end
         }
      }
      writessd(start);                       // because start will = end and exit the loop we need an extra writessd call
   }
}
