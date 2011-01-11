function StationSO::onAdd(%this, %obj)
{
	//%obj.base
	
	%obj.modules = new SimSet();
}

function StationSO::onRemove(%this, %obj)
{
	while (%obj.modules.getCount())
		%obj.modules.getObject(0).delete();
	
	%obj.modules.delete();
}

function StationSO::addModule(%obj, %mod)
{
	if (!isObject(%obj.base))
	{
		%obj.base = %mod;
	}
	
	%mod.owner = %obj;
	%obj.modules.add(%mod);
}

function StationSO::getPosition(%obj)
{
	if (isObject(%obj.base))
		return %obj.base.getPosition();
}