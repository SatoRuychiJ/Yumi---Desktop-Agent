# Keyboard Shortcuts - General Notes

## How to Write Them

Each key is represented by one or more characters. To specify a single keyboard character, use the character itself. For example, to represent the letter A, pass the string "A" to the method. To represent multiple characters, append each additional character to the one preceding it. To represent the letters A, B, and C, specify the parameter as "ABC".

The plus sign (+), caret (^), percent sign (%), tilde (~), and parentheses () have special meanings. To specify one of these characters, enclose it in braces ({}). For example, to specify the plus sign, use "{+}". To specify brace characters, use "{{}" and "{}}".

## Special Keys

To specify keys that do not produce a visible character when pressed (such as ENTER or TAB), and keys that represent an action rather than a character, use the codes in the table below.

| Key           | Code                       |
| :------------ | :------------------------- |
| Backspace     | {BACKSPACE}, {BS}, or {BKSP} |
| BREAK         | {BREAK}                    |
| Caps Lock     | {CAPSLOCK}                 |
| DEL or DELETE | {DELETE} or {DEL}          |
| Down Arrow    | {DOWN}                     |
| End           | {END}                      |
| Enter         | {ENTER} or ~               |
| ESC           | {ESC}                      |
| HELP          | {HELP}                     |
| Home          | {HOME}                     |
| INS or INSERT | {INSERT} or {INS}          |
| Left Arrow    | {LEFT}                     |
| Num Lock      | {NUMLOCK}                  |
| Page Down     | {PGDN}                     |
| Page Up       | {PGUP}                     |
| Print Screen  | {PRTSC} (reserved for future use) |
| Right Arrow   | {RIGHT}                    |
| Scroll Lock   | {SCROLLLOCK}               |
| Tab           | {TAB}                      |
| Up Arrow      | {UP}                       |
| F1            | {F1}                       |
| F2            | {F2}                       |
| F3            | {F3}                       |
| F4            | {F4}                       |
| F5            | {F5}                       |
| F6            | {F6}                       |
| F7            | {F7}                       |
| F8            | {F8}                       |
| F9            | {F9}                       |
| F10           | {F10}                      |
| F11           | {F11}                      |
| F12           | {F12}                      |
| F13           | {F13}                      |
| F14           | {F14}                      |
| F15           | {F15}                      |
| F16           | {F16}                      |
| Keypad Add    | {ADD}                      |
| Keypad Subtract | {SUBTRACT}               |
| Keypad Multiply | {MULTIPLY}               |
| Keypad Divide | {DIVIDE}                   |

## Keyboard Modifier Keys

To specify keys combined with any combination of the SHIFT, Ctrl, and ALT keys, precede the key code with one or more of the following codes.

| Key   | Code |
| :---- | :--- |
| SHIFT | +    |
| Ctrl  | ^    |
| ALT   | %    |

## Pressing Keys Simultaneously

To specify that SHIFT, Ctrl, and ALT should be held down while several other keys are pressed, enclose the codes for those keys in parentheses. For example, to hold down SHIFT while E and C are pressed, use "+(EC)". To hold down SHIFT while E is pressed, followed by C without SHIFT, use "+EC".

## Repeating Keys

To specify a repeated key, use the format {key number}. You must put a space between the key and the number. For example, {LEFT 42} means press the Left Arrow key 42 times; {h 10} means press H 10 times.

## Source and More

The keyboard key functionality uses SendKeys.Send(String). For more details about this function, see the [Microsoft documentation for the SendKeys.Send(String) method](https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.sendkeys.send?view=windowsdesktop-7.0#--).
