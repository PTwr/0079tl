-- G“fB“O
-- 2007/03/02
-- ‹e’r



Ending_arc		= "_2d/Ending/Ending.arc"
Ending_text_arc	= "_2d/Ending/Ending_text.arc"
Ending_etc_arc	= "_2d/Ending/Ending_etc.arc"

--[[
Title_arc		= "_2d/Title/Title.arc"
Title_text_arc	= "_2d/Title/Title_text.arc"
Title_etc_arc	= "_2d/Title/Title_etc.arc"
]]


if (__tk_system_set__ == nil) then -- #pragra once ‘ă‚í‚č
    __tk_system_set__ = true

Include("system.lua")
Include("tk_utility.lua")

end

--Include("MAIN_MENU_TextSet.lua")
--Include("MAIN_SELECT_ModelSet.lua")
--Include("MAIN_MENU_AnimSet.lua")


Include("ENDING_TextSet.lua")


EndingType	= nil
UseModel	= nil
BGM_List	= {"STRM_66", "STRM_67", "STRM_68", "STRM_69", "STRM_70"}--‚TŽí‚ĚG“fB“O‚˛‚Ć‚É•Ę‚Ě‚a‚f‚li‚m‚`“ü‚čj

ClearCount	= 0 --2‰ń–ÚČŤ~‚ĚG“fB“O‚©‚Ç‚¤‚©iXLbv‰Â”\‚©”Ű‚©‚ÉŠÖ‚í‚éj 
IsSkip		= false
--SkipKey		= Or(BTN_A, BTN_B, BTN_PLUS) --XLbv”»’č‚ÉŽg—p‚·‚éL[
SkipKey		= (BTN_PLUS) --XLbv”»’č‚ÉŽg—p‚·‚éL[

ScnRootName = "Ending_root"

EFF_MODEL	= "END_EFSF_00"
ZEON_MODEL	= "END_ZEON_00"

ScnRoot_Ending = {
	name 		= ScnRootName,
	brres   	= "Ending.brres", 		-- V[“Ajf[^‚Ş}[W‚ł‚ę‚˝brres‚đŽw’č
	arc_name    = Ending_arc,  			-- arct@C‹–Ľ
}

Resource_Ending = {
	res_name	= "Ending",				-- Š\[X–Ľ
	filename	= "model_.xml",			-- t@C‹–Ľ
	arc_name    = Ending_arc,  	 		-- arct@C‹–Ľ
	m_name		= {},					-- ‚f‹–ĽŠXg
	model		= {},					-- ‚f‹|C“^ŠXg
}

--[[
ScnRoot_Title = {
	name 		= "Title_root",
	brres   	= "Title.brres", 		-- V[“Ajf[^‚Ş}[W‚ł‚ę‚˝brres‚đŽw’č
	arc_name    = Title_arc,  			-- arct@C‹–Ľ
}

Resource_Title = {
	res_name	= "Title",				-- Š\[X–Ľ
	filename	= "Model_Title.xml",	-- t@C‹–Ľ
	arc_name    = Title_arc,  	 		-- arct@C‹–Ľ
	m_name		= {},					-- ‚f‹–ĽŠXg
	model		= {},					-- ‚f‹|C“^ŠXg
}
]]


ScnRootSet = {

	ScnRoot_Ending,
--	ScnRoot_Title,

}

ResourceSet = {

	Resource_Ending,
--	Resource_Title,

}


function InitResources()
	-- ˇ V[“‹[g‚ĚŹ‰Šú‰»
	InitResources_ScnRoot()

	-- ˇ Š\[X‚ĚŹ‰Šú‰»
	InitResources_Resource()

end

stf_x_pos	= 330
stf_z_pos	= -1.0
y_blank		= 30
y_line		= 17 --(480 / y_blank) + 1
up_speed	= 1

skip_speed	= 20

function CheckSkip()

	if(ClearCount <= 1)then --Ź‰‰ń‚ĚG“fB“O‚ľ‚Á‚˝‚ç
	    return --XLbv•s‰Â
	end

    if(IsSkip == false)then
    	if(GetPad(SkipKey))then--{‚ĹXLbv 
    		IsSkip = true
    		Fade(PLANE.MAIN, FADE_SHORT_F, 0, 0, 0, 255)
    		skip_count = FADE_SHORT_I
			Snd_BgmOff(FADE_SHORT_I) --‚a‚f‚ltF[hAEg	
			
           	Snd_On(SE.OK)
		end
	end

end
descriptionTextSet = {
	{
		text_name = "Subtitles",
		link_model = nil,
		link_name = nil,
		isdisp = false,
	}
}

frameCounter = 0
function descReplacer(str)
	return "FrameCounter: "..frameCounter
end
-- V“v‹@•¶Žš—ńŚŔ’č”Ĺ



rollStart = 1
function Roll(textSet)

	pos = {}

    --preposition credits
	for i = 1, #textSet, 1 do
		pos[i] = 480 + (30 * (i-1))
	end

	for frame=1,11000,1 do
		frameCounter = frame
		for i = rollStart, #textSet, 1 do
		
			--stop wasting cycles on offscreen text
			if(pos[i]<-50)then
				rollStart = rollStart + 1
				Text_SetIsDisp(pos[i], false)
			else
				--move text up a bit
				pos[i] = pos[i] - up_speed
				Text_SetPos(textSet[i].obj, 330, pos[i], -1)

				if (pos[i]<280) then
					Text_SetIsDisp(textSet[i].obj, true)
				end
			end
		end
	    		
		--some kind of wait for animation to finish?
		CheckSkip()
		if(IsSkip)then
			skip_count = skip_count - 1
			if(skip_count < 0)then
				break
			end
		end
		
		Sleep()
	end
end


function _Roll(textSet)


	if(textSet == nil)then return end

	-- X^btŤ[‹‚Ě—űŹK

	text_index = 1
	--roll_index = 1
	staffroll = {
		obj = {},
		pos = {},
	}

	--THIS IS tRASH gotta rewrite it to make use of it :D

	--Ź‰ŠúŹó‘ÔŤ\’z
	--17 lines displayed at same time
	--preload 17 lines
	for i = 1, #textSet, 1 do
		staffroll.obj[i] = textSet[i].obj
		staffroll.pos[i] = 480 + (y_blank * (i-1))

		
				--Text_SetPos(staffroll.obj[i], 50, 50, stf_z_pos)

		Text_SetIsDisp(staffroll.obj[i], false)

		--roll_index = roll_index + 1
		--text_index = text_index + 1
	end

	
--	    if(GetPad(BTN_DOWN, PAD_DATA))then
--			Obj3d_AnimFrameSetScale(UseModel, skip_speed)
--	    else
	--does nothing? :(
	--gotta do it from brress?
	--will screw up last two pictures (group photo and white dingo note)
	--translation and scale
	--Obj3d_SetScaleXY(15, 15, UseModel)
	--reposition has to be undoed before final big picture is shown
	--Obj3d_SetPosXY(UseModel, 0,15)
	--rotaton in 3d, not 2d
	--Obj3d_SetRotX(UseModel, 1)
	--rotations are perpendicular to axis, not around it (Z rotation is in XY plane)
	--Obj3d_SetRotZ(UseModel, 20)

	--reposition and rescale can be achieved via bress bones, gotta do it for sub bones to not screw up everything
	--will require bress reversible (de)serialzier

	--Ť[‹ŠJŽn
	is_finish = false
	while(is_finish == false)do

		--frameCounter = frameCounter + 1
		
	    is_finish = true

	    Obj3d_AnimFrameSetScale(UseModel, 1.0) 
	    
		CheckSkip()
		
		--TODO optimize loop by tweaking range start to skip scrolled out entries
		--TODO optimize ending range to not scroll to much at same time, then spawn incoming entries at bottom of scroll space
		frameCounter = frameCounter + 1
		--frameCounter = #staffroll
		for i = 1, #staffroll.obj, 1 do
			staffroll.pos[i] = staffroll.pos[i] - up_speed
			--	Text_SetPos(staffroll.obj[i], 50, 50, stf_z_pos)

			if (staffroll.pos[i]>-50) then
				staffroll.pos[i] = staffroll.pos[i] - up_speed
				Text_SetPos(staffroll.obj[i], stf_x_pos, staffroll.pos[i], stf_z_pos)

				if (staffroll.pos[i]<240) then
					Text_SetIsDisp(staffroll.obj[i], true)
				end
			end
		end

		if (frameCounter < 10500) then			
			is_finish = false
		end
	    		
		if(IsSkip)then --XLbvŽžAtF[hŹI—ą‘Ň‚ż 
			skip_count = skip_count - 1
			if(skip_count < 0)then
				is_finish = true
			end
		end
		
		Sleep()
	end	

end


--[[ ‚f‹Ť¬ŤÚ”Ĺ@”pŠü
function Roll(textSet)

	if(textSet == nil)then return end

	-- X^btŤ[‹‚Ě—űŹK

	text_index = 1
	staffroll = {
		obj = {},
		pos = {},
		flg = {},
	}

	--Ź‰ŠúŹó‘ÔŤ\’z	
	for i = 1, y_line, 1 do
	    if(textSet[text_index].model_name == nil)then
			staffroll.obj[i] = textSet[text_index].obj
            staffroll.flg[i] = false
		else
		    staffroll.obj[i] = GetResourceObj3d(textSet[text_index].model_name)
		    staffroll.flg[i] = true
		end
		staffroll.pos[i] = 480 + (y_blank * (i-1))

		if(staffroll.flg[i])then
		    Obj3d_SetScnRoot(staffroll.obj[i], "Title_root")
		else
			Text_SetIsDisp(staffroll.obj[i], true)
		end
		
		text_index = text_index + 1
	end

	--Ť[‹ŠJŽn 
	is_finish = false
	while(is_finish == false)do
	    is_finish = true
		for i = 1, #staffroll.obj, 1 do
			if(GetPad(BTN_DOWN, PAD_DATA))then
				staffroll.pos[i] = staffroll.pos[i] - (up_speed*20)
			else
		    	staffroll.pos[i] = staffroll.pos[i] - up_speed
		    end

            if(staffroll.flg[i])then
				--Obj3d_SetPosXY(staffroll.obj[i], stf_x_pos, staffroll.pos[i])
				--Obj3d_SetPosXY(staffroll.obj[i], 0, staffroll.pos[i])
				Obj3d_SetPosZ(staffroll.obj[i], -10.0)
				x,y,z = ScreenToWorld("Title_root", stf_x_pos, staffroll.pos[i])
				Obj3d_SetPosXY(staffroll.obj[i], x, y)
				--Obj3d_SetPosZ(staffroll.obj[i], z)
				--Dbg_Print("World Pos = "..x.."   "..y.."   "..z)
			else
		    	Text_SetPos(staffroll.obj[i], stf_x_pos, staffroll.pos[i], -100)
		    end
		    
		    if(staffroll.pos[i] < -y_blank)then
		        if(text_index < #textSet)then
		            
		            if(staffroll.flg[i])then
					    Obj3d_SetScnRoot(staffroll.obj[i], NULL)
					else
						Text_SetIsDisp(staffroll.obj[i], false)
					end
		            
				    if(textSet[text_index].model_name == nil)then
						staffroll.obj[i] = textSet[text_index].obj
			            staffroll.flg[i] = false
					else
					    staffroll.obj[i] = GetResourceObj3d(textSet[text_index].model_name)
					    staffroll.flg[i] = true
					end

					staffroll.pos[i] = (y_blank * (y_line-1))

					if(staffroll.flg[i])then
					    Obj3d_SetScnRoot(staffroll.obj[i], "Title_root")
					else
						Text_SetIsDisp(staffroll.obj[i], true)
					end

		            text_index = text_index + 1
				else --‚ŕ‚¤Žź‚Ş‚Č‚©‚Á‚˝‚ç
				
		        end
		    else
		        is_finish = false
		    end
		end
		Sleep()
	end

end
]]

function EndingProc()

	Fade(PLANE.MAIN, FADE_SHORT_F, 0, 0, 0, 0)
	Sleep(FADE_SHORT_I)

--	Sleep(180) --‚a‚f‚l‚Ě‚Ý‚Ĺ3•b

--	SetAnimSet(MAIN_MENU_BG_Fade_IN)
--	SetAnimSet(MAIN_MENU_MAIN_FIN)
	
	--Aj[V‡“X^[g
	Obj3d_SetAnim(UseModel, "_01") 
	--ŞAj[V‡“‘¤‚Ĺ3•bŠÔŤ•‚˘ŽžŠÔiHj‚đ“ü‚ę‚Ä‚ ‚é‚Ě‚Ĺ
	--Aj[V‡“‚ĚŚÄ‚ŃŹo‚µ‚Íć‚É‚â‚Á‚Ä‚¨‚­
	Sleep(180) --‚a‚f‚l‚Ě‚Ý‚Ĺ3•b


	Roll(ENDING_TextSet2)
--	Roll(MAIN_MENU_TextSet2)

	DispOffTextSet(ENDING_TextSet2)
--	DispOffTextSet(MAIN_MENU_TextSet2)

	--Aj[V‡“ŹI—ą‘Ň‚ż
	while(Obj3d_GetAnimFinish(UseModel) == false)do
	    if(GetPad(BTN_DOWN, PAD_DATA))then
			Obj3d_AnimFrameSetScale(UseModel, skip_speed)
	    else
	    	Obj3d_AnimFrameSetScale(UseModel, 1.0) 
	    end
	    
	    CheckSkip()
	    
  		if(IsSkip)then --XLbvŽžAtF[hŹI—ą‘Ň‚ż 
			skip_count = skip_count - 1
			if(skip_count < 0)then break end
		end

	    
		Sleep()
	end

	Fade(PLANE.MAIN, 1.0, 0, 0, 0, 0)
	Sleep()

	--»Ťě•\‹L 
	Obj3d_SetAnim(UseModel, "_02") 
	--Aj[V‡“ŹI—ą‘Ň‚ż
	while(Obj3d_GetAnimFinish(UseModel) == false)do
		Sleep()
	end

end


----------------------------------------------------------------------------------------------------
-- ĄĄĄ C“
function Main_ENDING()

	-- ‚Ü‚¸‘S‚Ä‚đ‰B‚·
	Fade(PLANE.MAIN, 1.0, 0, 0, 0, 255)
	Sleep()
	
	
	-- ‚±‚±‚ĹG“fB“O‚Ě^Cv‚đŽć“ľ 
	EndingType = GetEndingType()
	
	-- ‚±‚ĚvŚCf[^‚ĚNŠA‰ń”‚ÍH 
	ClearCount = GetCurrentClearCount()
	
	
	--debug
	if(EndingType < CEnding_END_EFF_1)then EndingType = CEnding_END_EFF_1 end --ŠëŚŻ‰ń”đ
--	EndingType = CEnding_END_EFF_2 --Ý’č‚ł‚ę‚Ä‚é‚í‚Ż‚Č‚˘‚Ě‚Ĺ‚Ć‚č‚ ‚¦‚¸‚Ĺ‚Á‚żŹă‚°
--	EndingType = CEnding_END_ZEON_3 --Ý’č‚ł‚ę‚Ä‚é‚í‚Ż‚Č‚˘‚Ě‚Ĺ‚Ć‚č‚ ‚¦‚¸‚Ĺ‚Á‚żŹă‚°
--	ClearCount  = 2 --NŠA‰ń”‚ŕ‚Ĺ‚Á‚żŹă‚°
	
	
	
	-- G“fB“O^Cv‚É‚ć‚Á‚ÄŽg‚¤‚f‹AeLXg‚đ•Ş‚Ż‚é
	-- ‚ ‚ĆŽźŽü‰ń—p‚ĚX^[g~bV‡“‚ŕÝ’č‚·‚é
	if(EndingType == CEnding_END_EFF_1)then
		UseModel = EFF_MODEL
		SetStartMission("ME01")
	elseif(EndingType == CEnding_END_EFF_2)then
		UseModel = EFF_MODEL
		SetStartMission("ME01")
	elseif(EndingType == CEnding_END_ZEON_1)then
		UseModel = ZEON_MODEL
		SetStartMission("MZ01")
	elseif(EndingType == CEnding_END_ZEON_2)then
		UseModel = ZEON_MODEL
		SetStartMission("MZ01")
	elseif(EndingType == CEnding_END_ZEON_3)then
		UseModel = ZEON_MODEL
		SetStartMission("MZ01")
	end
	
	
	TextArc = "_2d/ms_viewer/ms_viewer_text.arc"

    LoadMergeFile(Ending_arc)
    LoadMergeFile(Ending_text_arc)
    LoadMergeFile(Ending_etc_arc)
	LoadMergeFile(TextArc)
	Sleep()
--[[
	LoadMergeFile(Title_arc)
	LoadMergeFile(Title_text_arc)
	LoadMergeFile(Title_etc_arc)
	Sleep()
]]




	InitResources()

	-- ˇ tH“g“Ç‚ÝŤž‚Ý
	SetFontMax(2)
	title_font =  LoadFont("TITLE_FONT",  Ending_etc_arc)
	normal_font = LoadFont("NORMAL_FONT", Ending_etc_arc)
	if(title_font == nil)then
	    Dbg_Print("title_font ‚Ş¶¬‚Ĺ‚«‚Ä‚Ü‚ą‚ń\n")
	end
	if(normal_font == nil)then
	    Dbg_Print("normal_font ‚Ş¶¬‚Ĺ‚«‚Ä‚Ü‚ą‚ń\n")
	end

	-- •K—vŹ\•Ş‚Č”A‚©‚Â‚Š‚đ–ł‘Ę‚ÉŽg‚í‚Č‚˘‚ć‚¤ŤĹ‘ĺ”‚đŠm•Ű
	SetTextManMax(5)
	SetTextMax(600)

	-- ˇ eLXg}l[WŤě¬
	local man1 = AddTextMan(title_font,  ScnRootName, "ENDING", Ending_text_arc)
	local man2 = AddTextMan(normal_font, ScnRootName, "ENDING", Ending_text_arc)
--	local man3 = AddTextMan(title_font,  ScnRootName, "MAIN_MENU", Title_text_arc)
--	local man4 = AddTextMan(normal_font, ScnRootName, "MAIN_MENU", Title_text_arc)
	if(man1 == nil)then
	    Dbg_Print("man1 ‚Ş¶¬‚Ĺ‚«‚Ä‚Ü‚ą‚ń\n")
	end
	if(man2 == nil)then
	    Dbg_Print("man2 ‚Ş¶¬‚Ĺ‚«‚Ä‚Ü‚ą‚ń\n")
	end

--Ending_text_arc	= "_2d/Ending/Ending_text.arc"
	--manC = AddTextMan(title_font,  ScnRootName, "ENDING", Ending_text_arc)
	--manC = AddTextMan(title_font,  "main_root", "ms_viewer",TextArc)
	--root must match
	manC = AddTextMan(title_font,  ScnRootName, "ENDING", Ending_text_arc)
	
	n=1

	--only in pause?
	--SetReplaceText(descriptionTextSet[n].text_name, FIGHT_NONAME )

descriptionTextSet[n].obj = AddText(manC, descriptionTextSet[n].text_name)
--register dynamic text value
SetTextReplacer(descriptionTextSet , "descReplacer")

--preposition textbox
Text_SetPos(descriptionTextSet[1].obj, 320, 400, -3)
--set it as visible, mech switcher will just change it to empty string when needed
Text_SetIsDisp(descriptionTextSet[1].obj, true)
Text_SetShadow(descriptionTextSet[1].obj, true)
	
	SetHbmEnable(true) -- ‚g‚a‚l‹–‰Â
	
	-- ˇ eLXgŤě¬
--	SetTextSet(ENDING_TextSet1, man1)
--	SetTextSet(ENDING_TextSet2, man2)

--	SetTextSet(MAIN_MENU_TextSet1, man3)
--	SetTextSet(MAIN_MENU_TextSet2, man4)
--[[	--Š“N‚ÍŽ×–‚‚Č‚Ě‚Ĺ‹­§”rŹś
    for i = 1, #MAIN_MENU_TextSet2, 1 do
        MAIN_MENU_TextSet2[i].link_model = nil
        MAIN_MENU_TextSet2[i].link_name = nil
        MAIN_MENU_TextSet2[i].obj = AddText(man4, MAIN_MENU_TextSet2[i].text_name)
	end
	DispOffTextSet(MAIN_MENU_TextSet2)
]]
	--Š“N‚ÍŽ×–‚‚Č‚Ě‚Ĺ‹­§”rŹś
    for i = 1, #ENDING_TextSet2, 1 do
        ENDING_TextSet2[i].link_model = nil
        ENDING_TextSet2[i].link_name = nil
        ENDING_TextSet2[i].obj = AddText(man2, ENDING_TextSet2[i].text_name)
	end
	DispOffTextSet(ENDING_TextSet2)

	--‚±‚±‚Ĺ‚a‚f‚lX^[g 
	Snd_BgmOn(BGM_List[EndingType], 0)
	
	--SetScnRoot_ENDING()
	Obj3d_SetScnRoot(UseModel, ScnRootName)

	EndingProc()

	--‚a‚f‚ltF[hAEg	
	Snd_BgmOff(FADE_SHORT_I)
	
	Fade(PLANE.MAIN, FADE_SHORT_F, 0, 0, 0, 255)
	Sleep(FADE_SHORT_I)

	--RemoveScnRoot_ENDING()
	Obj3d_SetScnRoot(UseModel, NULL)

	--return NextFrom_ENDING
	
	SetHbmEnable(false) -- ‚g‚a‚l‹ÖŽ~

end



	
-- ŁŁŁ C“ 
----------------------------------------------------------------------------------------------------

-- «««««’P‘Ě‹N“®‚µ‚˝‚˘ŹęŤ‡A—LŚř‚É ««««« 

Main_ENDING()

Exit()
