# ImageDataSetTagEditor

A small desktop application to manage image tag captions for image training, written in C#
and [AvaloniaUI](https://avaloniaui.net/).
This project is heavily inspired by [BooryDatasetTagManager](https://github.com/starik222/BooruDatasetTagManager) and
has many of its features, including auto-complete of already used tags.

Can manage a dataset in subdirectories, where each caption is split into tags or just comma separated values.
These captions are put into a text file next to the image, named the same way. So image `0000.jpg` has a caption
file `0000.txt` with something like `person, woman, sitting on a chair`.

## Key bindings and other features

As I often accelerate my work with key bindings, I've added some here as well:

- Ctrl + Down: Select next image
- Ctrl + Up: Select previous image
- Ctrl + F: Enter image search box
- Ctrl + Shift + F: Enter tag search box
- Ctrl + Add(+): Add tag to current selected image
- Ctrl + Subtract(-): Delete current selected tag on current selected image
- Up & Down: Select the next or previous tag of the current image. Also includes auto-complete suggestions if they're shown.

Clicking on the tag list on the right also cycles through the images that have the clicked tag.

## Local development setup

Clone the repo and open the solution, just make sure you have .NET 7 installed and the IDE extension needed to develop Avalonia.