.text
.global main
main:
   addi $3, $0, 0x61	#hex value for the ascii 'a' to as a starting point
   addi $4, $0, 26	#a counter to reset for uppercase letters 
check:
   lw $13, 0x71003($0) #stores value from the serial port 2 status register
   andi $13, $13, 0x2	#ands the value above to get the 2nd significant bit
   beqz $13, check     #if that value is zero try again else
   sw $3, 0x71000($0)	#sends the val of reg3 to serial port 2 transmit data register
   addi $3, $3, 1	#increase the value in reg3 to get the next value in the alphabet
   subi $4, $4, 1	#decrease the count of lowercase to know when to break and change to upper
   seqi $7, $3, 0x5B	#if reg3 equals 'Z' then set reg7 to 1
   bnez $7, end	#if reg7 greater than 0 go to end of program
   beqz $4, uppercase	#if count of lowercase is zero break to reset for uppercase
   j check		#else loop back to check
uppercase:
   addi $3, $0, 0x41	#resets the value of characters to be displayed to 'A'
   j check		#jump back in to checkiing the value
end:
   syscall		#end program
