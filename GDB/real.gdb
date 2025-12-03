#set debug remote 1

file ../Venture/Blink/KeySquareBlink.elf

set architecture riscv:rv32

target remote localhost:50000

# monitor reset init

set print pretty on
