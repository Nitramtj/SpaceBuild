//Module Construction Facility

function MCF::onAdd(%this, %obj)
{
	%obj.setupQueue();
}

function MCF::onRemove(%this, %obj)
{
	%obj.removeQueue();
}

exec("./queue.cs");
exec("./events.cs");
exec("./building.cs");
exec("./constructors.cs");
exec("./storage.cs");
exec("./structure.cs");
exec("./Script_SlotTypes.cs");
exec("./slots.cs");
