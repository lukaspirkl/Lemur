#set debug remote 1

file ../Venture/Blink/KeySquareBlink.elf

set architecture riscv:rv32

target remote localhost:3333

# monitor reset init

set print pretty on
