# **RP2350 Datasheet**

A microcontroller by Raspberry Pi

# <span id="page-1-0"></span>**Colophon**

© 2023-2025 Raspberry Pi Ltd

This documentation is licensed under a Creative Commons [Attribution-NoDerivatives 4.0 International](https://creativecommons.org/licenses/by-nd/4.0/) (CC BY-ND).

Portions Copyright © 2019 Synopsys, Inc.

All rights reserved. Used with permission. Synopsys & DesignWare are registered trademarks of Synopsys, Inc.

Portions Copyright © 2000-2001, 2005, 2007, 2009, 2011-2012, 2016 Arm Limited.

All rights reserved. Used with permission.

build-date: 2025-02-20 build-version: 3184e62-clean

# <span id="page-1-1"></span>**Legal disclaimer notice**

TECHNICAL AND RELIABILITY DATA FOR RASPBERRY PI PRODUCTS (INCLUDING DATASHEETS) AS MODIFIED FROM TIME TO TIME ("RESOURCES") ARE PROVIDED BY RASPBERRY PI LTD ("RPL") "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. TO THE MAXIMUM EXTENT PERMITTED BY APPLICABLE LAW IN NO EVENT SHALL RPL BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE RESOURCES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

RPL reserves the right to make any enhancements, improvements, corrections or any other modifications to the RESOURCES or any products described in them at any time and without further notice.

The RESOURCES are intended for skilled users with suitable levels of design knowledge. Users are solely responsible for their selection and use of the RESOURCES and any application of the products described in them. User agrees to indemnify and hold RPL harmless against all liabilities, costs, damages or other losses arising out of their use of the RESOURCES.

RPL grants users permission to use the RESOURCES solely in conjunction with the Raspberry Pi products. All other use of the RESOURCES is prohibited. No licence is granted to any other RPL or other third party intellectual property right.

HIGH RISK ACTIVITIES. Raspberry Pi products are not designed, manufactured or intended for use in hazardous environments requiring fail safe performance, such as in the operation of nuclear facilities, aircraft navigation or communication systems, air traffic control, weapons systems or safety-critical applications (including life support systems and other medical devices), in which the failure of the products could lead directly to death, personal injury or severe physical or environmental damage ("High Risk Activities"). RPL specifically disclaims any express or implied warranty of fitness for High Risk Activities and accepts no liability for use or inclusions of Raspberry Pi products in High Risk Activities.

Raspberry Pi products are provided subject to RPL's [Standard Terms](https://www.raspberrypi.com/terms-conditions-sale/). RPL's provision of the RESOURCES does not expand or otherwise modify RPL's [Standard Terms](https://www.raspberrypi.com/terms-conditions-sale/) including but not limited to the disclaimers and warranties expressed in them.

Legal disclaimer notice **1**

| Colophon                                 |    |
|------------------------------------------|----|
| Legal disclaimer notice                  | 1  |
| 1. Introduction.                         | 13 |
| 1.1. The Chip                            | 14 |
| 1.2. Pinout Reference                    | 15 |
| 1.2.1. Pin Locations                     | 15 |
| 1.2.2. Pin Descriptions                  | 16 |
| 1.2.3. GPIO Functions (Bank 0)           | 17 |
| 1.2.4. GPIO Functions (Bank 1)           | 21 |
| 1.3. Why is the chip called RP2350?      |    |
| 2. System Bus                            |    |
| 2.1. Bus Fabric                          |    |
| 2.1.1. Bus Priority                      |    |
| 2.1.2. Bus Security Filtering            |    |
| 2.1.3. Atomic Register Access            |    |
| 2.1.4. APB Bridge                        |    |
| 2.1.5. Narrow IO Register Writes         |    |
| 2.1.6. Global Exclusive Monitor          |    |
| 2.1.7. Bus Performance Counters          |    |
| 2.2. Address Map                         |    |
| 2.2.1 ROM                                |    |
| 2.2.2. XIP                               |    |
| 2.2.3. SRAM.                             |    |
| 2.2.4. APB Registers                     |    |
| · · · · · · · · · · · · · · · · · · ·    |    |
| 2.2.5. AHB Registers                     |    |
| 2.2.6. Core-local Peripherals (SIO)      |    |
| 2.2.7. Cortex-M33 Private Peripherals    |    |
| 3. Processor Subsystem                   |    |
| 3.1. SIO                                 |    |
| 3.1.1. Secure and Non-secure SIO         |    |
| 3.1.2. CPUID                             |    |
| 3.1.3. GPIO Control                      |    |
| 3.1.4. Hardware Spinlocks                |    |
| 3.1.5. Inter-processor FIFOs (Mailboxes) |    |
| 3.1.6. Doorbells                         |    |
| 3.1.7. Integer Divider                   | 43 |
| 3.1.8. RISC-V Platform Timer             | 43 |
| 3.1.9. TMDS Encoder                      | 44 |
| 3.1.10. Interpolator                     | 44 |
| 3.1.11. List of Registers                | 54 |
| 3.2. Interrupts                          | 82 |
| 3.2.1. Non-maskable Interrupt (NMI)      |    |
| 3.2.2. Further Reading on Interrupts     |    |
| 3.3. Event Signals (Arm)                 |    |
| 3.4. Event Signals (RISC-V)              |    |
| 3.5. Debug                               |    |
| 3.5.1. Connecting to the SW-DP           |    |
| 3.5.2. Arm Debug                         |    |
| 3.5.3. RISC-V Debug                      |    |
| 3.5.4. Debug Power Domains               |    |
| · · · · · · · · · · · · · · · · · · ·    |    |
| 3.5.5. Software control of SWD pins      |    |
| 3.5.6. Self-hosted Debug                 |    |
| 3.5.7. Trace                             |    |
| 3.5.8. Rescue Reset                      |    |
| 3.5.9. Security                          | 91 |

| 3.5.10. RP-AP                          | 9,             |
|----------------------------------------|----------------|
|                                        |                |
| •                                      |                |
| . , ,                                  | DCP)           |
|                                        |                |
|                                        |                |
| _                                      | 123            |
|                                        | 12.            |
|                                        | 124            |
| 3                                      |                |
| •                                      |                |
|                                        |                |
| _                                      |                |
|                                        | 233            |
|                                        |                |
|                                        |                |
| -                                      | 279            |
| · · · · · · · · · · · · · · · · · · ·  |                |
| g .                                    |                |
|                                        |                |
| · · · · · · · · · · · · · · · · · · ·  |                |
| 9                                      |                |
|                                        | 30             |
| 3.9. Arm/RISC-V Architecture Switching |                |
| 3.9.1. Automatic Switching             |                |
| 3.9.2. Mixed Architecture Combination  | s330           |
| 4. Memory                              |                |
| 4.1. ROM                               |                |
| 4.2. SRAM                              |                |
|                                        |                |
|                                        |                |
|                                        |                |
| _                                      |                |
| , ,                                    |                |
|                                        |                |
|                                        |                |
| _                                      |                |
|                                        |                |
| •                                      |                |
| _                                      |                |
| 4.5. OTP                               | 35.            |
|                                        |                |
| ·                                      |                |
|                                        |                |
|                                        |                |
|                                        |                |
|                                        |                |
|                                        |                |
| 5.1.6. Block Versioning                | 35             |
| 5.1.7. A/B Versions                    |                |
| 5.1.8. Hashing and Signing             | 35             |
| 5.1.9. Load Maps                       | 35             |
| 5.1.10. Packaged Binaries              | 35             |
| •                                      | 35             |
|                                        | 359            |
| •                                      |                |
|                                        |                |
| •                                      |                |
|                                        | Downgrade. 36° |
|                                        |                |
|                                        |                |
|                                        |                |
| 5.1.19. Address Translation            |                |

