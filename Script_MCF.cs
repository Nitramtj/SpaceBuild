//Module Construction Facility
//This script object contains build generation scripts and module scanning scripts.

function MCFacility::onAdd(%this, %obj)
{
	//create the queue for modules
	%obj.queue = new SimSet(); //assuming objects keep order inserted in, might be wrong
}

function MCFacility::setPosition(%this, %pos)
{
	%this.position = %pos;
}

function MCFacility::getPosition(%this)
{
	return %this.position;
}

function MCFacility::setMCL(%this, %mcl)
{
	%this.mclayout = %mcl;
	%mcl.mcfacility = %this;
}

function MCFacility::getMCL(%this)
{
	return %this.mclayout;
}

function MCFacility::createSlotForClient(%this, %client)
{
	%slot = %this.getMCL().nextFreeSlot();
	
	if(%slot != -1)
		return %this.getMCL().createSlot(%slot, %client);
	else
		return -1;
}

function MCFacility::getSlot(%this, %num)
{
	return %this.slot[%num];
}

function MCFacility::setSlot(%this, %num, %obj)
{
	%this.slot[%num] = %obj;
}

function MCFacility::nextFreeSlot(%this)
{
	for(%i = 0; %i < %this.getMCL().maxSlots; %i++)
	{
		if(!isObject(%this.getSlot(%i)))
			return(%i);
	}
	
	return -1;
}

//Bonus!
function MCFacility::findSlotByBLID(%this, %blid)
{
	for(%i = 0; %i < %this.getMCL().maxSlots; %i++)
	{
		if(%this.getSlot(%i).ownerBLID == %blid)
			return %this.getSlot(%i);
	}
	
	return -1;
}

//Bonus!
function MCFacility::findSlotByName(%this, %name)
{
	for(%i = 0; %i < %this.getMCL().maxSlots; %i++)
	{
		if(%this.getSlot(%i).ownerName $= %name)
			return %this.getSlot(%i);
	}
	
	return -1;
}




function MCFacility::scanVBL(%obj, %vbl)
{
	%mod = newModuleSO();
	
	for (%i = 0; %i < %vbl.getCount(); %i++)
		%obj.scanModuleBrick(%vbl.getVirtualBrick(%i), %mod);
	
	%verificationError = %obj.finishScanningModuleBricks(%mod);
	
	return %verificationError;
}

function MCFacility::scanBuild(%obj, %brick)
{
	//need to just initialize the scan here
	%mod = newModuleSO();
	%bf = new ScriptObject()
	{
		class = "BrickFinder";
	};
	%bf.setOnSelectCommand(%obj @ ".onFoundBrick(%sb, " @ %mod @ ");");
	%bf.setFinishCommand(%obj @ ".onFinishedFinding(" @ %mod @ ", " @ %bf @ ");");
	%bf.search(%brick, "chain", "all", "spaceSupport", 0);
}

function MCFacility::onFoundBrick(%obj, %sb, %mod)
{
	%obj.scanModuleBrick(%sb, %mod);
}

function MCFacility::scanModuleBrick(%obj, %sb, %mod)
{
	if (%sb.isHatch())
	{
		%box = %sb.getWorldBox();
		%pos = %sb.getPosition();
		if (%sb.isHorizontalHatch())
		{
			switch (%sb.getAngleId())
			{
				case 0:
					%point = getWord(%pos, 0) SPC getWord(%box, 4) SPC getWord(%pos, 2);
				case 1:
					%point = getWord(%box, 3) SPC getWords(%pos, 1, 2);
				case 2:
					%point = getWord(%pos, 0) SPC getWord(%box, 1) SPC getWord(%pos, 2);
				case 3:
					%point = getWord(%box, 0) SPC getWords(%pos, 1, 2);
			}
			%dir = %sb.getAngleId();
		}
		else
		{
			if (%sb.isUpHatch())
			{
				%point = getWords(%pos, 0, 1) SPC getWord(%box, 5);
				%dir = 4; //up
			}
			else
			{
				%point = getWords(%pos, 0, 1) SPC getWord(%box, 2);
				%dir = 5; //down
			}
		}
		
		%sb.setColor(%mod.numHatches);
		%sb.hatchId = %mod.numHatches;
		%mod.addHatch(%point, %dir);
	}
	%mod.addBrick(%sb);
}

function MCFacility::finishScanningModuleBricks(%obj, %mod)
{
	%verificationError = %mod.verifyVBL();
	switch (%verificationError) //this should report errors in a better way, directly to the user
	{
		case 1: //module does not fit bounds
			error("SpaceBuild module verification error - does not fit within maximum module bounds!");
		case 2: //module is empty/doesn't have min bricks
			error("SpaceBuild module verification error - module is empty/has too few bricks!");
	}
	
	if (%verificationError != 0)
		%mod.delete();
	else
		%obj.addToQueue(%mod);
	
	return %verificationError;
}

