# Renderer
Script editor based on WPF built-in text formatter and OnRender event.

## Goal:
create lightweight script editor to replace previous one, created on RichTextBox's basis which was unimaginably slow.

## Navigation capabilities:
- move by arrow keys, home, end, mouse; 
- caret knows where the line ends and chooses correct position;
- caret returns on previous inRowPosition if needed. Optional;
- clicks on row counter will not affect your edit capabilities;

## Edit capabilities:
- put character;
- put line break. Enter;
- erase previous character. Back;
- erase current character. Delete;
- Undo character insert. Ctrl+Z;
- Undo line break. Ctrl+Z;
- Undo line merge. Ctrl+Z;

## Features:
- highlighting work in singular mode meaning transparent highlightings make no sense as only to one will be rendered;
- text color can be changed. Works the same way as the highilghting.

## Drawbacks:
- line endings are hard coded. Will be difficult to repair;

## Not planning to work on:
- text decorations do not interact with ChangeBuffer so they are needed to be reset each time.

## Known bugs:
- undo cannot discern whether change was on the left or right of caret and will move caret to the right anyway;
- undo, when unremoved character is located right to the right of caret, will cause caret to move beyond acceptable position and will be set on '\n' character;
