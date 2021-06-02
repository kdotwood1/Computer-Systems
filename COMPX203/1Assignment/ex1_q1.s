.text
.global main

main:
	addi $3, $0, 0		#initialise loop counter to zero
	
loop:
	seqi $8, $3, 10	#has the loop iterated 10 times
	bnez $8, main		#if so, go to "main"

	addi $3, $3, 1		#increment loop counter

	jal readswitches	#reads the current cinfiguration of the switches to store in $2
	add $2, $1, $0 	#transfers the value into reg 1 for "writessd" use
	jal writessd		#displays the value to display
	
	j loop			#loop again

