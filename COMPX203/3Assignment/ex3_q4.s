.text
.global main
main:
   jal serial_job
   jal parallel_job
   bnez $1, fin
   j main
fin:
   syscall
jumpb:
   jr $ra
   
#------------------------
serial_job:
   lw $13, 0x70003($0) 	
   andi $13, $13, 0x1
   beqz $13, jumpb
   lw $3, 0x70001($0)
   sgei $4, $3, 'a'
   slei $5, $3, 'z'
   sequ $6, $4, $5
   beqz $6, printchar
   addi $3, $0, '*'
printchar:
   lw $13, 0x70003($0) 
   andi $13, $13, 0x2
   beqz $13, printchar     
   sw $3, 0x70000($0)
   j jumpb

#------------------------
parallel_job:
   lw $13, 0x73001($0) #loads the value for the push buttons
   beqz $13, jumpb	#checks to see if not 0
   lw $9, 0x73000($0)	#reads the switches value
   seqi $7, $13, 1	#if value in reg13 is 1 the rightmost button has been pressed set value to 1
   bnez $7, calc	#if value is 1 break to the display segment
   seqi $1, $13, 4	#if value from the push button is 4 set reg4 to 1 as leftmost button pushed
   bnez $1, exit	#if value is 1 jump to left segment else continue to mid
mid:
   xori $9, $9, 0xFFFF	#invert the value
calc:
   remui $8, $9, 4
   beqz $8, lights
   sw $0, 0x7300A($0)
write:
   sw $9, 0x73009($0)	#can only send the hex number through 4 bits at a time
   srli $9, $9, 4	#so we shift right after sending it to rightmost SSD
   sw $9, 0x73008($0)	#then send it to the lowerleft SSD
   srli $9, $9, 4	#shift right again
   sw $9, 0x73007($0)	#send to the upperright SSD
   srli $9, $9, 4	#shift right again for the last 4 bits
   sw $9, 0x73006($0)	#send to the leftmost SSD
   j jumpb
lights:
   addi $6, $0, 0xFFFF
   sw $6, 0x7300A($0)
   j write
exit:
   addi $1, $0, 1
   j jumpb
   

