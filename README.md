Unity version: 6000.0.34f1

A simple tap match game

1. Open Scenes -> SampleScene
2. Enter Play Mode
3. Press New Game (can be spammed for new generations)

Code generates a random grid of colors. When player presses a color, a neighbour search is performed for matching colors, which are then moved to a queue pool.
Columns with empty unoccupied cells are updated and moved. Queue pool fills unoccupied cells after.

Interpolations and "animations" are done by using a batched queue which is waited for and emptied in the beginning of each frame.

Some notes:
1. I chose not to lock player input while the queues are being updated, because the pool queue will always fill unoccupied cells, and it makes for much snappier gameplay
2. The data contained by the class ```Grid``` has everything needed to set the state of ```Runtime```, so saving and loading is supported, just not implemented