| 5.1.20. Automatic Architecture Switching.                          |       |
|--------------------------------------------------------------------|-------|
| 5.2. Processor-Controlled Boot Sequence                            |       |
| 5.2.1. Boot Outcomes                                               |       |
| 5.2.2. Sequence                                                    |       |
| 5.2.3. POWMAN Boot Vector 5.2.4. Watchdog Boot Vector              |       |
|                                                                    |       |
| 5.2.5. RAM Image Boot                                              |       |
| 5.2.6. OTP Boot<br>5.2.7. Flash Boot                               |       |
|                                                                    |       |
| 5.2.8. BOOTSEL (USB/UART) Boot. 5.2.9. Boot Configuration (OTP)    |       |
| 5.3. Launching Code On Processor Core 1                            |       |
| 5.4. Bootrom APIs                                                  |       |
| 5.4.1. Locating The API Functions                                  |       |
| 5.4.1. Locating the API Functions 5.4.2. API Function Availability |       |
| 5.4.3. API Function Return Codes                                   |       |
| 5.4.4. API Functions And Exclusive Access                          |       |
| 5.4.5. SDK Access To The API                                       |       |
| 5.4.6. Categorised List Of API Functions and ROM Data.             |       |
| 5.4.7. Alphabetical List Of API Functions and ROM Data             |       |
| 5.4.8. API Function Listings                                       |       |
| 5.5. USB Mass Storage Interface                                    |       |
| 5.5.1. The RP2350 Drive                                            |       |
| 5.5.2. UF2 Format Details                                          |       |
| 5.5.2. UF2 Format Details 5.5.3. UF2 Targeting Rules               |       |
| 5.6. USB PICOBOOT Interface.                                       |       |
|                                                                    |       |
| 5.6.1. Identifying The Device                                      |       |
| 5.6.2. Identifying The Interface  5.6.3. Identifying The Endpoints |       |
| 5.6.4. PICOBOOT Commands                                           |       |
|                                                                    |       |
| 5.6.5. Control Requests                                            |       |
| 5.7. USB White-Labelling                                           |       |
| 5.7.1. USB Device Descriptor                                       |       |
| 5.7.2. USB Device Strings                                          |       |
| 5.7.3. USB Configuration Descriptor                                |       |
| 5.7.4. MSD Drive<br>5.7.5. UF2 INDEX.HTM File                      |       |
|                                                                    |       |
| 5.7.6. UF2 INFO_UF2.TXT File                                       |       |
| 5.7.7. SCSI Inquiry                                                |       |
| 5.7.8. Volume Label Simple Example                                 |       |
| 5.7.9. Volume Label In-Depth Example                               |       |
| 5.8. UART Boot                                                     |       |
| 5.8.1. Baud Rate and Clock Requirements                            |       |
| 5.8.2. UART Boot Shell Protocol                                    |       |
| 5.8.3. UART Boot Programming Flow                                  |       |
| 5.8.4. Recovering from a Stuck Interface                           |       |
| 5.8.5. Requirements for UART Boot Binaries                         |       |
| 5.9. Metadata Block Details                                        |       |
| 5.9.1. Blocks And block loops                                      |       |
| 5.9.2. Common Block Items                                          |       |
| 5.9.3. Image Definition Items                                      |       |
| 5.9.4. Partition Table Items                                       |       |
| 5.9.5. Minimum Viable Image Metadata                               |       |
| 5.10. Example Boot Scenarios                                       |       |
| 5.10.1. Secure Boot                                                |       |
| 5.10.2. Signed images                                              |       |
| 5.10.3. Packaged Binaries                                          |       |
| 5.10.4. A/B Booting                                                |       |
| 5.10.5. A/B Booting with Owned Partitions                          |       |
| 5.10.6. Custom Bootloader                                          | . 435 |

