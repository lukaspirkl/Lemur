# 12.7.4 Programmer's Model

#### **12.7.4.1. TinyUSB**

The RP2350 TinyUSB port is the reference implementation for this USB controller. This port can be found in the following files of the pico-sdk GitHub repository:

```
dcd_rp2040.c
hcd_rp2040.c
rp2040_usb.h
```

#### **12.7.4.2. Standalone Device Example**

A standalone USB device example, dev\_lowlevel, makes it easier to understand how to interact with the USB controller without needing to understand the TinyUSB abstractions. In addition to endpoint 0, the standalone device has two bulk endpoints: EP1 OUT and EP2 IN. The device is designed to send whatever data it receives on EP1 to EP2. The example comes with a small Python script that writes "Hello World" into EP1 and checks that it is correctly received on EP2.

The code included in this section explains setting up the USB device controller to receive. It also shows how software responds to a setup packet received from the host.

*Figure 124. USB analyser trace of the dev\_lowlevel USB device example. The control transfers are the device enumeration. The first bulk OUT (out from the host) transfer, highlighted in blue, is the host sending "Hello World" to the device. The second bulk transfer IN (in to the host), is the device returning "Hello World" to the host.*

![](_page_1153_Figure_2.jpeg)

#### **12.7.4.2.1. Device Controller Initialisation**

The following code initialises the USB device:

*Pico Examples: [https://github.com/raspberrypi/pico-examples/blob/master/usb/device/dev\\_lowlevel/dev\\_lowlevel.c](https://github.com/raspberrypi/pico-examples/blob/master/usb/device/dev_lowlevel/dev_lowlevel.c#L183-L217) Lines 183 - 217*

```
183 void usb_device_init() {
184 // Reset usb controller
185 reset_unreset_block_num_wait_blocking(RESET_USBCTRL);
186 
187 // Clear any previous state in dpram just in case
188 memset(usb_dpram, 0, sizeof(*usb_dpram)); ①
189 
190 // Enable USB interrupt at processor
191 irq_set_enabled(USBCTRL_IRQ, true);
192 
193 // Mux the controller to the onboard usb phy
194 usb_hw->muxing = USB_USB_MUXING_TO_PHY_BITS | USB_USB_MUXING_SOFTCON_BITS;
195 
196 // Force VBUS detect so the device thinks it is plugged into a host
197 usb_hw->pwr = USB_USB_PWR_VBUS_DETECT_BITS | USB_USB_PWR_VBUS_DETECT_OVERRIDE_EN_BITS;
198 
199 // Enable the USB controller in device mode.
200 usb_hw->main_ctrl = USB_MAIN_CTRL_CONTROLLER_EN_BITS;
201 
202 // Enable an interrupt per EP0 transaction
203 usb_hw->sie_ctrl = USB_SIE_CTRL_EP0_INT_1BUF_BITS; ②
204 
205 // Enable interrupts for when a buffer is done, when the bus is reset,
206 // and when a setup packet is received
207 usb_hw->inte = USB_INTS_BUFF_STATUS_BITS |
208 USB_INTS_BUS_RESET_BITS |
209 USB_INTS_SETUP_REQ_BITS;
210 
211 // Set up endpoints (endpoint control registers)
212 // described by device configuration
213 usb_setup_endpoints();
214 
215 // Present full speed device by enabling pull up on DP
```

```
216 usb_hw_set->sie_ctrl = USB_SIE_CTRL_PULLUP_EN_BITS;
217 }
```

#### **12.7.4.2.2. Configuring the Endpoint Control Registers for EP1 and EP2**

The function usb\_configure\_endpoints loops through each endpoint defined in the device configuration (including EP0 in and EP0 out, which don't have an endpoint control register defined) and calls the usb\_configure\_endpoint function. This sets up the endpoint control register for that endpoint:

*Pico Examples: [https://github.com/raspberrypi/pico-examples/blob/master/usb/device/dev\\_lowlevel/dev\\_lowlevel.c](https://github.com/raspberrypi/pico-examples/blob/master/usb/device/dev_lowlevel/dev_lowlevel.c#L149-L164) Lines 149 - 164*

```
149 void usb_setup_endpoint(const struct usb_endpoint_configuration *ep) {
150 printf("Set up endpoint 0x%x with buffer address 0x%p\n", ep->descriptor-
  >bEndpointAddress, ep->data_buffer);
151 
152 // EP0 doesn't have one so return if that is the case
153 if (!ep->endpoint_control) {
154 return;
155 }
156 
157 // Get the data buffer as an offset of the USB controller's DPRAM
158 uint32_t dpram_offset = usb_buffer_offset(ep->data_buffer);
159 uint32_t reg = EP_CTRL_ENABLE_BITS
160 | EP_CTRL_INTERRUPT_PER_BUFFER
161 | (ep->descriptor->bmAttributes << EP_CTRL_BUFFER_TYPE_LSB)
162 | dpram_offset;
163 *ep->endpoint_control = reg;
164 }
```

#### **12.7.4.2.3. Receiving a Setup Packet**

An interrupt is raised when a setup packet is received, so the interrupt handler must handle this event:

*Pico Examples: [https://github.com/raspberrypi/pico-examples/blob/master/usb/device/dev\\_lowlevel/dev\\_lowlevel.c](https://github.com/raspberrypi/pico-examples/blob/master/usb/device/dev_lowlevel/dev_lowlevel.c#L494-L504) Lines 494 - 504*

```
494 void isr_usbctrl(void) {
495 // USB interrupt handler
496 uint32_t status = usb_hw->ints;
497 uint32_t handled = 0;
498 
499 // Setup packet received
500 if (status & USB_INTS_SETUP_REQ_BITS) {
501 handled |= USB_INTS_SETUP_REQ_BITS;
502 usb_hw_clear->sie_status = USB_SIE_STATUS_SETUP_REC_BITS;
503 usb_handle_setup_packet();
504 }
```

The controller writes the SETUP packet to the first 8 bytes of the DPSRAM, so the setup packet handler casts that area of memory to struct usb\_setup\_packet \*:

*Pico Examples: [https://github.com/raspberrypi/pico-examples/blob/master/usb/device/dev\\_lowlevel/dev\\_lowlevel.c](https://github.com/raspberrypi/pico-examples/blob/master/usb/device/dev_lowlevel/dev_lowlevel.c#L383-L427) Lines 383 - 427*

```
383 void usb_handle_setup_packet(void) {
384 volatile struct usb_setup_packet *pkt = (volatile struct usb_setup_packet *) &usb_dpram
  ->setup_packet;
385 uint8_t req_direction = pkt->bmRequestType;
```

```
386 uint8_t req = pkt->bRequest;
387 
388 // Reset PID to 1 for EP0 IN
389 usb_get_endpoint_configuration(EP0_IN_ADDR)->next_pid = 1u;
390 
391 if (req_direction == USB_DIR_OUT) {
392 if (req == USB_REQUEST_SET_ADDRESS) {
393 usb_set_device_address(pkt);
394 } else if (req == USB_REQUEST_SET_CONFIGURATION) {
395 usb_set_device_configuration(pkt);
396 } else {
397 usb_acknowledge_out_request();
398 printf("Other OUT request (0x%x)\r\n", pkt->bRequest);
399 }
400 } else if (req_direction == USB_DIR_IN) {
401 if (req == USB_REQUEST_GET_DESCRIPTOR) {
402 uint16_t descriptor_type = pkt->wValue >> 8;
403 
404 switch (descriptor_type) {
405 case USB_DT_DEVICE:
406 usb_handle_device_descriptor(pkt);
407 printf("GET DEVICE DESCRIPTOR\r\n");
408 break;
409 
410 case USB_DT_CONFIG:
411 usb_handle_config_descriptor(pkt);
412 printf("GET CONFIG DESCRIPTOR\r\n");
413 break;
414 
415 case USB_DT_STRING:
416 usb_handle_string_descriptor(pkt);
417 printf("GET STRING DESCRIPTOR\r\n");
418 break;
419 
420 default:
421 printf("Unhandled GET_DESCRIPTOR type 0x%x\r\n", descriptor_type);
422 }
423 } else {
424 printf("Other IN request (0x%x)\r\n", pkt->bRequest);
425 }
426 }
427 }
```

#### **12.7.4.2.4. Replying to a Setup Packet on EP0 IN**

The host first requests the device descriptor. The following code handles that setup request:

*Pico Examples: [https://github.com/raspberrypi/pico-examples/blob/master/usb/device/dev\\_lowlevel/dev\\_lowlevel.c](https://github.com/raspberrypi/pico-examples/blob/master/usb/device/dev_lowlevel/dev_lowlevel.c#L266-L273) Lines 266 - 273*

```
266 void usb_handle_device_descriptor(volatile struct usb_setup_packet *pkt) {
267 const struct usb_device_descriptor *d = dev_config.device_descriptor;
268 // EP0 in
269 struct usb_endpoint_configuration *ep = usb_get_endpoint_configuration(EP0_IN_ADDR);
270 // Always respond with pid 1
271 ep->next_pid = 1;
272 usb_start_transfer(ep, (uint8_t *) d, MIN(sizeof(struct usb_device_descriptor), pkt-
  >wLength));
273 }
```

The usb\_start\_transfer function copies data to be sent into the appropriate hardware buffer and configures the buffer

control register. Once the buffer control register has been written to, the device controller responds to the host with the data. Before this point, the device replies with a NAK:

*Pico Examples: [https://github.com/raspberrypi/pico-examples/blob/master/usb/device/dev\\_lowlevel/dev\\_lowlevel.c](https://github.com/raspberrypi/pico-examples/blob/master/usb/device/dev_lowlevel/dev_lowlevel.c#L238-L260) Lines 238 - 260*

```
238 void usb_start_transfer(struct usb_endpoint_configuration *ep, uint8_t *buf, uint16_t len) {
239 // We are asserting that the length is <= 64 bytes for simplicity of the example.
240 // For multi packet transfers see the tinyusb port.
241 assert(len <= 64);
242 
243 printf("Start transfer of len %d on ep addr 0x%x\n", len, ep->descriptor-
  >bEndpointAddress);
244 
245 // Prepare buffer control register value
246 uint32_t val = len | USB_BUF_CTRL_AVAIL;
247 
248 if (ep_is_tx(ep)) {
249 // Need to copy the data from the user buffer to the usb memory
250 memcpy((void *) ep->data_buffer, (void *) buf, len);
251 // Mark as full
252 val |= USB_BUF_CTRL_FULL;
253 }
254 
255 // Set pid and flip for next transfer
256 val |= ep->next_pid ? USB_BUF_CTRL_DATA1_PID : USB_BUF_CTRL_DATA0_PID;
257 ep->next_pid ^= 1u;
258 
259 *ep->buffer_control = val;
260 }
```

