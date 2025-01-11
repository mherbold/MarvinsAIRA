volatile int numBytesRead = 0;
volatile int dataMode = 0;
volatile int powerLevel = 0;
volatile int fanTimeout = 0;
volatile int ledTimeout = 0;

byte handshake[] = { 'w', 'i', 'n', 'd' };

// Registers
//
// TCNT1 = Timer/Counter 1 (16-bit)
//
// OCR1A = Output Compare Register 1A (16-bit)
// OCR1B = Output Compare Register 1B (16-bit)
//
// ICR1 = Input Capture Register 1 (16-bit)
//
// TCCR1A = Timer Counter Control Register 1A (8-bit)
// TCCR1B = Timer Counter Control Register 1B (8-bit)
//
// PIN 9 = OC0B
// PIN 10 = OC0A

void setup() {
  // Clear timer counter control registers
  TCCR1A = 0;
  TCCR1B = 0;

  // Clear timer/counter register
  TCNT1 = 0;

  // COM1A1,COM1B1 = Set compre mode to clear OC1 on comare match
  // WGM11+WGM13 = Set timer/counter mode=Phase correct PWM, TOP=ICR1, Update of OCR1x at TOP, TOV1 flag set on BOTTOM
  // CS10 = Set clock source to clk(i/o) without prescaling (raw internal clock)
  TCCR1A = _BV(COM1A1) | _BV(COM1B1) | _BV(WGM11);
  TCCR1B = _BV(WGM13) | _BV(CS10);

  // set TOP value
  ICR1 = 320;

  // configure pins 9 and 10 to be output
  pinMode(9, OUTPUT);
  pinMode(10, OUTPUT);

  // configure led pin to be output
  pinMode(LED_BUILTIN, OUTPUT);

  // start USB port communications
  Serial.begin(9600);
}

void loop() {
  if (Serial.available() > 0) {
    fanTimeout = 0;
    ledTimeout = 0;

    digitalWrite(LED_BUILTIN, HIGH);

    bool dataIsValid = true;

    byte currentByte = Serial.read();

    numBytesRead++;

    if (numBytesRead == 1) {
      switch (currentByte) {
        case 'w':
          dataMode = 0;
          break;
        case 'L':
          dataMode = 1;
          break;
        case 'R':
          dataMode = 2;
          break;
        default:
          dataIsValid = false;
          break;
      }
    } else {
      switch (dataMode) {
        case 0:
          if (currentByte != handshake[numBytesRead - 1]) {
            dataIsValid = false;
          }
          break;
        case 1:
        case 2:
          if ((currentByte < '0') || (currentByte > '9')) {
            dataIsValid = false;
          } else {
            powerLevel = (powerLevel * 10) + (currentByte - '0');
          }
          break;
      }
    }

    if (!dataIsValid || (numBytesRead == 4)) {
      if (dataIsValid) {
        if (dataMode == 0) {
          Serial.write(handshake, 4);
        } else {
          if ((powerLevel >= 0) && (powerLevel <= 320)) {
            if (dataMode == 1) {
              OCR1A = powerLevel;
            } else {
              OCR1B = powerLevel;
            }
          }
        }
      }

      numBytesRead = 0;
      powerLevel = 0;
    }
  } else {
    delay(10);

    // turn fans off after 5 seconds of no data
    if (fanTimeout < 500) {
      fanTimeout++;
      if (fanTimeout == 500) {
        OCR1A = 0;
        OCR1B = 0;
        digitalWrite(LED_BUILTIN, HIGH);
        ledTimeout = 0;
      }
    }

    // turn led off after 0.1 seconds of no data
    if (ledTimeout < 5) {
      ledTimeout++;
      if (ledTimeout == 5) {
        digitalWrite(LED_BUILTIN, LOW);
      }
    }
  }
}
