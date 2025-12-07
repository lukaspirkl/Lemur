@echo off
title OpenOCD

%USERPROFILE%\.pico-sdk\openocd\0.12.0+dev\openocd.exe ^
  -c "gdb_port 50000" ^
  -c "tcl_port 50001" ^
  -c "telnet_port 50002" ^
  -s "%USERPROFILE%\.pico-sdk\openocd\0.12.0+dev\scripts" ^
  -f "%USERPROFILE%\.vscode\extensions\marus25.cortex-debug-1.12.1\support\openocd-helpers.tcl" ^
  -f interface/cmsis-dap.cfg ^
  -f target/rp2350-riscv.cfg ^
  -c "adapter speed 5000"
