.text
.global main
main:
   lw $13, 0x70003($0) 	#stores value of serial port 1 status reg
   andi $13, $13, 0x1		#logical and to get only the first bit (any input received)
   beqz $13, main		#if zero try again else (nothing has been typed in yet)
   lw $3, 0x70001($0)		#load the char into a reg
   sgei $4, $3, 'a'		#tests to see if character is > 'a'
   slei $5, $3, 'z'		#tests to see if character is > 'z'
   seq $6, $4, $5		#a test to see if both tests above returned true
   bnez $6, printchar		#if so jump to print the character
   addi $3, $0, '*'		#else save '*' as the character entered
printchar:
   lw $13, 0x70003($0)		#check again to see if serial port 1 is ready
   andi $13, $13, 0x2		#logical and for the second significant bit (able to receive output)
   beqz $13, printchar		#if not try again
   sw $3, 0x70000($0)		#transmits char to serial port 1
   j main			#start again
