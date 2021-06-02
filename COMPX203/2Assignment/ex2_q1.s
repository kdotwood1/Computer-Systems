.global	count
.text
count:
	subui	$sp, $sp, 6
	sw	$6, 1($sp)
	sw	$7, 2($sp)
	sw	$12, 3($sp)
	sw	$13, 4($sp)
	sw	$ra, 5($sp)
	lw	$13, 6($sp)
	sgt	$13, $0, $13
	bnez	$13, L.6
	addui	$7, $0, 1
	j	L.7
L.6:
	addu	$7, $0, $0
L.7:
	sgei	$13, $7, 10000
	bnez	$13, L.2
	lw	$13, 7($sp)
	sgt	$13, $0, $13
	bnez	$13, L.8
	addui	$6, $0, 1
	j	L.9
L.8:
	addu	$6, $0, $0
L.9:
	sgei	$13, $6, 10000
	bnez	$13, L.2
	lw	$13, 6($sp)
	lw	$12, 7($sp)
	sle	$13, $13, $12
	bnez	$13, L.16
	j	L.13
L.12:
	lw	$13, 6($sp)
	sw	$13, 0($sp)
	jal	writessd
	jal	delay
	lw	$13, 6($sp)
	subi	$13, $13, 1
	sw	$13, 6($sp)
L.13:
	lw	$13, 6($sp)
	lw	$12, 7($sp)
	sne	$13, $13, $12
	bnez	$13, L.12
	j	L.11
L.15:
	lw	$13, 6($sp)
	sw	$13, 0($sp)
	jal	writessd
	jal	delay
	lw	$13, 6($sp)
	addi	$13, $13, 1
	sw	$13, 6($sp)
L.16:
	lw	$13, 6($sp)
	lw	$12, 7($sp)
	sne	$13, $13, $12
	bnez	$13, L.15
L.11:
	lw	$13, 6($sp)
	sw	$13, 0($sp)
	jal	writessd
L.2:
L.1:
	lw	$6, 1($sp)
	lw	$7, 2($sp)
	lw	$12, 3($sp)
	lw	$13, 4($sp)
	lw	$ra, 5($sp)
	addui	$sp, $sp, 6
	jr	$ra
