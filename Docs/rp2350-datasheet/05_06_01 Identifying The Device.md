# 5.6.1 Identifying The Device

A RP2350 device can recognised by the Vendor ID and Product ID in its device descriptor (shown in [Table 455](#page-402-2)), unless different values have been set in OTP (see [Section 5.7\)](#page-410-0)

*Table 455. RP2350 Boot Device Descriptor*

<span id="page-402-2"></span>

| Field           | Value                                        |
|-----------------|----------------------------------------------|
| bLength         | 18                                           |
| bDescriptorType | 1                                            |
| bcdUSB          | 2.10                                         |
| bDeviceClass    | 0                                            |
| bDeviceSubClass | 0                                            |
| bDeviceProtocol | 0                                            |
| bMaxPacketSize0 | 64                                           |
| idVendor        | 0x2e8a - this value may be overridden in OTP |

| Field              | Value                                        |
|--------------------|----------------------------------------------|
| idProduct          | 0x000f - this value may be overridden in OTP |
| bcdDevice          | 1.00 - this value may be overridden in OTP   |
| iManufacturer      | 1                                            |
| iProduct           | 2                                            |
| iSerial            | 3                                            |
| bNumConfigurations | 1                                            |

