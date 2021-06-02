.equ SSDLR,		0x73002
.equ SSDLR,		0x73003
.equ PaControl,	0x73004
.equ PaIntAck,		0x73005


.global main
.text
main:
   movsg $2, $cctrl
   andi $2, $2, 0x000F
   ori $2, $2, 0x00A2
   movgs $cctrl, $2
   
   movsg $2, $evec
   sw $2, old_vector($0)
   la $2, ui_handlr
   movgs $evec, $2

   sw $0, PaIntAck
   
loop:
   lw $4, counter($0)
   remui $4, $4, 10
   lw $4, SSDLR($0)
   j loop

ui_handlr:
   movsg $13, $estat
   andi $13, $13, 0xFFA0
   sequi $13,
   beqz $13, handle
   lw $13, old_vector($0)
   jr $13

pp_handlr:
   movsg $13, $estat
   andi $13, $13, 0xFF80
   beqz $13, handle
   lw $13, old_vector($0)
   jr $13
   
handle:
   sw $0, ($0)
   lw $13, counter($0)
   addui $13, $13, 1
   sw $13, counter($0)
   rfe

.data
counter:	.word 0

.bss
old_vector:	.word
