# 5.4.5 SDK Access To The API

Bootrom functions are exposed in the SDK via the pico\_bootrom library (see [pico\\_bootrom\)](https://www.raspberrypi.com/documentation/pico-sdk/runtime.html#pico_bootrom).

Each bootrom function has a rom\_ wrapper function that looks up the bootrom function address and calls it.

The SDK provides a simple implementation of exclusive access via bootrom\_acquire\_lock\_blocking(n) and bootrom\_release\_lock(n). When enabled, as it is by default (PICO\_BOOTROM\_LOCKING\_ENABLED=1 is defined) the SDK enables bootrom locking via LOCK\_ENABLE, and these two functions use the other SHA\_256/FLASH\_OP/OTP boot locks to take ownership of/release ownership of the corresponding bootrom resource.

The rom\_ wrapper functions the SDK call bootrom\_acquire\_lock\_locking and bootrom\_relead\_lock functions around bootrom calls that have locking requirements.

