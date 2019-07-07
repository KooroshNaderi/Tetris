## Documentation 

A copy of tetris game has been developed. There are four main classes in this project:

1- CreateObject.cs (4h development): This class is the most important one among the other classes. This class is responsible for:
	- building the object with different shapes (I,O,L)
	- Moving left, right, down, and rotating the object while avoiding collision
	- Linking the objects to tetris map (Inside ControlEnvironment.cs) for collision detection
	
2- ControlEnvironment.cs (3h development): This class is as equally important as the first class. This class is responsible for:
	- Moving objects by time
	- Building tetris map and sending commands for building future objects
	- Removing a completed row
	- Sending commands for moving not removed rows down
	
3- BlockCollisionHandle.cs (0.5h development): This class handles the collsion between blocks, wall and the ground. Also, handles commands send to blocks.
	- It prevent the object from moving when collision happens
	- It moves the blocks down when a command is sent for moving

4- BlockObjectProperty.cs (0.5h development): This class holds the properties for moving an object containing a couple of blocks
	
