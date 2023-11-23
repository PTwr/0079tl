-- システム
-- 2006/04/26
-- 関本
-- 2006/12/15 REALG移殖開始
-- 菊池



if (__system__ == nil) then -- #pragma once 代わり 
    __system__ = true



-- システム
NULL = 0
SECOND = 60

-- テキストコントロール type
TEXT_ON		= 0
TEXT_OFF	= 1

-- テキストコントロール　timing
ANIM_START	= 2
ANIM_END	= 3


----------------------------------------------------------------------------------------------------
-- ▼▼▼ アニメーションセット管理

-- アニメーションセット再生 
-- @param animSet : アニメーションセットのテーブル
function SetAnimSet(animSet)

	if(animSet == nil)then
	    Dbg_Print("アニメーションセットの指定が不正です\n")
	    return
	end

	for i = 1, #animSet, 1 do
		if(animSet[i] ~= nil)then
		    if(animSet[i].model_name ~= nil)then
		        if(animSet[i].anim_name ~= nil)then
	       	    	if(animSet[i].frame_scale ~= nil)then
	                    Obj3d_SetAnim(animSet[i].model_name, animSet[i].anim_name, animSet[i].frame_scale)
					end
				end
			end
		end
	end

    -- アニメーション開始時のテキスト操作を行う 
    TextControlInAnimSet(animSet, ANIM_START)

end

-- アニメーションセット終了判定 
-- @param animSet : アニメーションセットのテーブル
-- @return        : true ならアニメーションは終了済み　 
function GetAnimSetFinish(animSet)

	if(animSet == nil)then
	    return false
	end

    for i = 1, #animSet, 1 do
   		if(animSet[i] ~= nil)then
		    if(animSet[i].model_name ~= nil)then
                if(Obj3d_GetAnimFinish(animSet[i].model_name) == false)then
                    return false
				end
			end
		end
	end


	if(animSet.nextAnimSet ~= nil)then  -- 次アニメセットの指定があるなら
	    SetAnimSet(animSet.nextAnimSet) -- 実行
	end

	
	-- アニメーション終了時のテキスト操作を行う 
	TextControlInAnimSet(animSet, ANIM_END)

	return true

end

-- アニメーションセット内テキストコントロール 
function TextControlInAnimSet(animSet, timing)

	if(animSet.textControlList ~= nil)then
    	--if(animSet.textControlList.textSet ~= nil)then --旧
     		for t = 1, #animSet.textControlList, 1 do
     		    if(animSet.textControlList[t].textSet ~= nil)then --新
     		        if(animSet.textControlList[t].timing == timing)then
                        index = GetTextIndex(animSet.textControlList[t].textSet, animSet.textControlList[t].ID)
	        			if(index ~= -1)then
	        			    if(animSet.textControlList[t].type == TEXT_ON)then
	        					Text_SetIsDisp(animSet.textControlList[t].textSet[index].obj, true)
	        				elseif(animSet.textControlList[t].type == TEXT_OFF)then
	        					Text_SetIsDisp(animSet.textControlList[t].textSet[index].obj, false)
	        				end
						end
					end
				end
			end
		--end
	end

end

-- 指定のアニメーションセットを再生し、先頭フレームで停止
function SetAnimReset(animSet)

	for i = 1, #animSet, 1 do
		if(animSet[i] ~= nil)then
		    if(animSet[i].model_name ~= nil)then
		        if(animSet[i].anim_name ~= nil)then
                    Obj3d_SetAnim(animSet[i].model_name, animSet[i].anim_name, 0.0)
                    --Obj3d_SetAnimFrame(animSet[i].model_name, 10.0) --0フレーム目じゃダメ 
				end
			end
		end
	end

end

-- ▲▲▲ アニメーションセット管理
----------------------------------------------------------------------------------------------------
-- ▼▼▼ テキストセット管理


