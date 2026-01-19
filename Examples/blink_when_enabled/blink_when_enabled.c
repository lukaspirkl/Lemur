#include "pico/stdlib.h"

const int pin_blink = 25;
const int pin_enabled = 5;
const int delay = 250;

int main()
{    
    gpio_init(pin_blink);
    gpio_set_dir(pin_blink, GPIO_OUT);

    gpio_init(pin_enabled);
    gpio_set_dir(pin_enabled, GPIO_IN);

    while (true)
    {
        if (!gpio_get(pin_enabled))
        {
            gpio_put(pin_blink, true);
            sleep_ms(delay);
            gpio_put(pin_blink, false);
        }
        sleep_ms(delay);
    }
}
