-- 複数モデル作成のテーブル
multiCounter = 1

-- 複数モデルのテンポラリテーブル
multiCodeTable = {}

-- 各イベントテーブル
inEvent 	= {}
outEvent	= {}
loopEvent	= {}

ManualPage 	= 1


-- 複数モデルの作成後の登録関数
function SetMultiCodeTable( codeName, idx )
	multiCodeTable[multiCounter][idx] = {}
	multiCodeTable[multiCounter][idx].MdlName = codeName
	multiCodeTable[multiCounter][idx].IsReticle = false
	multiCodeTable[multiCounter][idx].IsShow = false
	multiCodeTable[multiCounter][idx].IsInArea = false

end

-- 複数モデルの作成関数
function UseMultiModel( mdlName, nameTable, multiNum )
	nameTable.MdlNum 	= multiNum
	nameTable.PreDrawNum 	= 0
	multiCodeTable[multiCounter] = nameTable
	RegistMultiModel( mdlName, multiNum )
	multiCounter = multiCounter + 1
end

-- ポイント用のテキストの登録処理
function RegistPointText( points, parserName)
	for idx = 1, #points do
		RegistText( points[idx].MdlName, parserName, points.TextID, points[idx].MdlName, points.TextNode, points[idx].MdlName, true )
	end
end

TextSetTest = {
{
text_name = "MAP",
link_model = "PAUSE_MENU_00", 
link_name = "Cstatus",
isdisp = true, 
},
{
text_name = "STATUS",
link_model = "PAUSE_MENU_00",
link_name = "Cmap",
isdisp = true,
},
}

xx = ""
function blah()
xx = "xx"
end

