# 6.1.4 USB PHY and OTP Supply (USB\_OTP\_VDD)

USB\_OTP\_VDD supplies the chip's USB PHY and OTP memory, and should be powered at a nominal 3.3 V. To reduce the number of external power supplies, USB\_OTP\_VDD can use the same power source as the core voltage regulator analogue supply (VREG\_AVDD), or digital IO supply (IOVDD), assuming IOVDD is also powered at 3.3 V. This supply must always be provided, even in applications where the USB PHY is never used.

USB\_OTP\_VDD should be decoupled with a 100nF capacitor close to the chip's USB\_OTP\_VDD pin.

