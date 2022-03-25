[![GPLv2 License](https://img.shields.io/badge/License-GPL%20v2-green.svg)](https://opensource.org/licenses/GPL-2.0)
[![Discord Chat](https://img.shields.io/discord/527135227546435584.svg)](https://discord.gg/WHkuh2n)


Game research is welcome, for starters please check:
Join the CTR-tools Discord server: https://discord.gg/WHkuh2n

Forked from:
https://github.com/CTR-tools/CTR-tools

. 
## Project Description
The goal of this repository is to port core CTR behaviour to a Unity project which can be run on platforms like Windows, Linux and MacOS via reverse-engineering. UnityCTR is written in C# and HLSL. Project uses the direct binary disassembly and decompilation of the original game to determine its behavior, the project doesn't have any access to the original source code so the project has to be done by reverse-engineering the game. Project will not and can not host any of the original game assets. Therefor a prior copy of the game is required to extract the assets and play the game.

Our objectives are:
- Achieve the same feel with high accuracy.
- Research & education for programmers, speedrunners, scientists and historians. 
- Bug fix major issues encountered in the game like variable overflow and crashes like caused by wrong warp battle mode glitch.
- Better accessibility.
- x86-64 and ARM native execution, with high performance. CTR shouldn't be emulated, interpreted, or transpiled.
- Performance should be so good that the project can work on any desktop computer released in the past 15 years with better performance than on a PS2.


## Notable difference
All Threads are turned into Events, this shouldn't affect behaviour but changes how code is approached.
Levels are loaded in their highest LOD and collisions are handled with the Unity rigidbody system for best performance.
