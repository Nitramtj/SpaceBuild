//This will exec all other SpaceBuild-specific files.

//Contains no-gravity player scripts (should be replaced soon)
exec("./Script_Gravity.cs");

//Space Bricks
exec("./Brick_Space.cs");

//Cargo player (not yet used)
//exec("./Player_Cargo.cs");

//Construction Building ScriptObject
exec("./Script_MCF.cs");

//Modules
exec("./Script_Modules.cs");

//Station
exec("./Script_Station.cs");

//SBTool
exec("./Tool_SBTool.cs");

//Space Shuttle (also execs Particle_Shuttle.cs)
exec("./Vehicle_SpaceShuttle.cs");

//SBTick (currently just burning up script)
exec("./Script_SBTick.cs");

//Old gravity script (needs to be replaced)
exec("./Script_Gravity.cs");