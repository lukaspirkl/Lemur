# 5.7.9 Volume Label In-Depth Example

The following example demonstrates how to manually change the volume label using picotool to the value "SPOON":

1. First, define the row of white label structure to be 0x400:

```
$ picotool otp set -e OTP_DATA_USB_WHITE_LABEL_ADDR 0x400
```

2. Next, because the volume label is located at index 0x8 within OTP\_DATA\_USB\_WHITE\_LABEL\_ADDR, write to 0x408. Define the location of the volume label string to be offset from OTP\_DATA\_USB\_WHITE\_LABEL\_ADDR by 0x30. For this example, "SPOON" has 5 characters, so we write 0x3005 to 0x408:

```
$ picotool otp set -e 0x408 0x3005
```

3. Then, write the "S" and "P" characters:

```
$ picotool otp set -e 0x430 0x5053
```

4. Then, write the "O" and "O" characters:

```
$ picotool otp set -e 0x431 0x4f4f
```

5. Then, write the "N" character:

```
$ picotool otp set -e 0x432 0x4e
```

6. Finally, enable the valid override to use the new values (bit 8 marks the VOLUME\_LABEL override as valid, and bit 22 marks the OTP\_DATA\_USB\_WHITE\_LABEL\_ADDR override as valid):

```
$ picotool otp set -r OTP_DATA_USB_BOOT_FLAGS 0x400100
```

7. To put your changes into effect, reboot the device:

```
$ picotool reboot -u
```

