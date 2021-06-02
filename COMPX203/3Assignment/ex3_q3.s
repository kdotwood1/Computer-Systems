.text
.global main
main:
   lw $13, 0x73001($0) #loads the value for the push buttons
   beqz $13, main	#checks to see if not 0
   lw $3, 0x73000($0)	#reads the switches value
   seqi $4, $13, 1	#if value in reg13 is 1 the rightmost button has been pressed set value to 1
   bnez $4, calc	#if value is 1 break to the display segment
   seqi $4, $13, 4	#if value from the push button is 4 set reg4 to 1 as the leftmost button pushed
   bnez $4, left	#if value is 1 jump to left segment	else jump to mid
mid:
   xori $3, $3, 0xFFFF	#invert the value
calc:
   remui $5, $3, 4	#is this a multiple of 4
   beqz $5, lights	#break to the lights label if true
   sw $0, 0x7300A($0)	#turn the lights off
write:
   sw $3, 0x73009($0)	#can only send the hex number through 4 bits at a time
   srli $3, $3, 4	#so we shift right after sending it to rightmost SSD
   sw $3, 0x73008($0)	#then send it to the lowerleft SSD
   srli $3, $3, 4	#shift right again
   sw $3, 0x73007($0)	#send to the upperright SSD
   srli $3, $3, 4	#shift right again for the last 4 bits
   sw $3, 0x73006($0)	#send to the leftmost SSD
   j main
lights:
   addi $6, $0, 0xFFFF #stores all 1's for the lights
   sw $6, 0x7300A($0)	#sends the value through to turn all the lights on 
   j write		#jump to write the value to the ssds
left:
   jr $ra		#ends the program
