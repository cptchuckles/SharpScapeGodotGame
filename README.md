# ScapeScape Godot Game
The source for the game and server components for the [SharpScape website](https://github.com/cptchuckles/SharpScape).

### Client
This folder contains basic idea of the client that is exported as an HTML5 page with wasm. It communicates with a the server via text and binary write modes, where text packets will be relayed to other clients and binary packets are just recieved by the server.

### Server
This folder contains the basic idea of the server program that is run as an executable. It listens for connections from clients and will relay text packets to all clients but will only recieve binary packets.

### Shared
This folder contains shared files between both client and server processes, and will be included in both exports.

## Installation and usage
This project depends upon the [SharpScape ASP.NET web application](https://github.com/cptchuckles/SharpScape).  To run SharpScapeGodotGame, first download and launch the SharpScape web app.  SharpScapeGodotGame relies upon the database and backend of SharpScape.  You can test the game as one of the three default users that come with SharpScape's seed data, or create your own user account on the SharpScape web app by visiting `${SharpScape Host}/register`.