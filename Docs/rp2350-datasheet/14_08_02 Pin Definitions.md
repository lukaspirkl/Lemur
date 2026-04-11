# 14.8.2 Pin Definitions

#### **14.8.2.1. Pin Types**

In the following pin tables ([Table 1426](#page-1331-1)), the pin types are defined as shown below.

| Table 1425. Pin Types | Pin Type              | Direction                                     | Description                                                                                                                                                                                             |  |  |  |
|-----------------------|-----------------------|-----------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|--|--|--|
|                       | Digital In            | Input only                                    | Standard Digital. Programmable Pull-Up, Pull-Down, Slew Rate,                                                                                                                                           |  |  |  |
|                       | Digital IO            | Bi-directional                                | Schmitt Trigger and Drive Strength. Default Drive Strength is 4mA.                                                                                                                                      |  |  |  |
|                       | Digital In (FT)       | Input only                                    | Fault Tolerant Digital. These pins are described as Fault Tolerant,<br>which in this case means that very little current flows into the pin                                                             |  |  |  |
|                       | Digital IO (FT)       | Bi-directional                                | whilst it is below 3.63V and IOVDD is 0V. These pins have enhanced<br>ESD protection. Programmable Pull-Up, Pull-Down, Slew Rate, Schmitt<br>Trigger and Drive Strength. Default Drive Strength is 4mA. |  |  |  |
|                       | Digital IO / Analogue | Bi-directional (digital),<br>Input (Analogue) | Standard Digital and ADC input. Programmable Pull-Up, Pull-Down,<br>Slew Rate, Schmitt Trigger and Drive Strength. Default Drive Strength<br>is 4mA.                                                    |  |  |  |
|                       | USB IO                | Bi-directional                                | These pins are for USB use, and contain internal pull-up and pull-down<br>resistors, as per the USB specification. USB operation requires<br>external 27Ω series resistors.                             |  |  |  |
|                       | Analogue (XOSC)       |                                               | Oscillator input pins for attaching a 12MHz crystal. Alternatively, XIN<br>may be driven by a square wave.                                                                                              |  |  |  |

#### <span id="page-1331-1"></span>**14.8.2.2. Pin List**

| Table 1426. GPIO pins | Name   | QFN-60 Number | QFN-80 Number | Type            | Power Domain | Reset State | Description |
|-----------------------|--------|---------------|---------------|-----------------|--------------|-------------|-------------|
|                       | GPIO0  | 2             | 77            | Digital IO (FT) | IOVDD        | Pull-Down   | User IO     |
|                       | GPIO1  | 3             | 78            | Digital IO (FT) | IOVDD        | Pull-Down   | User IO     |
|                       | GPIO2  | 4             | 79            | Digital IO (FT) | IOVDD        | Pull-Down   | User IO     |
|                       | GPIO3  | 5             | 80            | Digital IO (FT) | IOVDD        | Pull-Down   | User IO     |
|                       | GPIO4  | 7             | 1             | Digital IO (FT) | IOVDD        | Pull-Down   | User IO     |
|                       | GPIO5  | 8             | 2             | Digital IO (FT) | IOVDD        | Pull-Down   | User IO     |
|                       | GPIO6  | 9             | 3             | Digital IO (FT) | IOVDD        | Pull-Down   | User IO     |
|                       | GPIO7  | 10            | 4             | Digital IO (FT) | IOVDD        | Pull-Down   | User IO     |
|                       | GPIO8  | 12            | 6             | Digital IO (FT) | IOVDD        | Pull-Down   | User IO     |
|                       | GPIO9  | 13            | 7             | Digital IO (FT) | IOVDD        | Pull-Down   | User IO     |
|                       | GPIO10 | 14            | 8             | Digital IO (FT) | IOVDD        | Pull-Down   | User IO     |
|                       | GPIO11 | 15            | 9             | Digital IO (FT) | IOVDD        | Pull-Down   | User IO     |
|                       | GPIO12 | 16            | 11            | Digital IO (FT) | IOVDD        | Pull-Down   | User IO     |
|                       | GPIO13 | 17            | 12            | Digital IO (FT) | IOVDD        | Pull-Down   | User IO     |
|                       | GPIO14 | 18            | 13            | Digital IO (FT) | IOVDD        | Pull-Down   | User IO     |
|                       | GPIO15 | 19            | 14            | Digital IO (FT) | IOVDD        | Pull-Down   | User IO     |

| Name        | QFN-60 Number | QFN-80 Number | Type                     | Power Domain        | Reset State | Description             |
|-------------|---------------|---------------|--------------------------|---------------------|-------------|-------------------------|
| GPIO16      | 27            | 16            | Digital IO (FT)          | IOVDD               | Pull-Down   | User IO                 |
| GPIO17      | 28            | 17            | Digital IO (FT)          | IOVDD               | Pull-Down   | User IO                 |
| GPIO18      | 29            | 18            | Digital IO (FT)          | IOVDD               | Pull-Down   | User IO                 |
| GPIO19      | 31            | 19            | Digital IO (FT)          | IOVDD               | Pull-Down   | User IO                 |
| GPIO20      | 32            | 20            | Digital IO (FT)          | IOVDD               | Pull-Down   | User IO                 |
| GPIO21      | 33            | 21            | Digital IO (FT)          | IOVDD               | Pull-Down   | User IO                 |
| GPIO22      | 34            | 22            | Digital IO (FT)          | IOVDD               | Pull-Down   | User IO                 |
| GPIO23      | 35            | 23            | Digital IO (FT)          | IOVDD               | Pull-Down   | User IO                 |
| GPIO24      | 36            | 25            | Digital IO (FT)          | IOVDD               | Pull-Down   | User IO                 |
| GPIO25      | 37            | 26            | Digital IO (FT)          | IOVDD               | Pull-Down   | User IO                 |
| GPIO26_ADC0 | 40            | -             | Digital IO /<br>Analogue | IOVDD /<br>ADC_AVDD | Pull-Down   | User IO or ADC<br>input |
| GPIO27_ADC1 | 41            | -             | Digital IO /<br>Analogue | IOVDD /<br>ADC_AVDD | Pull-Down   | User IO or ADC<br>input |
| GPIO28_ADC2 | 42            | -             | Digital IO /<br>Analogue | IOVDD /<br>ADC_AVDD | Pull-Down   | User IO or ADC<br>input |
| GPIO29_ADC3 | 43            | -             | Digital IO /<br>Analogue | IOVDD /<br>ADC_AVDD | Pull-Down   | User IO or ADC<br>input |
| GPIO26      | -             | 27            | Digital IO (FT)          | IOVDD               | Pull-Down   | User IO                 |
| GPIO27      | -             | 28            | Digital IO (FT)          | IOVDD               | Pull-Down   | User IO                 |
| GPIO28      | -             | 36            | Digital IO (FT)          | IOVDD               | Pull-Down   | User IO                 |
| GPIO29      | -             | 37            | Digital IO (FT)          | IOVDD               | Pull-Down   | User IO                 |
| GPIO30      | -             | 38            | Digital IO (FT)          | IOVDD               | Pull-Down   | User IO                 |
| GPIO31      | -             | 39            | Digital IO (FT)          | IOVDD               | Pull-Down   | User IO                 |
| GPIO32      | -             | 40            | Digital IO (FT)          | IOVDD               | Pull-Down   | User IO                 |
| GPIO33      | -             | 42            | Digital IO (FT)          | IOVDD               | Pull-Down   | User IO                 |
| GPIO34      | -             | 43            | Digital IO (FT)          | IOVDD               | Pull-Down   | User IO                 |
| GPIO35      | -             | 44            | Digital IO (FT)          | IOVDD               | Pull-Down   | User IO                 |
| GPIO36      | -             | 45            | Digital IO (FT)          | IOVDD               | Pull-Down   | User IO                 |
| GPIO37      | -             | 46            | Digital IO (FT)          | IOVDD               | Pull-Down   | User IO                 |
| GPIO38      | -             | 47            | Digital IO (FT)          | IOVDD               | Pull-Down   | User IO                 |
| GPIO39      | -             | 48            | Digital IO (FT)          | IOVDD               | Pull-Down   | User IO                 |
| GPIO40_ADC0 | -             | 49            | Digital IO /<br>Analogue | IOVDD /<br>ADC_AVDD | Pull-Down   | User IO or ADC<br>input |
| GPIO41_ADC1 | -             | 52            | Digital IO /<br>Analogue | IOVDD /<br>ADC_AVDD | Pull-Down   | User IO or ADC<br>input |
| GPIO42_ADC2 | -             | 53            | Digital IO /<br>Analogue | IOVDD /<br>ADC_AVDD | Pull-Down   | User IO or ADC<br>input |

| Name        | QFN-60 Number | QFN-80 Number | Type                     | Power Domain        | Reset State | Description             |
|-------------|---------------|---------------|--------------------------|---------------------|-------------|-------------------------|
| GPIO43_ADC3 | -             | 54            | Digital IO /<br>Analogue | IOVDD /<br>ADC_AVDD | Pull-Down   | User IO or ADC<br>input |
| GPIO44_ADC4 | -             | 55            | Digital IO /<br>Analogue | IOVDD /<br>ADC_AVDD | Pull-Down   | User IO or ADC<br>input |
| GPIO45_ADC5 | -             | 56            | Digital IO /<br>Analogue | IOVDD /<br>ADC_AVDD | Pull-Down   | User IO or ADC<br>input |
| GPIO46_ADC6 | -             | 57            | Digital IO /<br>Analogue | IOVDD /<br>ADC_AVDD | Pull-Down   | User IO or ADC<br>input |
| GPIO47_ADC7 | -             | 58            | Digital IO /<br>Analogue | IOVDD /<br>ADC_AVDD | Pull-Down   | User IO or ADC<br>input |

| Table 1427. QSPI pins | Name      | QFN-60 Number | QFN-80 Number | Type       | Power Domain | Reset State | Description                          |
|-----------------------|-----------|---------------|---------------|------------|--------------|-------------|--------------------------------------|
|                       | QSPI_SD3  | 55            | 70            | Digital IO | QSPI_IOVDD   | Pull-Up     | QSPI data                            |
|                       | QSPI_SCLK | 56            | 71            | Digital IO | QSPI_IOVDD   | Pull-Down   | QSPI clock                           |
|                       | QSPI_SD0  | 57            | 72            | Digital IO | QSPI_IOVDD   | Pull-Down   | QSPI data                            |
|                       | QSPI_SD2  | 58            | 73            | Digital IO | QSPI_IOVDD   | Pull-Up     | QSPI data                            |
|                       | QSPI_SD1  | 59            | 74            | Digital IO | QSPI_IOVDD   | Pull-Down   | QSPI data                            |
|                       | QSPI_SS   | 60            | 75            | Digital IO | QSPI_IOVDD   | Pull-Up     | QSPI chip<br>select / USB<br>BOOTSEL |

*Table 1428. Crystal oscillator pins*

| Name | QFN-60 Number | QFN-80 Number | Type            | Power Domain | Description                                                           |
|------|---------------|---------------|-----------------|--------------|-----------------------------------------------------------------------|
| XIN  | 21            | 30            | Analogue (XOSC) | IOVDD        | Crystal oscillator.<br>XIN may also be<br>driven by a square<br>wave. |
| XOUT | 22            | 31            | Analogue (XOSC) | IOVDD        | Crystal oscillator.                                                   |

*Table 1429. Miscellaneous pins*

<span id="page-1333-0"></span>

| Name  | QFN-60 Number | QFN-80 Number | Type            | Power Domain | Reset State | Description                |
|-------|---------------|---------------|-----------------|--------------|-------------|----------------------------|
| RUN   | 26            | 35            | Digital In (FT) | IOVDD        | Pull-Up     | Chip enable /<br>reset_n   |
| SWCLK | 24            | 33            | Digital In (FT) | IOVDD        | Pull-Up     | Serial Wire<br>Debug clock |
| SWDIO | 25            | 34            | Digital IO (FT) | IOVDD        | Pull-Up     | Serial Wire<br>Debug data  |

| Table 1430. USB pins | Name   | QFN-60 Number | QFN-80 Number | Type   | Power Domain | Description                            |
|----------------------|--------|---------------|---------------|--------|--------------|----------------------------------------|
|                      | USB_DP | 52            | 67            | USB IO | USB_OTP_VDD  | USB Data +ve.<br>27Ω series            |
|                      |        |               |               |        |              | resistor required<br>for USB operation |

| Name   | QFN-60 Number | QFN-80 Number | Type   | Power Domain | Description                                                           |
|--------|---------------|---------------|--------|--------------|-----------------------------------------------------------------------|
| USB_DM | 51            | 66            | USB IO | USB_OTP_VDD  | USB Data -ve. 27Ω<br>series resistor<br>required for USB<br>operation |

*Table 1431. Power supply pins*

| Name        | QFN-60 Number(s)       | QFN-80 Number(s)              | Description                                                 |
|-------------|------------------------|-------------------------------|-------------------------------------------------------------|
| DVDD        | 6, 23, 39              | 10, 32, 51                    | Core supply                                                 |
| IOVDD       | 11, 20, 30, 38, 45, 54 | 5, 15, 24, 29, 41, 50, 60, 76 | IO supply                                                   |
| QSPI_IOVDD  | 54                     | 69                            | QSPI IO supply                                              |
| USB_OTP_VDD | 53                     | 68                            | USB & OTP supply                                            |
| ADC_AVDD    | 44                     | 59                            | ADC supply                                                  |
| VREG_AVDD   | 46                     | 61                            | Voltage regulator analogue<br>supply                        |
| VREG_PGND   | 47                     | 62                            | Voltage regulator ground                                    |
| VREG_LX     | 48                     | 63                            | Voltage regulator switching<br>output (connect to inductor) |
| VREG_VIN    | 49                     | 64                            | Voltage regulator input<br>supply                           |
| VREG_FB     | 50                     | 65                            | Voltage regulator feedback<br>input                         |
| GND         | -                      | -                             | Ground connection via<br>central exposed pad                |