-- ■ テキストの初期化  
-- @param textSet : テキストセットのテーブル 
-- @param textMan : テキストマネージャ 
function SetTextSet(textSet, textMan)

	for i = 1, #textSet, 1 do
		if ( (textSet[i] ~= nil) 
		 and (textSet[i].text_name  ~= nil)
		 and (textSet[i].link_model ~= nil)
		 and (textSet[i].link_name  ~= nil) ) then
			textSet[i].obj = AddText(textMan, textSet[i].text_name)
			if (textSet[i].obj ~= nil) then
				Dbg_Print(textSet[i].link_model..", "..textSet[i].link_name..")")
				Text_SetLink(textSet[i].obj, textSet[i].link_model, textSet[i].link_name)
				if (textSet[i].isdisp == false) then
					Text_SetIsDisp(textSet[i].obj, false)
				else
					Text_SetIsDisp(textSet[i].obj, true)
				end
			end
		end
	end

end



-- ■ テキストリプレーサを TextSet 全体に定義 
-- @param  textSet : テキストセットのテーブル 
-- @param funcname : タグリプレース関数 
function SetTextReplacer(textSet, funcname)
	for i = 1, #textSet, 1 do
		if ( (textSet[i] ~= nil) 
		 and (textSet[i].obj  ~= nil) ) then
			Text_SetReplacer(textSet[i].obj, funcname)		-- リプレーサの設定 
		end
	end
end



-- ■ テキストの表示ＯＦＦ  
-- @param textSet : テキストセットのテーブル
function DispOffTextSet(textSet)

	for i = 1, #textSet, 1 do
		if ( (textSet[i] ~= nil) and (textSet[i].text_name ~= nil) ) then
			if (textSet[i].obj ~= nil) then
				Text_SetIsDisp(textSet[i].obj, false)
			end
		end
	end

end



