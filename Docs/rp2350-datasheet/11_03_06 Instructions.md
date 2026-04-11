# 11.3.6 Instructions

All pioasm instructions follow a common pattern:

```
<instruction> (side <side_set_value>) ([<delay_value>])
```

where:

<instruction> An assembly instruction detailed in the following sections. (see [Section 11.4](#page-886-1))

<side\_set\_value> A value (see [Section 11.3.2\)](#page-884-0) to apply to the side\_set pins at the start of the instruction. Note that the rules for a side-set value via side <side\_set\_value> are dependent on the .side\_set (see [pioasm\\_side\\_set](#page-882-2)) directive for the program. If no .side\_set is specified then the side <side\_set\_value> is invalid, if an optional number of sideset pins is specified then side <side\_set\_value> may be present, and if a non-optional number of sideset pins is specified, then side <side\_set\_value> is required. The <side\_set\_value> must fit within the number of side-set bits specified in the .side\_set directive.

<delay\_value> Specifies the number of cycles to delay after the instruction completes. The delay\_value is specified as a value (see [Section 11.3.2\)](#page-884-0), and in general is between 0 and 31 inclusive (a 5-bit value), however the number of bits is reduced when sideset is enabled via the .side\_set (see [pioasm\\_side\\_set](#page-882-2)) directive. If the <delay\_value> is not present, then the instruction has no delay.

#### **NOTE**

pioasm instruction names, keywords and directives are case insensitive; lower case is used in the *Assembly Syntax* sections below, as this is the style used in the SDK.

# **NOTE**

Commas appear in some *Assembly Syntax* sections below, but are entirely optional, e.g. out pins, 3 may be written out pins 3, and jmp x-- label may be written as jmp x--, label. The *Assembly Syntax* sections below uses the first style in each case as this is the style used in the SDK.

