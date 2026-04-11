# 8.3.6 ROSC divider

The ROSC frequency is too fast to be used directly, so it is divided in an integer divider controlled by the DIV register. You can change DIV while the ROSC is running, and the output clock will change frequency without glitching. The default divisor is 8, which ensures the output clock is in the specified range on chip startup.

The divider has two outputs, rosc\_clksrc and rosc\_clksrc\_ph. rosc\_clksrc\_ph is a phase shifted version of rosc\_clksrc. This is primarily intended for use during product development; the outputs are identical if the PHASE register is left in its default state.