|        | 5.10.7. OTP Bootloader                                       | 437 |
|--------|--------------------------------------------------------------|-----|
|        | 5.10.8. Rollback Versions And Bootloaders                    | 438 |
| 6. Pow | /er                                                          |     |
|        | . Power Supplies                                             |     |
| 0.1    | 6.1.1. Digital IO Supply (IOVDD)                             |     |
|        | 6.1.2. QSPI IO Supply (QSPI_IOVDD).                          |     |
|        | 6.1.3. Digital Core Supply (DVDD)                            |     |
|        | 6.1.4. USB PHY and OTP Supply (USB_OTP_VDD)                  |     |
|        | 6.1.5. ADC Supply (ADC_AVDD)                                 |     |
|        |                                                              |     |
|        | 6.1.6. Core Voltage Regulator Input Supply (VREG_VIN)        |     |
|        | 6.1.7. On-Chip Voltage Regulator Analogue Supply (VREG_AVDD) |     |
|        | 6.1.8. Power Supply Sequencing                               |     |
| 6.2    | Power Management                                             |     |
|        | 6.2.1. Core Power Domains                                    |     |
|        | 6.2.2. Power States                                          |     |
|        | 6.2.3. Power State Transitions                               |     |
| 6.3    | Core Voltage Regulator                                       |     |
|        | 6.3.1. Operating Modes                                       |     |
|        | 6.3.2. Software Control                                      |     |
|        | 6.3.3. Power Manager Control                                 | 447 |
|        | 6.3.4. Status                                                | 448 |
|        | 6.3.5. Current Limit.                                        | 448 |
|        | 6.3.6. Over Temperature Protection.                          | 448 |
|        | 6.3.7. Application Circuit                                   |     |
|        | 6.3.8. External Components and PCB layout requirements       |     |
|        | 6.3.9. List of Registers                                     |     |
| 6.4    | . Power Management (POWMAN) Registers                        |     |
|        | . Power Reduction Strategies                                 |     |
| 0.0    | 6.5.1. Top-level Clock Gates                                 |     |
|        | 6.5.2. SLEEP State                                           |     |
|        |                                                              |     |
|        | 6.5.3. DORMANT State                                         |     |
|        | 6.5.4. Memory Periphery Power Down.                          |     |
|        | 6.5.5. Full Memory Power Down                                |     |
|        | 6.5.6. Programmer's Model.                                   |     |
|        | ets                                                          |     |
|        | . Overview                                                   |     |
| 7.2    | Changes from RP2040                                          | 491 |
| 7.3    | Chip Level Resets                                            | 492 |
|        | 7.3.1. Chip-Level Reset table                                | 492 |
|        | 7.3.2. Chip-level Reset Destinations.                        | 493 |
|        | 7.3.3. Chip-level Reset Sources                              |     |
| 7.4    | System Resets (Power-on State Machine)                       |     |
|        | 7.4.1. Reset Sequence                                        |     |
|        | 7.4.2. Register Control                                      |     |
|        | 7.4.3. Interaction with Watchdog                             |     |
|        | 7.4.4. List of Registers                                     |     |
| 7.     |                                                              |     |
| 7.5    | Subsystem Resets                                             |     |
|        | 7.5.1. Overview                                              |     |
|        | 3                                                            |     |
|        | 7.5.3. List of Registers                                     |     |
| 7.6    | . Power-on Reset & Brownout Detection                        |     |
|        | 7.6.1. Power-on Reset (POR)                                  | 506 |
|        | 7.6.2. Brownout Detection (BOD)                              | 506 |
|        | 7.6.3. Supply Monitor                                        | 509 |
|        | 7.6.4. List of Registers                                     | 509 |
| 8. Clo | ks                                                           | 510 |
| 8.1    | . Overview                                                   |     |
|        | 8.1.1. Clock sources                                         |     |
|        | 8.1.2. Clock Generators                                      |     |
|        | 8.1.3. Frequency Counter                                     |     |
|        | U. 1. U. 1 TOQUOTION OUUTILL                                 | 010 |

