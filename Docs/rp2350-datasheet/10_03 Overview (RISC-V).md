# 10.3 Overview (RISC-V)

The RP2350 bootrom does not implement secure boot for RISC-V processors. Secure flash boot can still be implemented on RISC-V by storing secure boot code in OTP and disabling other boot media via the [BOOT\\_FLAGS0](#page-1304-1) row in OTP. However, this is not supported natively by the RP2350 bootrom.

The RISC-V processors on RP2350 implement Machine and User execution modes, and the standard Physical Memory Protection unit (PMP), which can be used to enforce internal security or safety boundaries. See [Section 10.4](#page-818-1).

Non-processor-specific hardware security features, such as debug disable OTP flags and the glitch detectors, are functionally identically between Arm and RISC-V. However, the redundancy coprocessor (RCP) is not accessible from the RISC-V processors, as it uses a Cortex-M33-specific coprocessor interface.

