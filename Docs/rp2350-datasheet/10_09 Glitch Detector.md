# 10.9 Glitch Detector

The glitch detector detects loss of setup and hold margin in the system clock domain, which may be caused by deliberate external manipulation of the system clock or core supply voltage. When it detects loss, the glitch detector triggers a system reset rather than allowing software to continue to execute in a possibly undefined state. It responds

10.8. OTP **866**

within one system clock cycle, unlike the brownout detector, which has much more limited analog bandwidth.

The glitch detector is disabled by default, and can be armed by setting the GLITCH\_DETECTOR\_ENABLE flag in OTP. For debugging purposes, you can also enable the glitch detector via the [ARM](#page-868-1) register. This is not recommended in securitysensitive applications, as the system is vulnerable until the point that software can enable the detectors.

