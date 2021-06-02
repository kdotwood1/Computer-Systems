.text
.global main

main:
	add $2, $0, $0		#resets the counting variable
	jal readswitches	#reads the state of the switches
	
loop:
	andi $3, $1, 1		#compares the readswitch value with 00000001
	add $2, $2, $3		#increase count of 1's by reg3 (0 or 1)
	srli $1, $1, 1		#gets rid of the rightmost digit
	bnez $1, loop		#if reg1 is >0 loop again 
	lw $2, myArray($2)
	jal writessd		#write the count variable to the display
	j main			#start again
	
.data
myArray:
	.word 0xA3 #A3	
	.word 0x22 #22
	.word 0x6B #6B
	.word 0x0D #0D
	.word 0x49 #49
	.word 0xC0 #C0
	.word 0x7F #7F
	.word 0xB8 #B8
	.word 0x31 #31