-- モデル構築後の処理
function Regist()

	LoadText( "normalText", TEXT_RES_PATH, MY_ARC_PATH )
	
	--pcall(blah)
	--MOD
	for idx = 1, #TextSetTest do
		RegistText( TextSetTest[idx].text_name, "normalText", TextSetTest[idx].text_name, TextSetTest[idx].link_model, TextSetTest[idx].link_name )
	end
	--[[
--preposition textbox
Text_SetPos(TextSetTest[1].obj, 300, 300, -1)
--set it as visible, mech switcher will just change it to empty string when needed
Text_SetIsDisp(TextSetTest[1].obj, true)
Text_SetShadow(TextSetTest[1].obj, true)
]]
	
	-- 各テキストセット作成
	for idx = 1, #TextSet1 do
		RegistText( TextSet1[idx].text_name, "normalText", TextSet1[idx].text_name, TextSet1[idx].link_model, TextSet1[idx].link_name )
	end

	-- EXITの文章のみ特殊処理
	if IsNoSave() then
		TextSet1["EXIT_TEXT"].strgrp_name = "WINDOW_QUIT_NO_SAVE"

	end

	-- 下のループ処理には回らないため特別処理
	RegistText( TextSet1["EXIT_TEXT"].text_name, "normalText", TextSet1["EXIT_TEXT"].strgrp_name, TextSet1["EXIT_TEXT"].link_model, TextSet1["EXIT_TEXT"].link_name )


	-- 友軍ポイント詳細文字の作成
	RegistPointText( FriendPoints, "normalText")

	-- 僚機ポイント詳細文字の作成
	RegistPointText( BrotherPoints, "normalText")

	-- 敵ポイント詳細文字の作成
	RegistPointText( EnemyPoints, "normalText")

	-- 防衛ポイント詳細文字の作成
	RegistPointText( DefensePoints, "normalText")

	-- 防衛ポイント詳細文字の作成
	RegistPointText( AttackPoints, "normalText")

	-- 補給ポイント詳細文字の作成
	RegistPointText( SupplyPoints, "normalText")

	-- 武器ポイント詳細文字の作成
	RegistPointText( WeaponPoints, "normalText")

	-- 退却ポイント詳細文字の作成
	RegistPointText( WithdrawPoints2, "normalText")

	-- メッセージに対して表示文字列をバインド
	AddMessageBindFunc("LinkText")
	
	-- 一応メニューオープン
	Message("MENU_OPEN")

	-- マップの２Ｄ表示範囲を設定
	SetMapArea( Map.Top, Map.Bottom, Map.Left, Map.Right )
	SetMapMakerArea( MapMaker.Top, MapMaker.Bottom, MapMaker.Left, MapMaker.Right )
	SetMapMakerStartPos( MapMaker.CenterX, MapMaker.CenterY )

	-- マニュアルページ数設定
	-- SetManualNum( #MANUAL_CONTENT )

	-- ミッションテキスト設定
	if MissionText then
		for idx = 1, #MissionText do
			AddMissionText( MissionText[idx].code, MissionText[idx].value )
		end
	end
	
end

-- テキストとメッセージをリンク
function LinkText( msg )
	if textMessage[msg] == nil then return end

	for idx = 1, #textMessage[msg] do
		DrawLinkText( msg, textMessage[ msg ][idx].text_name, textMessage[ msg ][idx].timing, textMessage[ msg ][idx].type, textMessage[ msg ][idx].color_type, textMessage[ msg ][idx].color)
	end
end

-- インゲーム呼び出し関数
function ProgmaMain()

	--local man2 = AddTextMan(TEXT_RES_PATH, "aaa", "ENDING", "_2d/Pause/PAUSE_A.arc")
	--[[
--load text stuff
descriptionTextSet[1].obj = AddText(man2, descriptionTextSet[1].text_name)
--register dynamic text value
SetTextReplacer(descriptionTextSet , "descReplacer")

--preposition textbox
Text_SetPos(descriptionTextSet[1].obj, 300, 300, -1)
--set it as visible, mech switcher will just change it to empty string when needed
Text_SetIsDisp(descriptionTextSet[1].obj, true)
Text_SetShadow(descriptionTextSet[1].obj, true)
]]


	Update()


	-- ポーズリセット
	if PauseTrigger( "PAUSE_RESET" ) then
		Message( "MENU_CLOSE" )
		MapClose()
	end
	
	-- ポーズのリセット終了（つまり開く）
	if PauseRelease( "PAUSE_RESET" ) then
		MapTextClose()
		Message( "MENU_OPEN" )
	end


	-- マップ選択中
	if PauseRelease( "MAP_SELECT_TRIG" ) then

		-- マップ⇒QUITへ
		if PauseTrigger( "QUIT_SELECT_TRIG" ) then
			Message( "MENU_rQUIT" )
			MapClose()
		end

		-- マップ⇒ステータスイベント
		if PauseTrigger( "STATUS_SELECT_TRIG" ) then
			Message( "MENU_STATUS" )
			MapClose()
		end
		

	end


	-- ステータスイベント
	if PauseRelease( "STATUS_SELECT_TRIG" ) then
		-- ステータス⇒マップ
		if PauseTrigger( "MAP_SELECT_TRIG" ) then
			Message( "MENU_rMAP" )
		end

		-- ステータス⇒ミッション
		if PauseTrigger( "MISSION_SELECT_TRIG" ) then
			Message( "MENU_MISSION" )
		end
		
	end

	-- ミッションイベント
	if PauseRelease( "MISSION_SELECT_TRIG" ) then

		if PauseTrigger( "STATUS_SELECT_TRIG" ) then
			Message( "MENU_rSTATUS" )
		end

		if PauseTrigger( "MANUAL_SELECT_TRIG" ) then
			Message( "MENU_MS" )
			Message( MANUAL_CONTENT[ ManualPage ].ON_MSG )
		end


	end

	-- マニュアル選択
	if PauseRelease( "MANUAL_SELECT_TRIG" ) then
		if PauseTrigger( "MISSION_SELECT_TRIG" ) then
			Message( "MENU_rMISSION" )
			Message( MANUAL_CONTENT[ ManualPage ].OFF_MSG )
		end

		if PauseTrigger( "RETRY_SELECT_TRIG" ) then
			Message( "MENU_RETRY" )
			Message( MANUAL_CONTENT[ ManualPage ].OFF_MSG )
		end

	end

	-- リトライ選択
	if PauseRelease( "RETRY_SELECT_TRIG" ) then
		if PauseTrigger( "MANUAL_SELECT_TRIG" ) then
			Message( "MENU_rMS" )
			Message( MANUAL_CONTENT[ ManualPage ].ON_MSG )
		end

		if PauseTrigger( "RETURN_SELECT_TRIG" ) then
			Message( "MENU_RETURN" )
		end

	end

	-- 戻る選択
	if PauseRelease( "RETURN_SELECT_TRIG" ) then
		if PauseTrigger( "RETRY_SELECT_TRIG" ) then
			Message( "MENU_rRETRY" )
		end

		if PauseTrigger( "QUIT_SELECT_TRIG" ) then
			Message( "MENU_QUIT" )
		end

	end

	-- 終わる選択
	if PauseRelease( "QUIT_SELECT_TRIG" ) then
		if PauseTrigger( "RETURN_SELECT_TRIG" ) then
			Message( "MENU_rRETURN" )
		end

		if PauseTrigger( "MAP_SELECT_TRIG" ) then
			Message( "MENU_MAP" )
		end

	end


	if PauseRelease( "BROTHER_ONE_SELECT_TRIG" ) then
		Message( "PLAYERl_S_OFF" )
	end

	if PauseRelease( "BROTHER_TWO_SELECT_TRIG" ) then
		Message( "PLAYERr_S_OFF" )
	end

	if PauseRelease( "MY_MS_SELECT_TRIG" ) then
		Message( "PLAYER_S_OFF" )
	end


	-- YesAndNoは表示より先に優先させる(メッセージ処理の関係から）
	if PauseTrigger( "SELECT_YES_TRIG" ) then
		inEvent["SELECT_YES_TRIG"]()
	end


	if PauseTrigger( "SELECT_NO_TRIG" ) then
		inEvent["SELECT_NO_TRIG"]()
	end

	eventNum = GetPauseEventNum()

	-- アウトイベント（リリース）
	for idx = 0, eventNum - 1, 1 do
		name = GetPauseEventName( idx )
		if PauseRelease( name ) then
			if outEvent[ name ] then
				outEvent[ name ]()
			end
		end

	end

	-- インイベント（トリガー）
	for idx = 0, eventNum - 1, 1 do
		name = GetPauseEventName( idx )
		if PauseTrigger( name ) then
			if inEvent[ name ] then
				inEvent[ name ]()
			end
		end

	end

	-- ループイベント（状態）
	for idx = 0, eventNum - 1, 1 do
		name = GetPauseEventName( idx )
		if PauseCheck( name ) then
			if loopEvent[ name ] then
				loopEvent[ name ]()
			end
		end

	end
	
end

-- マニュアルを一ページ戻る
function PrevManual()
	ManualPage = ManualPage - 1

	if ManualPage == 0 then
		ManualPage = #MANUAL_CONTENT
	end
end

-- マニュアルを１ページ進める
function NextManual()
	ManualPage = ManualPage + 1

	if ManualPage == #MANUAL_CONTENT + 1 then
		ManualPage = 1
	end
end

-- 自分自身のマップ表示
function DrawMapSelf( mypoint )

	x, y = GetPosition()
	Draw( mypoint.Name, 0.0, 0.0, 0.0, false, x, y, GetYAxisDir( GetRotation() ) )

	mypoint.IsPreShow = mypoint.IsShow

	if mypoint.IsPreShow == false then
		Message( "PLAYER_ON" )
		mypoint.IsShow 	= true
	end



end

-- 自分自身のマップ表示消去
function RemoveMapSelf( mypoint )

	mypoint.IsPreShow = mypoint.IsShow

	if mypoint.IsPreShow then
		Message( "PLAYER_OFF" )
	end
	mypoint.IsShow = false

end

-- 自分自身のマップ表示リセット
function ResetMapSelf( mypoint )
	mypoint.IsShow = false
end

-- マップ表示
function DrawMapPoints( points, name)
	targetNum 	= GetPointNum( name )
	pointIdx	= 1

	-- ポイント表示の初期化
	for idx = 1, points.MdlNum do
		points[idx].IsPreShow 	= points[idx].IsShow
		points[idx].IsShow 	= false
	end

	-- ターゲットの数分チェック
	for idx = 0, targetNum - 1 do
		Draw( points[pointIdx].MdlName, 0.0, 0.0, 0.0, false, GetPointPos( name, idx ) )
		SetPreDrawScale( points.ScaleX, points.ScaleY )
		points[pointIdx].IsShow = true

		if points[pointIdx].IsPreShow == false then
			Anim(points[pointIdx].MdlName, points.In )
		end


		pointIdx = pointIdx + 1

		if pointIdx > points.MdlNum then
			break
		end

		
	end

	-- モデルが残った場合は消去処理
	for idx = pointIdx, points.MdlNum do
		if points[idx].IsPreShow then
			Anim(points[idx].MdlName, points.Out )
		end
	end
end

-- マップ消去
function RemoveMapPoints( points )
	-- ポイント表示の初期化
	for idx = 1, points.MdlNum do
		points[idx].IsPreShow 	= points[idx].IsShow
		points[idx].IsShow 	= false
		if points[idx].IsPreShow then
			Anim(points[idx].MdlName, points.Out )
		end
	end

end

-- 状態のリセット
function ResetMapPoints( points )
	-- ポイント表示の初期化
	for idx = 1, points.MdlNum do
		points[idx].IsShow 	= false
	end
end

-- テキストの表示
function ShowText( points, name)
	targetNum 	= GetPointNum( name )
	pointIdx	= 1
	-- ターゲットの数分チェック
	for idx = 0, targetNum - 1 do
		if points[pointIdx].IsShow then
			Anim( points[pointIdx].MdlName, points.RotText, 0.0, false, 0.0 )
			DrawLinkText( nil, points[pointIdx].MdlName, MSG_START, TEXT_ON ) 
			SetSingleReplacerText( points[pointIdx].MdlName, points.TextID, GetPointCodeName( name, idx ) )
		end
		pointIdx = pointIdx + 1

		if pointIdx > points.MdlNum then
			break
		end


	end

end

-- テキストの非表示
function CloseText( points, name)
	-- ターゲットの数分チェック
	for idx = 1, points.MdlNum do

		Anim( points[idx].MdlName, points.OutText)
		DrawLinkText( nil, points[idx].MdlName, MSG_START, TEXT_OFF ) 

	end
end