| 8.1.4. Resus                                       | 519 |
|----------------------------------------------------|-----|
| 8.1.5. Programmer's Model                          |     |
| 8.1.6. List of Registers                           |     |
| 8.2. Crystal Oscillator (XOSC)                     |     |
| 8.2.1. Overview                                    |     |
| 8.2.2. Changes from RP2040                         | 553 |
| 8.2.3. Usage                                       |     |
| 8.2.4. Startup Delay                               |     |
| 8.2.5. XOSC Counter                                |     |
| 8.2.6. DORMANT mode                                | 554 |
| 8.2.7. Programmer's Model                          |     |
| 8.2.8. List of Registers                           |     |
| 8.3. Ring Oscillator (ROSC)                        |     |
| 8.3.1. Overview                                    |     |
| 8.3.2. Changes from RP2040                         |     |
| 8.3.3. ROSC/XOSC trade-offs                        |     |
| 8.3.4. Modifying the frequency                     |     |
| 8.3.5. Randomising the frequency                   |     |
| 8.3.6. ROSC divider                                |     |
| 8.3.7. Random Number Generator                     |     |
| 8.3.8. ROSC Counter                                |     |
| 8.3.9. DORMANT mode                                |     |
| 8.3.10. List of Registers                          |     |
| 8.4. Low Power Oscillator (LPOSC)                  |     |
| 8.4.1. Frequency Accuracy and Calibration          |     |
| 8.4.2. Using an External Low Power Clock           |     |
| 8.4.3. List of Registers                           |     |
| 8.5. Tick Generators                               |     |
| 8.5.1. Overview                                    |     |
| 8.5.2. List of Registers                           |     |
| 8.6. PLL                                           |     |
| 8.6.1. Overview                                    |     |
| 8.6.2. Changes from RP2040                         |     |
| 8.6.3. Calculating PLL parameters                  |     |
| 8.6.4. Configuration                               |     |
| 8.6.5. List of Registers                           |     |
| 9. GPIO.                                           |     |
| 9.1. Overview                                      |     |
| 9.2. Changes from RP2040                           | 585 |
| 9.3. Reset State                                   |     |
| 9.4. Function Select                               |     |
| 9.5. Interrupts                                    |     |
| 9.6. Pads                                          |     |
| 9.6.1. Bus Keeper Mode                             |     |
| 9.7. Pad Isolation Latches.                        |     |
| 9.8. Processor GPIO Controls (SIO)                 |     |
| 9.9. GPIO Coprocessor Port                         |     |
| 9.10. Software Examples                            |     |
| 9.10.1. Select an IO function                      |     |
| 9.10.2. Enable a GPIO interrupt                    |     |
| 9.11. List of Registers                            |     |
| 9.11.1. IO - User Bank                             |     |
| 9.11.2. IO - OSEI Bank                             |     |
| 9.11.3. Pad Control - User Bank                    |     |
| 9.11.4. Pad Control - OSPI Bank                    |     |
| 9.11.4. Pad Control - QSPI Bank  10. Security      |     |
| 10.1. Overview (Arm)                               |     |
| 10.1.1. Secure Boot                                |     |
| 10.1.2. Encrypted Boot                             |     |
| 10.1.3. Isolating Trusted and Untrusted Software   |     |
| TO. 1.3. ISOIdUITU TTUSTEU ATIU OTITUSTEU SOITWATE | 013 |