-- ■ テキストの色変更 
-- @param textSet  : テキストセットのテーブル 
-- @param textName : テキスト名 
-- @param r,g,b,a  : ＲＧＢＡ(Ａを省略すると２５５、ＲＧＢＡを省略するとカラー設定無し） 
function SetColorTextSet(textSet, textName, r,g,b,a)

	if ( (r == nil) and (g == nil) and (b == nil) and (a == nil) ) then
		mode = false
	else
		mode = true
		if (a == nil) then a = 255 end
	end

	for i = 1, #textSet, 1 do
		if ( (textSet[i] ~= nil) and (textSet[i].text_name == textName) ) then
			if (textSet[i].obj ~= nil) then
				if (mode == true) then
					Text_SetColor(textSet[i].obj, r,g,b,a)
				else
					Text_SetColor(textSet[i].obj)
				end
				return
			end
		end
	end

end



-- ■ テキストのＩＤからインデックスを取得 
-- @param textSet : テキストセットのテーブル
-- @param textMan : テキストのＩＤ 
function GetTextIndex(textSet, name)

	for i = 1, #textSet, 1 do
		if(textSet[i] ~= nil)then
			if(textSet[i].text_name == name)then
				return i
			end
		end
	end

	Dbg_Print("### GetTextIndex: NOT found Text ID! >"..name)
	return -1 -- 見つからない場合

end


-- ▲▲▲ テキストセット管理
----------------------------------------------------------------------------------------------------
-- ▼▼▼ シーンルート管理 


--
-- ■ シーンルートの数を得る
-- @return : レイヤーの数
function GetScnRootCount()
	return #ScnRootSet
end


-- ■ インデックス番号からシーンルート名を得る
-- @param  index : インデックス番号
-- @return       : シーンルート名
function GetScnRootName(index)
	return ScnRootSet[index].name
end


-- ■ シーンルート名からインデックス番号を得る
-- @param layer_name : シーンルート名
-- @return          : インデックス番号
function GetScnRootIndex(scnroot_name)
	local i
	for i = 1, GetScnRootCount(), 1 do
		if(scnroot_name == GetScnRootName(i))then
			return i
		end
	end
	
	Dbg_Print("### GetScnRootIndex: NOT found ScnRoot Name! >"..scnroot_name)
	return -1 -- 見つからない場合
end


-- ■ シーンルートのクラスを得る
-- @param layer_name : シーンルート名
-- @return           : シーンルート
function GetScnRootClass(scnroot_name)
	return ScnRootSet[GetScnRootIndex(scnroot_name)].class
end



-- ■ シーンルートの初期化(単体)
function InitResources_ScnRoot_Single(ScnRoot)

	-- ■ ※シーンルートの確保を先にやっておくこと ※ 

	-- ■ シーンルートの初期化
	local i
	isParallel = true
	if (ScnRoot.isParallel == false) then isParallel = false end
	if (isParallel == false) then
		ScnRoot.class = AddScnRoot(false, "Lua/"..ScnRoot.name, ScnRoot.brres, ScnRoot.arc_name)
	else
		ScnRoot.class = AddScnRoot(true,  "Lua/"..ScnRoot.name, ScnRoot.brres, ScnRoot.arc_name)
	end
end



-- ■ シーンルートの初期化
function InitResources_ScnRoot()
	-- ■ シーンルートの確保
	SetScnRootMax(GetScnRootCount())			-- シーンルートの数を通達
	
	Dbg_Print("ScnRoot Max = "..GetScnRootCount().."\n")

	-- ■ シーンルートの初期化
	for i = 1, GetScnRootCount(), 1 do
		InitResources_ScnRoot_Single(ScnRootSet[i])
	end
--[[
	local i
	for i = 1, GetScnRootCount(), 1 do
		isParallel = true
		if (ScnRootSet[i].isParallel == false) then isParallel = false end
		if (isParallel == false) then
			ScnRootSet[i].class = AddScnRoot(false, "Lua/"..ScnRootSet[i].name, ScnRootSet[i].brres, ScnRootSet[i].arc_name)
		else
			ScnRootSet[i].class = AddScnRoot(true, "Lua/"..ScnRootSet[i].name, ScnRootSet[i].brres, ScnRootSet[i].arc_name)
		end
	end
]]
end



-- ▲▲▲ シーンルート管理 
----------------------------------------------------------------------------------------------------
-- ▼▼▼ リソース管理 



-- ■ リソースの数を得る
-- @return : リソースの数
function GetResourceCount()
--	return table.getn(ResourceSet)
	return #ResourceSet
end
-- ■ 指定リソースのモデル数を得る
-- @return : モデルの数
function GetModelCount(Res)
	return #Res.model
end



-- ■ インデックス番号からリソース名を得る 
-- @param  index : インデックス番号
-- @return       : リソース名
function GetResourceName(index)
	return ResourceSet[index].res_name
end

-- ■ モデル番号から指定リソースのモデル名を得る 
-- @param  Res     : リソース
-- @param  m_index : モデルインデックス番号
-- @return       　: モデル名
function GetModelName(Res, m_index)
	return Res.m_name[m_index]
end


-- ■ リソース名からインデックス番号を得る
-- @param resource_name : リソース名
-- @return              : インデックス番号
function GetResourceIndex(resource_name)
	local i
	for i = 1, GetResourceCount(), 1 do
		if (resource_name == GetResourceName(i)) then
			--Dbg_Print("GetResourceIndex:"..resource_name.." Index="..i)
			return i
		end
	end

	Dbg_Print("### GetResourceIndex: NOT found 3d Obj Name! >"..resource_name)
	return -1 -- 見つからない場合
end

-- ■ モデル名から指定リソース内のインデックス番号を得る
-- @param  Res          : リソース
-- @param resource_name : モデル名
-- @return              : インデックス番号
function GetModelIndex(Res, model_name)
	local i
	for i = 1, GetModelCount(Res), 1 do
		if (model_name == GetModelName(Res, i)) then
			return i
		end
	end

	Dbg_Print("### GetModelIndex: NOT found 3d Model Name! >"..model_name)
	return -1 -- 見つからない場合
end




-- ■ リソースのファイル名を得る（デバッグ用）
-- @param resource_name : リソース名
-- @return              : ファイル名
function GetResourceFileName(resource_name)
	return ResourceSet[GetResourceIndex(resource_name)].filename
end

-- ■ リソースのリンク対象オブジェクト名を得る
-- @param resource_name : リソース名
-- @return              : リンク対象オブジェクト名
function GetResourceLinkObj(resource_name)
	return ResourceSet[GetResourceIndex(resource_name)].link_obj
end

-- ■ リソースの３Ｄオブジェクトを得る
-- @param res_name : リソース名
-- @param mdl_name : モデル名
-- @return         : ３Ｄオブジェクト
--function GetResourceObj3d(resource_name)
--	return ResourceSet[GetResourceIndex(resource_name)].obj
--end
function GetResourceObj3d(mdl_name) --モデル名指定版
--    Dbg_Print("モデル名  "..mdl_name.."  で探す\n")
	local i
	local m
	for i = 1, #ResourceSet, 1 do
	    for m = 1, #ResourceSet[i].m_name, 1 do
			if(ResourceSet[i].m_name[m] == mdl_name)then
			    return ResourceSet[i].model[m]
			end
		end
	end
	return nil
end


-- ■ 全てのリソースを読み込み（デバッグ用）
function LoadResourceAll()
	local i
	local ResNo
	for i = 1, GetResourceCount(), 1 do
	    if(ResourceSet[i].arc_name ~= nil)then
	        ResNo = LoadResource(ResourceSet[i].arc_name, ResourceSet[i].res_name)
	    else
			ResNo = LoadResource(ResourceSet[i].filename, ResourceSet[i].res_name)
		end
		-- ■ 追加アニメの設定
--		if ( (ResNo ~= nil) and (ResourceSet[i].anim ~= nil) ) then
--			local no = 1
--			while (ResourceSet[i].anim[no] ~= nil) do
--			    AddResourceAnim(ResourceSet[i].anim[no], ResNo)
--			    no = no + 1
--			end
--		end
	end
end

model_init_count = 0 -- モデル初期化済み数 

-- ■ リソースの初期化(単体セット指定)
function InitResources_Resource_Single(resSet)

	-- ■ ※リソースの確保を先にやっておくこと。 ※ 
	
	-- ■ リソースの初期化
	local i
	local m
--	local c = model_init_count
	-- ■ 生成された３Ｄオブジェクトクラスを得る
	--ResourceSet[i].obj = GetObj3d(i-1)
	loop = 0
    if(resSet.arc_name ~= nil)then
        loop = GetModelNum(resSet.arc_name)
    else
		loop = GetModelNum(resSet.filename)
	end

--[[
	for m = 1, loop, 1 do
		resSet.model[m] = GetObj3d(c)
		c = c + 1
	end
]]

	-- リソースファイル読み込み
	if(resSet.arc_name ~= nil)then
        LoadResource(resSet.arc_name, resSet.res_name)
    else
		LoadResource(resSet.filename, resSet.res_name)
	end
	
	-- モデル名確保 
--	c = model_init_count
	for m = 1, loop, 1 do
		c = sys_Obj3D[m]
		resSet.model[m] = GetObj3d(c)
		resSet.m_name[m] = GetObj3dName(c)
		Dbg_Print("Obj#"..c.."/Model Ucode = "..resSet.m_name[m].."\n")
--		c = c + 1
	end

--	model_init_count = c

	-- ■ リソースのオブジェクト関連の初期化
	-- ■ シーンルートの設定
	if (resSet.root ~= nil) then
		for m = 1, #resSet.model, 1 do
			Obj3d_SetScnRoot(resSet.model[m], resSet.root)
		end
	end
	-- ■ 別オブジェクトへのリンク
	if (resSet.link_obj ~= nil) then
		for m = 1, #resSet.model, 1 do
			if (resSet.link_mode == nil) then
				Obj3d_SetLink(resSet.model[m], GetResourceObj3d(resSet.link_obj), resSet.joint_name)
			else
				Obj3d_SetLink(resSet.model[m], GetResourceObj3d(resSet.link_obj), resSet.joint_name, resSet.link_mode)
			end
		end
	end

end

ResourceModelMax = 0 -- リソース中のモデル総数 

-- ■ リソースの初期化
-- maxofs : 追加のリソース数 
function InitResources_Resource(maxofs)

	ResourceModelMax = 0
	num = 0
	-- ■ 必要リソース数のカウント 
	for i = 1, GetResourceCount(), 1 do
	    if(ResourceSet[i].arc_name ~= nil)then
	        num = GetModelNum(ResourceSet[i].arc_name)
	    else
			num = GetModelNum(ResourceSet[i].filename)
		end
		Dbg_Print("Model num = "..num.."\n")
		ResourceModelMax = ResourceModelMax + num
	end

	-- ■ リソースの確保
--	SetResourceMax(GetResourceCount())		-- リソースの数を通達
	if (maxofs ~= nil) then ResourceModelMax = ResourceModelMax + maxofs end	-- オフセットを足す 
	SetResourceMax(ResourceModelMax)		-- リソースの数を通達
	
	Dbg_Print("Model Max = "..ResourceModelMax.."\n")

	for i = 1, GetResourceCount(), 1 do
		InitResources_Resource_Single(ResourceSet[i])
	end
--[[
	-- ■ リソースの初期化
	local i
	local m
	local c = 0
	for i = 1, GetResourceCount(), 1 do
		-- ■ 生成された３Ｄオブジェクトクラスを得る
		--ResourceSet[i].obj = GetObj3d(i-1)
		loop = 0
	    if(ResourceSet[i].arc_name ~= nil)then
	        loop = GetModelNum(ResourceSet[i].arc_name)
	    else
			loop = GetModelNum(ResourceSet[i].filename)
		end
--		for m = 1, GetModelNum(ResourceSet[i].filename), 1 do
		for m = 1, loop, 1 do
			ResourceSet[i].model[m] = GetObj3d(c)
			c = c + 1
		end
	end

	-- ■ リソースのオブジェクト関連の初期化
	for i = 1, GetResourceCount(), 1 do
		-- ■ シーンルートの設定
		if (ResourceSet[i].root ~= nil) then
			for m = 1, #ResourceSet[i].model, 1 do
				Obj3d_SetScnRoot(ResourceSet[i].model[m], ResourceSet[i].root)
			end
		end
		-- ■ 別オブジェクトへのリンク
		if (ResourceSet[i].link_obj ~= nil) then
			for m = 1, #ResourceSet[i].model, 1 do
				if (ResourceSet[i].link_mode == nil) then
					Obj3d_SetLink(ResourceSet[i].model[m], GetResourceObj3d(ResourceSet[i].link_obj), ResourceSet[i].joint_name)
				else
					Obj3d_SetLink(ResourceSet[i].model[m], GetResourceObj3d(ResourceSet[i].link_obj), ResourceSet[i].joint_name, ResourceSet[i].link_mode)
				end
			end
		end
	end

	LoadResourceAll()						-- 全てのリソースファイル読み込み
	
	-- モデル名確保 
	c = 0
	for i = 1, GetResourceCount(), 1 do
		for m = 1, #ResourceSet[i].model, 1 do
			ResourceSet[i].m_name[m] = GetObj3dName(c)
			Dbg_Print("Model Ucode = "..ResourceSet[i].m_name[m].."\n")
			c = c + 1
		end
	end
]]
end

-- ■ リソースの初期化
function ReleaseResource()

    model_init_count = 0
    C_ReleaseResource()

end


-- ■ リソースの解放 
function ReleaseResource_Single(resSet)
	local i
	local j

	for j = #resSet, 1, -1 do
		for i = #resSet[j].m_name, 1, -1 do
			Dbg_Print("Remove '"..resSet[j].m_name[i].."'")
			Obj3d_SetScnRoot(resSet[j].model[i], NULL)	-- シーンから削除 
			Obj3d_Remove(resSet[j].model[i])			-- オブジェクト削除 
		end
	end
end



-- ▲▲▲ リソース管理 
----------------------------------------------------------------------------------------------------
-- ▼▼▼ スプライト管理 

--[[

-- ■ スプライトの数を得る
-- @return : スプライトの数
function GetSpriteCount()
	return table.getn(SpriteSet)
end


-- ■ インデックス番号からスプライト名を得る
-- @param  index : インデックス番号
-- @return       : スプライト名
function GetSpriteName(index)
	return SpriteSet[index].name
end


-- ■ スプライト名からインデックス番号を得る
-- @param resource_name : スプライト名
-- @return              : インデックス番号
function GetSpriteIndex(resource_name)
	local i
	for i = 1, GetSpriteCount(), 1 do
		if (resource_name == GetSpriteName(i)) then
--			print("GetSpriteIndex:"", resource_name, "" Index=", i)
			return i
		end
	end
	Dbg_Print("### GetSpriteIndex: NOT found Sprite Obj Name! >"..resource_name)
	return -1 -- 見つからない場合
end


-- ■ スプライトのファイル名を得る（デバッグ用）
-- @param resource_name : スプライト名
-- @return              : ファイル名
function GetSpriteFileName(resource_name)
	return SpriteSet[GetSpriteIndex(resource_name)].filename
end


-- ■ スプライトのスプライトオブジェクトを得る
-- @param resource_name : スプライト名
-- @return              : スプライトオブジェクト
function GetSpriteObj(resource_name)
	return SpriteSet[GetSpriteIndex(resource_name)].obj
end


-- ■ 全てのスプライトを読み込み（デバッグ用）
function LoadSpriteAll()
	local i
	for i = 1, GetSpriteCount(), 1 do
		LoadSprite(SpriteSet[i].filename, SpriteSet[i].name)
	end
end



-- ■ スプライトの初期化
function InitResources_Sprite()
	-- ■ スプライトの確保
	SetSpriteMax(GetSpriteCount())		-- スプライトの数を通達

	-- ■ スプライトの初期化
	local i
	for i = 1, GetSpriteCount(), 1 do
		-- ■ 生成されたスプライトオブジェクトクラスを得る
		SpriteSet[i].obj = GetSprite(i-1)
	end

	LoadSpriteAll()
end

]]

-- ▲▲▲ スプライト管理 
----------------------------------------------------------------------------------------------------
-- ▼▼▼ フォント管理 



-- ■ フォントの数を得る
-- @return : フォントの数
function GetFontCount()
	--return table.getn(FontSet)
	return #FontSet
end


-- ■ インデックス番号からフォント名を得る
-- @param  index : インデックス番号
-- @return       : フォント名
function GetFontName(index)
	return FontSet[index].name
end


-- ■ フォント名からインデックス番号を得る
-- @param font_name : フォント名
-- @return          : インデックス番号
function GetFontIndex(font_name)
	local i
	for i = 1, GetFontCount(), 1 do
		if (font_name == GetFontName(i)) then
			return i
		end
	end

	Dbg_Print("### GetFontIndex: NOT found Font Name! >"..font_name)
	return -1 -- 見つからない場合
end



-- ■ フォントのファイル名を得る（デバッグ用）
-- @param font_name : フォント名
-- @return          : ファイル名
function GetFontFileName(font_name)
	return FontSet[GetFontIndex(font_name)].filename
end

-- ■ フォントのクラスを得る
-- @param font_name : フォント名
-- @return           : フォント
function GetFontClass(font_name)
	return FontSet[GetFontIndex(font_name)].class
end



-- ■ フォントの初期化
function InitResources_Font()
	-- ■ フォントの確保
	SetFontMax(GetFontCount())

	-- ■ フォント初期化
	for i = 1, GetFontCount(), 1 do
		FontSet[i].class = LoadFont(FontSet[i].filename)
	end
end



-- ▲▲▲ フォント管理 
----------------------------------------------------------------------------------------------------
-- ▼▼▼ マージファイル管理 



function LoadMergeFileAll(IsRealArc)
	for i = 1, #ArcSet, 1 do
		if (IsRealArc == true) then
			-- 本番(arc)
			LoadMergeFile(ArcSet.path..ArcSet[i].filename..".arc")
		else
			-- arcエミュレーション(xml)
			name = "test/ScriptShellTestData/Model/"..ArcSet[i].filename..".xml"
			Dbg_Print("● Reading arc(xml-emulated) '"..name.."'\n")
			LoadMergeFile(name)
		end
		Sleep()
	end
end



-- ▲▲▲ マージファイル管理 
----------------------------------------------------------------------------------------------------



end -- if (__system__ == nil)
