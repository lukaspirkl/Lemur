# 5.4.3 API Function Return Codes

Some functions do not support returning any error, and are marked void. The remainder return either 0 (BOOTROM\_OK) or a positive value (if data needs to be returned) for success. These bootrom error codes are identical to the error codes used by the SDK, so they can be used interchangeably. This explains the gaps in the numbering for SDK error codes that aren't used by the bootrom.

| The function succeeded and returned the value                                                                                                  |  |
|------------------------------------------------------------------------------------------------------------------------------------------------|--|
| The function executed successfully                                                                                                             |  |
| The operation was disallowed by a security constraint                                                                                          |  |
| One or more parameters passed to the function is outside<br>BOOTROM_ERROR_INVALID_ADDRESS and<br>BOOTROM_ERROR_BAD_ALIGNMENT are more specific |  |
| An address argument was out-of-bounds or was<br>determined to be an address that the caller may not                                            |  |
| An address passed to the function was not correctly                                                                                            |  |
| Something happened or failed to happen in the past, and<br>consequently the request cannot currently be serviced.                              |  |
| A user-allocated buffer was too small to hold the result or                                                                                    |  |
| The call failed because another bootrom function must be                                                                                       |  |
| Cached data was determined to be inconsistent with the<br>full version of the data it was copied from.                                         |  |
| The contents of a data structure are invalid                                                                                                   |  |
| An attempt was made to access something that does not                                                                                          |  |
| Modification is impossible based on current state; e.g.                                                                                        |  |
| A required lock is not owned. See Section 5.4.4.                                                                                               |  |
|                                                                                                                                                |  |

