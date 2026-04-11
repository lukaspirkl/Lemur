# 12.2.4 I2C Terminology

This section defines key terms used in various parts of the I2C.

#### **12.2.4.1. I2C Bus Terms**

The following terms relate to how the role of the I2C device and how it interacts with other I2C devices on the bus.

#### **Transmitter**

the device that sends data to the bus. A transmitter can either be a device that initiates the data transmission to the bus (a master-transmitter) or the device that responds to a request from the master to send data to the bus (a slave-transmitter).

#### **Receiver**

the device that receives data from the bus. A receiver can either be a device that receives data on its own request (a master-receiver) or a device that receives data in response to a request from the master (a slave-receiver).

#### **Master**

the component that initializes a transfer (START command), generates the clock SCL signal and terminates the transfer (STOP command). A master can be either a transmitter or a receiver.

#### **Slave**

the device addressed by the master. A slave can be either receiver or transmitter.

#### **Multi-master**

the ability for more than one master to co-exist on the bus at the same time without collision or data loss.

#### **Arbitration**

the predefined procedure that authorizes only one master at a time to take control of the bus. For more information about this behaviour, refer to [Section 12.2.8](#page-992-0).

#### **Synchronization**

the predefined procedure that synchronizes the clock signals provided by two or more masters. For more information about this feature, refer to [Section 12.2.9](#page-992-1).

#### **SDA**

the data signal line (Serial Data).

#### **SCL**

the clock signal line (Serial Clock).

#### **12.2.4.2. Bus Transfer Terms**

The following terms are specific to data transfers that occur to and from the I2C bus.

#### **START (RESTART)**

data transfer begins with a START or RESTART condition. The level of the SDA data line changes from high to low, while the SCL clock line remains high. When this occurs, the bus becomes busy.

![](_page_985_Figure_5.jpeg)

START and RESTART conditions are functionally identical.

#### **STOP**

data transfer is terminated by a STOP condition. This occurs when the level on the SDA data line passes from the low state to the high state, while the SCL clock line remains high. When the data transfer has been terminated, the bus is free or idle once again. The bus stays busy if a RESTART is generated instead of a STOP condition.