| 10.2. Prod  | cessor Security Features (Arm)         | 816 |
|-------------|----------------------------------------|-----|
| 10.2.1      | . Background                           | 816 |
|             | 2. IDAU Address Map                    |     |
|             | rview (RISC-V).                        |     |
| 10.4. Prod  | cessor Security Features (RISC-V)      | 818 |
|             | ure Boot Enable Procedure              |     |
| 10.6. Acc   | ess Control                            | 819 |
| 10.6.1      | . GPIO Access Control                  | 820 |
|             | 2. Bus Access Control                  |     |
| 10.6.3      | B. List of Registers                   | 823 |
|             | Α                                      |     |
| 10.7.1      | . Channel Security Attributes          | 865 |
| 10.7.2      | 2. Memory Protection Unit              | 865 |
| 10.7.3      | B. DREQ Attributes                     | 865 |
| 10.7.4      | I. IRQ Attributes                      | 865 |
| 10.8. OTP   | )                                      | 866 |
|             | ch Detector                            |     |
| 10.9.1      | . Theory of Operation                  | 867 |
| 10.9.2      | 2. Trigger Response                    | 867 |
|             | B. List of Registers                   |     |
| 10.10. Fac  | ctory Test JTAG                        | 871 |
| 10.11. De   | commissioning                          | 871 |
| 11. PIO     |                                        | 873 |
|             | rview                                  |     |
| 11.1.1      | . Changes from RP2040                  | 874 |
| 11.2. Prog  | grammer's Model                        | 875 |
| 11.2.1      | . PIO Programs                         | 876 |
| 11.2.2      | 2. Control Flow                        | 876 |
| 11.2.3      | B. Registers.                          | 878 |
| 11.2.4      | l. Autopull                            | 878 |
| 11.2.5      | i. Stalling                            | 881 |
| 11.2.6      | 5. Pin Mapping                         | 881 |
| 11.2.7      | '. IRQ Flags                           | 881 |
| 11.2.8      | 3. Interactions Between State Machines | 882 |
| 11.3. PIO   | Assembler (pioasm)                     | 882 |
|             | . Directives                           |     |
| 11.3.2      | 2. Values                              | 884 |
| 11.3.3      | B. Expressions                         | 884 |
|             | l. Comments                            |     |
| 11.3.5      | i. Labels                              | 885 |
| 11.3.6      | . Instructions                         | 885 |
| 11.3.7      | 7. Pseudoinstructions                  | 886 |
| 11.4. Instr | ruction Set                            | 886 |
| 11.4.1      | . Summary                              | 886 |
|             | . JMP                                  |     |
|             | B. WAIT                                |     |
| 11.4.4      | l. IN                                  | 889 |
| 11.4.5      | i. OUT                                 | 890 |
| 11.4.6      | b. PUSH                                | 891 |
| 11.4.7      | '. PULL                                | 892 |
|             | B. MOV (to RX)                         |     |
|             | D. MOV (from RX)                       |     |
|             | 0. MOV                                 |     |
|             | 1. IRQ.                                |     |
|             | 2. SET                                 |     |
|             | ctional Details                        |     |
|             | . Side-set                             |     |
|             | 2. Program Wrapping                    |     |
|             | B. FIFO Joining                        |     |
|             | k. Autopush and Autopull               |     |

