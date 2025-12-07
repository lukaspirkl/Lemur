#set debug remote 1

file ../Venture/Blink/KeySquareBlink.elf

set architecture riscv:rv32

target remote 172.24.64.1:50000

# monitor reset init

set print pretty on
