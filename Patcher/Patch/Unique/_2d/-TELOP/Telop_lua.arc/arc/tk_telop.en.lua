--テロップ
--2007/2/14
--菊池




Telop_path		= "_2d/TELOP/TELOP_"


if (__tk_system_set__ == nil) then -- #pragra once 代わり
    __tk_system_set__ = true

Include("system.lua")
Include("tk_utility.lua")

end



ScnRootSet = {
	{
		name 		= "Telop_root",
		brres   	= "", 					-- シーンアニメデータがマージされたbrresを指定
		arc_name    = "",		  			-- arcファイル名
	},
}

ResourceSet = {
	{
		res_name	= "Telop",				-- リソース名
		filename	= "model_.xml",			-- ファイル名
		arc_name    = "",  			 		-- arcファイル名
		m_name		= {},					-- モデル名リスト
		model		= {},					-- モデルポインタリスト
		root	 	= "Telop_root",			-- シーンルート名
	},
}


function InitResources()
	-- ■ シーンルートの初期化
	InitResources_ScnRoot()

	-- ■ リソースの初期化
	InitResources_Resource()

end

-- ストーリー連邦、ストーリージオン、エース連邦、エースジオン、その他
telop_bgm_list = {"STRM_49", "STRM_46", "STRM_47", "STRM_54", "STRM_58"}

--[[
-- ミッションコード一覧
ACE_MISSION_CODE_TABLE = {
	{"AA01","AA02","AA03","AA04","AA05","AA06",},
	{"AS01","AS02","AS03",},
	{"AK01",},
	{"AY01",},
	{"AC01","AC02","AC03",},
	{"AR01",},
	{"AG01",},
	{"AN01","AN02",},
	{"AB01",},
	{"AH01",},
}
]]

code_list = {"ME", "MZ", "AA", "AS", "AK", "AY", "AC", "AR", "AG", "AN", "AB", "AH"}
bgm_no_list = {1, 2, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4}

function Get_BGM(code)

	for i = 1, #code_list, 1 do
		s,e = string.find(code, code_list[i])
		if((s == 1)and(e==2))then
		    return bgm_no_list[i]
		end
	end

--[[
	if(code[1] == 'M')then
	    if(code[2] == 'E')then --連邦
	        return 1
		elseif(code[2] == 'Z')then --ジオン
			return 2
		end
	elseif(code[1] == 'A')then
		if(code[2] == 'A')then --アムロ
	        return 3
		elseif(code[2] == 'S')then --シロー
	        return 3
		elseif(code[2] == 'K')then --クリス
	        return 3
		elseif(code[2] == 'Y')then --ユウ
	        return 3
		elseif(code[2] == 'C')then --シャア
	        return 4
		elseif(code[2] == 'R')then --ラル
	        return 4
		elseif(code[2] == 'G')then --ガイア
	        return 4
		elseif(code[2] == 'N')then --ノリス
	        return 4
		elseif(code[2] == 'B')then --バーニィ
	        return 4
		elseif(code[2] == 'H')then --アカハナ
	        return 4
		end
	end
]]

	return 5 --該当なし

end

----------------------------------------------------------------------------------------------------
-- ▼▼▼ メイン

	Dbg_Print("テロップ起動")
	SetLoadState(LST_TELOP, LS_NOW)

	-- まず全てを隠す
	Fade(PLANE.MAIN, 1.0, 0, 0, 0, 255)
	Sleep()


	------------------------------------
	--　ここでミッションコード取得
	--M_CODE = "ME01"
	M_CODE = GetMissionCode()
	Dbg_Print("Mission Code  =  "..M_CODE)
	------------------------------------

	arc_name = Telop_path..M_CODE..".arc"
	brres_name = "TELOP_"..M_CODE..".brres"

	--そのファイルは存在する？ 
	if(IsExistFile(arc_name) == false)then
		arc_name = Telop_path.."LOADING.arc"
		brres_name = "TELOP_LOADING.brres"
	end

	bgm_no = Get_BGM(M_CODE)
	
    Snd_BgmOn(telop_bgm_list[bgm_no], 0)

	--ファイル名等を設定しておいて…
	ResourceSet[1].arc_name = arc_name
	ScnRootSet[1].arc_name  = arc_name
	ScnRootSet[1].brres     = brres_name
	
	EtcArc = "_2d/ms_viewer/ms_viewer_etc.arc"	
	LoadMergeFile(EtcArc)
	Sleep()
	normal_font = LoadFont("NORMAL_FONT",EtcArc)
	TextArc = "_2d/ms_viewer/ms_viewer_text.arc"
	LoadMergeFile(TextArc)


	LoadMergeFile(arc_name)
	Sleep()
	SetLoadState(LST_TELOP, LS_FINISH)

	InitResources()
	
	Fade(PLANE.MAIN, FADE_SHORT_F, 0, 0, 0, 0)
	
	Obj3d_SetAnim(ResourceSet[1].model[1], "_00")

	-------------------------------------------------------------------------
	descriptionTextSet = {
	{
		text_name = "MS_NAME_BIG",
		link_model = nil,
		link_name = nil,
	}
}
	--
	function testReplacer(string)
		return "aaaBBBccc"
	end
	
	Sleep()
	
	--SetTextManMax(1)
	--SetTextMax(10)
--man2 = AddTextMan(normal_font,ScnRootSet[1], "ms_viewer",TextArc)
--[[
--load text stuff
descriptionTextSet[1].obj = AddText(man2, descriptionTextSet[1].text_name)
--register dynamic text value
SetTextReplacer(descriptionTextSet , "testReplacer")

--preposition textbox
Text_SetPos(descriptionTextSet[1].obj, 10, 10, -122)
--set it as visible, mech switcher will just change it to empty string when needed
Text_SetIsDisp(descriptionTextSet[1].obj, true)
Text_SetShadow(descriptionTextSet[1].obj, true)]]

	-------------------------------------------------------------------------
	


	wait_time =  60*0 --300 --5秒
	counter = 0
	finish = false
	while(finish == false)do
	    if(GetLoadState(LST_INGAME) == LS_FINISH)then
--	    if(true)then
	        if(GetPad(BTN_A))then
	            counter = wait_time --スキップ
	        end

			if(counter > wait_time)then
			    finish = true
			    Dbg_Print("テロップ終了検知")
			end
		end
		counter = counter + 1
		Sleep()
	end

	
	Fade(PLANE.MAIN, FADE_SHORT_F, 0, 0, 0, 255)
	Sleep(FADE_SHORT_I)

    Exit()

-- ▲▲▲ メイン
----------------------------------------------------------------------------------------------------




