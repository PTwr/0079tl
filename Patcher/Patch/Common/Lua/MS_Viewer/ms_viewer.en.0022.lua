--link on-screen-text with xbf entry
descriptionTextSet = {
	{
		text_name = "MechDescription",
		link_model = nil,
		link_name = nil,
	}
}
--mech description display logic
descCode = ""
desc = ""
function descReplacer(string)
	return desc
end

function DescBlank()
	desc = ""
end
function SetDesc(id)
  code = D_UnitName[id+1].code
  desc = descTable[code]
  Text_SetPos(descriptionTextSet[1].obj, 30, 100, -1)
end

--22 EFF followed by 24 Zeon machines
function  SetDescZ(id)
	SetDesc(id+22)
end