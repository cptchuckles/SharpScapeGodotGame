# ScapeScape Godot Game
The source for the game and server components for the [SharpScape website](https://github.com/cptchuckles/SharpScape).

## Client
This project contains basic idea of the client that is exported as an HTML5 page with wasm. It communicates with a the server via text and binary write modes, where text packets will be relayed to other clients and binary packets are just recieved by the server.

## Server
This project contains the basic idea of the server program that is run as an executable. It listens for connections from clients and will relay text packets to all clients but will only recieve binary packets.

## Shared
This folder contains shared files between both projects. Any editing of the files should be done in this folder and copied into the base of each project folder before compilation of either.
