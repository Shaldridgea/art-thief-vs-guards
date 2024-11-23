# Art Thief vs. Guards

This project is an interactive simulation of competing AI techniques, presented as a thief trying to steal a piece of art in a gallery that's patrolled by a few guards.

The thief uses a utility/goal-based AI, while the guards use behaviour trees. These AI systems have been written from scratch for this project.

Assets taken from online are credited in the [credits file](Asset%20Credits.txt), anything not credited was made by me for this project.

A playable web build is available to view here: [Link](https://shaldridgea.github.io/art-thief-vs-guards)

Notable features and systems:
- A simulation customisation menu for tweaking starting parameters
- Utility system AI that autonomously navigates and avoids enemies while attempting to fulfill its goal
- Behaviour tree system with a graphical node editor UI in Unity
- In-depth behaviour trees for the guards with unique behaviour and patrols per-guard
- Sensory systems for agents that detect sound as well as taking into account lighting
- Suspicion system for guards to notice, raise awareness, and detect the thief and their actions
- Runtime visualisation and views for agent data and the AI systems running them

Controls:
- Left click agents in view to see options for them
- Hold right click to control the camera. While controlling it;
  - Scroll wheel zooms the camera in and out
  - Moving the mouse will rotate the camera
- The camera toggle at the top will change between being locked to an agent, or moving the camera freely
- Space toggles pausing the simulation
- R will restart the simulation
