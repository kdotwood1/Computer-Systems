.text
.global main

main:
	add $2, $0, $0		#
	jal readswitches	#
	
loop:
	andi $3, $1, 1		#compares the readswitch value with 00000001
	add $2, $2, $3		#increase count of 1's by 1
	srli $1, $1, 1		#gets rid of the rightmost digit
	bnez $1, loop		#if reg6 is 1 go to count 
	jal writessd
	j main
