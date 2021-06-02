.text
.global main
main:
   subui $sp, $sp, 3  # move the stack pointer 3 to create space for the values that need to pushed on
   sw $ra, 2($sp)     # saves the return address on the third space on the stack so it's not lost during calculation
   jal readswitches   # calls the readswitches function from another file
getparams:
   srli $4, $1, 8     # shifts the value in $1 right 8 spaces to obtain the  8 leftmost switches' value
   sw $4, 1($sp)      # pushes the above value onto the stack
   andi $3, $1, 0xff  # "and" compares the value in $1 with 8x 1's to get the 8 rightmost switches' value
   sw $3, 0($sp)      # pushes the above value onto the stack
   jal count          # calls count from another file
   lw $3, 0($sp)      # pops all the values from the stack and stores them in a variable
   lw $4, 1($sp)
   lw $ra, 2($sp)     # reloads the return address
   addui $sp, $sp, 3  # Move stack pointer up by 3 to remove all items from it
   jr $ra             # return to wherever this was called
