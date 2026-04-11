# 10.7.1 Channel Security Attributes

Each channel is assigned a security level using the per-channel registers starting at [SECCFG\\_CH0.](#page-1134-0) This defines:

- The minimum privilege required to configure and control the channel, or observe its status
- The bus privilege at which the channel performs its memory accesses

For the sake of comparing security levels, the DMA assigns the following total order to AHB5 security/privilege attributes: Secure + Privileged > Secure + Unprivileged > Non-secure + Privileged > Non-secure + Unprivileged.

A channel's security level can be changed freely up until any of the channel's control registers is written. After this point, its security level is locked, and cannot be changed until the DMA block resets. At reset, all channels become Secure + Privileged (security level = 3, the maximum).

The effects of the channel SECCFG registers are listed exhaustively in the relevant DMA documentation, [Section 12.6.6.1](#page-1100-0).