function MCFacility::onFinishedFinding(%obj, %mod, %bf)
{
	//delete the Brick Finder and make a new module with that vbl
	//echo("onfinishedfinding" SPC %mod SPC %bf);
	
	%verificationError = %obj.finishScanningModuleBricks(%mod);
	%bf.delete();
}

function MCFacility::addToQueue(%obj, %mod)
{
	%obj.queue.add(%mod);
}

function MCFacility::peekModule(%obj, %i)
{
	if (%i $= "")
		%i = 0;
	%mod = 0;
	if (%i < %obj.queue.getCount())
		%mod = %obj.queue.getObject(%i);
	return %mod;
}

function MCFacility::popModule(%obj)
{
	%mod = false;
	if (%obj.queue.getCount() > 0)
	{
		%mod = %obj.queue.getObject(0);
		%obj.queue.remove(%mod);
	}
	
	return %mod;
}

function MCFacility::debugAttach(%obj)
{
	%obj.queue.getObject(1).attachTo(%obj.queue.getObject(0), "hatch1", "hatch0");
}

function MCFacility::export(%obj, %filePath)
{
	%path = filePath(%filePath);
	%fileName = fileBase(%filePath);
	
	%file = new FileObject();
	%file.openForWrite(%filePath);
	
	%file.writeLine(%obj.getPosition()); //first line is always position
	for(%i = 0; %i < %obj.getMCL().maxSlots; %i++)
	{
		if(isObject(%obj.slot[%i]))
		{
			//save format is:
			//slotNum^ownerBLID^ownerName
			//position can be derived from slotNum (MCL.numberToPosition(slot))
			%file.writeLine(%i TAB %obj.slot[%i].ownerBLID TAB %obj.slot[%i].ownerName TAB %obj.slot[%i].size);
			
			//save VBL
			%obj.slot[%i].readyVbl();
			%obj.slot[%i].vbl.exportBLSFile(%path @ "/" @ %fileName @ "_vbl" @ %i @ ".vbl");
		}
	}
	%file.close();
	%file.delete();
}

function MCFacility::import(%obj, %filePath)
{
	if(!isObject(%obj.getMCL()))
	{
		error("MCFacility::import - this MCF has no MCL!");
		return;
	}
	if(!isObject(%obj.getMCL().templateVBL))
	{
		error("MCFacility::import - this MCF's MCL has no templateVBL!");
		return;
	}
	
	%path = filePath(%filePath);
	%fileName = fileBase(%filePath);
	
	%file = new FileObject();
	%file.openForRead(%filePath);
	%obj.setPosition(%file.readLine()); //first line is always position
	while(!%file.isEOF())
	{
		%line = %file.readLine();
		
		%slotNum = getField(%line, 0);
		%blid = getField(%line, 1);
		%name = getField(%line, 2);
		%size = getField(%line, 3);
		%vblPath = %path @ "/" @ %fileName @ "_vbl" @ %slotNum @ ".vbl";
		
		%slotSO = new ScriptObject()
		{
			class = "MCSlot";
			number = %slotNum;
			position = %obj.getMCL().numberToPosition(%slotNum);
			ownerBLID = %blid;
			ownerName = %name;
			size = %size;
		};
		
		//create template
		%slotSO.createTemplate(%obj.getMCL().templateVBL);
		
		//load VBL
		%slotSO.vbl.loadBLSFile(%vblPath);
		
		//place VBL bricks
		%slotSO.createBricks();
		
		//register slotSO with this MCF
		%obj.setSlot(%slotNum, %slotSO);
	}
	
	%file.close();
	%file.delete();
}

//so we can save hatchbricks
addCustSave("SPACEHATCH");
function virtualBrickList::cs_addReal_SPACEHATCH(%obj, %num, %brick)
{
	if (%brick.hatchId !$= "") %obj.virBricks[%num, "SPACEHATCH"] = %brick.hatchId;
	else %obj.virBricks[%num, "SPACEHATCH"] = "";
}

function virtualBrickList::cs_create_SPACEHATCH(%obj, %num, %brick)
{
	if (%obj.virBricks[%num, "SPACEHATCH"] !$= "")
		%brick.hatchId = %obj.virBricks[%num, "SPACEHATCH"];
}

function virtualBrickList::cs_save_SPACEHATCH(%obj, %num, %file)
{
	if (%obj.virBricks[%num, "SPACEHATCH"] !$= "")
		%file.writeLine("+-SPACEHATCH" SPC %obj.virBricks[%num, "SPACEHATCH"]);
}

function virtualBrickList::cs_load_SPACEHATCH(%obj, %num, %addData, %addInfo, %addArgs, %line)
{
	%obj.virBricks[%num, "SPACEHATCH"] = %addInfo;
}

function bfSpaceSupport(%brick)
{
	if (%brick.getName() $= "_spacebuildSupport")
		return 1;
	return 0;
}

addBFType("spaceSupport", "bfSpaceSupport");