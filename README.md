# Renderer
Script editor based on WPF built-in text formatter and OnRender event.

## Goal:
create lightweight script editor to replace previous one, created on RichTextBox's basis which was unfathomably slow.

## Navigation capabilities:
- move by arrow keys, home, end, mouse; 
- caret knows where the line ends and chooses correct position
- caret returns on previous inRowPosition if needed. Optional;

## Edit capabilities:
- put character;
- put line break. Enter;
- erase previous character. Back;
- erase current character. Delete;

## Drawbacks:
- line endings are hard coded. Will be difficult to repair;

## Known bugs:

## Update:
line breakes no more break the lines.