| 11.5.5. Clock Dividers                                                                                 |                  |
|--------------------------------------------------------------------------------------------------------|------------------|
| 11.5.6. GPIO Mapping                                                                                   |                  |
| 11.5.7. Forced and EXEC'd Instructions                                                                 |                  |
| 11.6. Examples                                                                                         |                  |
| 11.6.1. Duplex SPI                                                                                     |                  |
| 11.6.2. WS2812 LEDs                                                                                    |                  |
| 11.6.4. UART RX                                                                                        |                  |
| 11.6.5. Manchester Serial TX and RX                                                                    |                  |
| 11.6.6. Differential Manchester (BMC) TX and RX                                                        |                  |
| 11.6.7. I2C                                                                                            |                  |
| 11.6.8. PWM                                                                                            |                  |
| 11.6.9. Addition                                                                                       |                  |
| 11.6.10. Further Examples                                                                              |                  |
| 11.7. List of Registers                                                                                |                  |
| Peripherals                                                                                            |                  |
| 12.1. UART                                                                                             |                  |
| 12.1.1. Overview                                                                                       |                  |
| 12.1.2. Functional description                                                                         | 959              |
| 12.1.3. Operation                                                                                      |                  |
| 12.1.4. UART hardware flow control                                                                     |                  |
| 12.1.5. UART DMA Interface                                                                             |                  |
| 12.1.6. Interrupts                                                                                     | 966              |
| 12.1.7. Programmer's Model                                                                             |                  |
| 12.1.8. List of Registers                                                                              |                  |
| 12.2. I2C                                                                                              |                  |
| 12.2.1. Features                                                                                       |                  |
| 12.2.2. IP Configuration.                                                                              |                  |
| 12.2.3. I2C Overview                                                                                   |                  |
| 12.2.4. I2C Terminology                                                                                |                  |
| 12.2.5. I2C Behaviour                                                                                  |                  |
| 12.2.6. I2C Protocols  12.2.7. TX FIFO Management and START, STOP and RESTART Generation               |                  |
| 12.2.7. TX FIFO Management and START, STOP and RESTART Generation  12.2.8. Multiple Master Arbitration |                  |
| 12.2.9. Clock Synchronization                                                                          |                  |
| 12.2.10. Operation Modes                                                                               |                  |
| 12.2.11. Spike Suppression                                                                             |                  |
| 12.2.11. Spike Suppression  12.2.12. Fast Mode Plus Operation                                          |                  |
| 12.2.13. Bus Clear Feature                                                                             |                  |
| 12.2.14. IC_CLK Frequency Configuration                                                                |                  |
| 12.2.15. DMA Controller Interface                                                                      |                  |
| 12.2.16. Operation of Interrupt Registers                                                              |                  |
| 12.2.17. List of Registers                                                                             |                  |
| 12.3. SPI                                                                                              |                  |
| 12.3.1. Changes from RP2040                                                                            | 1044             |
| 12.3.2. Overview                                                                                       | 1044             |
| 12.3.3. Functional Description                                                                         |                  |
| 12.3.4. Operation                                                                                      | . 1047           |
| 12.3.5. List of Registers.                                                                             | . 1057           |
| 12.4. ADC and Temperature Sensor                                                                       |                  |
| 12.4.1. Changes from RP2040                                                                            |                  |
| 12.4.2. ADC controller                                                                                 |                  |
| 12.4.3. SAR ADC                                                                                        |                  |
| 12.4.4. ADC ENOB                                                                                       |                  |
| 12.4.5. INL and DNL                                                                                    |                  |
| 12.4.6. Temperature Sensor                                                                             |                  |
| 12.4.7. List of Registers.                                                                             |                  |
| 12.5. PWM.                                                                                             |                  |
| 12.5.1. Overview 12.5.2. Programmer's Model                                                            | . 1074<br>. 1074 |
| 14.J.L. 1 1001 attitite 5 Miouet                                                                       | . iu/4           |

