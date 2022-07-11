# Multiplayer Card Turn Based PlayerIO

Implementation of PlayerIO SDK to make multiplayer games

Implementations
===============
- Authentication (QuickConnect)
- Register of users to BigDB (NoSQL database)
- Leaderboard
- Lobby
- Logout
- Game Room
- Turn Control 
- Round cycle with automatic player play
 
Getting started
===============
To launch the development server do the following:

	a) Launch the Visual Studio Solution: "Server/Serverside Code.sln"

	b) Right-click Player.IO Test Server and choose "Set as Startup Project"

	c) Press F5. This should start the development server.
	   If a windows Firewall dialog pops up, press allow.
	
To launch the Unity3D client file do the following
	
	a) Open the Unity3D folder as a project in Unity3D.

	b) Put your gameid into the Unity3D/Assets/NetworkdClient.cs file where it says "[YOUR GAME ID]"
		If you don't have a gameid, simply login into the PlayerIO Control Panel
		and click "create game".

	c) Press Play in Unity to try the game

If all goes well, you should see the client connect to the development
server in both the client and the development server window.
