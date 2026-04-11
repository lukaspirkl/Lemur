# 10.1 Overview (Arm)

RP2350 provides hardware and bootrom security features for three purposes:

- 1. Prevent unauthorised code from running on the device
- 2. Prevent unauthorised reading of user code and data
- 3. Isolate trusted and untrusted software, running concurrently on the device, from one another

Point **1** is referred to in this datasheet as **secure boot**. Secure boot is a prerequisite to points two and three, since running unauthorised code on the device allows that code to access device internals. The bootrom secure boot implementation and related hardware security features implement the root of trust for secure RP2350 applications; bootrom contents are fixed at design time and immutable.

Point **2** is referred to in this datasheet as **encrypted boot**. Encrypted boot is an additional layer of protection which makes it more difficult to clone devices, or dump and reverse-engineer device firmware. Encrypted boot is implemented using a signed decryption stage prepended to a binary as a post-build step. Encrypted boot stores decryption keys in ondevice OTP memory, which can be locked down after use.

Point **3** allows applications to enforce internal security boundaries such that one part of an application being compromised does not allow access to critical hardware, such as the voltage regulator or protected OTP storage used for cryptographic keys.

Hardware features such as the glitch detector and redundancy coprocessor mitigate common classes of fault injection attacks and help maintain boot integrity, even when an attacker has physical access.

