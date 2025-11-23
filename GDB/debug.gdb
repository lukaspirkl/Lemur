# 1. Enable the raw RSP packet logging (as you requested)
set debug remote 1

# 2. Connect to the OpenOCD server
target remote localhost:3333

# 3. (Optional) Reset the chip and halt at the start
# monitor reset init

# 4. (Optional) Turn on "pretty printing" for clearer structure viewing
# set print pretty on
