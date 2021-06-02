.text
.global print
print:
   subui $sp, $sp, 7   # move the stack pointer 7 to create space for the values that need to pushed on
   sw $3, 1($sp)
   sw $4, 2($sp)
   sw $5, 3($sp)
   sw $6, 4($sp)
   sw $7, 5($sp)
   sw $ra, 6($sp)  
   lw $8, 7($sp)       # loads the parameter given from the caller of "print"
   addi $3, $0, 10000  # doubles as a counter and a divisor for the value to print in the correct order
loop:
   beqz $3, fin        # checks if the "10000" value is zero and if so jumps to "fin" 
   div $5, $8, $3      # takes the value passed to and divides it by $3 to get the digits starting l to r
   divi $3, $3, 10     # divides the divisor to decrease it toward 0 (10000,1000,100,10,1,0)
   remui $5, $5, 10    # leaves the remainder which should be the rightmost digit of whatever is in $5
   addi $7, $5, '0'    # converts the value to the correect ascii value 
   j check             # jumps to check
check:
   lw $13, 0x70003($0) 
   andi $13, $13, 0x2
   beqz $13, check     # checks to see if the first serial port is ready else loop back to "check"
   sw $7, 0x70000($0)  # sends the values through to the first serial port
   j loop              # loops back to get the next number
fin:
   lw $3, 1($sp)       # pops all values off the stack
   lw $4, 2($sp)
   lw $5, 3($sp)
   lw $6, 4($sp)
   lw $7, 5($sp)
   lw $ra, 6($sp)  
   addui $sp, $sp, 7   # moves the stack pointer to remove all the items
   jr $ra              # return to caller