| 12.5.3. List of Registers.                                    | . 1083 |
|---------------------------------------------------------------|--------|
| 12.6. DMA                                                     | . 1091 |
| 12.6.1. Changes from RP2040                                   | . 1092 |
| 12.6.2. Configuring Channels                                  | . 1093 |
| 12.6.3. Triggering Channels                                   | . 1095 |
| 12.6.4. Data Request (DREQ)                                   | . 1097 |
| 12.6.5. Interrupts                                            | . 1099 |
| 12.6.6. Security                                              | . 1099 |
| 12.6.7. Bus Error Handling                                    | . 1102 |
| 12.6.8. Additional Features                                   | . 1104 |
| 12.6.9. Example Use Cases                                     | . 1105 |
| 12.6.10. List of Registers                                    | . 1109 |
| 12.7. USB                                                     | . 1138 |
| 12.7.1. Overview                                              | . 1138 |
| 12.7.2. Changes from RP2040                                   | . 1139 |
| 12.7.3. Architecture                                          | . 1141 |
| 12.7.4. Programmer's Model                                    | . 1152 |
| 12.7.5. List of Registers.                                    | . 1156 |
| 12.8. System Timers                                           | . 1179 |
| 12.8.1. Overview                                              | . 1179 |
| 12.8.2. Counter                                               | . 1180 |
| 12.8.3. Alarms                                                | . 1180 |
| 12.8.4. Programmer's Model                                    | . 1181 |
| 12.8.5. List of Registers.                                    |        |
| 12.9. Watchdog                                                |        |
| 12.9.1. Overview                                              |        |
| 12.9.2. Changes from RP2040                                   |        |
| 12.9.3. Watchdog Counter                                      | . 1190 |
| 12.9.4. Control Watchdog Reset Levels                         | . 1191 |
| 12.9.5. Scratch Registers                                     | . 1191 |
| 12.9.6. Programmer's Model                                    | . 1191 |
| 12.9.7. List of Registers.                                    | . 1193 |
| 12.10. Always-On Timer                                        |        |
| 12.10.1. Overview                                             |        |
| 12.10.2. Changes from RP2040                                  | . 1195 |
| 12.10.3. Accessing the AON Timer                              |        |
| 12.10.4. Using the Alarm                                      |        |
| 12.10.5. Selecting the AON Timer Tick Source.                 |        |
| 12.10.6. Synchronising the AON Timer to an External 1Hz Clock | . 1198 |
| 12.10.7. Using an external clock or tick from GPIO            |        |
| 12.10.8. Using a Tick Faster than 1ms                         |        |
| 12.10.9. List of Registers                                    |        |
| 12.11. HSTX                                                   | . 1199 |
| 12.11.1. Data FIFO                                            | . 1200 |
| 12.11.2. Output Shift Register                                | . 1200 |
| 12.11.3. Bit Crossbar                                         | . 1201 |
| 12.11.4. Clock Generator.                                     |        |
| 12.11.5. Command Expander                                     | . 1203 |
| 12.11.6. PIO-to-HSTX Coupled Mode.                            |        |
| 12.11.7. List of Control Registers                            |        |
| 12.11.8. List of FIFO Registers                               |        |
| 12.12. TRNG                                                   | . 1209 |
| 12.12.1. Overview                                             | . 1209 |
| 12.12.2. Configuration                                        | . 1210 |
| 12.12.3. Operation                                            |        |
| 12.12.4. Caveats                                              | . 1211 |
| 12.12.5. List of Registers                                    | . 1212 |
| 12.13. SHA-256 Accelerator                                    | 1218   |
|                                                               | – . 0  |
| 12.13.1. Message Padding                                      |        |

