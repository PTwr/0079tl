--load text stuff
descriptionTextSet[1].obj = AddText(man2, descriptionTextSet[1].text_name)
--register dynamic text value
SetTextReplacer(descriptionTextSet , "descReplacer")

--preposition textbox
Text_SetPos(descriptionTextSet[1].obj, 300, 300, -1)
--set it as visible, mech switcher will just change it to empty string when needed
Text_SetIsDisp(descriptionTextSet[1].obj, true)
Text_SetShadow(descriptionTextSet[1].obj, true)