|       | 12.13.3. Data Size and Endianness                     | 1219 |
|-------|-------------------------------------------------------|------|
|       | 12.13.4. DMA DREQ Interface                           |      |
|       | 12.13.5. List of Registers                            |      |
| 1:    | 2.14. QSPI Memory Interface (QMI).                    |      |
|       | 12.14.1. Overview.                                    |      |
|       | 12.14.2. QSPI Transfers                               |      |
|       | 12.14.3. Timing.                                      |      |
|       | 12.14.4. Address Translation                          |      |
|       | 12.14.6. List of Registers                            |      |
| 1'    | 2.15. System Control Registers                        |      |
|       | 12.15.1. SYSINFO                                      |      |
|       | 12.15.2. SYSCFG                                       |      |
|       | 12.15.3. TBMAN                                        |      |
|       | 12.15.4. BUSCTRL                                      | 1252 |
| 13. 0 | TP                                                    | 1265 |
| 1:    | 3.1. OTP Address Map                                  | 1265 |
|       | 13.1.1. Guarded Reads                                 | 1266 |
|       | 3.2. Background: OTP IP Details                       |      |
| 13    | 3.3. Background: OTP Hardware Architecture            |      |
|       | 13.3.1. Lock Shim.                                    |      |
|       | 13.3.2. External Interfaces                           |      |
|       | 13.3.3. OTP Boot Oscillator.                          |      |
|       | 13.3.4. Power-up State Machine                        |      |
|       | 3.4. Critical Flags                                   |      |
| 13    | 3.5. Page Locks<br>13.5.1. Lock Progression           |      |
|       | 13.5.1. Lock Progression 13.5.2. OTP Access Keys      |      |
|       | 13.5.2. OTP Access keys  13.5.3. Lock Encoding in OTP |      |
|       | 13.5.4. Special Pages                                 |      |
|       | 13.5.5. Permissions of Blank Devices                  |      |
| 1:    | 3.6. Error Correction Code (ECC)                      |      |
|       | 13.6.1. Bit repair by polarity (BRP)                  |      |
|       | 13.6.2. Modified Hamming ECC                          |      |
| 1:    | 3.7. Device Decommissioning (RMA)                     |      |
|       | 3.8. List of Registers                                |      |
|       | 3.9. Predefined OTP Data Locations                    |      |
| 14. E | lectrical and Mechanical                              |      |
| 14    | 4.1. QFN-60 Package                                   | 1323 |
|       | 14.1.1. Thermal characteristics                       |      |
|       | 14.1.2. Recommended PCB Footprint                     |      |
| 14    | 4.2. QFN-80 Package                                   |      |
|       | 14.2.1. Thermal characteristics                       |      |
| _     | 14.2.2. Recommended PCB Footprint                     |      |
|       | 4.3. Flash in Package                                 |      |
|       | 4.4. Package Markings                                 |      |
|       | 4.5. Storage conditions  4.6. Solder profile          |      |
|       | 4.6. Solder profile 4.7. Compliance                   |      |
|       | 4.8. Pinout                                           |      |
| 1.4   | 14.8.1. Pin Locations                                 |      |
|       | 14.8.2. Pin Definitions                               |      |
| 14    | 4.9. Electrical Specifications                        |      |
|       | 14.9.1. Absolute Maximum Ratings                      |      |
|       | 14.9.2. ESD Performance                               |      |
|       | 14.9.3. Thermal Performance                           |      |
|       | 14.9.4. IO Electrical Characteristics                 |      |
|       | 14.9.5. Power Supplies                                | 1339 |
|       | 14.9.6. Core Voltage Regulator.                       | 1340 |
|       | 14.9.7. Power Consumption                             | 1341 |

| Appendix A: Register Field Types.          |      |
|--------------------------------------------|------|
| Changes from RP2040                        |      |
| Standard types                             | 1345 |
| RW:                                        | 1345 |
| RO:                                        | 1345 |
| WO:                                        | 1345 |
| Clear types                                | 1345 |
| sc:                                        | 1345 |
| WC:                                        | 1345 |
| FIFO types                                 |      |
| RWF:                                       |      |
| RF:                                        |      |
| WF:                                        | 1346 |
| Appendix B: Units Used in This Document    |      |
| Memory and Storage Capacity                |      |
| Transfer Rate                              |      |
| Physical Quantities                        |      |
| Scale Prefixes                             |      |
| Digit Separators                           |      |
| Appendix E: Errata                         |      |
| ACCESSCTRL .                               |      |
| RP2350-E3                                  |      |
| Bootrom                                    |      |
| RP2350-E10                                 |      |
| RP2350-E10                                 |      |
| RP2350-E13                                 |      |
| RP2350-E14  RP2350-E15                     |      |
| RP2350-E13                                 |      |
| RP2350-E18 RP2350-E19                      |      |
| RP2350-E19 RP2350-E20                      |      |
| RP2350-E20<br>RP2350-E21                   |      |
|                                            |      |
| RP2350-E22                                 |      |
| RP2350-E23                                 |      |
| RP2350-E24                                 |      |
| RP2350-E25                                 |      |
| DMA                                        |      |
| RP2350-E5                                  |      |
| RP2350-E8                                  |      |
| GPI0                                       |      |
| RP2350-E9                                  |      |
| Hazard3                                    |      |
| RP2350-E4                                  |      |
| RP2350-E6                                  |      |
| RP2350-E7                                  | 1360 |
| OTP                                        |      |
| RP2350-E16                                 | 1361 |
| RP2350-E17                                 | 1361 |
| RCP                                        | 1362 |
| RP2350-E26                                 | 1362 |
| SIO                                        | 1363 |
| RP2350-E1                                  | 1363 |
| RP2350-E2                                  | 1363 |
| XIP                                        | 1364 |
| RP2350-E11                                 | 1364 |
| USB                                        |      |
| RP2350-E12                                 |      |
| Appendix H: Documentation Release History. |      |
| 20 February 2025                           |      |
| 04 December 2024.                          |      |
| 16 October 2024                            | 1367 |

| 15 October 2024  | 1367 |
|------------------|------|
| 6 September 2024 | 1367 |
| 8 August 2024    | 1367 |

